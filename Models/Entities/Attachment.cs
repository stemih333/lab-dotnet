namespace Models.Entities;
public class Attachment : BaseAuditableEntity
{
    public long Size { get; set; }
    public string? Path { get; set; }
    public string? Name { get; set; }
    public string? ContentType { get; set; }
    public int? BookingId { get; set; }
    public Booking? Booking { get; set; }
}