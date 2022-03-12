using Common.Interfaces;
using Models.Values;

namespace Auth;

public class DevAuthService : IAuthService
{

    public Task<AppUser> GetAppUserAsync()
    {
        return Task.FromResult(new AppUser { Email = "ste_mih@msn.com", Name = "Mihailovic, Stefan" });
    }

    public Task<bool> IsAuthenticatedAsync()
        => Task.FromResult(true);

    public Task<bool> IsBookingStatusChangerAsync()
        => Task.FromResult(false);

    public Task<bool> IsBookingReaderAsync()
        => Task.FromResult(true);

    public Task<bool> IsBookingWriterAsync()
        => Task.FromResult(true);
}
