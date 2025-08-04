using AccountService.Models;
using AccountService.Services;
using AccountService.Configuration;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace AccountService.Handlers
{

    /// <summary>
    /// Команда создания счета
    /// </summary>
    public sealed record CreateAccountCommand(
        /// <summary>
        /// Идентификатор владельца счета
        /// </summary>
        Guid OwnerId,

        /// <summary>
        /// Тип счета
        /// </summary>
        EAccountType Type,

        /// <summary>
        /// Валюта счета (ISO 4217 код)
        /// </summary>
        string Currency,

        /// <summary>
        /// Процентная ставка (для депозитных и кредитных счетов)
        /// </summary>
        decimal? InterestRate,

        /// <summary>
        /// Дата открытия счета (если не указана, используется текущая дата)
        /// </summary>
        DateTime? OpeningDate = null,

        /// <summary>
        /// Дата закрытия счета (если не указана, счет считается открытым)
        /// </summary>
        DateTime? ClosingDate = null
    ) : IRequest<Guid>;

    public sealed class CreateAccountValidator : AbstractValidator<CreateAccountCommand>
    {
        private const decimal MinDepositRate = 0.01m;
        private const decimal MaxCreditRate = -0.01m;

        public CreateAccountValidator(
            IClientService clientService,
            ICurrencyService currencyService,
            IOptions<AccountServiceOptions> options)
        {
            var settings = options.Value.Commission;

            RuleFor(x => x.OwnerId)
                .MustAsync(async (id, ct) =>
                {
                    return await clientService.VerifyClientAsync(id);
                })
                .WithMessage("Клиент не существует");

            RuleFor(x => x.Currency)
                .MustAsync(async (c, ct) =>
                {
                    return await currencyService.IsSupportedAsync(c, ct);
                })
                .WithMessage("Неподдерживаемая валюта");

            RuleFor(x => x.InterestRate)
                .NotNull().WithMessage("Процентная ставка обязательна")
                .When(x => x.Type is EAccountType.Deposit or EAccountType.Credit);

            RuleFor(x => x.InterestRate)
                .GreaterThan(MinDepositRate).WithMessage($"Ставка должна быть > {MinDepositRate}")
                .When(x => x.Type == EAccountType.Deposit);

            RuleFor(x => x.InterestRate)
                .LessThan(MaxCreditRate).WithMessage($"Ставка должна быть < {MaxCreditRate}")
                .When(x => x.Type == EAccountType.Credit);

            RuleFor(x => x.OpeningDate)
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("Дата открытия не может быть в будущем")
                .When(x => x.OpeningDate.HasValue);

            RuleFor(x => x.ClosingDate)
                .GreaterThan(x => x.OpeningDate ?? DateTime.UtcNow)
                .WithMessage("Дата закрытия должна быть после даты открытия")
                .When(x => x.ClosingDate.HasValue);
        }
    }

    public sealed class CreateAccountHandler : IRequestHandler<CreateAccountCommand, Guid>
    {
        private readonly IAccountRepository _repository;

        public CreateAccountHandler(IAccountRepository repository)
        {
            _repository = repository;
        }

        public async Task<Guid> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
        {
            var account = new Account();
            account.OwnerId = request.OwnerId;
            account.Type = request.Type;
            account.Currency = request.Currency;
            account.InterestRate = request.InterestRate;
            account.OpeningDate = request.OpeningDate ?? DateTime.UtcNow;
            account.ClosingDate = request.ClosingDate;
            account.Balance = 0;

            var created = await _repository.CreateAsync(account, cancellationToken);
            return created.Id;
        }
    }
}