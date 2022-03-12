
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using BlazorComponents.Interfaces;
using BlazorComponents.Services;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Serilog;
using System.Globalization;

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

    if (!builder.Environment.IsDevelopment())
    {
        var vaultUri = Environment.GetEnvironmentVariable("VAULT_URI");
        Log.Information("Fetching secrets from keyvault URI: {vaultUri}", vaultUri);
        var secretClient = new SecretClient(
                            new Uri(vaultUri!),
                            new DefaultAzureCredential(new DefaultAzureCredentialOptions()));
        builder.Configuration.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
    }

    Log.Information("Configuring services");
    ConfigureServices(builder);

    Log.Information("Configuring app");
    var app = builder.Build();
    ConfigureApp(app, builder.Environment);

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


static void ConfigureServices(WebApplicationBuilder builder)
{
    var apiName = builder.Configuration.GetValue<string>("DownstreamApi:Name");
    builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration, "AzureAd")
        .EnableTokenAcquisitionToCallDownstreamApi()
        .AddDownstreamWebApi(apiName, builder.Configuration.GetSection("DownstreamApi"))
        .AddDistributedTokenCaches();

    // Use redis for token caching as im memory cache gets deleted every time app is restarted. Need to login every time app starts.
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("Redis");
        options.InstanceName = "token_cache";
    });

    builder.Services.AddControllersWithViews()
        .AddMicrosoftIdentityUI(); //Sign in/sign out functionality

    builder.Services.AddAuthorization(options =>
    {
        //Require login automatically when accessing application
        options.FallbackPolicy = options.DefaultPolicy; 
    });
    builder.Services.AddHttpContextAccessor();

    builder.Services.AddRazorPages();
    builder.Services.AddServerSideBlazor().AddMicrosoftIdentityConsentHandler();

    builder.Services.AddSingleton<ILoadingService, LoadingService>();
    builder.Services.AddSingleton<IAlertService, AlertService>();

    builder.Services.AddTransient<IAccountApiService, AccountApiService>();
    builder.Services.AddTransient<IBookingsApiService, BookingsApiService>();
    builder.Services.AddTransient<IBookingRowApiService, BookingRowApiService>();
    builder.Services.AddTransient<IBookingStatusApiService, BookingStatusApiService>();
    builder.Services.AddTransient<ICommentApiService, CommentApiService>();

    if (!builder.Environment.IsDevelopment())
        builder.Services.AddApplicationInsightsTelemetry();
}

static void ConfigureApp(WebApplication app, IWebHostEnvironment env)
{
    var culture = new CultureInfo("en-US");
    CultureInfo.CurrentUICulture = culture;
    CultureInfo.DefaultThreadCurrentCulture = culture;
    CultureInfo.DefaultThreadCurrentUICulture = culture;

    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }
    app.UseStaticFiles();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseRouting();

    app.MapControllers();
    app.MapBlazorHub();
    app.MapFallbackToPage("/_Host");
}