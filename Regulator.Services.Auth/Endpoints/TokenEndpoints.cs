using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Regulator.Services.Auth.Dtos.Requests;
using Regulator.Services.Auth.Dtos.Responses;
using Regulator.Services.Auth.Services.Interfaces;
using Regulator.Services.Shared.Constants;
using Regulator.Services.Shared.Extensions;
using Regulator.Services.Shared.Models;
using Regulator.Services.Shared.Services.Interfaces;

namespace Regulator.Services.Auth.Endpoints;

public static class TokenEndpoints
{
    public static void MapTokenEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet("/token", async (
                IAccessTokenService accessTokenService,
                IBlacklistService blacklistService,
                IRefreshTokenService refreshTokenService,
                IHttpContextAccessor contextAccessor,
                [FromQuery] ulong characterId,
                [FromQuery] string redirectUri) =>
            {
                var user = contextAccessor.HttpContext!.User;
                var discordId = user.GetDiscordId();

                var blacklistResponse = await blacklistService.IsDiscordIdOrCharacterIdBlacklistedAsync(discordId, characterId);
                if (blacklistResponse.IsBlacklisted)
                {
                    return Results.Json(blacklistResponse, statusCode: StatusCodes.Status403Forbidden, context: AppJsonSerializerContext.Default);
                }

                var accessTokenResult = await accessTokenService.GenerateAccessTokenAsync(characterId);
                if (!accessTokenResult.IsSuccess)
                {
                    return Results.Problem(accessTokenResult.ErrorMessage, statusCode: accessTokenResult.StatusCode);
                }

                var refreshTokenResult = await refreshTokenService.GenerateRefreshTokenAsync(discordId);
                if (!refreshTokenResult.IsSuccess)
                {
                    return Results.Problem(refreshTokenResult.ErrorMessage, statusCode: refreshTokenResult.StatusCode);
                }

                var accessToken = accessTokenResult.Value;
                var refreshToken = refreshTokenResult.Value;
                
                // Redirect to the provided redirect URI with the tokens as query parameters
                var redirectUrl = redirectUri = QueryHelpers.AddQueryString(redirectUri, "token", accessToken.TokenValue);
                return Results.Redirect(redirectUrl);
            })
            .RequireAuthorization(RegulatorAuthenticationSchemes.Cookie)
            .WithName("Get Access and Refresh Token")
            .WithTags("Token")
            .Produces<TokenResponseDto>()
            .Produces<BlacklistResponse>(StatusCodes.Status403Forbidden);
        
        endpoints
            .MapGet("/refresh", async (
                IAccessTokenService accessTokenService,
                IBlacklistService blacklistService,
                IRefreshTokenService refreshTokenService, 
                [FromBody] RefreshRequestDto refreshRequest) =>
            {
                var validateResult = await refreshTokenService.ValidateRefreshToken(refreshRequest.DiscordId, refreshRequest.RefreshToken);
                
                var blacklistResponse = await blacklistService.IsDiscordIdOrCharacterIdBlacklistedAsync(refreshRequest.DiscordId, refreshRequest.CharacterId);
                if (blacklistResponse.IsBlacklisted)
                {
                    return Results.Json(blacklistResponse, statusCode: StatusCodes.Status403Forbidden, context: AppJsonSerializerContext.Default);
                }
                
                if (!validateResult.IsSuccess)
                {
                    return Results.Problem("Unable to refresh access token.", statusCode: validateResult.StatusCode);
                }
                
                var accessTokenResult = await accessTokenService.GenerateAccessTokenAsync(refreshRequest.CharacterId);
                
                if (!accessTokenResult.IsSuccess)
                {
                    return Results.Problem(accessTokenResult.ErrorMessage, statusCode: accessTokenResult.StatusCode);
                }
                
                var newRefreshTokenResult = await refreshTokenService.GenerateRefreshTokenAsync(refreshRequest.DiscordId);
                
                if (!newRefreshTokenResult.IsSuccess)
                {
                    return Results.Problem(newRefreshTokenResult.ErrorMessage, statusCode: newRefreshTokenResult.StatusCode);
                }
                
                var accessToken = accessTokenResult.Value;
                var refreshToken = newRefreshTokenResult.Value;
                
                var response = new TokenResponseDto(accessToken.TokenValue, accessToken.Expiry, refreshToken.TokenValue, refreshToken.Expiry);
                return Results.Ok(response);
            })
            .WithName("Refresh Access Token")
            .WithTags("Token")
            .Produces<TokenResponseDto>()
            .Produces<BlacklistResponse>(StatusCodes.Status403Forbidden);
    }
}