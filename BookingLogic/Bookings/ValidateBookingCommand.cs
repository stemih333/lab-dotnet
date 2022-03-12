namespace AppLogic.Bookings
{
    public class ValidateBookingCommand : IRequest
    {
        public int? BookingId { get; set; }

        public class ValidateBookingValidation : AbstractValidator<ValidateBookingCommand>
        {
            public ValidateBookingValidation()
            {
                RuleFor(_ => _.BookingId).NotEmpty();
            }
        }

        public class ValidateBookingHandler : IRequestHandler<ValidateBookingCommand>
        {
            private readonly IBookingUnitOfWork _bookingUnitOfWork;
            private readonly IAuthService _auth;
            private readonly IBookingValidationService _validator;
            private readonly ILogger<ValidateBookingHandler> _logger;

            public ValidateBookingHandler(IBookingUnitOfWork bookingUnitOfWork, IAuthService auth, IBookingValidationService validator, ILogger<ValidateBookingHandler> logger)
            {
                _bookingUnitOfWork = bookingUnitOfWork;
                _auth = auth;
                _validator = validator;
                _logger = logger;
            }

            public async Task<Unit> Handle(ValidateBookingCommand request, CancellationToken cancellationToken)
            {
                var isWriter = await _auth.IsBookingWriterAsync();
                if (!isWriter) throw new ForbiddenException();

                var booking = await _bookingUnitOfWork.Bookings.Include(_ => _.Rows).FirstOrDefaultAsync(_ => _.Id == request.BookingId, cancellationToken);
                if (booking == null) throw new NotFoundException();

                var accounts = _bookingUnitOfWork.Accounts.Select(_ => _.Number).ToList();
                if (!accounts.Any())
                {
                    _logger.LogWarning("Table Accounts does not contain any data.");
                    throw new NotFoundException();
                }
                await _validator.ValidateBookingAsync(booking, accounts);

                return await Unit.Task;
            }
        }
    }
}