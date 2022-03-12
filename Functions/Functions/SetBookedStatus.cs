using System;
using System.IO;
using System.Threading.Tasks;
using AppLogic.Bookings;
using FormatWith;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Models.Enums;
using Models.Values;
using SendGrid.Helpers.Mail;

namespace Functions.Functions
{
    public class SetBookedStatus
    {
        public readonly ILogger<SetBookedStatus> _logger;
        public readonly IMediator _mediator;
        public readonly AppOptions _options;

        public SetBookedStatus(ILogger<SetBookedStatus> logger, IMediator mediator, IOptions<AppOptions> options)
        {
            _logger = logger;
            _mediator = mediator;
            _options = options.Value;
        }

        [FunctionName("O_SetBookedStatus")]
        public static async Task<int> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var bookingIdStringInput = context.GetInput<string>();

            var retryOptions = new RetryOptions(
                firstRetryInterval: TimeSpan.FromSeconds(30),
                maxNumberOfAttempts: 5);

            var bookingId = await context.CallActivityAsync<int>("A_ParseBookingNoToInt", bookingIdStringInput);
            await context.CallActivityAsync("A_ValidateBooking", bookingId);
            await context.CallActivityWithRetryAsync("A_ChangeBookingStatus", retryOptions, bookingId);
            await context.CallActivityWithRetryAsync("A_NotifyCreator", retryOptions, bookingId);

            return bookingId;
        }

        [FunctionName("A_ParseBookingNoToInt")]
        public static int ParseBookingNoToInt([ActivityTrigger] string bookingNoString, ILogger log)
        {
            log.LogInformation("Trying to parse booking number {bookingNoString} to integer.", bookingNoString);
            if (string.IsNullOrWhiteSpace(bookingNoString)) throw new ArgumentNullException(nameof(bookingNoString), "Argument cannot be null or whitespace.");
            if (!int.TryParse(bookingNoString, out var result)) throw new ArgumentException("Failed to parse booking number string to int.");
            return result;
        }

        [FunctionName("A_ValidateBooking")]
        public async Task ValidateBooking([ActivityTrigger] int bookingId, ILogger log)
        {
            log.LogInformation("Validating booking with ID {bookingId}.", bookingId);
            await _mediator.Send(new ValidateBookingCommand { BookingId = bookingId });
        }
        
        [FunctionName("A_ChangeBookingStatus")]
        public async Task ChangeBookingStatus([ActivityTrigger] int bookingId, ILogger log)
        {
            log.LogInformation("Changing status to 'Booked' for booking with ID {bookingId}.", bookingId);
            await _mediator.Send(new UpdateBookingStatusCommand { BookingId = bookingId, Status = BookingStatus.Booked });
        }

        [FunctionName("A_NotifyCreator")]
        public async Task NotifyCreator(
            [ActivityTrigger] int bookingId,
            [Blob("internal-files/template.html", access: FileAccess.Read, Connection = "AzureWebJobsStorage")] Stream stream,
            [SendGrid(ApiKey = "AzureWebJobsSendGridApiKey")] IAsyncCollector<SendGridMessage> messageCollector, 
            ILogger log)
        {
            log.LogInformation("Retrieving booking with ID {bookingId}.", bookingId);
            var booking = await _mediator.Send(new GetBookingByIdQuery { BookingId = bookingId });
            var subject = $"Booking {bookingId} booked";
            using var fileReader = new StreamReader(stream);
            var template = fileReader.ReadToEnd();
            var mailTemplate = template.FormatWith(booking).FormatWith(_options.GuiUrl);
            log.LogInformation("Sending mail to {createdBy}.", booking.CreatedBy);
            var message = new SendGridMessage();
            message.AddTo(booking.CreatedBy);
            message.AddContent("text/html", mailTemplate);
            message.SetFrom(new EmailAddress(_options.FromEmail));
            message.SetSubject(subject);
            await messageCollector.AddAsync(message);
        }


        [FunctionName("T_QueueStart")]
        public static async Task QueueStart(
            [QueueTrigger("tobebooked", Connection = "AzureWebJobsStorage")] string bookingId,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var instanceId = await starter.StartNewAsync<string>("O_SetBookedStatus", bookingId);

            log.LogInformation("Started orchestration with ID = '{instanceId}', booking ID = '{bookingId}'.", instanceId, bookingId);

            await Task.CompletedTask;
        }
    }
}