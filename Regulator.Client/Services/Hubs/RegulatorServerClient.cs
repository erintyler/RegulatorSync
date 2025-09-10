using System;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Interface.ImGuiNotification;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Regulator.Client.Events.Client.Notifications;
using Regulator.Client.Services.Authentication.Interfaces;
using Regulator.Client.Services.Utilities.Interfaces;
using Regulator.Services.Sync.Shared.Dtos.Client.Glamourer;
using Regulator.Services.Sync.Shared.Dtos.Server;
using Regulator.Services.Sync.Shared.Dtos.Server.Glamourer;
using Regulator.Services.Sync.Shared.Hubs;

namespace Regulator.Client.Services.Hubs;

public class RegulatorServerClient(HubConnection connection, IAccessTokenProvider accessTokenProvider, IMediator mediator, ILogger<RegulatorServerClient> logger) : IRegulatorServerMethods, IHostedService, IDisposable
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try 
        {
            BindEventHandlers();

            var accessToken = accessTokenProvider.GetAccessToken();
            
            if (string.IsNullOrEmpty(accessToken))
            {
                logger.LogWarning("No access token available, cannot connect to Regulator");
                return;
            }
            
            await connection.StartAsync(cancellationToken);
            logger.LogInformation("Connected to Regulator with connection ID: {ConnectionId}", connection.ConnectionId);
            
            await SendConnectedNotification(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to connect to Regulator server hub.");

            await SendFailedToConnectNotification(cancellationToken);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try 
        {
            await connection.StopAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to disconnect from Regulator server hub.");
        }
    }

    public async Task OnAccessTokenChangedAsync()
    {
        var accessToken = accessTokenProvider.GetAccessToken();
        
        if (string.IsNullOrEmpty(accessToken))
        {
            logger.LogWarning("No access token available, cannot reconnect to Regulator server hub.");
            return;
        }

        try
        {
            if (connection.State == HubConnectionState.Connected)
            {
                await connection.StopAsync();
            }
        
            logger.LogInformation("Connecting to Regulator with new access token...");
            await connection.StartAsync();
            logger.LogInformation("Connected to Regulator with new access token.");
            
            await SendConnectedNotification(CancellationToken.None);
        } 
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to reconnect to Regulator server hub with new access token.");
            await SendFailedToConnectNotification(CancellationToken.None);
        }
    }
    
    public async Task NotifyCustomizationsResetAsync(CustomizationsResetDto dto)
    {
        await connection.SendAsync(nameof(NotifyCustomizationsResetAsync), dto);
    }

    public async Task NotifyCustomizationsUpdatedAsync(NotifyCustomizationsUpdatedDto dto)
    {
        await connection.SendAsync(nameof(NotifyCustomizationsUpdatedAsync), dto);
    }

    public async Task RequestCustomizationsAsync(RequestCustomizationsDto dto)
    {
        await connection.SendAsync(nameof(RequestCustomizationsAsync), dto);
    }

    public async Task AddSyncCodeAsync(AddSyncCodeDto dto)
    {
        await connection.SendAsync(nameof(AddSyncCodeAsync), dto);
    }

    private void BindEventHandlers()
    {
        accessTokenProvider.AccessTokenChangedAsync += OnAccessTokenChangedAsync;
            
        connection.Reconnecting += ex =>
        {
            logger.LogWarning(ex, "Reconnecting to Regulator...");
            return Task.CompletedTask;
        };
            
        connection.Reconnected += connectionId =>
        {
            logger.LogInformation("Reconnected to Regulator with connection ID: {ConnectionId}", connectionId);
            return Task.CompletedTask;
        };
            
        connection.Closed += ex =>
        {
            if (ex != null)
            {
                logger.LogError(ex, "Connection to Regulator closed unexpectedly.");
            }
            else
            {
                logger.LogInformation("Connection to Regulator closed.");
            }

            return Task.CompletedTask;
        };
    }
    
    private async Task SendConnectedNotification(CancellationToken cancellationToken)
    {
        var notification = new NotificationMessage("Connected to Regulator server.", Type: NotificationType.Success);
        await mediator.PublishAsync(notification, cancellationToken);
    }

    private async Task SendFailedToConnectNotification(CancellationToken cancellationToken)
    {
        var notification = new NotificationMessage("Failed to connect to Regulator server. Check logs for details.", Type: NotificationType.Error);
        await mediator.PublishAsync(notification, cancellationToken);
    }

    public void Dispose()
    {
        connection.DisposeAsync().AsTask().GetAwaiter().GetResult();
        accessTokenProvider.AccessTokenChangedAsync -= OnAccessTokenChangedAsync;
        
        GC.SuppressFinalize(this);
    }
}