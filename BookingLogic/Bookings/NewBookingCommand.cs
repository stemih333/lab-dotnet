using AppLogic.BaseClasses;

namespace AppLogic.Bookings;

public class NewBookingCommand : BaseBookingCommand, IRequest<IdResultDto>
{
    public class NewBookingValidation : AbstractValidator<NewBookingCommand>
    {
        public NewBookingValidation()
        {
            RuleFor(_ => _.Approver).MaximumLength(200);
            RuleFor(_ => _.BookingDate).NotEmpty();
            When(_ => _.Rows != null && _.Rows.Any(), () =>
            {
                RuleFor(_ => _.Rows).Custom((rows, ctx) =>
                {
                    foreach(var item in rows.Select((_, i) => new { Row = _, Index = i }))
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
                RuleFor(_ => _.Account).MaximumLength(5).WithName($"Account (Row {rowNo})");
                RuleFor(_ => _.CostCenter).MaximumLength(20).WithName($"Cost center (Row {rowNo})");
                RuleFor(_ => _.SubAccount).MaximumLength(10).WithName($"Sub account (Row {rowNo})");
                RuleFor(_ => _.Amount).ScalePrecision(2, 16).WithName($"Amount (Row {rowNo})");
            }
        }
    }



    public class NewBookingHandler : IRequestHandler<NewBookingCommand, IdResultDto>
    {
        private readonly IMapper _mapper;
        private readonly IBookingUnitOfWork _bookingUnitOfWork;
        private readonly IAuthService _auth;

        public NewBookingHandler(IMapper mapper, IBookingUnitOfWork bookingUnitOfWork, IAuthService auth)
        {
            _mapper = mapper;
            _bookingUnitOfWork = bookingUnitOfWork;
            _auth = auth;
        }

        public async Task<IdResultDto> Handle(NewBookingCommand request, CancellationToken cancellationToken)
        {
            var isWriter = await _auth.IsBookingWriterAsync();
            if (!isWriter)
                throw new ForbiddenException();

            var newBooking = _mapper.Map<Booking>(request);
            await _bookingUnitOfWork.Bookings.AddAsync(newBooking, cancellationToken);
            await _bookingUnitOfWork.SaveChangesAsync(cancellationToken);
            return new IdResultDto { Id = newBooking.Id };
        }
    }
}
