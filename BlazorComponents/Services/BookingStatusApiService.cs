using Models.Values;
using Newtonsoft.Json;
using System.Text;

namespace BlazorComponents.Services;

public class BookingStatusApiService : BaseHttpService, IBookingStatusApiService
{
    private readonly string _apiName;
    private readonly IDownstreamWebApi _api;
    private readonly string _relativePath = "api/bookingStatus";
    private readonly IAlertService _alertService;

    public BookingStatusApiService(IConfiguration configuration, IDownstreamWebApi api, ILoadingService loadingService, IAlertService alertService) : base(loadingService)
    {
        _apiName = configuration.GetValue<string>("DownstreamApi:Name");
        _api = api;
        _alertService = alertService;
    }

    public async Task<AlertMessage> ChangeBookingStatus(UpdateBookingStatusCommand command)
    {
        return await RunRequest(async () =>
        {
            var res = await _api.CallWebApiForUserAsync(_apiName, opts =>
            {
                opts.HttpMethod = HttpMethod.Post;
                opts.RelativePath = _relativePath;
            }, content: new StringContent(JsonConvert.SerializeObject(command), Encoding.UTF8, "application/json"));

            if (res.IsSuccessStatusCode) return null;

            var errorMessage = res.ReasonPhrase;
            try
            {
                var content = await res.Content.ReadAsStringAsync();
                if (res.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var json = JsonConvert.DeserializeObject<IEnumerable<ValidationError>>(content);
                    var stringBuilder = new StringBuilder("<b>Validation errors have occurred:</b><br />");
                    foreach (var error in json)
                    {
                        stringBuilder.AppendLine(error.ErrorMessage + "<br />");
                    }
                    return new AlertMessage { CssClass = "alert-danger", Duration = 999999, Message = stringBuilder.ToString() };
                }
            }
            catch
            {
                errorMessage = "A error occurred when parsing error response from API.";
            }

            return new AlertMessage { Message = errorMessage, CssClass = "alert-danger" };
        });
    }
}
