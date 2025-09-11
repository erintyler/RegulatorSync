using Dalamud.Plugin.Services;
using Microsoft.Extensions.Logging;
using Regulator.Client.Services.Data.Interfaces;
using Regulator.Client.Services.Ui.Interfaces;
using Regulator.Client.Windows;
using Regulator.Services.Sync.Shared.Services.Interfaces;

namespace Regulator.Client.Commands;

public class ShowWindowCommand(
    IWindowService windowService,
    ICommandManager commandManager, 
    ISyncRequestService syncRequestService,
    IHashService hashService,
    ILogger<ShowWindowCommand> logger) : BaseCommand(commandManager, logger)
{
    public override string Name => Constants.Commands.ShowWindow;
    public override string HelpMessage => Constants.Commands.ShowWindowHelpMessage;
    
    public override void OnCommand(string command, string args)
    {
        windowService.ToggleWindow<MainWindow>();
    }
}