using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AppLogic.Bookings;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Models.Dtos;
using Models.Values;

namespace Functions.Functions
{
	public class Bookings : BaseFunction<Bookings>
    {
        public Bookings(ILogger<Bookings> logger, IMediator mediator) : base(logger, mediator)
        {
        }

        [FunctionName("GetBookingById")]
        [OpenApiOperation(operationId: "Get", tags: new[] { "Bookings" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "Booking ID.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(BookingDto), Description = "The booking response.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "text/plain", bodyType: typeof(string), Description = "User not authorized.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(IEnumerable<ValidationError>), Description = "Validation errors.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "text/plain", bodyType: typeof(string), Description = "Not allowed to access resource.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "text/plain", bodyType: typeof(string), Description = "Server error.")]
        public async Task<IActionResult> Get([HttpTrigger(AuthorizationLevel.Function, "get", Route = "bookings/{id:int}")] HttpRequest req, int id)
        {
            return await BaseHttpHandler(async () =>
            {
                var booking = await _mediator.Send(new GetBookingByIdQuery { BookingId = id });

                return new OkObjectResult(booking);
            });
        }

        [FunctionName("AddNewBooking")]
        [OpenApiOperation(operationId: "Post", tags: new[] { "Bookings" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(NewBookingCommand))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(int), Description = "New booking id returned.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "text/plain", bodyType: typeof(string), Description = "User not authorized.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(IEnumerable<ValidationError>), Description = "Validation errors.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "text/plain", bodyType: typeof(string), Description = "Not allowed to access resource.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "text/plain", bodyType: typeof(string), Description = "Server error.")]
        public async Task<IActionResult> Post([HttpTrigger(AuthorizationLevel.Function, "post", Route = "bookings")] NewBookingCommand model)
        {
            return await BaseHttpHandler(async () =>
            {
                var id = await _mediator.Send(model);

                return new OkObjectResult(id);
            });
        }

        [FunctionName("UpdateBooking")]
        [OpenApiOperation(operationId: "Put", tags: new[] { "Bookings" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(UpdateBookingCommand))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "Ok result")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "text/plain", bodyType: typeof(string), Description = "User not authorized.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(IEnumerable<ValidationError>), Description = "Validation errors.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "text/plain", bodyType: typeof(string), Description = "Not allowed to access resource.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "text/plain", bodyType: typeof(string), Description = "Server error.")]
        public async Task<IActionResult> Put([HttpTrigger(AuthorizationLevel.Function, "put", Route = "bookings")] UpdateBookingCommand model)
        {
            return await BaseHttpHandler(async () =>
            {
                await _mediator.Send(model);

                return new OkResult();
            });
        }

        [FunctionName("DeleteBooking")]
        [OpenApiOperation(operationId: "Delete", tags: new[] { "Bookings" })]
        [OpenApiParameter(name: "Booking ID", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "ID of the booking to delete")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "Ok result")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "text/plain", bodyType: typeof(string), Description = "User not authorized.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(IEnumerable<ValidationError>), Description = "Validation errors.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "text/plain", bodyType: typeof(string), Description = "Not allowed to access resource.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "text/plain", bodyType: typeof(string), Description = "Server error.")]
        public async Task<IActionResult> Delete([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "bookings/{bookingId:int?}")] DeleteBookingCommand model)
        {
            return await BaseHttpHandler(async () =>
            {
                await _mediator.Send(model);

                return new OkResult();
            });
        }
    }
}

