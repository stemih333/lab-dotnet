namespace Common.Interfaces;

public interface IAuthService
{
    Task<AppUser> GetAppUserAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<bool> IsBookingReaderAsync();
    Task<bool> IsBookingWriterAsync();
    Task<bool> IsBookingStatusChangerAsync();
}