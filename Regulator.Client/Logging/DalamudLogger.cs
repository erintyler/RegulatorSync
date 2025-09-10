using System;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace Regulator.Client.Logging;

public class DalamudLogger(IPluginLog pluginLog, string CategoryName) : ILogger
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = $"[{CategoryName}] {formatter(state, exception)}";

        switch (logLevel)
        {
            case LogLevel.Trace:
            case LogLevel.Debug:
                pluginLog.Debug(message);
                break;
            case LogLevel.Information:
                pluginLog.Info(message);
                break;
            case LogLevel.Warning:
                pluginLog.Warning(message);
                break;
            case LogLevel.Error:
                pluginLog.Error(exception, message);
                break;
            case LogLevel.Critical:
                pluginLog.Fatal(exception, message);
                break;
            case LogLevel.None:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
        }
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        var serilogLevel = logLevel switch
        {
            LogLevel.Trace => LogEventLevel.Verbose,
            LogLevel.Debug => LogEventLevel.Debug,
            LogLevel.Information => LogEventLevel.Information,
            LogLevel.Warning => LogEventLevel.Warning,
            LogLevel.Error => LogEventLevel.Error,
            LogLevel.Critical => LogEventLevel.Fatal,
            LogLevel.None => LogEventLevel.Fatal + 1, // No logging
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
        };
        
        return serilogLevel >= pluginLog.MinimumLogLevel;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }
}