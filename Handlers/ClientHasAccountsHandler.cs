using AccountService.Services;
using MediatR;

namespace AccountService.Handlers
{
    /// <summary>
    /// Запрос проверки наличия счетов у клиента
    /// </summary>
    public sealed record ClientHasAccountsQuery( 
        /// <summary>
        /// Идентификатор клиента
        /// </summary>
        Guid ClientId
    ) : IRequest<bool>;

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
