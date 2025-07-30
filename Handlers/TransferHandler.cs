using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using AccountService.Models;
using AccountService.Services;
using AccountService.Configuration;

namespace AccountService.Handlers
{
    /// <summary>
    /// Команда для перевода средств между счетами
    /// </summary>
    public sealed record TransferCommand(
        Guid FromAccountId,
        Guid ToAccountId,
        decimal Amount,
        string Description
    ) : IRequest<Unit>;

    /// <summary>
    /// Обработчик перевода средств между счетами
    /// </summary>
    public sealed class TransferHandler : IRequestHandler<TransferCommand, Unit>
    {
        private readonly IAccountRepository _repository;
        private readonly ICurrencyService _currencyService;
        private readonly CommissionSettings _commissionSettings;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public TransferHandler(
            IAccountRepository repository,
            ICurrencyService currencyService,
            IOptions<AccountServiceOptions> options)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _currencyService = currencyService ?? throw new ArgumentNullException(nameof(currencyService));
            _commissionSettings = options?.Value?.Commission ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<Unit> Handle(TransferCommand request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var fromAccount = await _repository.GetByIdAsync(request.FromAccountId, cancellationToken).ConfigureAwait(false);
            if (fromAccount == null)
                throw new ArgumentException("Счет отправителя не найден", nameof(request.FromAccountId));

            var toAccount = await _repository.GetByIdAsync(request.ToAccountId, cancellationToken).ConfigureAwait(false);
            if (toAccount == null)
                throw new ArgumentException("Счет получателя не найден", nameof(request.ToAccountId));

            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                ValidateAccounts(fromAccount, toAccount);

                if (!await _currencyService.IsSupportedAsync(fromAccount.Currency, cancellationToken).ConfigureAwait(false))
                    throw new InvalidOperationException($"Валюта {fromAccount.Currency} не поддерживается");

                decimal commission = CalculateCommission(request.Amount);
                decimal totalDebit = request.Amount + commission;

                ValidateBalance(fromAccount, totalDebit);

                ExecuteTransfer(fromAccount, toAccount, request.Amount, commission, request.Description);

                await _repository.UpdateAsync(fromAccount, cancellationToken).ConfigureAwait(false);
                await _repository.UpdateAsync(toAccount, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }

            return Unit.Value;
        }

        private void ValidateAccounts(Account fromAccount, Account toAccount)
        {
            if (fromAccount.ClosingDate.HasValue)
                throw new InvalidOperationException("Счет отправителя закрыт");

            if (toAccount.ClosingDate.HasValue)
                throw new InvalidOperationException("Счет получателя закрыт");

            if (!string.Equals(fromAccount.Currency, toAccount.Currency, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Валюты счетов не совпадают: {fromAccount.Currency} vs {toAccount.Currency}");
            }
        }

        private void ValidateBalance(Account account, decimal requiredAmount)
        {
            if (account.Balance < requiredAmount && account.Type != EAccountType.Credit)
            {
                throw new InvalidOperationException(
                    $"Недостаточно средств. Требуется: {requiredAmount}, доступно: {account.Balance}");
            }
        }

        private void ExecuteTransfer(
            Account fromAccount,
            Account toAccount,
            decimal amount,
            decimal commission,
            string description)
        {
            fromAccount.Balance -= amount + commission;
            toAccount.Balance += amount;

            fromAccount.Transactions.Add(CreateTransaction(
                fromAccount,
                toAccount,
                amount,
                commission,
                description,
                ETransactionType.Debit));

            toAccount.Transactions.Add(CreateTransaction(
                toAccount,
                fromAccount,
                amount,
                0,
                description,
                ETransactionType.Credit));
        }

        private decimal CalculateCommission(decimal amount)
        {
            decimal commission = amount * _commissionSettings.Rate;
            return Math.Clamp(commission, _commissionSettings.Min, _commissionSettings.Max);
        }

        private static Transaction CreateTransaction(
            Account account,
            Account counterparty,
            decimal amount,
            decimal commission,
            string description,
            ETransactionType type)
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
                    ? $"{description} + комиссия {commission}"
                    : description,
                DateTime = DateTime.UtcNow
            };
        }
    }
}