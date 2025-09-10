using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Regulator.Client.Commands;

public abstract class BaseCommand(ICommandManager commandManager, ILogger<BaseCommand> logger) : IHostedService
{
    public abstract string Name { get; }
    public abstract string HelpMessage { get; }
    public abstract void OnCommand(string command, string args);

    public Task StartAsync(CancellationToken cancellationToken)
    {
        commandManager.AddHandler(Name, new CommandInfo(OnCommand)
        {
            HelpMessage = HelpMessage
        });
        
        logger.LogInformation("Registered command {CommandName}", Name);
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        commandManager.RemoveHandler(Name);
        return Task.CompletedTask;
    }
}