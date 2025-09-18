using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Glamourer.Api.Helpers;
using Microsoft.Extensions.Logging;
using Penumbra.Api.Enums;
using Penumbra.Api.Helpers;
using Penumbra.Api.IpcSubscribers;
using Regulator.Client.Events.Client.Files;
using Regulator.Client.Models;
using Regulator.Client.Models.Penumbra;
using Regulator.Client.Services.Files.Interfaces;
using Regulator.Client.Services.Interop.Interfaces;
using Regulator.Client.Services.Providers.Interfaces;
using Regulator.Client.Services.Utilities.Interfaces;

namespace Regulator.Client.Services.Interop;

public class PenumbraApiClient : IPenumbraApiClient
{
    public const string Identity = "Regulator";
    
    private readonly IClientState _clientState;
    private readonly ILogger<PenumbraApiClient> _logger;
    private readonly IFileHashService _fileHashService;
    private readonly IThreadService _threadService;
    private readonly IMediator _mediator;
    private readonly IPlayerProvider _playerProvider;
    
    private readonly GetPlayerResourcePaths _getPlayerResourcePaths;
    private readonly CreateTemporaryCollection _createTemporaryCollection;
    private readonly AddTemporaryMod _addTemporaryMod;
    private readonly AssignTemporaryCollection _assignTemporaryCollection;
    
    private readonly Penumbra.Api.Helpers.EventSubscriber<nint, string, string> _gameObjectResourcePathResolved;
    
    private readonly Dictionary<string, Guid> _temporaryCollections = new();

    public PenumbraApiClient(IClientState clientState, IDalamudPluginInterface pluginInterface, ILogger<PenumbraApiClient> logger, IMediator mediator, IFileHashService fileHashService, IThreadService threadService, IPlayerProvider playerProvider)
    {
        _clientState = clientState;
        _logger = logger;
        _mediator = mediator;
        _fileHashService = fileHashService;
        _threadService = threadService;
        _playerProvider = playerProvider;
        _getPlayerResourcePaths = new GetPlayerResourcePaths(pluginInterface);
        _createTemporaryCollection = new CreateTemporaryCollection(pluginInterface);
        _addTemporaryMod = new AddTemporaryMod(pluginInterface);
        _gameObjectResourcePathResolved = GameObjectResourcePathResolved.Subscriber(pluginInterface, OnGameObjectResourcePathResolved);
        _assignTemporaryCollection = new AssignTemporaryCollection(pluginInterface);
        
        _playerProvider.OnPlayerSeen += OnPlayerSeen;
    }

    private void OnPlayerSeen(Player player)
    {
        CreateTemporaryCollection(player.SyncCode);
        AssignTemporaryCollection(player.SyncCode);
    }

    // TODO: Add actual comment. (This is for transient resources like emotes)
    private void OnGameObjectResourcePathResolved(IntPtr pointer, string originalFilePath, string redirectedFilePath)
    {
        redirectedFilePath = redirectedFilePath
            .Split('|')
            .Last()
            .Replace('\\', '/');
        
        if (pointer == IntPtr.Zero || string.Compare(originalFilePath, redirectedFilePath, ignoreCase: true, System.Globalization.CultureInfo.InvariantCulture) == 0)
        {
            return;
        }
        
        var filePaths = new Dictionary<nint, HashSet<FileReplacement>>
        {
            { pointer, [new FileReplacement(originalFilePath, redirectedFilePath, string.Empty)] }
        };
        
        var message = new UploadFiles(filePaths);
        _ = _mediator.PublishAsync(message);
    }

    public async Task<Dictionary<string, HashSet<string>>> GetCustomResourcePathsAsync(bool isDownload)
    {
        var objectId = await _threadService.RunOnFrameworkThreadAsync(() => _clientState.LocalPlayer?.ObjectIndex ?? null);

        if (objectId == null)
        {
            _logger.LogWarning("Cannot get resource paths: LocalPlayer is null.");
            return new Dictionary<string, HashSet<string>>();
        }
        
        var paths = _getPlayerResourcePaths.Invoke()[0];
        paths = paths.Where(IsCustomResource).ToDictionary(kv => kv.Key, kv => kv.Value);

        foreach (var path in paths)
        {
            _logger.LogInformation("Resource Path: {Path}", path.Key);
            
            foreach (var subPath in path.Value)
            {
                _logger.LogInformation(" - {SubPath}", subPath);
            }
        }

        if (!isDownload)
        {
            var filePathsByPointer = new Dictionary<nint, HashSet<FileReplacement>>
            {
                { 0, paths.Select(kv => new FileReplacement(kv.Value.First(), kv.Key, string.Empty)).ToHashSet() }
            };
            var message = new UploadFiles(filePathsByPointer);
            
            await _mediator.PublishAsync(message);
        }
        else
        {
            CreateTemporaryCollection("test");
            
            foreach (var path in paths)
            {
                await using var file = File.OpenRead(path.Key);
                var hash = await Task.Run(() => _fileHashService.ComputeHashAsync(file));
                
                //await _mediator.PublishAsync(new DownloadFiles([hash]));

                foreach (var subPath in path.Value)
                {
                    await AddTemporaryMod("test", new FileReplacement(subPath, path.Key, hash));
                }
            }
        }
        
        return paths;
    }

    public Guid CreateTemporaryCollection(string syncCode)
    {
        if (_temporaryCollections.TryGetValue(syncCode, out var existingCollectionId))
        {
            return existingCollectionId;
        }
        
        var result = _createTemporaryCollection.Invoke(Identity, syncCode, out var collectionId);

        if (result is not PenumbraApiEc.Success)
        {
            _logger.LogWarning("Failed to create temporary Penumbra collection for sync code {SyncCode}: {Result}", syncCode, result);
            return Guid.Empty;
        }
        
        _logger.LogInformation("Created temporary Penumbra collection {CollectionId} for sync code {SyncCode}", collectionId, syncCode);
        _temporaryCollections.TryAdd(syncCode, collectionId);
        return collectionId;
    }

    public async Task AddTemporaryMod(string syncCode, FileReplacement fileReplacement)
    {
        if (!_temporaryCollections.TryGetValue(syncCode, out var collectionId))
        {
            CreateTemporaryCollection(syncCode);
        }

        var filePaths = new Dictionary<string, string>
        {
            { fileReplacement.OriginalPath, fileReplacement.ReplacementPath }
        };

        var result = _addTemporaryMod.Invoke(fileReplacement.Hash, collectionId, filePaths, string.Empty, 0);
        
        if (result is not PenumbraApiEc.Success)
        {
            _logger.LogWarning("Failed to add temporary Penumbra mod for sync code {SyncCode}: {Result}", syncCode, result);
            return;
        }
        
        _logger.LogInformation("Added temporary Penumbra mod {ModId} to collection {CollectionId} for sync code {SyncCode}", fileReplacement.Hash, collectionId, syncCode);
    }

    public async Task AddTemporaryMods(string syncCode, IEnumerable<FileReplacement> fileReplacements)
    {
        foreach (var fileReplacement in fileReplacements)
        {
            await AddTemporaryMod(syncCode, fileReplacement);
        }
    }

    public void AssignTemporaryCollection(string syncCode)
    {
        var player = _playerProvider.GetCachedPlayerBySyncCode(syncCode);
        
        if (player is null)
        {
            _logger.LogWarning("No player found for sync code {SyncCode} when assigning temporary collection.", syncCode);
            return;
        }
        
        if (!_temporaryCollections.TryGetValue(syncCode, out var collectionId))
        {
            _logger.LogWarning("No temporary Penumbra collection found for sync code {SyncCode} when assigning to player.", syncCode);
            return;
        }
        
        var result = _assignTemporaryCollection.Invoke(collectionId, player.ObjectIndex);
    }

    private static bool IsCustomResource(KeyValuePair<string, HashSet<string>> resource)
    {
        if (resource.Value.Count > 1)
        {
            return true;
        }
        
        var singlePath = resource.Value.First();
        return !resource.Key.Equals(singlePath);
    }

    public void Dispose()
    {
        _gameObjectResourcePathResolved.Dispose();
        _playerProvider.OnPlayerSeen -= OnPlayerSeen;
        
        GC.SuppressFinalize(this);
    }
}