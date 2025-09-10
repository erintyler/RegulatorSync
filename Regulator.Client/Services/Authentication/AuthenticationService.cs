using System;
using System.Diagnostics;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Regulator.Client.Models.Configuration;
using Regulator.Client.Services.Authentication.Interfaces;
using Regulator.Services.Sync.Shared.Services.Interfaces;

namespace Regulator.Client.Services.Authentication;

public class AuthenticationService(
    ICallbackService callbackService,
    IClientState clientState, 
    IHashService hashService, 
    IOptions<AuthenticationSettings> authSettings,
    ILogger<AuthenticationService> logger) : IAuthenticationService
{
    public bool IsAuthenticated { get; private set; }
    public bool IsAuthenticationInProgress { get; private set; }

    public void OpenOAuthLoginPage()
    {
        if (IsAuthenticated)
        {
            return;
        }
        
        IsAuthenticationInProgress = true;
        
        callbackService.StartCallbackListener();
        var characterHash = GetCharacterHash();

        var psi = new ProcessStartInfo
        {
            FileName = $"{authSettings.Value.OAuthUrl}?CharacterId={characterHash}&RedirectUri={Uri.EscapeDataString(CallbackService.CallbackUrl)}",
            UseShellExecute = true
        };
        
        IsAuthenticationInProgress = false;
        Process.Start(psi);
    }

    // TODO: Should this be a separate service?
    private ulong GetCharacterHash()
    {
        var characterName = clientState.LocalPlayer?.Name.TextValue;
        
        if (string.IsNullOrEmpty(characterName))
        {
            IsAuthenticationInProgress = false;
            logger.LogError("Cannot start authentication: Character name is null or empty.");
            
            return 0;
        }

        var homeWorldId = clientState.LocalPlayer?.HomeWorld.RowId;
        
        if (homeWorldId == null)
        {
            IsAuthenticationInProgress = false;
            logger.LogError("Cannot start authentication: Home World ID is null.");
            
            return 0;
        }
        
        var characterAndWorld = $"{characterName}:{homeWorldId}";
        var characterHash = hashService.ComputeHash(characterAndWorld);
        return characterHash;
    }

    public void CompleteAuthentication()
    {
        IsAuthenticated = true;
        IsAuthenticationInProgress = false;
    }
}