namespace BlazorComponents.Interfaces;

public interface ICommentApiService
{
    Task<IdResultDto> AddNewComment(NewCommentCommand command);
}
