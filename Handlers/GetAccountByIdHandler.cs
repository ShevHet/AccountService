using AccountService.Models;
using AccountService.Services;
using MediatR;

namespace AccountService.Handlers
{
    /// <summary>
    /// Запрос получения счета по идентификатору
    /// </summary>
    /// <param name="AccountId">Идентификатор счета</param>
    public sealed record GetAccountByIdQuery(
        /// <summary>
        /// Идентификатор счета
        /// </summary>
        Guid AccountId
    ) : IRequest<Account>;

    public class GetAccountByIdHandler : IRequestHandler<GetAccountByIdQuery, Account>
    {
        private readonly IAccountRepository _repo;

        public GetAccountByIdHandler(IAccountRepository repo)
        {
            _repo = repo;
        }

        public async Task<Account> Handle(GetAccountByIdQuery req, CancellationToken ct)
        {
            var account = await _repo.GetByIdAsync(req.AccountId, ct);
            return account ?? throw new KeyNotFoundException("Счет не найден");
        }
        
    }
}
