namespace Database.Configuration;

public class BookingRowConfiguration : BaseEntityConfiguration<BookingRow>
{
    protected override void ConfigureTypeProperties(EntityTypeBuilder<BookingRow> builder)
    {
        builder.Property(_ => _.CostCenter).HasMaxLength(20);
        builder.Property(_ => _.Account).HasMaxLength(5);
        builder.Property(_ => _.SubAccount).HasMaxLength(10);
        builder.Property(_ => _.Amount).IsRequired().HasPrecision(16, 2);
        builder.Property(_ => _.BookingId).IsRequired();
    }
}
