using System;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Plugin.Services;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Regulator.Client.Events.Client.Notifications;
using Regulator.Client.Services.Authentication.Interfaces;
using Regulator.Client.Services.Utilities.Interfaces;
using Regulator.Services.Sync.Shared.Dtos.Client.Glamourer;
using Regulator.Services.Sync.Shared.Dtos.Client.Penumbra;
using Regulator.Services.Sync.Shared.Dtos.Server;
using Regulator.Services.Sync.Shared.Dtos.Server.Glamourer;
using Regulator.Services.Sync.Shared.Dtos.Server.Penumbra;
using Regulator.Services.Sync.Shared.Enums;
using Regulator.Services.Sync.Shared.Hubs;

namespace Regulator.Client.Services.Hubs;

public class RegulatorServerClient(
    HubConnection connection, 
    IAccessTokenProvider accessTokenProvider, 
    IClientState clientState,
    IMediator mediator, 
    ILogger<RegulatorServerClient> logger) : IRegulatorServerMethods, IHostedService, IDisposable
{
    public ConnectionState ConnectionState { get; private set; } = ConnectionState.Disconnected;
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try 
        {
            clientState.Login += ClientStateOnLogin;
            clientState.Logout += ClientStateOnLogout;
            
            if (!clientState.IsLoggedIn)
            {
                return;
            }
            
            BindEventHandlers();

            var accessToken = accessTokenProvider.GetAccessToken();
            
            if (string.IsNullOrEmpty(accessToken))
            {
                logger.LogWarning("No access token available, cannot connect to Regulator");
                return;
            }
            
            ConnectionState = ConnectionState.Connecting;
            await connection.StartAsync(cancellationToken);
            ConnectionState = ConnectionState.Connected;
            logger.LogInformation("Connected to Regulator with connection ID: {ConnectionId}", connection.ConnectionId);
            
            await SendConnectedNotification(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to connect to Regulator server hub.");
            ConnectionState = ConnectionState.Disconnected;

            await SendFailedToConnectNotification(ex, cancellationToken);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try 
        {
            await connection.StopAsync(cancellationToken);
            ConnectionState = ConnectionState.Disconnected;
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
                ConnectionState = ConnectionState.Disconnected;
            }
        
            logger.LogInformation("Connecting to Regulator with new access token...");
            ConnectionState = ConnectionState.Connecting;
            await connection.StartAsync();
            ConnectionState = ConnectionState.Connected;
            logger.LogInformation("Connected to Regulator with new access token.");
            
            await SendConnectedNotification(CancellationToken.None);
        } 
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to reconnect to Regulator server hub with new access token.");
            ConnectionState = ConnectionState.Disconnected;
            await SendFailedToConnectNotification(ex, CancellationToken.None);
        }
    }

    public async Task NotifyCustomizationsResetAsync(CustomizationsResetDto dto)
    {
        if (connection.State is not HubConnectionState.Connected)
        {
            return;
        }
        
        await connection.SendAsync(nameof(NotifyCustomizationsResetAsync), dto);
    }

    public async Task NotifyCustomizationsUpdatedAsync(NotifyCustomizationsUpdatedDto dto)
    {
        if (connection.State is not HubConnectionState.Connected)
        {
            return;
        }
        
        await connection.SendAsync(nameof(NotifyCustomizationsUpdatedAsync), dto);
    }

    public async Task RequestCustomizationsAsync(RequestCustomizationsDto dto)
    {
        if (connection.State is not HubConnectionState.Connected)
        {
            return;
        }
        
        await connection.SendAsync(nameof(RequestCustomizationsAsync), dto);
    }

    public async Task AddSyncCodeAsync(SyncRequestDto dto)
    {
        if (connection.State is not HubConnectionState.Connected)
        {
            return;
        }
        
        await connection.SendAsync(nameof(AddSyncCodeAsync), dto);
    }

    public async Task RespondToSyncRequestAsync(SyncRequestResponseDto dto)
    {
        if (connection.State is not HubConnectionState.Connected)
        {
            return;
        }
        
        await connection.SendAsync(nameof(RespondToSyncRequestAsync), dto);
    }

    public async Task NotifyResourceAppliedAsync(NotifyResourceAppliedDto dto)
    {
        if (connection.State is not HubConnectionState.Connected)
        {
            return;
        }
        
        await connection.SendAsync(nameof(NotifyResourceAppliedAsync), dto);
    }

    public async Task SendOnlineDataAsync(SendOnlineDataDto dto)
    {
        if (connection.State is not HubConnectionState.Connected)
        {
            return;
        }
        
        await connection.SendAsync(nameof(SendOnlineDataAsync), dto);
    }

    private void BindEventHandlers()
    {
        accessTokenProvider.AccessTokenChangedAsync += OnAccessTokenChangedAsync;
            
        connection.Reconnecting += ex =>
        {
            logger.LogWarning(ex, "Reconnecting to Regulator...");
            ConnectionState = ConnectionState.Reconnecting;
            return Task.CompletedTask;
        };
            
        connection.Reconnected += connectionId =>
        {
            logger.LogInformation("Reconnected to Regulator with connection ID: {ConnectionId}", connectionId);
            ConnectionState = ConnectionState.Connected;
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
            
            ConnectionState = ConnectionState.Disconnected;

            return Task.CompletedTask;
        };
    }
    
    private async Task SendConnectedNotification(CancellationToken cancellationToken)
    {
        var notification = new NotificationMessage("Connected to Regulator server.", Type: NotificationType.Success);
        await mediator.PublishAsync(notification, cancellationToken);
    }

    private async Task SendFailedToConnectNotification(Exception ex, CancellationToken cancellationToken)
    {
        var notification = new NotificationMessage("Failed to connect to Regulator server", ex.Message, Type: NotificationType.Error);
        await mediator.PublishAsync(notification, cancellationToken);
    }
    
    private void ClientStateOnLogin()
    {
        OnAccessTokenChangedAsync().GetAwaiter().GetResult();
    }
    
    private void ClientStateOnLogout(int type, int code)
    {
        connection.StopAsync().GetAwaiter().GetResult();
        ConnectionState = ConnectionState.Disconnected;
    }

    public void Dispose()
    {
        connection.DisposeAsync().AsTask().GetAwaiter().GetResult();
        accessTokenProvider.AccessTokenChangedAsync -= OnAccessTokenChangedAsync;
        
        clientState.Login -= ClientStateOnLogin;
        clientState.Logout -= ClientStateOnLogout;
        
        GC.SuppressFinalize(this);
    }
}