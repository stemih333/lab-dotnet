using Common.Interfaces;
using Models.Values;
using System.Threading.Tasks;

namespace IntegrationTests
{
    public class TestAuthService : IAuthService
    {
        private readonly bool _isAuthenticated;
        private readonly bool _isBatchUser;
        private readonly bool _isWriter;
        private readonly bool _isReader;

        public TestAuthService(bool isAuthenticated = true, bool isBatchUser = true, bool isWriter = true, bool isReader = true)
        {
            _isAuthenticated = isAuthenticated;
            _isBatchUser = isBatchUser;
            _isWriter = isWriter;
            _isReader = isReader;
        }

        public Task<AppUser> GetAppUserAsync()
        {
            return Task.FromResult(new AppUser { Email = "stefan.mihailovic@if.se", Name = "Mihailovic, Stefan" });
        }

        public Task<bool> IsAuthenticatedAsync()
            => Task.FromResult(_isAuthenticated);

        public Task<bool> IsBookingStatusChangerAsync()
            => Task.FromResult(_isBatchUser);

        public Task<bool> IsBookingReaderAsync()
            => Task.FromResult(_isReader);

        public Task<bool> IsBookingWriterAsync()
            => Task.FromResult(_isWriter);
    }
}