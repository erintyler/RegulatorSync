using System.Text.Json.Serialization;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.AspNetCore.Mvc;
using Regulator.Data.DynamoDb.Repositories;
using Regulator.Data.DynamoDb.Repositories.Interfaces;
using Regulator.Services.Files.Configuration.Models;
using Regulator.Services.Files.Services;
using Regulator.Services.Files.Services.Interfaces;
using Regulator.Services.Files.Shared.Dtos.Requests;
using Regulator.Services.Files.Shared.Dtos.Responses;
using Regulator.Services.Shared.Configuration;
using Regulator.Services.Shared.Configuration.Models;
using Regulator.Services.Shared.Services;
using Regulator.Services.Shared.Services.Interfaces;
using Sentry.OpenTelemetry;

var builder = WebApplication.CreateSlimBuilder(args);
builder.AddServiceDefaults();
builder.WebHost.UseSentry(o =>
{
    o.UseOpenTelemetry();
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var tokenSettings = new TokenSettings
{
    AccessTokenExpirationMinutes = builder.Configuration.GetValue("TokenSettings:AccessTokenExpirationMinutes", 60),
    RefreshTokenExpirationDays = builder.Configuration.GetValue("TokenSettings:RefreshTokenExpirationDays", 30),
    Issuer = builder.Configuration["TokenSettings:Issuer"] ?? "Regulator",
    Audience = builder.Configuration["TokenSettings:Audience"] ?? "RegulatorAudience",
    Secret = builder.Configuration["TokenSettings:Secret"] ??
             throw new InvalidOperationException("TokenSettings:Secret is not configured.")
};

builder.Services.Configure<FileStoreSettings>(builder.Configuration.GetSection(nameof(FileStoreSettings)));

builder.Services.AddHttpContextAccessor();
builder.Services.AddJwtAuthentication(tokenSettings);

var awsOptions = builder.Configuration.GetAWSOptions();

#if DEBUG
awsOptions.DefaultClientConfig.ServiceSpecificSettings.Add("ForcePathStyle", "true");
#endif

builder.Services.AddDefaultAWSOptions(awsOptions);

builder.Services.AddAWSService<IAmazonDynamoDB>();

var fileStoreSettings = builder.Configuration.GetSection(nameof(FileStoreSettings)).Get<FileStoreSettings>() ?? throw new InvalidOperationException("FileStoreSettings section is not configured.");
var s3Options = builder.Configuration.GetAWSOptions("S3");
s3Options.Credentials = new BasicAWSCredentials(fileStoreSettings.AccessKey, fileStoreSettings.SecretKey);
#if DEBUG
s3Options.DefaultClientConfig.ServiceSpecificSettings.Add("ForcePathStyle", "true");
#endif

builder.Services.AddAWSService<IAmazonS3>(s3Options);
builder.Services.AddSingleton<IDynamoDBContext, DynamoDBContext>();
builder.Services.AddSingleton<IFileStore, S3FileStore>();
builder.Services.AddScoped<IFileService, FileService>();

builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IFileRepository, FileRepository>();
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddSingleton<IUserCreationService, UserCreationService>();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapPost("/{hash}/upload-url/", async (IFileService fileService, string hash, [FromBody] GetPresignedUploadUrlRequestDto dto) =>
{
    var exists = await fileService.CheckFileExistsAsync(hash);
    
    if (exists)
    {
        return Results.Conflict();
    }
    
    var result = await fileService.GetPresignedUploadUrlAsync(hash, dto.FileExtension, (int)dto.Size);
    return Results.Ok(new GetPresignedUploadUrlResponseDto(result));
}).RequireAuthorization();

app.MapPut("/{hash}/complete/", async (IFileService fileService, string hash) =>
{
    try
    {
        await fileService.FinalizeUploadAsync(hash);
    } 
    catch (FileNotFoundException)
    {
        return Results.NotFound(new { Message = "File not found in file store." });
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest();
    }
    
    return Results.Ok();
}).RequireAuthorization();

app.MapGet("/{hash}/download-url/", async (IFileService fileService, string hash) =>
{
    var result = await fileService.GetPresignedDownloadUrlAsync(hash);
    return Results.Ok(result);
}).RequireAuthorization();

app.MapGet("/{hash}/exists/", async (IFileService fileService, string hash) =>
{
    var exists = await fileService.CheckFileExistsAsync(hash);
    
    if (!exists)
    {
        return Results.NotFound();
    }
    
    return Results.Ok();
}).RequireAuthorization();

app.Run();

[JsonSerializable(typeof(GetPresignedUploadUrlResponseDto))]
[JsonSerializable(typeof(GetPresignedUploadUrlRequestDto))]
[JsonSerializable(typeof(GetPresignedDownloadUrlResponseDto))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}