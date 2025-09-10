using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Regulator.Client.Services.Utilities.Interfaces;

namespace Regulator.Client.Services.Utilities;

public class DebounceService : IDebounceService
{
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _tokens = new();
    
    public void Debounce(string key, Action action, int milliseconds = 300)
    {
        if (_tokens.TryGetValue(key, out var existingToken))
        {
            existingToken.Cancel();
            existingToken.Dispose();
        }

        var cts = new CancellationTokenSource();
        _tokens[key] = cts;

        var token = cts.Token;
        Task.Delay(milliseconds, token).ContinueWith(t =>
        {
            if (t.IsCanceled)
            {
                return;
            }
            
            action();
            _tokens.TryRemove(key, out _);
        }, token);
    }

    public void DebounceAsync(string key, Func<Task> action, int milliseconds = 300)
    {
        if (_tokens.TryGetValue(key, out var existingToken))
        {
            existingToken.Cancel();
            existingToken.Dispose();
        }

        var cts = new CancellationTokenSource();
        _tokens[key] = cts;

        var token = cts.Token;
        Task.Delay(milliseconds, token).ContinueWith(async t =>
        {
            if (t.IsCanceled)
            {
                return;
            }
            
            await action();
            _tokens.TryRemove(key, out _);
        }, token);
    }
}