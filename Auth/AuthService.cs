using Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Models.Values;

namespace Auth;

public class AuthService : IAuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<AppUser> GetAppUserAsync()
    {
        if (_httpContextAccessor.HttpContext.User.Identity?.IsAuthenticated != true)
            throw new UnauthorizedAccessException();

        var email = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(_ => _.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
        if (email == null)
            throw new UnauthorizedAccessException("User does not have any E-mail claim.");
        var name = _httpContextAccessor.HttpContext.User.Identity.Name;
        return new AppUser {
            IsBookingReader = await IsBookingReaderAsync(),
            IsBookingWriter = await IsBookingWriterAsync(),
            StatusChanger = await IsBookingStatusChangerAsync(),
            Email = email.Value,
            Name = name ?? "N/A"
        };
    }

    public Task<bool> IsAuthenticatedAsync()
        => Task.FromResult(_httpContextAccessor.HttpContext.User.Identity?.IsAuthenticated == true);

    public Task<bool> IsBookingStatusChangerAsync()
    {
        var roles = _httpContextAccessor.HttpContext.User.Claims.Where(_ => _.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
        return Task.FromResult(roles.Any(_ => _.Value == "BookingRole.StatusChanger"));
    }

    public Task<bool> IsBookingReaderAsync()
    {
        var roles = _httpContextAccessor.HttpContext.User.Claims.Where(_ => _.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
        return Task.FromResult(roles.Any(_ => _.Value == "BookingRole.Reader"));
    }

    public Task<bool> IsBookingWriterAsync()
    {
        var roles = _httpContextAccessor.HttpContext.User.Claims.Where(_ => _.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
        return Task.FromResult(roles.Any(_ => _.Value == "BookingRole.Writer"));
    }
}
