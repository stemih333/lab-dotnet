namespace Database.Configuration;

public class AccountConfiguration : BaseEntityConfiguration<Account>
{
    protected override void ConfigureTypeProperties(EntityTypeBuilder<Account> builder)
    {
        builder.Property(x => x.Name).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Number).IsRequired().HasMaxLength(5);

        builder.HasIndex(_ => _.Number);
    }
}
