using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AppLogic.Accounts;
using Functions.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Models.Dtos;
using Models.Values;

namespace Functions.Functions;

public class Accounts : BaseFunction<Accounts>
{
    public Accounts(ILogger<Accounts> logger, IMediator mediator) : base(logger, mediator)
    { }

    [FunctionName("SearchAccounts")]
    [OpenApiOperation(operationId: "SearchAccounts", tags: new[] { "Accounts" })]
    [OpenApiParameter(name: "SearchTerm", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The search term parameter.")]
    [OpenApiParameter(name: "ReturnNumber", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "Number of values to return. Must be greater than 0.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<AccountDto>), Description = "The OK response.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "text/plain", bodyType: typeof(string), Description = "User not authorized.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(IEnumerable<ValidationError>), Description = "Validation errors.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "text/plain", bodyType: typeof(string), Description = "Not allowed to access resource.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "text/plain", bodyType: typeof(string), Description = "Server error.")]
    public async Task<IActionResult> SearchAccounts([HttpTrigger(AuthorizationLevel.Function, "get", Route = "accounts/all")] HttpRequest req)
    {
        return await BaseHttpHandler(async () =>
        {
            var model = req.ParseQueryString<SearchAccountsQuery>();

            _logger.LogInformation("Search accounts with term {SearchTerm} and return {ReturnNumber} values.", model.SearchTerm, model.ReturnNumber);

            var accounts = await _mediator.Send(model);

            return new OkObjectResult(accounts);
        });
    }


}

