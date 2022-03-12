
namespace AppLogic;

public class AutomapperProfile : Profile
{
    public AutomapperProfile()
    {
        CreateMap<Account, AccountDto>();
        CreateMap<BookingRowDto, BookingRow>();
        CreateMap<BookingRow, BookingRowDto>();
        CreateMap<NewBookingCommand, Booking>();
        CreateMap<Attachment, AttachmentDto>();
        CreateMap<Comment, CommentDto>();
        CreateMap<Booking, SearchBookingDto>();
        CreateMap<Booking, BookingDto>();
    }
}
