using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Microsoft.Extensions.Hosting;
using Regulator.Client.Models;
using Regulator.Client.Services.Providers.Interfaces;
using Regulator.Services.Sync.Shared.Services.Interfaces;

namespace Regulator.Client.Services.Providers;

public class PlayerProvider : IPlayerProvider, IHostedService, IDisposable
{
    private readonly ConcurrentDictionary<ulong, Player> _visiblePlayersByHash = new();
    private readonly ConcurrentDictionary<uint, ulong> _objectIdToHash = new();
    private readonly HashSet<uint> _unsyncedObjectIds = [];
    
    private readonly ICharacterHashProvider _hashProvider;
    private readonly IFramework _framework;
    private readonly IObjectTable _objectTable;
    private readonly IHashService _hashService;
    private readonly ISyncCodeProvider _syncCodeProvider;
    private readonly IPluginLog _logger;

    public PlayerProvider(ICharacterHashProvider hashProvider,
        IFramework framework,
        IObjectTable objectTable, 
        IHashService hashService, 
        ISyncCodeProvider syncCodeProvider,
        IPluginLog logger)
    {
        _hashProvider = hashProvider;
        _framework = framework;
        _objectTable = objectTable;
        _hashService = hashService;
        _syncCodeProvider = syncCodeProvider;
        _logger = logger;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _framework.Update += UpdateVisiblePlayers;
        
        var character = "Lop Laurent:83";
        _syncCodeProvider.AddSyncCode(_hashService.ComputeHash(character), "2854e6587949422aac67a04b44a03a7f");
        _hashProvider.AddOrUpdateHash("2854e6587949422aac67a04b44a03a7f", _hashService.ComputeHash(character));
        _logger.Info("Added test sync code for {Character}", character);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _framework.Update -= UpdateVisiblePlayers;
    }

    public Player? GetPlayerBySyncCode(string syncCode)
    {
        var hash = _hashProvider.GetHashBySyncCode(syncCode);
        return _visiblePlayersByHash.GetValueOrDefault(hash);
    }

    public unsafe void UpdateVisiblePlayers(IFramework framework)
    {
        try
        {
            var seenHashes = new HashSet<ulong>();
        
            foreach (var obj in _objectTable.CharacterManagerObjects.Where(o => o.ObjectKind is ObjectKind.Player))
            {
                if (_unsyncedObjectIds.Contains(obj.EntityId))
                {
                    continue;
                }
                
                var cachedHash = _objectIdToHash.GetValueOrDefault(obj.EntityId);

                if (cachedHash != 0)
                {
                    seenHashes.Add(cachedHash);
                    if (_visiblePlayersByHash.ContainsKey(cachedHash))
                    {
                        continue;
                    }
                }
                
                var name = obj.Name.TextValue;
                var world = ((BattleChara*)obj.Address)->Character.HomeWorld;
                var hash = _hashService.ComputeHash($"{name}:{world}");

                if (_visiblePlayersByHash.ContainsKey(hash))
                {
                    seenHashes.Add(hash);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(_syncCodeProvider.GetSyncCodeByHash(hash)))
                {
                    _unsyncedObjectIds.Add(obj.EntityId);
                    continue;
                }

                var player = new Player(obj.EntityId, name, world, obj.Address, obj.ObjectIndex);
            
                _visiblePlayersByHash.TryAdd(hash, player);
                _objectIdToHash.TryAdd(obj.EntityId, hash);
                _unsyncedObjectIds.Remove(obj.EntityId);
                seenHashes.Add(hash);
                
                _logger.Info("Added visible player: {Name} ({World})", name, world);
            }
        
            var toRemove = _visiblePlayersByHash.Keys.Except(seenHashes).ToList();
            foreach (var hash in toRemove)
            {
                _visiblePlayersByHash.TryRemove(hash, out _);
                _logger.Info("Removed visible player with hash: {Hash}", hash);
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error updating visible players");
        }
    }

    public void Dispose()
    {
        _visiblePlayersByHash.Clear();
        _framework.Update -= UpdateVisiblePlayers;
        GC.SuppressFinalize(this);
    }
}