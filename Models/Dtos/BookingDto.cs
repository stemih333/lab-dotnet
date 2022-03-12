using Models.Enums;

namespace Models.Dtos;

public class BookingDto
{
	public int Id { get; set; }
	public DateTime? BookingDate { get; set; }
	public string? Approver { get; set; }
	public string? CreatedBy { get; set; }
	public BookingStatus? BookingStatus { get; set; }
	public IEnumerable<BookingRowDto> Rows { get; set; }
	public IEnumerable<AttachmentDto> Attachments { get; set; }
	public IEnumerable<CommentDto> Comments { get; set; }
}
