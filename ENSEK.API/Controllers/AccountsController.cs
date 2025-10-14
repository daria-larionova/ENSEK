using Asp.Versioning;
using ENSEK.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ENSEK.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAccounts()
    {
        var accounts = await _accountService.GetAllAccountsAsync();
        return Ok(accounts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccountWithReadings(int id)
    {
        var account = await _accountService.GetAccountWithReadingsAsync(id);
        return account == null ? NotFound() : Ok(account);
    }
}
