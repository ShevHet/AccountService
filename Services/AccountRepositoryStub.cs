using AccountService.Models;
using System.Collections.Concurrent;

namespace AccountService.Services
{
    public class AccountRepositoryStub : IAccountRepository
    {
        private readonly ConcurrentDictionary<Guid, Account> _accounts = new();
        private const int MaxInMemory = 1000;

        public Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult(_accounts.GetValueOrDefault(id));
        }

        public Task<PagedResponse<Account>> GetAccountsAsync(
            int page,
            int size,
            Guid? ownerId = null,
            AccountType? type = null,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var query = _accounts.Values.AsQueryable();

            if (ownerId.HasValue)
                query = query.Where(a => a.OwnerId == ownerId.Value);

            if (type.HasValue)
                query = query.Where(a => a.Type == type.Value);

            var total = query.Count();
            var items = query
                .Skip((page - 1) * size)
                .Take(size)
                .ToList();

            return Task.FromResult(new PagedResponse<Account>(items, page, size, total));
        }

        public Task<Account> CreateAsync(Account account, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            if (_accounts.Count >= MaxInMemory)
                throw new InvalidOperationException("—лишком много счетов, попробуйте позже");

            account.Id = Guid.NewGuid();
            account.OpeningDate = DateTime.UtcNow;
            _accounts[account.Id] = account;
            return Task.FromResult(account);
        }

        public Task UpdateAsync(Account account, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            if (!_accounts.ContainsKey(account.Id))
                throw new KeyNotFoundException("—чет не найден");

            _accounts[account.Id] = account;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Account account, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            if (_accounts.TryGetValue(account.Id, out var acc))
            {
                acc.ClosingDate = DateTime.UtcNow;
            }
            return Task.CompletedTask;
        }

        public Task<bool> ClientHasAccountsAsync(Guid clientId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            return Task.FromResult(
                _accounts.Values.Any(a =>
                    a.OwnerId == clientId &&
                    !a.ClosingDate.HasValue)
            );
        }
    }
}