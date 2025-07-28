namespace AccountService.Services
{
    public interface ICurrencyService
    {
        Task<bool> IsSupportedAsync(string currency, CancellationToken ct = default);
    }
}