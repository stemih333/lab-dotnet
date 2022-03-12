using AppLogic.Accounts;
using System;
using System.Linq;
using System.Threading;
using Xunit;

namespace IntegrationTests;

public class AccountRequestTests
{
    [Theory]
    [InlineData(10, "10", 10)]
    [InlineData(100, "10", 40)]
    [InlineData(10, "101", 7)]
    [InlineData(10, "1110", 1)]
    [InlineData(1000, "", 1000)]
    public async void SearchAccountsQuery_Pass(int returnNumber, string searchTerm, int expectedCount)
    {
        // Arrange
        var auth = new TestAuthService();
        var dateTime = Utils.GetDateTimeService(DateTime.UtcNow);
        var context = Utils.GetDbContext(auth, dateTime);
        var mapper = Utils.GetAutoMapper();
        var request = new SearchAccountsQuery
        {
            ReturnNumber = returnNumber,
            SearchTerm = searchTerm
        };
        var handler = new SearchAccountsQuery.SearchAccountsHandler(mapper, context, auth);

        // Act
        var res = await handler.Handle(request, new CancellationToken());

        // Assert
        Assert.NotNull(res);
        Assert.Equal(expectedCount, res.Count());
    }
}
