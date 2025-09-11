namespace Regulator.Client.Constants;

public static class Commands
{
    public const string Login = "/regulator";
    public const string LoginHelpMessage = "Start OAuth login process to link your game client to your Regulator account.";
    
    public const string AddSyncCode = "/regulator-add";
    public const string AddSyncCodeHelpMessage = "Add a sync code to link your game client to another Regulator client.";
    
    public const string ShowWindow = "/regulator-show";
    public const string ShowWindowHelpMessage = "Show or hide the main Regulator window.";
}