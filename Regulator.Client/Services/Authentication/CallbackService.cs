using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Interface.ImGuiNotification;
using Microsoft.Extensions.Logging;
using Regulator.Client.Events.Client.Notifications;
using Regulator.Client.Services.Authentication.Interfaces;
using Regulator.Client.Services.Utilities.Interfaces;

namespace Regulator.Client.Services.Authentication;

public class CallbackService(IAccessTokenProvider accessTokenProvider, IMediator mediator, ILogger<CallbackService> logger) : ICallbackService
{
    public const string CallbackUrl = "http://localhost:5000/callback/";
    private CancellationTokenSource? _cts;
    private HttpListener? _listener;
    
    public void StartCallbackListener()
    {
        if (_listener is { IsListening: true })
        {
            // Kill existing listener if already running
            _listener.Stop();
            _cts?.Cancel();
        }
        
        _listener = new HttpListener();
        _listener.Prefixes.Add(CallbackUrl);
        _listener.Start();
        _cts = new CancellationTokenSource();
        
        logger.LogInformation("Callback listener started on {CallbackUrl}", CallbackUrl);

        Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                var context = await _listener.GetContextAsync();
                
                // Handle the callback request (Get token from query parameters)
                var query = context.Request.QueryString;
                var accessToken = query["token"] ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    accessTokenProvider.SetAccessToken(accessToken);

                    context.Response.StatusCode = 200;
                    await context.Response.OutputStream.WriteAsync("""
                                                                   <html>
                                                                     <body>
                                                                       Authentication completed. You can now close your browser.
                                                                       <script>
                                                                         window.close();
                                                                       </script>
                                                                     </body>
                                                                   </html>
                                                                   """u8.ToArray(), _cts.Token);
                    context.Response.Close();
                    
                    logger.LogInformation("Access token received and set.");
                    var notification = new NotificationMessage("Authentication successful!", Type: NotificationType.Success);
                    await mediator.PublishAsync(notification, _cts.Token);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    await context.Response.OutputStream.WriteAsync("Missing token"u8.ToArray(), _cts.Token);
                    context.Response.Close();

                    logger.LogWarning("Callback received without a token.");
                    var notification = new NotificationMessage("Authentication failed", Type: NotificationType.Error);
                    await mediator.PublishAsync(notification, _cts.Token);
                }
                
                // Optionally, stop the listener after receiving the token
                _listener.Stop();
                await _cts.CancelAsync();
            }
        }, _cts.Token);
    }

    public void StopCallbackListener()
    {
        _cts?.Cancel();
        _listener?.Stop();
        _listener?.Close();
    }

    public void Dispose()
    {
        _cts?.Dispose();
        _listener?.Close();
        
        GC.SuppressFinalize(this);
    }
}