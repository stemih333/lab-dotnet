namespace Models.Entities;
public class Comment : BaseAuditableEntity
{
    public string? Content { get; set; }
    public int? BookingId { get; set; }
    public Booking? Booking { get; set; }
}