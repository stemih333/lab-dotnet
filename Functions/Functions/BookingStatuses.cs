using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AppLogic.Bookings;
using Functions.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Models.Enums;
using Models.Values;

namespace Functions.Functions;

public class BookingStatuses : BaseFunction<Accounts>
{
    public BookingStatuses(ILogger<Accounts> logger, IMediator mediator) : base(logger, mediator)
    { }

    [FunctionName("SavedStatus")]
    [OpenApiOperation(operationId: "SavedStatus", tags: new[] { "BookingStatuses" })]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(BookingInput), Required = true, Description = "Booking ID")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "text/plain", bodyType: typeof(string), Description = "User not authorized.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(IEnumerable<ValidationError>), Description = "Validation errors.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "text/plain", bodyType: typeof(string), Description = "Not allowed to access resource.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "text/plain", bodyType: typeof(string), Description = "Server error.")]
    public async Task<IActionResult> SavedStatus([HttpTrigger(AuthorizationLevel.Function, "post", Route = "bookingStatus/saved")] BookingInput model)
    {
        return await BaseHttpHandler(async () =>
        {
            await _mediator.Send(new UpdateBookingStatusCommand { BookingId = model.BookingId, Status = BookingStatus.Saved });

            return new OkResult();
        });
    }

    [FunctionName("ToBeApprovedStatus")]
    [OpenApiOperation(operationId: "ToBeApprovedStatus", tags: new[] { "BookingStatuses" })]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(BookingInput), Required = true, Description = "Booking ID")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "text/plain", bodyType: typeof(string), Description = "User not authorized.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(IEnumerable<ValidationError>), Description = "Validation errors.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "text/plain", bodyType: typeof(string), Description = "Not allowed to access resource.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "text/plain", bodyType: typeof(string), Description = "Server error.")]
    public async Task<IActionResult> ToBeApprovedStatus([HttpTrigger(AuthorizationLevel.Function, "post", Route = "bookingStatus/toBeApproved")] BookingInput model)
    {
        return await BaseHttpHandler(async () =>
        {
            await _mediator.Send(new ValidateBookingCommand { BookingId = model.BookingId });
            await _mediator.Send(new UpdateBookingStatusCommand { BookingId = model.BookingId, Status = BookingStatus.ToBeApproved });

            return new OkResult();
        });
    }

    [FunctionName("CancelledStatus")]
    [OpenApiOperation(operationId: "CancelledStatus", tags: new[] { "BookingStatuses" })]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(BookingInput), Required = true, Description = "Booking ID")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "text/plain", bodyType: typeof(string), Description = "User not authorized.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(IEnumerable<ValidationError>), Description = "Validation errors.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "text/plain", bodyType: typeof(string), Description = "Not allowed to access resource.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "text/plain", bodyType: typeof(string), Description = "Server error.")]
    public async Task<IActionResult> CancelledStatus([HttpTrigger(AuthorizationLevel.Function, "post", Route = "bookingStatus/cancelled")] BookingInput model)
    {
        return await BaseHttpHandler(async () =>
        {
            await _mediator.Send(new UpdateBookingStatusCommand { BookingId = model.BookingId, Status = BookingStatus.Cancelled });

            return new OkResult();
        });
    }

    [FunctionName("ToBeBookedStatus")]
    [OpenApiOperation(operationId: "ToBeBookedStatus", tags: new[] { "BookingStatuses" })]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(BookingInput), Required = true, Description = "Booking ID")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "text/plain", bodyType: typeof(string), Description = "User not authorized.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(IEnumerable<ValidationError>), Description = "Validation errors.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "text/plain", bodyType: typeof(string), Description = "Not allowed to access resource.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "text/plain", bodyType: typeof(string), Description = "Server error.")]
    public async Task<IActionResult> ToBeBookedStatus([HttpTrigger(AuthorizationLevel.Function, "post", Route = "bookingStatus/toBeBooked")] BookingInput model)
    {
        return await BaseHttpHandler(async () =>
        {
            await _mediator.Send(new ValidateBookingCommand { BookingId = model.BookingId });
            await _mediator.Send(new UpdateBookingStatusCommand { BookingId = model.BookingId, Status = BookingStatus.ToBeBooked });

            return new OkResult();
        });
    }
}

