using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.SignalR;
using Regulator.Data.DynamoDb.Repositories;
using Regulator.Data.DynamoDb.Repositories.Interfaces;
using Regulator.Data.Redis.Configuration;
using Regulator.Services.Shared.Configuration;
using Regulator.Services.Shared.Configuration.Models;
using Regulator.Services.Shared.Services;
using Regulator.Services.Shared.Services.Interfaces;
using Regulator.Services.Sync.Hubs;
using Regulator.Services.Sync.RequestHandlers;
using Regulator.Services.Sync.RequestHandlers.Glamourer;
using Regulator.Services.Sync.RequestHandlers.Interfaces;
using Regulator.Services.Sync.Services;
using Regulator.Services.Sync.Shared.Dtos.Server;
using Regulator.Services.Sync.Shared.Dtos.Server.Glamourer;

var builder = WebApplication.CreateSlimBuilder(args);

var tokenSettings = new TokenSettings
{
    AccessTokenExpirationMinutes = builder.Configuration.GetValue("TokenSettings:AccessTokenExpirationMinutes", 60),
    RefreshTokenExpirationDays = builder.Configuration.GetValue("TokenSettings:RefreshTokenExpirationDays", 30),
    Issuer = builder.Configuration["TokenSettings:Issuer"] ?? "Regulator",
    Audience = builder.Configuration["TokenSettings:Audience"] ?? "RegulatorAudience",
    Secret = builder.Configuration["TokenSettings:Secret"] ??
             throw new InvalidOperationException("TokenSettings:Secret is not configured.")
};

builder.Services.AddHttpContextAccessor();
builder.Services.AddJwtAuthentication(tokenSettings);
builder.Services.AddRedisDataServices();

builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddSingleton<IDynamoDBContext, DynamoDBContext>();

builder.Services.AddSingleton<IUserIdProvider, SyncCodeIdProvider>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<IUserCreationService, UserCreationService>();

builder.Services.AddScoped<IRequestHandler<NotifyCustomizationsResetDto>, NotifyCustomizationsResetHandler>();
builder.Services.AddScoped<IRequestHandler<NotifyCustomizationsUpdatedDto>, NotifyCustomizationsUpdatedHandler>();
builder.Services.AddScoped<IRequestHandler<RequestCustomizationsDto>, RequestCustomizationsHandler>();
builder.Services.AddScoped<IRequestHandler<SyncRequestDto>, SyncRequestHandler>();
builder.Services.AddScoped<IRequestHandler<SyncRequestResponseDto>, SyncRequestResponseHandler>();
builder.Services.AddScoped<IRequestHandlerFactory, RequestHandlerFactory>();

builder.Services.AddSignalR();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<RegulatorHub>("/sync");

app.Run();