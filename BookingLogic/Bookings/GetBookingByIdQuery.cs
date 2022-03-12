namespace AppLogic.Bookings;

public class GetBookingByIdQuery : IRequest<BookingDto>
{
    public int BookingId { get; set; }

    public class GetBookingByIdValidation : AbstractValidator<GetBookingByIdQuery>
    {
        public GetBookingByIdValidation()
        {
            RuleFor(_ => _.BookingId).NotEmpty();

        }
    }

    public class GetBookingByIdHandler : IRequestHandler<GetBookingByIdQuery, BookingDto>
    {
        private readonly IMapper _mapper;
        private readonly IBookingUnitOfWork _bookingUnitOfWork;
        private readonly IAuthService _auth;

        public GetBookingByIdHandler(IMapper mapper, IBookingUnitOfWork bookingUnitOfWork, IAuthService auth)
        {
            _mapper = mapper;
            _bookingUnitOfWork = bookingUnitOfWork;
            _auth = auth;
        }

        public async Task<BookingDto> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
        {
            var isReader = await _auth.IsBookingReaderAsync();
            if (!isReader) throw new ForbiddenException();

            var booking = await _bookingUnitOfWork.Bookings
                .Include(_ => _.Rows)
                .Include(_ => _.Attachments)
                .Include(_ => _.Comments)
                .FirstOrDefaultAsync(_ => _.Id == request.BookingId, cancellationToken);

            if (booking == null) throw new NotFoundException();
            
            return _mapper.Map<BookingDto>(booking);
        }
    }
}
