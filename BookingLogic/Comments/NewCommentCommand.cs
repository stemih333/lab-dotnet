namespace AppLogic.Comments;
public class NewCommentCommand : IRequest<IdResultDto>
{
    public int? BookingId { get; set; }
    public string Comment { get; set; }

    public class NewCommentValidation : AbstractValidator<NewCommentCommand>
    {
        public NewCommentValidation()
        {
            RuleFor(_ => _.BookingId).NotEmpty();
            RuleFor(_ => _.Comment).NotEmpty().MaximumLength(300);
        }
    }

    public class NewCommentHandler : IRequestHandler<NewCommentCommand, IdResultDto>
    {
        private readonly IBookingUnitOfWork _bookingUnitOfWork;
        private readonly IAuthService _auth;

        public NewCommentHandler(IBookingUnitOfWork bookingUnitOfWork, IAuthService auth)
        {
            _bookingUnitOfWork = bookingUnitOfWork;
            _auth = auth;
        }

        public async Task<IdResultDto> Handle(NewCommentCommand request, CancellationToken cancellationToken)
        {
            var isWriter = await _auth.IsBookingWriterAsync();
            if (!isWriter) throw new ForbiddenException();

            var booking = await _bookingUnitOfWork.Bookings.FirstOrDefaultAsync(_ => _.Id == request.BookingId, cancellationToken);
            if (booking == null) throw new NotFoundException();
            if (booking.BookingStatus != BookingStatus.Saved)
                throw new BadRequestException($"Booking with id {request.BookingId} does not have status 'Saved'.");

            var newComment = new Comment { BookingId = request.BookingId, Content = request.Comment };
            await _bookingUnitOfWork.Comments.AddAsync(newComment, cancellationToken);
            await _bookingUnitOfWork.SaveChangesAsync(cancellationToken);

            return new IdResultDto
            {
                Id = newComment.Id
            };
        }
    }
}