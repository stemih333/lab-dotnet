using Common.Interfaces;

namespace Common.Mediatr;

public class AuthBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly IAuthService _authService;

    public AuthBehaviour(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        if (await _authService.IsAuthenticatedAsync() == false)
            throw new UnauthorizedAccessException();

        return await next();
    }
}