using Common.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Functions.Functions;

public abstract class BaseFunction<M>
{
    protected readonly ILogger<M> _logger;
    protected readonly IMediator _mediator;

    protected BaseFunction(ILogger<M> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public async Task<IActionResult> BaseHttpHandler(Func<Task<IActionResult>> func)
    {
        try
        {
            return await func();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception has occurred.");

            return ex switch
            {
                AppValidationException => new BadRequestObjectResult(((AppValidationException)ex).Errors),
                BadRequestException or ArgumentNullException => new BadRequestObjectResult(ex.Message),
                UnauthorizedAccessException => new UnauthorizedResult(),
                NotFoundException => new NotFoundObjectResult("Requested resource not found."),
                ForbiddenException => new ForbidResult(),
                _ => throw new Exception("An internal server error occurred.")
            };
        }
    }
}
