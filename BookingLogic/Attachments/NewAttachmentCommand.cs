namespace AppLogic.Attachments;

public class NewAttachmentCommand : IRequest<IdResultDto>
{
    public Stream Stream { get; set; }
    public byte[] Data { get; set; }
    public string Name { get; set; }
    public string ContentType { get; set; }
    public long Size { get; set; }
    public int BookingId { get; set; }

    public class NewAttachmentValidation : AbstractValidator<NewAttachmentCommand>
    {
        public NewAttachmentValidation()
        {
            RuleFor(_ => _.Name).NotEmpty().MaximumLength(500);
            RuleFor(_ => _.Size).NotEmpty();
            RuleFor(_ => _.ContentType).NotEmpty();
            RuleFor(_ => _.BookingId).NotEmpty();
            When(_ => _.Stream == null, () =>
            {
                RuleFor(x => x.Data).NotEmpty();
            });
            When(_ => _.Data == null, () =>
            {
                RuleFor(x => x.Stream).NotEmpty();
            });
        }
    }

    public class NewAttachmentHandler : IRequestHandler<NewAttachmentCommand, IdResultDto>
    {
        private readonly IFileService _fileSaver;
        private readonly IBookingUnitOfWork _bookingUnitOfWork;
        private readonly IAuthService _auth;

        public NewAttachmentHandler(IFileService fileSaver, IBookingUnitOfWork bookingUnitOfWork, IAuthService auth)
        {
            _fileSaver = fileSaver;
            _bookingUnitOfWork = bookingUnitOfWork;
            _auth = auth;
        }

        public async Task<IdResultDto> Handle(NewAttachmentCommand request, CancellationToken cancellationToken)
        {
            var uploadFile = async () =>
            {
                var internalFileName = Path.GetRandomFileName();
                if (request.Stream == null)
                    return await _fileSaver.SaveFileAsync(internalFileName, request.Data, StorageNames.BlobStorageName);
                else
                    return await _fileSaver.SaveFileAsync(internalFileName, request.Stream, StorageNames.BlobStorageName);
            };

            var isWriter = await _auth.IsBookingWriterAsync();
            if (!isWriter)
                throw new ForbiddenException();

            var booking = await _bookingUnitOfWork.Bookings.FirstOrDefaultAsync(_ => _.Id == request.BookingId, cancellationToken);
            if (booking == null) throw new NotFoundException();
            if (booking.BookingStatus != BookingStatus.Saved)
                throw new BadRequestException($"Cannot upload attachment. Booking with id {request.BookingId} does not have status 'Saved'.");

            var filePath = await uploadFile();
            var newAttachment = new Attachment
            {
                BookingId = request.BookingId,
                Size = request.Size,
                Name = request.Name,
                Path = filePath,
                ContentType = request.ContentType
            };
            await _bookingUnitOfWork.Attachments.AddAsync(newAttachment, cancellationToken);
            await _bookingUnitOfWork.SaveChangesAsync(cancellationToken);

            return new IdResultDto
            {
                Id = newAttachment.Id
            };
        }
    }
}
