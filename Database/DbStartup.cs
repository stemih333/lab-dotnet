namespace Database;
public static class DbStartup
{
    public static void ConfigureServices(IServiceCollection services, string connectionString)
    {
        services.AddDbContext<IBookingUnitOfWork, BookingContext>(options => {
            options.UseSqlServer(connectionString, _ => {
                _.CommandTimeout(60);
                _.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            });
        }, ServiceLifetime.Scoped);
    }
}
