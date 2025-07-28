using AccountService.Models;
using AccountService.Services;
using FluentValidation;
using MediatR;

namespace AccountService.Handlers
{
    public sealed record CreateAccountCommand(
        Guid OwnerId,
        AccountType Type,
        string Currency,
        decimal? InterestRate,
        DateTime? OpeningDate = null,
        DateTime? ClosingDate = null
    ) : IRequest<Guid>;

    public class CreateAccountValidator : AbstractValidator<CreateAccountCommand>
    {
        public CreateAccountValidator(IClientService clientService, ICurrencyService currencyService)
        {
            RuleFor(x => x.OwnerId)
                .MustAsync(async (id, _) => await clientService.VerifyClientAsync(id))
                .WithMessage("Клиент не существует");

            RuleFor(x => x.Currency)
                .MustAsync(async (c, _) => await currencyService.IsSupportedAsync(c))
                .WithMessage("Неподдерживаемая валюта");

            RuleFor(x => x.InterestRate)
                .NotNull().WithMessage("Процентная ставка обязательна")
                .When(x => x.Type is AccountType.Deposit or AccountType.Credit);

            RuleFor(x => x.InterestRate)
                .GreaterThan(0).WithMessage("Ставка должна быть > 0")
                .When(x => x.Type == AccountType.Deposit);

            RuleFor(x => x.InterestRate)
                .LessThan(0).WithMessage("Ставка должна быть < 0")
                .When(x => x.Type == AccountType.Credit);

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