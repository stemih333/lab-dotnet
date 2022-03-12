namespace API.Controllers;

public class CommentController : BaseController
{
    [HttpPost]
    public async Task<IdResultDto> Post([FromBody] NewCommentCommand model) => await Mediator.Send(model);
}
