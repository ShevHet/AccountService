namespace AccountService.Services
{
    public class CurrencyServiceStub : ICurrencyService
    {
        private static readonly HashSet<string> Supported = ["RUB", "USD", "EUR"];

        public Task<bool> IsSupportedAsync(string currency, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult(Supported.Contains(currency.ToUpper()));
        }
    }
}