using AccountService.Models;
using AccountService.Services;
using FluentValidation;
using MediatR;

namespace AccountService.Handlers
{
    public sealed record GetTransactionsQuery(
        Guid? AccountId = null,
        int Page = 1,
        int Size = 10
    ) : IRequest<PagedResponse<Transaction>>;

    public class GetTransactionsValidator : AbstractValidator<GetTransactionsQuery>
    {
        public GetTransactionsValidator()
        {
            RuleFor(x => x.Page).GreaterThan(0);
            RuleFor(x => x.Size).InclusiveBetween(1, 100);
        }
    }

    public class GetTransactionsHandler : IRequestHandler<GetTransactionsQuery, PagedResponse<Transaction>>
    {
        private readonly IAccountRepository _repository;

        public GetTransactionsHandler(IAccountRepository repository)
        {
            _repository = repository;
        }

        public async Task<PagedResponse<Transaction>> Handle(GetTransactionsQuery request, CancellationToken ct)
        {
            var accountsResponse = await _repository.GetAccountsAsync(1, int.MaxValue);
            var allTransactions = new List<Transaction>();

            foreach (var account in accountsResponse.Items)
            {
                if (request.AccountId == null || account.Id == request.AccountId)
                {
                    allTransactions.AddRange(account.Transactions);
                }
            }

            var totalCount = allTransactions.Count;
            var items = allTransactions
                .Skip((request.Page - 1) * request.Size)
                .Take(request.Size)
                .ToList();

            return new PagedResponse<Transaction>(items, request.Page, request.Size, totalCount);
        }
    }
}