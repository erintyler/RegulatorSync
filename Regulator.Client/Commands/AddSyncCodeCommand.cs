using Dalamud.Plugin.Services;
using Microsoft.Extensions.Logging;
using Regulator.Client.Events.Client.Management;
using Regulator.Client.Services.Utilities.Interfaces;
using Regulator.Services.Sync.Shared.Dtos.Server;
using Regulator.Services.Sync.Shared.Hubs;

namespace Regulator.Client.Commands;

public class AddSyncCodeCommand(
    IMediator mediator,
    ICommandManager commandManager, 
    ILogger<AddSyncCodeCommand> logger) : BaseCommand(commandManager, logger)
{
    public override string Name => Constants.Commands.AddSyncCode;
    public override string HelpMessage => Constants.Commands.AddSyncCodeHelpMessage;
    
    public override void OnCommand(string command, string args)
    {
        mediator.PublishAsync(new AddSyncCode(args.Trim()));
    }
}