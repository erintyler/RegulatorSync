using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using Microsoft.Extensions.Logging;
using Regulator.Client.Events.Client.Management;
using Regulator.Client.Services.Authentication.Interfaces;
using Regulator.Client.Services.Data.Interfaces;
using Regulator.Client.Services.Interop.Interfaces;
using Regulator.Client.Services.Utilities.Interfaces;
using Regulator.Services.Sync.Shared.Enums;
using Regulator.Services.Sync.Shared.Hubs;

namespace Regulator.Client.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly IMediator _mediator;
    private readonly ILogger<MainWindow> _logger;
    private readonly IAuthenticationService _authenticationService;
    private readonly IRegulatorServerMethods _client;
    private readonly IClientDataService _clientDataService;
    private readonly IPenumbraApiClient _penumbraApiClient;
    
    private string _syncCode = string.Empty;
    private bool _showAddSyncCodePopup;
    
    public MainWindow(IMediator mediator, ILogger<MainWindow> logger, IRegulatorServerMethods client, IClientDataService clientDataService, IAuthenticationService authenticationService, IPenumbraApiClient penumbraApiClient) : base("Regulator", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        _mediator = mediator;
        _logger = logger;
        _client = client;
        _clientDataService = clientDataService;
        _authenticationService = authenticationService;
        _penumbraApiClient = penumbraApiClient;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public override void Draw()
    {
        var clientData = _clientDataService.GetClientData();
        
        switch (_client.ConnectionState)
        {
            case ConnectionState.Disconnected:
                ImGui.TextColored(ImGuiColors.DalamudRed, "Disconnected");
                break;
            case ConnectionState.Connecting:
                ImGui.TextColored(ImGuiColors.DalamudYellow, "Connecting...");
                break;
            case ConnectionState.Connected:
                ImGui.TextColored(ImGuiColors.HealerGreen, "Connected");
                break;
            case ConnectionState.Reconnecting:
                ImGui.TextColored(ImGuiColors.DalamudYellow, "Reconnecting...");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        if (ImGui.Button("Penumbra Test"))
        {
            _logger.LogInformation("Fetching Penumbra resource paths for local player.");
            _penumbraApiClient.GetCustomResourcePathsAsync(false).GetAwaiter().GetResult();
        }
        
        ImGui.SameLine();
        
        if (ImGui.Button("Penumbra Download Test"))
        {
            _logger.LogInformation("Fetching Penumbra resource paths for local player (download test).");
            _penumbraApiClient.GetCustomResourcePathsAsync(true).GetAwaiter().GetResult();
            _penumbraApiClient.AssignTemporaryCollection("test").GetAwaiter().GetResult();
        }
        
        // Add login button if disconnected
        if (_client.ConnectionState == ConnectionState.Disconnected) 
        {
            if (ImGui.Button("Connect"))
            {
                _authenticationService.OpenOAuthLoginPage();
            }
            
            return; // Skip the rest of the UI if not connected
        }
        
        ImGui.Separator();
        
        ImGui.Text($"Sync Code: {clientData?.SyncCode ?? "N/A"}");
        
        if (ImGui.Button("Add Sync Code"))
        {
            _logger.LogInformation("Opening Add Sync Code popup.");
            
            // Create ImGui popup to enter sync code
            ImGui.OpenPopup("Add Sync Code");
            _showAddSyncCodePopup = true;
            _syncCode = string.Empty;
        }
        
        if (ImGui.BeginPopupModal("Add Sync Code", ref _showAddSyncCodePopup, ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text("Enter Sync Code:");
            ImGui.InputText("##synccode", ref _syncCode, 64);
                
            if (ImGui.Button("Submit") && !string.IsNullOrWhiteSpace(_syncCode))
            {
                // Handle sync code submission
                // e.g., call a method to process the sync code
                _logger.LogInformation("Submitted Sync Code: {SyncCode}", _syncCode);
                _showAddSyncCodePopup = false;
                _mediator.PublishAsync(new AddSyncCode(_syncCode.Trim()));
            }
                
            ImGui.SameLine();
                
            if (ImGui.Button("Cancel"))
            {
                _logger.LogInformation("Cancelled Add Sync Code popup.");
                _showAddSyncCodePopup = false;
            }
                
            ImGui.EndPopup();
        }
        
        ImGui.Separator();
        ImGui.Text("Added Sync Codes:");
        if (clientData?.AddedUsers is { Count: > 0 })
        {
            foreach (var user in clientData.AddedUsers)
            {
                if (user.IsOnline) 
                {
                    ImGui.TextColored(ImGuiColors.HealerGreen, $"- {user.SyncCode} (Online)");
                }
                else
                {
                    ImGui.TextColored(ImGuiColors.DalamudRed, $"- {user.SyncCode} (Offline)");
                }
            }
        }
        else
        {
            ImGui.Text("No added sync codes.");
        }
    }
    
    public void Dispose()
    {
        // TODO release managed resources here
    }
}