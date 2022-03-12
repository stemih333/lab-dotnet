using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using AppLogic.Attachments;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Models.Values;

namespace Functions.Functions;

public class Attachments : BaseFunction<Attachments>
{
    public Attachments(ILogger<Attachments> logger, IMediator mediator) : base(logger, mediator)
    {
    }

    [FunctionName("AddNewAttachment")]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiOperation(operationId: "Post", tags: new[] { "Attachments" })]
    [OpenApiParameter(name: "Booking ID", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "Booking ID.")]
    [OpenApiRequestBody(contentType: "multipart/form-data", bodyType: typeof(IEnumerable<IFormFileCollection>), Required = true, Description = "Form file collection")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(int), Description = "New booking row id returned.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized, Description = "User not authorized.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(IEnumerable<ValidationError>), Description = "Validation errors.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "text/plain", bodyType: typeof(string), Description = "Not allowed to access resource.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.InternalServerError, Description = "Server error.")]
    public async Task<IActionResult> Post([HttpTrigger(AuthorizationLevel.Function, "post", Route = "attachments/{id}")] HttpRequest req, int id)
    {
        return await BaseHttpHandler(async () =>
        {
            var form = await req.ReadFormAsync();

            var ids = new List<int>();

            foreach(var file in form.Files)
            {
                using var fileStream = file.OpenReadStream();
                using var stream = new MemoryStream();
                await fileStream.CopyToAsync(stream);
                var content = stream.ToArray();

                var command = new NewAttachmentCommand
                {
                    BookingId = id,
                    Name = file.FileName,
                    Size = file.Length,
                    Data = content
                };
                var attachmentId = await _mediator.Send(command);
                ids.Add(attachmentId.Id.Value);
            }

            return new OkObjectResult(ids);
        });
    }

    [FunctionName("DeleteAttachment")]
    [OpenApiOperation(operationId: "Delete", tags: new[] { "Attachments" })]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiParameter(name: "AttachmentId", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "Attachment ID.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "Ok result.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized, Description = "User not authorized.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(IEnumerable<ValidationError>), Description = "Validation errors.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "text/plain", bodyType: typeof(string), Description = "Not allowed to access resource.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.InternalServerError, Description = "Server error.")]
    public async Task<IActionResult> Delete([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "attachments/{AttachmentId:int?}")] DeleteAttachmentCommand model)
    {
        return await BaseHttpHandler(async () =>
        {
            await _mediator.Send(model);
            return new OkResult();
        });
    }
}

