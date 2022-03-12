namespace BlazorComponents.Interfaces;

public interface IBookingStatusApiService
{
    Task<AlertMessage> ChangeBookingStatus(UpdateBookingStatusCommand command);

}
