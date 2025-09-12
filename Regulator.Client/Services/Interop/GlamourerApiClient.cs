using System;
using System.Threading.Tasks;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Glamourer.Api.Enums;
using Glamourer.Api.Helpers;
using Glamourer.Api.IpcSubscribers;
using Glamourer.Api.IpcSubscribers.Legacy;
using Microsoft.Extensions.Logging;
using Regulator.Client.Enums;
using Regulator.Client.Events.Client.Glamourer;
using Regulator.Client.Services.Interop.Interfaces;
using Regulator.Client.Services.Providers.Interfaces;
using Regulator.Client.Services.Utilities.Interfaces;

namespace Regulator.Client.Services.Interop;

public class GlamourerApiClient : IGlamourerApiClient
{
    private readonly IClientState _clientState;
    private readonly IThreadService _threadService;
    private readonly IDebounceService _debounceService;
    private readonly IDependencyMonitoringService _dependencyMonitoringService;
    private readonly IMediator _mediator;
    private readonly ILogger<GlamourerApiClient> _logger;
    private readonly IPlayerProvider _playerProvider;
    
    private readonly GetStateBase64 _getAllCustomizations;
    private readonly ApplyState _applyState;
    
    private readonly EventSubscriber<IntPtr, StateChangeType> _stateChangedSubscriber;

    public bool ApiAvailable { get; private set; }
    
    public GlamourerApiClient(
        IClientState clientState, 
        IDalamudPluginInterface pluginInterface, 
        IThreadService threadService, 
        IDebounceService debounceService,
        IDependencyMonitoringService dependencyMonitoringService,
        IMediator mediator,
        ILogger<GlamourerApiClient> logger, 
        IPlayerProvider playerProvider)
    {
        _clientState = clientState;
        _threadService = threadService;
        _debounceService = debounceService;
        _dependencyMonitoringService = dependencyMonitoringService;
        _mediator = mediator;
        _logger = logger;
        _playerProvider = playerProvider;
        _getAllCustomizations = new GetStateBase64(pluginInterface);
        _applyState = new ApplyState(pluginInterface);

        _stateChangedSubscriber = StateChangedWithType.Subscriber(pluginInterface);
        _stateChangedSubscriber.Event += OnStateChanged;
        
        _dependencyMonitoringService.OnGlamourerStateChanged += OnGlamourerStateChanged;
    }

    public Task<GlamourerApiEc> ResetCustomizationsAsync(string syncCode)
    {
        throw new System.NotImplementedException();
    }

    public async Task<GlamourerApiEc> ApplyCustomizationsAsync(string syncCode, string customizations)
    {
        var player = _playerProvider.GetCachedPlayerBySyncCode(syncCode);
        
        if (player is null)
        {
            _logger.LogWarning("No player found for sync code {SyncCode} when applying customizations.", syncCode);
            return GlamourerApiEc.ActorNotFound;
        }
        
        return await _threadService.RunOnFrameworkThreadAsync(() => _applyState.Invoke(customizations, player!.ObjectIndex));
    }

    public async Task<string> RequestCustomizationsAsync()
    {
        return await _threadService.RunOnFrameworkThreadAsync(() =>
        {
            if (_clientState.LocalPlayer is null)
            {
                _logger.LogWarning("No local player found when requesting customizations.");
                return string.Empty;
            }
        
            var playerIndex = _clientState.LocalPlayer.ObjectIndex;

            return _getAllCustomizations.Invoke(playerIndex).Item2 ?? string.Empty;
        });
    }

    public void Dispose()
    {
        _stateChangedSubscriber.Event -= OnStateChanged;
        _stateChangedSubscriber.Dispose();
        
        _dependencyMonitoringService.OnGlamourerStateChanged -= OnGlamourerStateChanged;
        
        GC.SuppressFinalize(this);
    }
    
    private void OnGlamourerStateChanged(PluginState state)
    {
        ApiAvailable = state == PluginState.Active;
    }

    private void OnStateChanged(IntPtr characterPtr, StateChangeType type)
    {
        var localPlayerPtr = _clientState.LocalPlayer?.Address ?? IntPtr.Zero;
        if (characterPtr != localPlayerPtr)
        {
            _logger.LogDebug("State change event received for non-local player, ignoring.");
            return;
        }
        
        _debounceService.DebounceAsync("GlamourerApiClient_OnStateChanged", async () =>
        {
            var customizations = await RequestCustomizationsAsync();
            _logger.LogInformation("Local player customizations changed. Type: {Type} | New customizations: {Customizations}", type, customizations);

            var customizationsUpdated = new CustomizationsUpdated(customizations);
            await _mediator.PublishAsync(customizationsUpdated);
        });
    }
}