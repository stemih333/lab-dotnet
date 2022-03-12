using Models.Enums;

namespace Database.Configuration;

public class BookingConfiguration : BaseEntityConfiguration<Booking>
{
    protected override void ConfigureTypeProperties(EntityTypeBuilder<Booking> builder)
    {
        builder.Property(x => x.Approver).HasMaxLength(200);
        builder.Property(x => x.BookingDate).IsRequired();
        builder.Property(x => x.BookingStatus)
        .IsRequired()
        .HasMaxLength(100)
        .HasDefaultValue(BookingStatus.Saved)
        .HasConversion(
            v => v.ToString(),
#pragma warning disable CS8604 // Possible null reference argument.
            v => (BookingStatus)Enum.Parse(typeof(BookingStatus), v)); ;
#pragma warning restore CS8604 // Possible null reference argument.

        builder.HasMany(_ => _.Attachments).WithOne(_ => _.Booking).HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(_ => _.Rows).WithOne(_ => _.Booking).HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(_ => _.Comments).WithOne(_ => _.Booking).HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.Cascade);
    }
}
