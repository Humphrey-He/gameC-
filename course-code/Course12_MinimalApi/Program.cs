// Course 12: Minimal API 入门示例。

using Course12_MinimalApi.Dtos;
using Course12_MinimalApi.Models;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

List<Player> players = new();

app.MapGet("/", () => Results.Ok(new
{
    name = "Game Player API",
    status = "running"
}));

app.MapGet("/players", () => Results.Ok(players));

app.MapPost("/players", (CreatePlayerRequest request) =>
{
    Player player = new Player
    {
        Name = request.Name,
        Level = request.Level,
        Region = request.Region,
        Gold = request.Gold
    };

    players.Add(player);

    return Results.Created($"/players/{player.Id}", player);
});

app.MapGet("/rankings/top", (int? count) =>
{
    int rankingCount = count.GetValueOrDefault(10);

    return Results.Ok(players
        .OrderByDescending(p => p.Level * 100 + p.Gold / 10)
        .Take(rankingCount)
        .ToList());
});

app.Run();
