using System.Text.Json.Serialization;
using Regulator.Services.Auth.Dtos.Requests;
using Regulator.Services.Auth.Dtos.Responses;
using Regulator.Services.Auth.Endpoints;
using Regulator.Services.Auth.Services;
using Regulator.Services.Auth.Services.Interfaces;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddScoped<IAccessTokenService, AccessTokenService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddProblemDetails();

var app = builder.Build();
var authApi = app.MapGroup("/auth").WithTags("Authentication");
authApi.MapTokenEndpoints();

await app.RunAsync();

[JsonSerializable(typeof(RefreshRequestDto))]
[JsonSerializable(typeof(TokenResponseDto))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
