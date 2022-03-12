namespace Models.Dtos;

public class SearchBookingDto
{
	public int Id { get; set; }
	public DateTime? BookingDate { get; set; }
	public string? Approver { get; set; }
	public string? CreatedBy { get; set; }
	public DateTime? Created { get; set; }
	public string BookingStatus { get; set; }
	public IEnumerable<AttachmentDto> Attachments { get; set; }
	public IEnumerable<CommentDto> Comments { get; set; }
}
