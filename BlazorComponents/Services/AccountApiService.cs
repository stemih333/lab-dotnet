namespace BlazorComponents.Services;

public class AccountApiService : BaseHttpService, IAccountApiService
{
    private readonly string _apiName;
    private readonly IDownstreamWebApi _api;

    public AccountApiService(IConfiguration configuration, IDownstreamWebApi api, ILoadingService loadingService) : base(loadingService)
    {
        _apiName = configuration.GetValue<string>("DownstreamApi:Name");
        _api = api;
    }


    public async Task<IEnumerable<AccountDto>?> GetAccounts(string searchTerm)
    {
        return await RunRequest(async () =>
        {
            return await _api.CallWebApiForUserAsync<IEnumerable<AccountDto>>(_apiName, opts =>
            {
                opts.RelativePath = "api/account?SearchTerm=" + searchTerm;
            });
        });
    }
}
