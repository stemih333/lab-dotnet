namespace AppLogic.BookingRows;

public class DeleteBookingRowCommand : IRequest
{
    public int? RowId { get; set; }

    public class DeleteBookingRowValidation : AbstractValidator<DeleteBookingRowCommand>
    {
        public DeleteBookingRowValidation()
        {
            RuleFor(_ => _.RowId).NotEmpty();

        }
    }

    public class DeleteBookingRowHandler : IRequestHandler<DeleteBookingRowCommand>
    {
        private readonly IBookingUnitOfWork _bookingUnitOfWork;
        private readonly IAuthService _auth;

        public DeleteBookingRowHandler(IBookingUnitOfWork bookingUnitOfWork, IAuthService auth)
        {
            _bookingUnitOfWork = bookingUnitOfWork;
            _auth = auth;
        }

        public async Task<Unit> Handle(DeleteBookingRowCommand request, CancellationToken cancellationToken)
        {
            var isWriter = await _auth.IsBookingWriterAsync();
            if (!isWriter) throw new ForbiddenException();

            var row = await _bookingUnitOfWork.BookingRows
                .Include(_ => _.Booking)
                .FirstOrDefaultAsync(_ => _.Id == request.RowId, cancellationToken);

            if (row == null) throw new NotFoundException();
            if (row.Booking?.BookingStatus != BookingStatus.Saved)
                throw new BadRequestException($"Booking with id {row.Booking?.Id} does not have status 'Saved' and row cannot be deleted.");

            var user = await _auth.GetAppUserAsync();
            if (user.Email != row.Booking?.CreatedBy) throw new ForbiddenException("Only creator of booking is allowed to make changes to the booking.");

            _bookingUnitOfWork.BookingRows.Remove(row);
            await _bookingUnitOfWork.SaveChangesAsync(cancellationToken);

            return await Unit.Task;
        }
    }
}
