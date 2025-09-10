using System;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Regulator.Client.Services.Utilities.Interfaces;

namespace Regulator.Client.Services.Utilities;

public class ThreadService(IFramework framework) : IThreadService
{
    public ValueTask RunOnFrameworkThreadAsync(Action action)
    {
        if (!framework.IsInFrameworkUpdateThread)
        {
            return new ValueTask(framework.RunOnFrameworkThread(action));
        }
        
        try
        {
            action();
            return ValueTask.CompletedTask;
        }
        catch (Exception ex)
        {
            return ValueTask.FromException(ex);
        }
    }

    public ValueTask<T> RunOnFrameworkThreadAsync<T>(Func<T> func)
    {
        if (!framework.IsInFrameworkUpdateThread)
        {
            return new ValueTask<T>(framework.RunOnFrameworkThread(func));
        }
        
        try
        {
            return ValueTask.FromResult(func());
        }
        catch (Exception ex)
        {
            return ValueTask.FromException<T>(ex);
        }
    }
}