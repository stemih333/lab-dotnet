namespace Models.Dtos;

public class CommentDto
{
    public string Content { get; set; }
    public DateTime? Created { get; set; }
    public string CreatedBy { get; set; }
}
