using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Microsoft.Extensions.Logging;
using Regulator.Client.Events.Client.Management;
using Regulator.Client.Models;
using Regulator.Client.Services.Data.Interfaces;
using Regulator.Client.Services.Providers.Interfaces;
using Regulator.Client.Services.Utilities.Interfaces;

namespace Regulator.Client.Windows;

public class NewSyncRequestWindow : Window, IDisposable
{
    private SyncRequest? _currentSyncRequest;
    private Player? _currentSyncRequestPlayer;
    
    private readonly IMediator _mediator;
    private readonly ILogger<MainWindow> _logger;
    private readonly ISyncRequestService _syncRequestService;
    private readonly IPlayerProvider _playerProvider;
    
    public NewSyncRequestWindow(
        IMediator mediator, 
        ILogger<MainWindow> logger, 
        ISyncRequestService syncRequestService, 
        IPlayerProvider playerProvider) : base("New Sync Request", ImGuiWindowFlags.AlwaysAutoResize)
    {
        _mediator = mediator;
        _logger = logger;
        _syncRequestService = syncRequestService;
        _playerProvider = playerProvider;
    }
    
    public override void Draw()
    {
        if (_currentSyncRequest is null)
        {
            _currentSyncRequest = _syncRequestService.GetNextSyncRequest();
            
            if (_currentSyncRequest is null)
            {
                IsOpen = false;
                return;
            }
        }
        
        if (_currentSyncRequestPlayer is null)
        {
            _currentSyncRequestPlayer = _playerProvider.GetPlayerByHash(_currentSyncRequest.RequestingSyncCode, _currentSyncRequest.CharacterId);
        }

        // Center the text
        var windowWidth = ImGui.GetWindowSize().X;
        var titleText = ImGui.CalcTextSize($"{_currentSyncRequestPlayer?.Name} is requesting to sync with you.").X;
        ImGui.SetCursorPosX((windowWidth - titleText) * 0.5f);
        ImGui.Text($"{_currentSyncRequestPlayer?.Name} is requesting to sync with you.");
        
        ImGui.Spacing();
        var descriptionText = ImGui.CalcTextSize("Do you want to accept or decline the request?").X;
        ImGui.SetCursorPosX((windowWidth - descriptionText) * 0.5f);
        ImGui.Text("Do you want to accept or decline the request?");
        
        ImGui.Spacing();
        ImGui.Separator();

        // Center the buttons
        var buttonWidth = ImGui.CalcTextSize("Accept").X + ImGui.GetStyle().FramePadding.X * 2;
        var buttonWidth2 = ImGui.CalcTextSize("Decline").X + ImGui.GetStyle().FramePadding.X * 2;
        var totalWidth = buttonWidth + ImGui.GetStyle().ItemSpacing.X + buttonWidth2;
        ImGui.SetCursorPosX((windowWidth - totalWidth) * 0.5f);
        
        // Buttons for Accept and Decline
        if (ImGui.Button("Accept"))
        {
            var syncRequestResponse = new SyncRequestResponse(
                _currentSyncRequestPlayer?.Name ?? string.Empty,
                _currentSyncRequest.RequestingSyncCode,
                _currentSyncRequest.RequestId,
                true);
            
            _mediator.PublishAsync(syncRequestResponse).GetAwaiter().GetResult();
            
            _currentSyncRequest = null;
            _currentSyncRequestPlayer = null;
        }
        
        ImGui.SameLine();
        
        if (ImGui.Button("Decline")) 
        {
            var syncRequestResponse = new SyncRequestResponse(
                _currentSyncRequestPlayer?.Name ?? string.Empty,
                _currentSyncRequest!.RequestingSyncCode,
                _currentSyncRequest.RequestId,
                true);
            
            _mediator.PublishAsync(syncRequestResponse).GetAwaiter().GetResult();
            
            _currentSyncRequest = null;
            _currentSyncRequestPlayer = null;
        }
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }
}