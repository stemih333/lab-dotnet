namespace Common.Interfaces;

public interface IBookingUnitOfWork
{
    DbSet<Attachment> Attachments { get; set; }
    DbSet<Booking> Bookings { get; set; }
    DbSet<Comment> Comments { get; set; }
    DbSet<BookingRow> BookingRows { get; set; }
    DbSet<Account> Accounts { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

}
