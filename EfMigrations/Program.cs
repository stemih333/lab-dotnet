using Auth;
using Common.Interfaces;
using Common.Services;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Models.Entities;
using System.Globalization;

var host = Host.CreateDefaultBuilder().ConfigureServices((_, services) => {
    var connectionString = "Data Source=localhost,1433;Initial Catalog=BookingsDb;Integrated Security=False;User ID=sa;Password=Sommaren1984!;";
    DbStartup.ConfigureServices(services, connectionString);
    AuthStartup.ConfigureDevelopmentServices(services);
    services.AddSingleton<IDateTimeService, DateTimeService>();
});

var build = host.Build();

var config = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    HasHeaderRecord = false,
};

using var reader = new StreamReader(@"C:\Users\ste_m\Downloads\Accounts.csv");
using var csv = new CsvReader(reader, config);

var records = csv.GetRecords<CsvAccount>();
var filteredRecords = records
    .Where(_ => !string.IsNullOrWhiteSpace(_.Account) && _.Account.All(char.IsDigit) && _.Account.Length > 3 && _.Account.Length <= 5)
    .Select(_ => new Account
    {
        Name = _.Name.Trim(),
        Number = _.Account.Trim(),
        Created = DateTime.UtcNow,
        Updated = DateTime.UtcNow,
        CreatedBy = "BATCH",
        UpdatedBy = "BATCH"
    });

var unitOfWork = build.Services.GetService<IBookingUnitOfWork>();
if (unitOfWork != null)
{
    await unitOfWork.Accounts.AddRangeAsync(filteredRecords);
    await unitOfWork.SaveChangesAsync();
}


class CsvAccount
{
    [Index(0)]
    public string Account { get; set; }

    [Index(1)]
    public string Name { get; set; }
}