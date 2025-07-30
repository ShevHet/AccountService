using AccountService.Models;
using AccountService.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;

namespace AccountService.Handlers
{
    public sealed record PatchAccountCommand(
    Guid AccountId,
    JsonPatchDocument<Account> PatchDoc
) : IRequest<Unit>; 

    public class PatchAccountValidator : AbstractValidator<PatchAccountCommand>
    {
        public PatchAccountValidator()
        {
            RuleFor(x => x.AccountId).NotEmpty();
            RuleFor(x => x.PatchDoc).NotNull();
        }
    }

    public class PatchAccountHandler : IRequestHandler<PatchAccountCommand, Unit>
    {
        private readonly IAccountRepository _repository;

        public PatchAccountHandler(IAccountRepository repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(PatchAccountCommand request, CancellationToken ct)
        {
            var account = await _repository.GetByIdAsync(request.AccountId);
            if (account == null)
            {
                throw new ArgumentException("Счет не найден");
            }

            request.PatchDoc.ApplyTo(account);

            if (account.Type == EAccountType.Deposit && account.InterestRate <= 0)
                throw new InvalidOperationException("Для депозита ставка должна быть положительной");

            if (account.Type == EAccountType.Credit && account.InterestRate >= 0)
                throw new InvalidOperationException("Для кредита ставка должна быть отрицательной");

            await _repository.UpdateAsync(account);
            return Unit.Value;
        }
    }
}
