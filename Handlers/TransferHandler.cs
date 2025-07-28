using AccountService.Models;
using AccountService.Services;
using MediatR;


namespace AccountService.Handlers
{
    public sealed record TransferCommand(
        Guid FromAccountId,
        Guid ToAccountId,
        decimal Amount,
        string Description
    ) : IRequest<Unit>;

    public class TransferHandler(
        IAccountRepository repository,
        ICurrencyService currencyService)
        : IRequestHandler<TransferCommand, Unit>
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public async Task<Unit> Handle(TransferCommand request, CancellationToken ct)
        {
            var fromAccount = await repository.GetByIdAsync(request.FromAccountId, ct);
            if (fromAccount == null)
            {
                throw new ArgumentException("���� ����������� �� ������");
            }

            var toAccount = await repository.GetByIdAsync(request.ToAccountId, ct);
            if (toAccount == null)
            {
                throw new ArgumentException("���� ���������� �� ������");
            }

            await _semaphore.WaitAsync(ct);
            try
            {
                if (fromAccount.ClosingDate.HasValue)
                {
                    throw new InvalidOperationException("���� ����������� ������");
                }

                if (toAccount.ClosingDate.HasValue)
                {
                    throw new InvalidOperationException("���� ���������� ������");
                }

                if (!fromAccount.Currency.Equals(toAccount.Currency, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"������ ������ �� ���������: {fromAccount.Currency} vs {toAccount.Currency}");
                }

                if (!await currencyService.IsSupportedAsync(fromAccount.Currency, ct))
                {
                    throw new InvalidOperationException("������ �� ��������������");
                }

                decimal commission = CalculateCommission(request.Amount);
                decimal totalDebit = request.Amount + commission;

                if (fromAccount.Balance < totalDebit && fromAccount.Type != AccountType.Credit)
                {
                    throw new InvalidOperationException(
                        $"������������ �������. ���������: {totalDebit}, ��������: {fromAccount.Balance}");
                }

                fromAccount.Balance -= totalDebit;
                toAccount.Balance += request.Amount;

                fromAccount.Transactions.Add(CreateTransaction(
                    fromAccount,
                    toAccount,
                    request.Amount,
                    commission,
                    request.Description,
                    TransactionType.Debit));

                toAccount.Transactions.Add(CreateTransaction(
                    toAccount,
                    fromAccount,
                    request.Amount,
                    0,
                    request.Description,
                    TransactionType.Credit));

                await repository.UpdateAsync(fromAccount, ct);
                await repository.UpdateAsync(toAccount, ct);
            }
            finally
            {
                _semaphore.Release();
            }

            return Unit.Value;
        }

        private static decimal CalculateCommission(decimal amount)
        {
            decimal commission = amount * 0.005m;
            return Math.Clamp(commission, 10, 1000);
        }

        private static Transaction CreateTransaction(
            Account account,
            Account counterparty,
            decimal amount,
            decimal commission,
            string description,
            TransactionType type)
        {
            return new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = account.Id,
                CounterpartyAccountId = counterparty.Id,
                Amount = amount,
                Currency = account.Currency,
                Type = type,
                Description = commission > 0
                    ? $"{description} + �������� {commission}"
                    : description,
                DateTime = DateTime.UtcNow
            };
        }
    }
}