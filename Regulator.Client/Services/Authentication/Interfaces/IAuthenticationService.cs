using System.Threading.Tasks;

namespace Regulator.Client.Services.Authentication.Interfaces;

public interface IAuthenticationService
{
    bool IsAuthenticated { get; }
    bool IsAuthenticationInProgress { get; }
    void OpenOAuthLoginPage();
    void CompleteAuthentication();
}