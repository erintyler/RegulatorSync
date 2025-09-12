using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Authentication;
using Regulator.Data.DynamoDb.Repositories;
using Regulator.Data.DynamoDb.Repositories.Interfaces;
using Regulator.Services.Auth.Dtos.Requests;
using Regulator.Services.Auth.Dtos.Responses;
using Regulator.Services.Auth.Endpoints;
using Regulator.Services.Auth.Services;
using Regulator.Services.Auth.Services.Interfaces;
using Regulator.Services.Shared.Configuration;
using Regulator.Services.Shared.Configuration.Models;
using Regulator.Services.Shared.Constants;
using Regulator.Services.Shared.Services;
using Regulator.Services.Shared.Services.Interfaces;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddOptions<TokenSettings>().Bind(builder.Configuration.GetSection(nameof(TokenSettings)));
builder.Services.AddSharedServices();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<ICharacterBlacklistRepository, CharacterBlacklistRepository>();
builder.Services.AddSingleton<IDiscordBlacklistRepository, DiscordBlacklistRepository>();

builder.Services.AddScoped<IAccessTokenService, AccessTokenService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IBlacklistService, BlacklistService>();
builder.Services.AddProblemDetails();

var awsOptions = builder.Configuration.GetAWSOptions();
builder.Services.AddDefaultAWSOptions(awsOptions);

builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddSingleton<IDynamoDBContext, DynamoDBContext>();

builder.Services.AddAuthentication(o =>
    {
        o.DefaultScheme = RegulatorAuthenticationSchemes.Cookie;
        o.DefaultChallengeScheme = RegulatorAuthenticationSchemes.Discord;
    })
    .AddCookie(RegulatorAuthenticationSchemes.Cookie)
    .AddOAuth(RegulatorAuthenticationSchemes.Discord, options =>
    {
        options.ClientId = builder.Configuration["Authentication:Discord:ClientId"] ?? throw new InvalidOperationException("Discord ClientId is not configured.");
        options.ClientSecret = builder.Configuration["Authentication:Discord:ClientSecret"] ?? throw new InvalidOperationException("Discord ClientSecret is not configured.");
        options.CallbackPath = "/auth/discord/callback";
        options.AuthorizationEndpoint = "https://discord.com/oauth2/authorize";
        options.TokenEndpoint = "https://discord.com/api/oauth2/token";
        options.UserInformationEndpoint = "https://discord.com/api/users/@me";
        options.Scope.Add("identify");
        options.SaveTokens = true;
        options.ClaimActions.MapJsonKey(RegulatorClaimTypes.DiscordId, "id");
        options.Events.OnCreatingTicket = async context =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);

            var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
                context.HttpContext.RequestAborted);
            response.EnsureSuccessStatusCode();

            using var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync(context.HttpContext.RequestAborted));
            context.RunClaimActions(user.RootElement);
        };
    });

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy(RegulatorAuthenticationSchemes.Cookie, p =>
    {
        p.AddAuthenticationSchemes(RegulatorAuthenticationSchemes.Discord);
        p.RequireAuthenticatedUser();
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

var authApi = app.MapGroup("/auth").WithTags("Authentication");
authApi.MapTokenEndpoints();

await app.RunAsync();

[JsonSerializable(typeof(RefreshRequestDto))]
[JsonSerializable(typeof(TokenResponseDto))]
[JsonSerializable(typeof(TokenRequestDto))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
