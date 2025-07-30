using AccountService.Services;
using MediatR;

namespace AccountService.Handlers
{
	public sealed record ClientHasAccountsQuery(Guid ClientId) : IRequest<bool>;

	public class ClientHasAccountsHandler : IRequestHandler<ClientHasAccountsQuery, bool>
	{
		private readonly IAccountRepository _repository;

		public ClientHasAccountsHandler(IAccountRepository repository)
		{
			_repository = repository;
		}

		public async Task<bool> Handle(ClientHasAccountsQuery request, CancellationToken ct)
		{
			return await _repository.ClientHasAccountsAsync(request.ClientId);
		}
	}
}
