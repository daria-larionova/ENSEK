using AutoMapper;
using ENSEK.API.Data;
using ENSEK.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ENSEK.API.Services;

public class AccountService : IAccountService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public AccountService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AccountDto>> GetAllAccountsAsync()
    {
        var accounts = await _context.Accounts.ToListAsync();
        return _mapper.Map<IEnumerable<AccountDto>>(accounts);
    }

    public async Task<AccountWithReadingsDto?> GetAccountWithReadingsAsync(int accountId)
    {
        var account = await _context.Accounts
            .Include(a => a.MeterReadings)
            .FirstOrDefaultAsync(a => a.AccountId == accountId);
        
        return account == null ? null : _mapper.Map<AccountWithReadingsDto>(account);
    }
}

