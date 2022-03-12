namespace BlazorComponents.Interfaces;

public interface IBookingsApiService
{
    Task<SearchResultDto<SearchBookingDto>?> SearchBookings(SearchBookingsQuery model);
    Task<IdResultDto> CreateNewBooking(NewBookingCommand command);
    Task UpdateBooking(UpdateBookingCommand command);
    Task<BookingDto> GetBookingById(int id);

}
