using AccountService.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResponse<Account>> GetAccountsAsync(
        int page,
        int size,
        Guid? ownerId = null,
        EAccountType? type = null,
        CancellationToken ct = default);
    Task<Account> CreateAsync(Account account, CancellationToken ct = default);
    Task UpdateAsync(Account account, CancellationToken ct = default);
    Task DeleteAsync(Account account, CancellationToken ct = default);
    Task<bool> ClientHasAccountsAsync(Guid clientId, CancellationToken ct = default);
    Task<bool> CheckHealthAsync();
}