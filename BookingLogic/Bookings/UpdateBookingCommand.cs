using AppLogic.BaseClasses;

namespace AppLogic.Bookings;
public class UpdateBookingCommand : BaseBookingCommand, IRequest
{
    public class UpdateBookingValidation : AbstractValidator<UpdateBookingCommand>
    {
        public UpdateBookingValidation()
        {
            RuleFor(_ => _.BookingId).NotEmpty();
            RuleFor(_ => _.Approver).MaximumLength(200);
            RuleFor(_ => _.BookingDate).NotEmpty();
            When(_ => _.Rows != null && _.Rows.Any(), () =>
            {
                RuleFor(_ => _.Rows).Custom((rows, ctx) =>
                {
                    foreach (var item in rows.Select((_, i) => new { Row = _, Index = i }))
                    {
                        var result = new BookingRowModelValidation(item.Index + 1).Validate(item.Row);
                        if (!result.IsValid)
                            result.Errors.ForEach(_ => ctx.AddFailure(_));
                    }
                });
            });
        }

        class BookingRowModelValidation : AbstractValidator<BookingRowDto>
        {
            public BookingRowModelValidation(int rowNo)
            {
                RuleFor(_ => _.Id).NotEmpty();
                RuleFor(_ => _.Account).MaximumLength(5).WithName($"Account (Row {rowNo})");
                RuleFor(_ => _.CostCenter).MaximumLength(20).WithName($"Cost center (Row {rowNo})");
                RuleFor(_ => _.SubAccount).MaximumLength(10).WithName($"Sub account (Row {rowNo})");
                RuleFor(_ => _.Amount).ScalePrecision(2, 16).WithName($"Amount (Row {rowNo})");
            }
        }
    }

    public class UpdateBookingHandler : IRequestHandler<UpdateBookingCommand>
    {
        private readonly IBookingUnitOfWork _bookingUnitOfWork;
        private readonly IAuthService _auth;

        public UpdateBookingHandler(IBookingUnitOfWork bookingUnitOfWork, IAuthService auth)
        {
            _bookingUnitOfWork = bookingUnitOfWork;
            _auth = auth;
        }

        public async Task<Unit> Handle(UpdateBookingCommand request, CancellationToken cancellationToken)
        {
            var isWriter = await _auth.IsBookingWriterAsync();
            if (!isWriter) throw new ForbiddenException();

            var booking = await _bookingUnitOfWork.Bookings.Include(_ => _.Rows).FirstOrDefaultAsync(_ => _.Id == request.BookingId, cancellationToken);
            if (booking == null) throw new NotFoundException();
            if (booking.BookingStatus != BookingStatus.Saved)
                throw new BadRequestException($"Booking with id {request.BookingId} does not have status 'Saved'.");

            var user = await _auth.GetAppUserAsync();
            if (user.Email != booking.CreatedBy) throw new ForbiddenException("Only creator of booking is allowed to make changes to the booking.");

            booking.Approver = request.Approver;
            booking.BookingDate = request.BookingDate;

            foreach (var row in request.Rows)
            {
                var rowToUpdate = booking.Rows.FirstOrDefault(_ => _.Id == row.Id);
                if (rowToUpdate == null) throw new BadRequestException($"Failed to update booking. Row with ID {row.Id} could not be found.");
                rowToUpdate.Amount = row.Amount;
                rowToUpdate.CostCenter = row.CostCenter;
                rowToUpdate.SubAccount = row.SubAccount;
                rowToUpdate.Account = row.Account;
            }

            await _bookingUnitOfWork.SaveChangesAsync(cancellationToken);

            return await Unit.Task;
        }
    }
}
