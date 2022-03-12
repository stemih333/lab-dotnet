namespace Database.Configuration;

public class CommentConfiguration : BaseEntityConfiguration<Comment>
{
    protected override void ConfigureTypeProperties(EntityTypeBuilder<Comment> builder)
    {
        builder.Property(_ => _.Content).HasMaxLength(300).IsRequired();
        builder.Property(_ => _.BookingId).IsRequired();
    }
}
