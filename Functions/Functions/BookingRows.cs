using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AppLogic.BookingRows;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Models.Values;

namespace Functions.Functions;

public class BookingRows : BaseFunction<BookingRows>
{
    public BookingRows(ILogger<BookingRows> logger, IMediator mediator) : base(logger, mediator)
    {
    }

    [FunctionName("AddNewBookingRow")]
    [OpenApiOperation(operationId: "Post", tags: new[] { "BookingRows" })]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(NewBookingRowCommand))]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(int), Description = "New booking row id returned.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "text/plain", bodyType: typeof(string), Description = "User not authorized.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(IEnumerable<ValidationError>), Description = "Validation errors.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "text/plain", bodyType: typeof(string), Description = "Not allowed to access resource.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "text/plain", bodyType: typeof(string), Description = "Server error.")]
    public async Task<IActionResult> Post([HttpTrigger(AuthorizationLevel.Function, "post", Route = "bookingRows")] NewBookingRowCommand model)
    {
        return await BaseHttpHandler(async () =>
        {
            var id = await _mediator.Send(model);

            return new OkObjectResult(id);
        });
    }

    [FunctionName("DeleteBookingRow")]
    [OpenApiOperation(operationId: "Delete", tags: new[] { "BookingRows" })]
    [OpenApiParameter(name: "Booking row ID", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "ID of the booking row to delete")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "Ok result")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "text/plain", bodyType: typeof(string), Description = "User not authorized.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(IEnumerable<ValidationError>), Description = "Validation errors.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "text/plain", bodyType: typeof(string), Description = "Not allowed to access resource.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "text/plain", bodyType: typeof(string), Description = "Server error.")]
    public async Task<IActionResult> Delete([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "bookingRows/{rowId:int?}")] DeleteBookingRowCommand model)
    {
        return await BaseHttpHandler(async () =>
        {
            await _mediator.Send(model);

            return new OkResult();
        });
    }
}

