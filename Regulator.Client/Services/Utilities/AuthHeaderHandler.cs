using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Regulator.Client.Services.Authentication.Interfaces;

namespace Regulator.Client.Services.Utilities;

public class AuthHeaderHandler(IAccessTokenProvider accessTokenProvider, ILogger<AuthHeaderHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = accessTokenProvider.GetAccessToken();
        
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            logger.LogWarning("No access token available for request to {RequestUri}", request.RequestUri);
        }
        
        return await base.SendAsync(request, cancellationToken);
    }
}