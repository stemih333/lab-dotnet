namespace Models.BaseClasses;
public abstract class BaseAuditableEntity : BaseEntity
{
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
    public string? UpdatedBy { get; set; }
    public string? CreatedBy { get; set; }
}
