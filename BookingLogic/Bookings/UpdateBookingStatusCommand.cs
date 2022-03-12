namespace AppLogic.Bookings;

public class UpdateBookingStatusCommand : IRequest
{
    public int? BookingId { get; set; }
    public BookingStatus? Status { get; set; }

    public class UpdateBookingStatusValidation : AbstractValidator<UpdateBookingStatusCommand>
    {
        public UpdateBookingStatusValidation()
        {
            RuleFor(_ => _.BookingId).NotEmpty();
            RuleFor(_ => _.Status).NotNull();
        }
    }

    public class UpdateBookingStatusHandler : IRequestHandler<UpdateBookingStatusCommand>
    {
        private readonly IBookingUnitOfWork _bookingUnitOfWork;
        private readonly IAuthService _auth;
        private readonly IQueueService _queueService;
        private readonly ILogger<UpdateBookingStatusHandler> _logger;

        public UpdateBookingStatusHandler(IBookingUnitOfWork bookingUnitOfWork, IAuthService auth, IQueueService queueService, ILogger<UpdateBookingStatusHandler> logger)
        {
            _bookingUnitOfWork = bookingUnitOfWork;
            _auth = auth;
            _queueService = queueService;
            _logger = logger;
        }

        public async Task<Unit> Handle(UpdateBookingStatusCommand request, CancellationToken cancellationToken)
        {
            var isWriter = await _auth.IsBookingWriterAsync();
            if (!isWriter) throw new ForbiddenException();

            var booking = await _bookingUnitOfWork.Bookings.FirstOrDefaultAsync(_ => _.Id == request.BookingId, cancellationToken);
            if (booking == null) throw new NotFoundException();
           
            if (booking.BookingStatus == BookingStatus.Booked || booking.BookingStatus == BookingStatus.Cancelled)
                throw new BadRequestException($"Booking with ID {request.BookingId} cannot change status from Booked/Cancelled.");

            var user = await _auth.GetAppUserAsync();
            var isCreator = user.Email == booking.CreatedBy;
            var isApprover = user.Email == booking.Approver;

            if (request.Status == BookingStatus.Saved)
            {
                if (booking.BookingStatus == BookingStatus.Saved) throw new BadRequestException("Cannot update status from 'Saved' to 'Saved'.");
                if (!isCreator) throw new ForbiddenException("Only creator of booking is allowed to make status changes to the booking.");
            }
            else if (request.Status == BookingStatus.ToBeApproved)
            {
                if (booking.BookingStatus != BookingStatus.Saved) throw new BadRequestException("Status 'ToBeApproved' can only be changed from status 'Saved'.");
                if (!isCreator) throw new ForbiddenException("Only creator of booking is allowed to make status changes to the booking.");
            }
            else if (request.Status == BookingStatus.Cancelled)
            {
                if (!isCreator) throw new ForbiddenException("Only creator of booking is allowed to make status changes to the booking.");
            }
            else if (request.Status == BookingStatus.ToBeBooked)
            {
                if (booking.BookingStatus == BookingStatus.ToBeBooked)
                {
                    throw new ForbiddenException("Booking already has status 'ToBeBooked'.");
                }
                else if (booking.BookingStatus == BookingStatus.ToBeApproved)
                {
                    if (!isApprover) throw new ForbiddenException("Only approver is allowed to make status change.");                   
                } else if (booking.BookingStatus == BookingStatus.Saved)
                {
                    if (!isCreator) throw new ForbiddenException("Only creator of booking is allowed to make status changes to the booking.");
                }
                // await _queueService.InsertMessageToQueue(booking.Id.ToString(), StorageNames.QueueName);
            }
            else if (request.Status != BookingStatus.Booked)
            {
                var isBatchUser = await _auth.IsBookingStatusChangerAsync();
                if (booking.BookingStatus != BookingStatus.ToBeBooked) throw new BadRequestException("Status 'Booked' can only be changed from status 'ToBeBoooked'.");
                if (!isBatchUser) throw new ForbiddenException("Only batch user is allowed to make status change.");
            }

            booking.BookingStatus = request.Status;
            await _bookingUnitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Updated booking {Id} from status {BookingStatus} to status {Status}", booking.Id, booking.BookingStatus.ToString(), request.Status.ToString());
            return await Unit.Task;
        }
    }
}
