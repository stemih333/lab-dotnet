using Models.Enums;

namespace Models.Entities;
public class Booking : BaseAuditableEntity
{
    public DateTime? BookingDate { get; set; }
    public string? Approver { get; set; }
    public BookingStatus? BookingStatus { get; set; }
    public IEnumerable<BookingRow> Rows { get; set; } = new HashSet<BookingRow>();
    public IEnumerable<Attachment> Attachments { get; set; } = new HashSet<Attachment>();
    public IEnumerable<Comment> Comments { get; set; } = new HashSet<Comment>();
}
