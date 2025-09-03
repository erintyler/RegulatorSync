using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Serilog;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(o =>
    {
        o.DefaultScheme = "Cookies";
        o.DefaultChallengeScheme = "Discord";
    })
    .AddCookie()
    .AddJwtBearer(o =>
    {
        o.Authority = builder.Configuration["Authentication:Jwt:Authority"] ?? throw new InvalidOperationException("JWT Authority is not configured.");
        o.Audience = builder.Configuration["Authentication:Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured.");
        o.RequireHttpsMetadata = true;
    })
    .AddOAuth("Discord", options =>
    {
        options.ClientId = builder.Configuration["Authentication:Discord:ClientId"] ?? throw new InvalidOperationException("Discord ClientId is not configured.");
        options.ClientSecret = builder.Configuration["Authentication:Discord:ClientSecret"] ?? throw new InvalidOperationException("Discord ClientSecret is not configured.");
        options.CallbackPath = "/auth/discord/callback";
        options.AuthorizationEndpoint = "https://discord.com/oauth2/authorize";
        options.TokenEndpoint = "https://discord.com/api/oauth2/token";
        options.UserInformationEndpoint = "https://discord.com/api/users/@me";
        options.Scope.Add("identify");
        options.SaveTokens = true;
        options.ClaimActions.MapAll();
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

builder.Services.AddAuthorization();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

var sampleTodos = new Todo[]
{
    new(1, "Walk the dog"),
    new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
    new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
    new(4, "Clean the bathroom"),
    new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
};

var todosApi = app.MapGroup("/todos").RequireAuthorization();
todosApi.MapGet("/", (IHttpContextAccessor httpContextAccessor) => sampleTodos);
todosApi.MapGet("/{id}", (int id) =>
    sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
        ? Results.Ok(todo)
        : Results.NotFound());

app.Run();

public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

[JsonSerializable(typeof(Todo[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}