namespace AppLogic.Attachments;

public class DeleteAttachmentCommand : IRequest
{
    public int? AttachmentId { get; set; }

    public class DeleteAttachmentValidation : AbstractValidator<DeleteAttachmentCommand>
    {
        public DeleteAttachmentValidation()
        {
            RuleFor(x => x.AttachmentId).NotEmpty();
        }
    }

    public class UpdateAttachmentHandler : IRequestHandler<DeleteAttachmentCommand>
    {
        private readonly IBookingUnitOfWork _bookingUnitOfWork;
        private readonly IAuthService _auth;
        private readonly IFileService _fileSaver;

        public UpdateAttachmentHandler(IBookingUnitOfWork bookingUnitOfWork, IAuthService auth, IFileService fileSaver)
        {
            _bookingUnitOfWork = bookingUnitOfWork;
            _auth = auth;
            _fileSaver = fileSaver;
        }

        public async Task<Unit> Handle(DeleteAttachmentCommand request, CancellationToken cancellationToken)
        {
            var isWriter = await _auth.IsBookingWriterAsync();
            if (!isWriter)
                throw new ForbiddenException();

            var attachmentToDelete = await _bookingUnitOfWork.Attachments.FirstOrDefaultAsync(_ => _.Id == request.AttachmentId, cancellationToken);
            if (attachmentToDelete == null) throw new NotFoundException();

            await _fileSaver.DeleteFileAsync(attachmentToDelete.Name!, StorageNames.BlobStorageName);

            _bookingUnitOfWork.Attachments.Remove(attachmentToDelete);
            await _bookingUnitOfWork.SaveChangesAsync(cancellationToken);

            return await Unit.Task;
        }
    }
}
