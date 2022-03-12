using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AppLogic.BookingRows;
using AppLogic.Comments;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Models.Values;

namespace Functions.Functions;

public class Comments : BaseFunction<Comments>
{
    public Comments(ILogger<Comments> logger, IMediator mediator) : base(logger, mediator)
    {
    }

    [FunctionName("AddNewComments")]
    [OpenApiOperation(operationId: "Post", tags: new[] { "Comments" })]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(NewBookingRowCommand), Required = true, Description = "Booking ID and comment text.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(int), Description = "New booking row id returned.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "text/plain", bodyType: typeof(string), Description = "User not authorized.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(IEnumerable<ValidationError>), Description = "Validation errors.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "text/plain", bodyType: typeof(string), Description = "Not allowed to access resource.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "text/plain", bodyType: typeof(string), Description = "Server error.")]
    public async Task<IActionResult> Post([HttpTrigger(AuthorizationLevel.Function, "post", Route = "comments")] NewCommentCommand model)
    {
        return await BaseHttpHandler(async () =>
        {
            var id = await _mediator.Send(model);

            return new OkObjectResult(id);
        });
    }
}

