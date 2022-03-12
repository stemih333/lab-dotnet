namespace API.Controllers;

public class AccountController : BaseController
{
    [HttpGet]
    public async Task<IEnumerable<AccountDto>> Get([FromQuery] SearchAccountsQuery model) => await Mediator.Send(model);
}
