namespace BlazorComponents.Interfaces;

public interface IAccountApiService
{
    Task<IEnumerable<AccountDto>?> GetAccounts(string searchTerm);
}
