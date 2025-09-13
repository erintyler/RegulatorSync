using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Regulator.Client.Configuration;
using Regulator.Client.Services.Authentication.Interfaces;

namespace Regulator.Client.Services.Authentication;

public class AccessTokenProvider(PluginConfig pluginConfig, ILogger<AccessTokenProvider> logger) : IAccessTokenProvider
{
    public event Func<Task>? AccessTokenChangedAsync;

    public string? GetAccessToken()
    {
        return pluginConfig.AccessToken;
    }

    public void SetAccessToken(string token)
    {
        logger.LogInformation("Access token set to: {Token}", token);
        pluginConfig.AccessToken = token;
        pluginConfig.Save();
        _ = AccessTokenChangedAsync?.Invoke();
    }

    public void ClearAccessToken()
    {
        pluginConfig.AccessToken = null;
        pluginConfig.Save();
    }
}