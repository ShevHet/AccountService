using AccountService.Services;
using MediatR;

namespace AccountService.Handlers
{
    public sealed record DeleteAccountCommand(Guid AccountId) : IRequest<Unit>; // ��������

    public class DeleteAccountHandler : IRequestHandler<DeleteAccountCommand, Unit> // �������� Unit
    {
        private readonly IAccountRepository _repository;

        public DeleteAccountHandler(IAccountRepository repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(DeleteAccountCommand request, CancellationToken ct)
        {
            var account = await _repository.GetByIdAsync(request.AccountId);
            if (account == null || account.ClosingDate.HasValue)
            {
                return Unit.Value;
            }

            if (account.Balance != 0)
            {
                throw new InvalidOperationException(
                    $"������ ������� ���� � �������� {account.Balance} {account.Currency}");
            }

            account.ClosingDate = DateTime.UtcNow;
            await _repository.UpdateAsync(account);
            return Unit.Value; // �����!
        }
    }
}