using AccountService.Models;
using AccountService.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;
using AccountService.Configuration;

namespace AccountService.Handlers
{
    /// <summary>
    /// Команда регистрации транзакции
    /// </summary>
    public sealed record RegisterTransactionCommand( 
        /// <summary>
        /// Идентификатор счета
        /// </summary>
        Guid AccountId,
        /// <summary>
        /// Сумма транзакции
        /// </summary>
        decimal Amount,

        /// <summary>
        /// Валюта транзакции (ISO 4217)
        /// </summary>
        string Currency,

        /// <summary>
        /// Тип транзакции (Credit/Debit)
        /// </summary>
        ETransactionType Type,

        /// <summary>
        /// Описание транзакции
        /// </summary>
        string Description
    ) : IRequest<Unit>;

    public class RegisterTransactionValidator : AbstractValidator<RegisterTransactionCommand>
    {
        public RegisterTransactionValidator(ICurrencyService currencyService,
            IOptions<AccountServiceOptions> options)
        {
            var maxLength = options.Value.Validation.MaxDescriptionLength;

            RuleFor(x => x.AccountId).NotEmpty();
            RuleFor(x => x.Amount).GreaterThan(0);
            RuleFor(x => x.Currency)
                .MustAsync(async (c, _) => await currencyService.IsSupportedAsync(c))
                .WithMessage("Валюта операции не поддерживается");
            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(maxLength);
        }
    }

    public class RegisterTransactionHandler : IRequestHandler<RegisterTransactionCommand, Unit>
    {
        private readonly IAccountRepository _repository;
        private readonly IValidator<RegisterTransactionCommand> _validator;

        public RegisterTransactionHandler(
            IAccountRepository repository,
            IValidator<RegisterTransactionCommand> validator)
        {
            _repository = repository;
            _validator = validator;
        }

        public async Task<Unit> Handle(RegisterTransactionCommand request, CancellationToken ct)
        {
            var validationResult = await _validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var account = await _repository.GetByIdAsync(request.AccountId);
            if (account == null)
            {
                throw new ArgumentException("Счет не найден");

            }

            // Проверка что счет активен
            if (account.ClosingDate.HasValue)
            {
                throw new InvalidOperationException("Операции по закрытому счету запрещены");
            }

            // Проверка валюты
            if (!account.Currency.Equals(request.Currency, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Валюта счета ({account.Currency}) не совпадает с валютой операции ({request.Currency})");
            }

            // Для списания - проверка баланса (кроме кредитных счетов)
            if (request.Type == ETransactionType.Debit && account.Balance < request.Amount && account.Type != EAccountType.Credit)
            {
                throw new InvalidOperationException(
                    $"Недостаточно средств. Требуется: {request.Amount}, доступно: {account.Balance}");
            }

            // Создаем транзакцию
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = request.AccountId,
                Amount = request.Amount,
                Currency = request.Currency,
                Type = request.Type,
                Description = request.Description,
                DateTime = DateTime.UtcNow
            };

            // Обновляем баланс
            if (request.Type == ETransactionType.Credit)
            {
                account.Balance += request.Amount;
            }
            else
            {
                account.Balance -= request.Amount;
            }

            account.Transactions.Add(transaction);
            await _repository.UpdateAsync(account);
            return Unit.Value;
        }
    }
}
