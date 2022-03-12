using Microsoft.AspNetCore.WebUtilities;

namespace BlazorComponents.Services;

public class BookingsApiService : BaseHttpService, IBookingsApiService
{
    private readonly string _apiName;
    private readonly IDownstreamWebApi _api;
    private readonly string _relativePath = "api/booking";

    public BookingsApiService(IConfiguration configuration, IDownstreamWebApi api, ILoadingService loadingService) : base(loadingService)
    {
        _apiName = configuration.GetValue<string>("DownstreamApi:Name");
        _api = api;
    }

    public async Task<IdResultDto> CreateNewBooking(NewBookingCommand model)
    {
        return await RunRequest(async () =>
        {
            return await _api.CallWebApiForUserAsync<NewBookingCommand, IdResultDto>(_apiName, model, opts =>
            {
                opts.HttpMethod = HttpMethod.Post;
                opts.RelativePath = _relativePath;
            });
        });
    }
    
    public async Task UpdateBooking(UpdateBookingCommand model)
    {
        await RunRequest(async () =>
        {
            return await _api.CallWebApiForUserAsync<UpdateBookingCommand, object>(_apiName, model, opts =>
            {
                opts.HttpMethod = HttpMethod.Put;
                opts.RelativePath = _relativePath;
            });
        });
    }

    public async Task<BookingDto> GetBookingById(int id)
    {
        return await RunRequest(async () =>
        {
            return await _api.CallWebApiForUserAsync<BookingDto>(_apiName, opts =>
            {
                opts.RelativePath = _relativePath + "/" + id;
            });
        });
    }

    public async Task<SearchResultDto<SearchBookingDto>?> SearchBookings(SearchBookingsQuery query)
	{
        return await RunRequest(async () =>
        {
            return await _api.CallWebApiForUserAsync<SearchResultDto<SearchBookingDto>>(_apiName, opts =>
            {
                var basePath = _relativePath;
                var dict = GetDictionaryFromSimpleClass(query);
                if (dict != null)
                    basePath = QueryHelpers.AddQueryString(basePath, dict!);

                opts.RelativePath = basePath;
            });
        });
    }

}
