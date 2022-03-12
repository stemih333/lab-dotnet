using Common.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Filters;

public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logging;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logging)
    {
        _logging = logging;
    }

    public void OnException(ExceptionContext context)
    {
        var ex = context.Exception;
        _logging.LogError(ex, "An error has occurred.");
        context.Result = ex switch
        {
            AppValidationException => new BadRequestObjectResult(((AppValidationException)ex).Errors),
            BadRequestException or ArgumentNullException => new BadRequestObjectResult(ex.Message),
            UnauthorizedAccessException => new UnauthorizedResult(),
            NotFoundException => new NotFoundObjectResult("Requested resource not found."),
            ForbiddenException => new ForbidResult(),
            _ => new StatusCodeResult(500)
        };
    }
}
