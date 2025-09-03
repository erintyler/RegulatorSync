using Microsoft.AspNetCore.Mvc;
using Regulator.Services.Auth.Dtos.Requests;
using Regulator.Services.Auth.Dtos.Responses;
using Regulator.Services.Auth.Services.Interfaces;
using Regulator.Services.Shared.Constants;
using Regulator.Services.Shared.Extensions;

namespace Regulator.Services.Auth.Endpoints;

public static class TokenEndpoints
{
    public static void MapTokenEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet("/token", async (IAccessTokenService accessTokenService, IRefreshTokenService refreshTokenService, IHttpContextAccessor contextAccessor) =>
            {
                var user = contextAccessor.HttpContext!.User;
                var discordId = user.GetDiscordId();
                
                var accessTokenResult = await accessTokenService.GenerateAccessTokenAsync();
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
                
                var response = new TokenResponseDto(accessToken.TokenValue, accessToken.Expiry, refreshToken.TokenValue, refreshToken.Expiry);
                return Results.Ok(response);
            })
            .RequireAuthorization(RegulatorAuthenticationSchemes.Cookie)
            .WithName("Get Access and Refresh Token")
            .WithTags("Token")
            .Produces<TokenResponseDto>();
        
        endpoints
            .MapGet("/refresh", async (IAccessTokenService accessTokenService, IRefreshTokenService refreshTokenService, [FromBody] RefreshRequestDto refreshRequest) =>
            {
                var validateResult = await refreshTokenService.ValidateRefreshToken(refreshRequest.DiscordId, refreshRequest.RefreshToken);
                
                if (!validateResult.IsSuccess)
                {
                    return Results.Problem("Unable to refresh access token.", statusCode: validateResult.StatusCode);
                }
                
                var accessTokenResult = await accessTokenService.GenerateAccessTokenAsync();
                
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
            .Produces<TokenResponseDto>();
    }
}