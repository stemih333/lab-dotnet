namespace AppLogic.Bookings;
public class DeleteBookingCommand : IRequest
{
    public int? BookingId { get; set; }

    public class DeleteBookingValidation : AbstractValidator<DeleteBookingCommand>
    {
        public DeleteBookingValidation()
        {
            RuleFor(_ => _.BookingId).NotEmpty();

        }
    }

    public class DeleteBookingHandler : IRequestHandler<DeleteBookingCommand>
    {
        private readonly IBookingUnitOfWork _bookingUnitOfWork;
        private readonly IAuthService _auth;

        public DeleteBookingHandler(IBookingUnitOfWork bookingUnitOfWork, IAuthService auth)
        {
            _bookingUnitOfWork = bookingUnitOfWork;
            _auth = auth;
        }

        public async Task<Unit> Handle(DeleteBookingCommand request, CancellationToken cancellationToken)
        {
            var isWriter = await _auth.IsBookingWriterAsync();
            if (!isWriter) throw new ForbiddenException();

            var booking = await _bookingUnitOfWork.Bookings.FirstOrDefaultAsync(_ => _.Id == request.BookingId, cancellationToken);
            if (booking == null) throw new NotFoundException();
            if (booking.BookingStatus != BookingStatus.Saved)
                throw new BadRequestException($"Booking with id {request.BookingId} does not have status 'Saved' and cannot be deleted.");

            var user = await _auth.GetAppUserAsync();
            if (user.Email != booking.CreatedBy) throw new ForbiddenException("Only creator of booking is allowed to make changes to the booking.");

            _bookingUnitOfWork.Bookings.Remove(booking);
            await _bookingUnitOfWork.SaveChangesAsync(cancellationToken);

            return await Unit.Task;
        }
    }
}
