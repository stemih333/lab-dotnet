namespace BlazorComponents.Interfaces;

public interface IBookingRowApiService
{
    Task<IdResultDto> AddNewBookingRow(NewBookingRowCommand command);
    Task DeleteBookingRow(int rowId);
}
