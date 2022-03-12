namespace AppLogic.BookingRows;
public class NewBookingRowCommand : IRequest<IdResultDto>
{
    public int? BookingId { get; set; }

    public class NewBookingRowValidation : AbstractValidator<NewBookingRowCommand>
    {
        public NewBookingRowValidation()
        {
            RuleFor(_ => _.BookingId).NotEmpty();
        }
    }

    public class NewBookingRowHandler : IRequestHandler<NewBookingRowCommand, IdResultDto>
    {
        private readonly IBookingUnitOfWork _bookingUnitOfWork;
        private readonly IAuthService _auth;

        public NewBookingRowHandler(IBookingUnitOfWork bookingUnitOfWork, IAuthService auth)
        {
            _bookingUnitOfWork = bookingUnitOfWork;
            _auth = auth;
        }

        public async Task<IdResultDto> Handle(NewBookingRowCommand request, CancellationToken cancellationToken)
        {
            var isWriter = await _auth.IsBookingWriterAsync();
            if (!isWriter) throw new ForbiddenException();

            var booking = await _bookingUnitOfWork.Bookings.FirstOrDefaultAsync(_ => _.Id == request.BookingId, cancellationToken);
            if (booking == null) throw new NotFoundException();
            if (booking.BookingStatus != BookingStatus.Saved)
                throw new BadRequestException($"Booking with id {request.BookingId} does not have status 'Saved'.");

            var newRow = new BookingRow { Amount = 0.00M, BookingId = request.BookingId };
            await _bookingUnitOfWork.BookingRows.AddAsync(newRow, cancellationToken);
            await _bookingUnitOfWork.SaveChangesAsync(cancellationToken);

            return new IdResultDto { Id = newRow.Id };
        }
    }
}