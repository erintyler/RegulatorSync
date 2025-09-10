using System;
using System.Threading.Tasks;

namespace Regulator.Client.Services.Authentication.Interfaces;

public interface IAccessTokenProvider
{
    event Func<Task>? AccessTokenChangedAsync;
    string? GetAccessToken();
    void SetAccessToken(string token);
    void ClearAccessToken();
}