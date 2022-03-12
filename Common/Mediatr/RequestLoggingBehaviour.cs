using Common.Interfaces;

namespace Common.Mediatr;

public class RequestLoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<RequestLoggingBehaviour<TRequest, TResponse>> _logger;
    private readonly IAuthService _fhubAuthorizationService;

    public RequestLoggingBehaviour(ILogger<RequestLoggingBehaviour<TRequest, TResponse>> logger, IAuthService fhubAuthorizationService)
    {
        _logger = logger;
        _fhubAuthorizationService = fhubAuthorizationService;
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        var user = await _fhubAuthorizationService.GetAppUserAsync();
        var requestJson = JsonConvert.SerializeObject(request);
        _logger.LogInformation("{Email}|{request}|{requestJson}", user.Email, request, requestJson);
        return await next();
    }
}