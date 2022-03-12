namespace Database.Configuration;

public abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : BaseAuditableEntity
{
    protected abstract void ConfigureTypeProperties(EntityTypeBuilder<TEntity> builder);

    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        ConfigureTypeProperties(builder);

        builder.Property(_ => _.Updated).IsRequired();
        builder.Property(_ => _.Created).IsRequired();
        builder.Property(_ => _.CreatedBy).IsRequired().HasMaxLength(200);
        builder.Property(_ => _.UpdatedBy).IsRequired().HasMaxLength(200);
    }
}
