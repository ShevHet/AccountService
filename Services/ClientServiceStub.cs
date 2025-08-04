namespace AccountService.Services
{
    public class ClientServiceStub : IClientService
    {
        private static readonly HashSet<Guid> _clients = new()
        {
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Guid.Parse("22222222-2222-2222-2222-222222222222")
        };

        public Task<bool> VerifyClientAsync(Guid clientId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult(_clients.Contains(clientId));
        }
    }
}