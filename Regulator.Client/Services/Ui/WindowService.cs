using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Regulator.Client.Services.Ui.Interfaces;

namespace Regulator.Client.Services.Ui;

public class WindowService(
    WindowSystem windowSystem, 
    IDalamudPluginInterface pluginInterface,
    IEnumerable<Window> windows, 
    ILogger<WindowService> logger) : IWindowService, IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting WindowService and adding windows to WindowSystem.");
        
        foreach (var window in windows)
        {
            windowSystem.AddWindow(window);
            logger.LogInformation("Added window of type {WindowType} to WindowSystem.", window.GetType().FullName);
        }
        
        pluginInterface.UiBuilder.Draw += windowSystem.Draw;
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var window in windows)
        {
            windowSystem.RemoveWindow(window);
            
            // Check if window implements IDisposable and dispose it
            if (window is IDisposable disposable) 
            {
                disposable.Dispose();
            }
        }
        
        pluginInterface.UiBuilder.Draw -= windowSystem.Draw;
        
        return Task.CompletedTask;
    }
    
    public void ToggleWindow<T>() where T : Window
    {
        foreach (var window in windows)
        {
            if (window is T typedWindow)
            {
                typedWindow.Toggle();
                return;
            }
        }
        
        throw new InvalidOperationException($"No window of type {typeof(T).FullName} found.");
    }

    public void ShowWindow<T>() where T : Window
    {
        foreach (var window in windows)
        {
            if (window is T typedWindow)
            {
                typedWindow.IsOpen = true;
                return;
            }
        }
        
        throw new InvalidOperationException($"No window of type {typeof(T).FullName} found.");
    }

    public void HideWindow<T>() where T : Window
    {
        foreach (var window in windows)
        {
            if (window is T typedWindow)
            {
                typedWindow.IsOpen = false;
                return;
            }
        }
        
        throw new InvalidOperationException($"No window of type {typeof(T).FullName} found.");
    }
}