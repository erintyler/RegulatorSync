using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Conditions;
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
    private readonly ConcurrentDictionary<string, Player> _pendingPlayersBySyncCode = new();
    private readonly ConcurrentDictionary<uint, ulong> _objectIdToHash = new();
    private readonly ConcurrentDictionary<uint, byte> _unsyncedObjectIds = [];
    
    private readonly ICharacterHashProvider _hashProvider;
    private readonly IClientState _clientState;
    private readonly ICondition _condition;
    private readonly IFramework _framework;
    private readonly IObjectTable _objectTable;
    private readonly IHashService _hashService;
    private readonly ISyncCodeProvider _syncCodeProvider;
    private readonly IPluginLog _logger;

    public event Action<Player>? OnPlayerSeen;
    public event Action<Player>? OnPlayerLeft;

    public PlayerProvider(ICharacterHashProvider hashProvider,
        IClientState clientState,
        ICondition condition,
        IFramework framework,
        IObjectTable objectTable, 
        IHashService hashService, 
        ISyncCodeProvider syncCodeProvider,
        IPluginLog logger)
    {
        _hashProvider = hashProvider;
        _clientState = clientState;
        _condition = condition;
        _framework = framework;
        _objectTable = objectTable;
        _hashService = hashService;
        _syncCodeProvider = syncCodeProvider;
        _logger = logger;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var hash = _hashService.ComputeHash("Anastasia Rossweiss:83");
        _hashProvider.AddOrUpdateHash("test", hash);
        _syncCodeProvider.AddSyncCode(hash, "test");
        
        _framework.Update += UpdateVisiblePlayers;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _framework.Update -= UpdateVisiblePlayers;
    }

    public Player? GetCachedPlayerBySyncCode(string syncCode)
    {
        var hash = _hashProvider.GetHashBySyncCode(syncCode);
        return _visiblePlayersByHash.GetValueOrDefault(hash);
    }

    public Player? GetCachedPlayerByHash(ulong hash)
    {
        return _visiblePlayersByHash.GetValueOrDefault(hash);
    }
    
    public void ClearUnsyncedObjectIds()
    {
        _unsyncedObjectIds.Clear();
    }

    public unsafe Player? GetPlayerByHash(string syncCode, ulong hash)
    {
        try
        {
            var player = GetCachedPlayerByHash(hash);
            if (player != null)
            {
                return player;
            }

            foreach (var obj in _objectTable.CharacterManagerObjects.Where(o => o.ObjectKind is ObjectKind.Player))
            {
                var name = obj.Name.TextValue;
                var world = ((BattleChara*)obj.Address)->Character.HomeWorld;
                var computedHash = _hashService.ComputeHash($"{name}:{world}");
                if (computedHash == hash)
                {
                    _logger.Info("Found player by hash: {Name} ({World})", name, world);
                    player = new Player(obj.EntityId, name, world, obj.Address, obj.ObjectIndex, syncCode);
                    _pendingPlayersBySyncCode.TryAdd(syncCode, player);
                    
                    return player;
                }
            }

            _logger.Info("No player found by hash: {Hash}", hash);
            return null;
        } 
        catch (Exception ex)
        {
            _logger.Error(ex, "Error getting player by hash");
            return null;
        }
    }

    public Player? GetPendingPlayerBySyncCode(string syncCode)
    {
        _pendingPlayersBySyncCode.TryRemove(syncCode, out var player);
        
        return player;
    }

    public unsafe void UpdateVisiblePlayers(IFramework framework)
    {
        try
        {
            if (!_clientState.IsLoggedIn || _condition[ConditionFlag.BetweenAreas])
            {
                return;
            }
            
            var seenHashes = new HashSet<ulong>();
        
            foreach (var obj in _objectTable.CharacterManagerObjects.Where(o => o.ObjectKind is ObjectKind.Player))
            {
                if (_unsyncedObjectIds.ContainsKey(obj.EntityId))
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

                var syncCode = _syncCodeProvider.GetSyncCodeByHash(hash);
                if (string.IsNullOrWhiteSpace(syncCode))
                {
                    _unsyncedObjectIds.TryAdd(obj.EntityId, 0);
                    continue;
                }

                var player = new Player(obj.EntityId, name, world, obj.Address, obj.ObjectIndex, syncCode);
            
                _visiblePlayersByHash.TryAdd(hash, player);
                _objectIdToHash.TryAdd(obj.EntityId, hash);
                _unsyncedObjectIds.TryRemove(obj.EntityId, out _);
                seenHashes.Add(hash);
                
                _logger.Info("Added visible player: {Name} ({World})", name, world);

                OnPlayerSeen?.Invoke(player);
            }
        
            var toRemove = _visiblePlayersByHash.Keys.Except(seenHashes).ToList();
            foreach (var hash in toRemove)
            {
                if (_visiblePlayersByHash.TryGetValue(hash, out var player))
                {
                    OnPlayerLeft?.Invoke(player);
                }
                
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