using Course13_ApiProjectStandards.Dtos;
using Course13_ApiProjectStandards.Models;
using Course13_ApiProjectStandards.Responses;

namespace Course13_ApiProjectStandards.Endpoints;

public static class PlayerEndpoints
{
    public static RouteGroupBuilder MapPlayerEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/players")
            .WithTags("Players");

        group.MapGet("/", GetPlayers)
            .WithName("GetPlayers")
            .WithSummary("获取玩家列表");

        group.MapPost("/", CreatePlayer)
            .WithName("CreatePlayer")
            .WithSummary("新增玩家");

        return group;
    }

    private static IResult GetPlayers(List<Player> players, ILoggerFactory loggerFactory)
    {
        ILogger logger = loggerFactory.CreateLogger("PlayerEndpoints");
        logger.LogInformation("Players requested");

        return Results.Ok(players);
    }

    private static IResult CreatePlayer(
        CreatePlayerRequest request,
        List<Player> players,
        ILoggerFactory loggerFactory)
    {
        ILogger logger = loggerFactory.CreateLogger("PlayerEndpoints");

        bool exists = players.Any(p =>
            string.Equals(p.Name, request.Name, StringComparison.OrdinalIgnoreCase));

        if (exists)
        {
            logger.LogWarning("Create player failed. Name exists: {PlayerName}", request.Name);
            return Results.BadRequest(ErrorResponse.From("玩家名称已存在"));
        }

        Player player = new Player
        {
            Name = request.Name,
            Level = request.Level,
            Region = request.Region,
            Gold = request.Gold
        };

        players.Add(player);

        logger.LogInformation("Player created. PlayerId: {PlayerId}", player.Id);

        return Results.Created($"/players/{player.Id}", player);
    }
}
