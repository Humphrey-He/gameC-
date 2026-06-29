using GamePlayerSystem.Api.Endpoints;
using GamePlayerSystem.Core;
using GamePlayerSystem.Core.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<PlayerApplication>();

var app = builder.Build();

app.MapOpenApi();

app.MapGet("/", () => Results.Ok(new
{
    name = ProjectInfo.Name,
    status = ProjectInfo.Status
}))
.WithTags("Health")
.WithSummary("服务健康检查");

app.MapPlayerEndpoints();
app.MapRankingEndpoints();
app.MapStatsEndpoints();

app.Run();
