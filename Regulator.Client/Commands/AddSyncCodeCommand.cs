using Dalamud.Plugin.Services;
using Microsoft.Extensions.Logging;
using Regulator.Services.Sync.Shared.Dtos.Server;
using Regulator.Services.Sync.Shared.Hubs;

namespace Regulator.Client.Commands;

public class AddSyncCodeCommand(
    IRegulatorServerMethods client,
    ICommandManager commandManager, 
    ILogger<AddSyncCodeCommand> logger) : BaseCommand(commandManager, logger)
{
    public override string Name => Constants.Commands.AddSyncCode;
    public override string HelpMessage => Constants.Commands.AddSyncCodeHelpMessage;
    
    public override void OnCommand(string command, string args)
    {
        var dto = new AddSyncCodeDto
        {
            TargetSyncCode = args.Trim()
        };

        client.AddSyncCodeAsync(dto);
    }
}