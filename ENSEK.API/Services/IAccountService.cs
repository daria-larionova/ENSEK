using ENSEK.API.DTOs;

namespace ENSEK.API.Services;

public interface IAccountService
{
    Task<IEnumerable<AccountDto>> GetAllAccountsAsync();
    Task<AccountWithReadingsDto?> GetAccountWithReadingsAsync(int accountId);
}


