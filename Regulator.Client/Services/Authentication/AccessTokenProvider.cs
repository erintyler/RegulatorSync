using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Regulator.Client.Services.Authentication.Interfaces;

namespace Regulator.Client.Services.Authentication;

public class AccessTokenProvider(ILogger<AccessTokenProvider> logger) : IAccessTokenProvider
{
    // In-memory storage for the access token
    private string? _accessToken;

    public event Func<Task>? AccessTokenChangedAsync;

    public string? GetAccessToken()
    {
        return _accessToken;
    }

    public void SetAccessToken(string token)
    {
        logger.LogInformation("Access token set to: {Token}", token);
        _accessToken = token;
        _ = AccessTokenChangedAsync?.Invoke();
    }

    public void ClearAccessToken()
    {
        _accessToken = null;
    }
}