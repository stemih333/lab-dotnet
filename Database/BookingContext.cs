namespace Database;
public class BookingContext : DbContext, IBookingUnitOfWork
{
    private readonly IAuthService _authService;
    private readonly IDateTimeService _dateTimeService;

    public BookingContext(DbContextOptions options, IAuthService authService, IDateTimeService dateTimeService) : base(options)
    {
        _authService = authService;
        _dateTimeService = dateTimeService;
    }

    public virtual DbSet<Attachment> Attachments { get; set; }
    public virtual DbSet<Booking> Bookings { get; set; }
    public virtual DbSet<Comment> Comments { get; set; }
    public virtual DbSet<BookingRow> BookingRows { get; set; }
    public virtual DbSet<Account> Accounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var assemblyWithConfigurations = GetType().Assembly;
        modelBuilder.ApplyConfigurationsFromAssembly(assemblyWithConfigurations);
    }
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseAuditableEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        var user = await _authService.GetAppUserAsync();
        var now = _dateTimeService.GetUtcDateTime();
        foreach (var entityEntry in entries)
        {
            ((BaseAuditableEntity)entityEntry.Entity).Updated = now;
            ((BaseAuditableEntity)entityEntry.Entity).UpdatedBy = user.Email;

            if (entityEntry.State == EntityState.Added)
            {
                ((BaseAuditableEntity)entityEntry.Entity).Created = now;
                ((BaseAuditableEntity)entityEntry.Entity).CreatedBy = user.Email;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

}