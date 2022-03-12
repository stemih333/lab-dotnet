namespace Models.Entities;

public class Account : BaseAuditableEntity
{
    public string Number { get; set; }
    public string Name { get; set; }
}
