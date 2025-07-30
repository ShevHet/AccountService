
namespace AccountService.Services;

public interface IClientVerificationService
{
    bool ClientExists(Guid clientId);
}
