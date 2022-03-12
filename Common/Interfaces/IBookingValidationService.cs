namespace Common.Interfaces;

public interface IBookingValidationService
{
    Task ValidateBookingAsync(Booking booking, IEnumerable<string> accounts);
}
