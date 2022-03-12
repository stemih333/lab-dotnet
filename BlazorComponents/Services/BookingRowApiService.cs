namespace BlazorComponents.Services;

public class BookingRowApiService : BaseHttpService, IBookingRowApiService
{
    private readonly string _apiName;
    private readonly IDownstreamWebApi _api;
    private readonly string _relativePath = "api/bookingRow";

    public BookingRowApiService(IConfiguration configuration, IDownstreamWebApi api, ILoadingService loadingService) : base(loadingService)
    {
        _apiName = configuration.GetValue<string>("DownstreamApi:Name");
        _api = api;
    }

    public async Task<IdResultDto> AddNewBookingRow(NewBookingRowCommand command)
    {
        return await RunRequest(async () =>
        {
            return await _api.CallWebApiForUserAsync<NewBookingRowCommand, IdResultDto>(_apiName, command, opts =>
            {
                opts.HttpMethod = HttpMethod.Post;
                opts.RelativePath = _relativePath;
            });
        });
    }

    public async Task DeleteBookingRow(int rowId)
    {
        await RunRequest(async () =>
        {
            return await _api.CallWebApiForUserAsync(_apiName, opts =>
            {
                opts.HttpMethod = HttpMethod.Delete;
                opts.RelativePath = _relativePath + "/" + rowId;
            });
        });
    }
}
