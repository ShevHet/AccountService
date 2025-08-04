using AccountService.Models;
using AccountService.Services;
using FluentValidation;
using MediatR;

namespace AccountService.Handlers
{
    /// <summary>
    /// Команда обновления счета
    /// </summary>
    /// <param name="Id">Идентификатор обновляемого счета</param>
    /// <param name="InterestRate">Новая процентная ставка</param>
    /// <param name="ClosingDate">Новая дата закрытия счета</param>
    public sealed record UpdateAccountCommand(
    /// <summary>
    /// Идентификатор обновляемого счета
    /// </summary>
    Guid Id,

    /// <summary>
    /// Новая процентная ставка
    /// </summary>
    decimal? InterestRate,

    /// <summary>
    /// Новая дата закрытия счета
    /// </summary>
    DateTime? ClosingDate
    ) : IRequest<Unit>;

    public class UpdateAccountValidator : AbstractValidator<UpdateAccountCommand>
    {
        public UpdateAccountValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.InterestRate)
                .GreaterThan(0).WithMessage("Ставка должна быть положительной")
                .When(x => x.InterestRate.HasValue);
        }
    }

    public class UpdateAccountHandler : IRequestHandler<UpdateAccountCommand, Unit>
    {
        private readonly IAccountRepository _repository;
        private readonly IValidator<UpdateAccountCommand> _validator;

        public UpdateAccountHandler(
            IAccountRepository repository,
            IValidator<UpdateAccountCommand> validator)
        {
            _repository = repository;
            _validator = validator;
        }

        public async Task<Unit> Handle(UpdateAccountCommand request, CancellationToken ct)
        {
            var validationResult = await _validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var account = await _repository.GetByIdAsync(request.Id);
            if (account == null)
            {
                throw new ArgumentException("Такого счета не существует");
            }

            if (request.InterestRate.HasValue)
            {
                account.InterestRate = request.InterestRate;
            }

            if (request.ClosingDate.HasValue)
            {
                if (request.ClosingDate.Value < DateTime.UtcNow.AddMinutes(-5))
                {
                    throw new InvalidOperationException("Дата закрытия не может быть в прошлом");
                }
                account.ClosingDate = request.ClosingDate;
            }

            await _repository.UpdateAsync(account);
            return Unit.Value;
        }
    }
}
