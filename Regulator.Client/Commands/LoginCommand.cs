using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Logging;
using Regulator.Client.Services.Authentication.Interfaces;

namespace Regulator.Client.Commands;

public class LoginCommand(
    IAuthenticationService authenticationService, 
    ICommandManager commandManager, 
    ILogger<LoginCommand> logger) : BaseCommand(commandManager, logger)
{
    public override string Name => Constants.Commands.Login;
    public override string HelpMessage => Constants.Commands.LoginHelpMessage;
    
    public override void OnCommand(string command, string args)
    {
        authenticationService.OpenOAuthLoginPage();
    }
}