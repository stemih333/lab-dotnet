namespace Models.Dtos;

public class AttachmentDto
{
	public int Id { get; set; }
	public long? Size { get; set; }
	public string? Path { get; set; }
	public string? Name { get; set; }
	public string? ContentType { get; set; }
	public byte[]? FileContent { get; set; }
	public Stream? FileStream { get; set; }
	public DateTime? Created { get; set; }
}
