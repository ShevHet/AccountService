namespace AccountService.Services
{
    public interface IClientService
    {
        Task<bool> VerifyClientAsync(Guid clientId);
    }
}
