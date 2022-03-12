namespace AppLogic.Attachments;

public class GetAttachmentByIdQuery : IRequest<AttachmentDto>
{
    public int AttachmentId { get; set; }
    public bool GetAsStream { get; set; }

    public class GetAttachmentByIdValidation : AbstractValidator<GetAttachmentByIdQuery>
    {
        public GetAttachmentByIdValidation()
        {
            RuleFor(_ => _.AttachmentId).NotEmpty();
        }
    }

    public class GetAttachmentByIdHandler : IRequestHandler<GetAttachmentByIdQuery, AttachmentDto>
    {
        private readonly IFileService _fileSaver;
        private readonly IBookingUnitOfWork _bookingUnitOfWork;
        private readonly IAuthService _auth;

        public GetAttachmentByIdHandler(IFileService fileSaver, IBookingUnitOfWork bookingUnitOfWork, IAuthService auth)
        {
            _fileSaver = fileSaver;
            _bookingUnitOfWork = bookingUnitOfWork;
            _auth = auth;
        }

        public async Task<AttachmentDto> Handle(GetAttachmentByIdQuery request, CancellationToken cancellationToken)
        {
            var isReader = await _auth.IsBookingReaderAsync();
            if (!isReader)
                throw new ForbiddenException();

            var file = await _bookingUnitOfWork.Attachments.FirstOrDefaultAsync(_ => _.Id == request.AttachmentId, cancellationToken);
            if (file == null)
                throw new NotFoundException();

            return new AttachmentDto
            {
                ContentType = file.ContentType,
                Created = file.Created,
                Name = file.Name,
                Path = file.Path,
                Size = file.Size,
                FileContent = !request.GetAsStream ? await _fileSaver.GetBlobBytesAsync(file.Path!, StorageNames.BlobStorageName) : default,
                FileStream = request.GetAsStream ? await _fileSaver.GetBlobStreamAsync(file.Path!, StorageNames.BlobStorageName) : default,
            };
        }
    }
}
