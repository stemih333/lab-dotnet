using API.Filters;
using Microsoft.ApplicationInsights.Extensibility;

Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

try
{
    Log.Information("Starting up");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .Enrich.FromLogContext()
            .MinimumLevel.Information()
            .WriteTo.Console();

        if (!builder.Environment.IsDevelopment())
            configuration
                .WriteTo.ApplicationInsights(services.GetRequiredService<TelemetryConfiguration>(), TelemetryConverter.Traces);
    });

    var vaultUri = Environment.GetEnvironmentVariable("VAULT_URI");

    if (!string.IsNullOrEmpty(vaultUri))
	{
		Log.Information("Fetching secrets from keyvault URI: {vaultUri}", vaultUri);
		var secretClient = new SecretClient(
							new Uri(vaultUri!),
							new DefaultAzureCredential(new DefaultAzureCredentialOptions()));
		builder.Configuration.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
	}
    var authEnabled = Environment.GetEnvironmentVariable("DISABLE_AUTH") == null;
    Log.Information("Auth is enabled: " + authEnabled);
    Log.Information("Configuring services");
    ConfigureServices(builder, authEnabled);

    Log.Information("Configuring app");
    var app = builder.Build();
    ConfigureApp(app, builder.Environment, authEnabled);

    Log.Information("Running app...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}


static void ConfigureServices(WebApplicationBuilder builder, bool authEnabled)
{
    builder.Services.AddControllers(_ =>
    {
        _.Filters.Add<GlobalExceptionFilter>();
    });//.AddNewtonsoftJson(_ => _.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore);

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    
    builder.Services
        .AddOptions<AppOptions>()
        .Configure<IConfiguration>((opts, configuration) =>
        {
            configuration
            .GetSection("AppOptions")
            .Bind(opts);
        });

    var connectionString = builder.Configuration.GetConnectionString("BookingDbConnectionString");

    CommonAppLogicStartup.ConfigureServices(builder.Services);
    BookingLogicStartup.ConfigureServices(builder.Services);
    DbStartup.ConfigureServices(builder.Services, connectionString);
    StorageServicesStartup.ConfigureServices(builder.Services);

    if (!authEnabled)
    {
        AuthStartup.ConfigureDevelopmentServices(builder.Services);       
    }
    else
    {
        AuthStartup.ConfigureServices(builder.Services);

        builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration, "AzureAd")
            .EnableTokenAcquisitionToCallDownstreamApi()
            // .AddMicrosoftGraph(builder.Configuration.GetSection("DownstreamApi"))
            .AddInMemoryTokenCaches();


        builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            // Sets IIdentity Name property
            options.TokenValidationParameters.NameClaimType = "name";
            options.TokenValidationParameters.RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
        });
    }

    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        // Disable automatic model state validation. Fluentvalidation takes care of validation in AppLogic layer.
        options.SuppressModelStateInvalidFilter = true;
    });

    if (!builder.Environment.IsDevelopment())
        builder.Services.AddApplicationInsightsTelemetry();
}

static void ConfigureApp(WebApplication app, IWebHostEnvironment env, bool authEnabled)
{
    var culture = new CultureInfo("en-US");
    CultureInfo.CurrentUICulture = culture;
    CultureInfo.DefaultThreadCurrentCulture = culture;
    CultureInfo.DefaultThreadCurrentUICulture = culture;

    app.UseHttpsRedirection();

    app.UseSwagger();
    app.UseSwaggerUI();

    if (!authEnabled)
	{
        app.MapControllers().AllowAnonymous();      
    } else
	{
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers().RequireAuthorization();
    }      
}
