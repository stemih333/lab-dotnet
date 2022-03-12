namespace API.Controllers;

public class AttachmentController : BaseController
{
    [HttpGet("{attachmentId:int}")]
    public async Task<IActionResult> Get(int attachmentId, [FromQuery] bool getAsStream)
    {
        var attachment = await Mediator.Send(new GetAttachmentByIdQuery { AttachmentId = attachmentId, GetAsStream = getAsStream });
        if (attachment.FileContent != null)
            return new FileContentResult(attachment.FileContent, attachment.ContentType!)
            {
                FileDownloadName = attachment.Name
            };
        else if (attachment.FileStream != null)
            return new FileStreamResult(attachment.FileStream, attachment.ContentType!)
            {
                FileDownloadName = attachment.Name
            };

        return BadRequest();
    }

    [HttpPost("{bookingId:int}")]
    public async Task<IEnumerable<IdResultDto>> Post(int bookingId, [FromForm]IEnumerable<IFormFile> files)
    {
        var ids = new List<IdResultDto>();

        foreach (var file in files)
        {
            using var fileStream = file.OpenReadStream();
            using var stream = new MemoryStream();
            await fileStream.CopyToAsync(stream);
            var content = stream.ToArray();

            var command = new NewAttachmentCommand
            {
                BookingId = bookingId,
                Name = file.FileName,
                Size = file.Length,
                Data = content,
                ContentType = file.ContentType
            };
            var attachmentId = await Mediator.Send(command);
            ids.Add(attachmentId);
        }

        return ids;
    }

    [HttpDelete("{attachmentId:int}")]
    public async Task Delete(int attachmentId) => await Mediator.Send(new DeleteAttachmentCommand { AttachmentId = attachmentId });
}
