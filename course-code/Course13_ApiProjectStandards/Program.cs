// Course 13: 拆分 Endpoints、OpenAPI、统一错误响应、ILogger。

using Course13_ApiProjectStandards.Endpoints;
using Course13_ApiProjectStandards.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<List<Player>>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => Results.Ok(new
{
    name = "Game Player API",
    status = "running"
}))
.WithTags("Health")
.WithSummary("服务健康检查");

app.MapPlayerEndpoints();

app.Run();
