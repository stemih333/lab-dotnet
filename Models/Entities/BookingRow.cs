namespace Models.Entities;
public class BookingRow : BaseAuditableEntity
{
    public string? CostCenter { get; set; }
    public string? Account { get; set; }
    public string? SubAccount { get; set; }
    public decimal? Amount { get; set; }
    public int? BookingId { get; set; }
    public Booking? Booking { get; set; }
}