using AccountService.Models;
using AccountService.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;
using AccountService.Configuration;

namespace AccountService.Handlers
{
    public sealed record CreateAccountCommand(
        Guid OwnerId,
        EAccountType Type,
        string Currency,
        decimal? InterestRate,
        DateTime? OpeningDate = null,
        DateTime? ClosingDate = null
    ) : IRequest<Guid>;

    public class CreateAccountValidator : AbstractValidator<CreateAccountCommand>
    {
        public CreateAccountValidator(
            IClientService clientService,
            ICurrencyService currencyService,
            IOptions<AccountServiceOptions> options)
        {
            const decimal minDepositRate = 0.01m;
            const decimal maxCreditRate = -0.01m;

            RuleFor(x => x.OwnerId)
                .MustAsync(async (id, _) => await clientService.VerifyClientAsync(id))
                .WithMessage("Клиент не существует");

            RuleFor(x => x.Currency)
                .MustAsync(async (c, _) => await currencyService.IsSupportedAsync(c))
                .WithMessage("Неподдерживаемая валюта");

            RuleFor(x => x.InterestRate)
                .NotNull().WithMessage("Процентная ставка обязательна")
                .When(x => x.Type is EAccountType.Deposit or EAccountType.Credit);

            RuleFor(x => x.InterestRate)
                .GreaterThan(minDepositRate)
                .WithMessage($"Ставка должна быть > {minDepositRate}")
                .When(x => x.Type == EAccountType.Deposit);

            RuleFor(x => x.InterestRate)
                .LessThan(maxCreditRate)
                .WithMessage($"Ставка должна быть < {maxCreditRate}")
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

    public class CreateAccountHandler : IRequestHandler<CreateAccountCommand, Guid>
    {
        private readonly IAccountRepository _repository;

        public CreateAccountHandler(IAccountRepository repository)
        {
            _repository = repository;
        }

        public async Task<Guid> Handle(CreateAccountCommand request, CancellationToken ct)
        {
            var account = new Account
            {
                OwnerId = request.OwnerId,
                Type = request.Type,
                Currency = request.Currency,
                InterestRate = request.InterestRate,
                OpeningDate = request.OpeningDate ?? DateTime.UtcNow,
                ClosingDate = request.ClosingDate,
                Balance = 0
            };

            var created = await _repository.CreateAsync(account, ct);
            return created.Id;
        }
    }
}
