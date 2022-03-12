namespace Database.Configuration;

public class AttachmentConfiguration : BaseEntityConfiguration<Attachment>
{
    protected override void ConfigureTypeProperties(EntityTypeBuilder<Attachment> builder)
    {
        builder.Property(x => x.Name).IsRequired().HasMaxLength(500);
        builder.Property(x => x.ContentType).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.Size).IsRequired();
        builder.Property(x => x.Path).IsRequired().HasMaxLength(2100);
        builder.Property(_ => _.BookingId).IsRequired();
    }
}
