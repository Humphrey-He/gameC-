using GamePlayerSystem.Api.Responses;
using GamePlayerSystem.Core.Common;
using GamePlayerSystem.Core.Dtos;
using GamePlayerSystem.Core.Services;

namespace GamePlayerSystem.Api.Endpoints;

public static class PlayerEndpoints
{
    public static RouteGroupBuilder MapPlayerEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/players")
            .WithTags("Players");

        group.MapGet("/", GetPlayers)
            .WithName("GetPlayers")
            .WithSummary("获取玩家列表")
            .Produces<List<PlayerSummaryDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetPlayerById)
            .WithName("GetPlayerById")
            .WithSummary("按 ID 查询玩家")
            .Produces<PlayerSummaryDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/", CreatePlayer)
            .WithName("CreatePlayer")
            .WithSummary("新增玩家")
            .Produces<PlayerSummaryDto>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", UpdatePlayer)
            .WithName("UpdatePlayer")
            .WithSummary("修改玩家")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeletePlayer)
            .WithName("DeletePlayer")
            .WithSummary("删除玩家")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/disable", DisablePlayer)
            .WithName("DisablePlayer")
            .WithSummary("禁用玩家")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapGet("/by-region/{region}", GetPlayersByRegion)
            .WithName("GetPlayersByRegion")
            .WithSummary("按区服查询活跃玩家")
            .Produces<List<PlayerSummaryDto>>(StatusCodes.Status200OK);

        group.MapGet("/search", SearchPlayers)
            .WithName("SearchPlayers")
            .WithSummary("按名称搜索玩家")
            .Produces<List<PlayerSummaryDto>>(StatusCodes.Status200OK);

        return group;
    }

    private static async Task<IResult> GetPlayers(
        int? pageNumber,
        int? pageSize,
        PlayerApplication playerApplication,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        ILogger logger = loggerFactory.CreateLogger("PlayerEndpoints");
        logger.LogInformation("Players requested");

        List<PlayerSummaryDto> players = await playerApplication.GetPlayersAsync(
            pageNumber.GetValueOrDefault(1),
            pageSize.GetValueOrDefault(20),
            cancellationToken);

        return Results.Ok(players);
    }

    private static async Task<IResult> GetPlayerById(
        Guid id,
        PlayerApplication playerApplication,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        ILogger logger = loggerFactory.CreateLogger("PlayerEndpoints");
        Result<PlayerSummaryDto> result = await playerApplication.GetPlayerAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            logger.LogWarning("Get player failed. PlayerId: {PlayerId}, Error: {Error}", id, result.ErrorMessage);
            return Results.NotFound(ErrorResponse.From(result.ErrorMessage));
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> CreatePlayer(
        CreatePlayerRequest request,
        PlayerApplication playerApplication,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        ILogger logger = loggerFactory.CreateLogger("PlayerEndpoints");
        Result<PlayerSummaryDto> result = await playerApplication.AddPlayerAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            logger.LogWarning("Create player failed. Name: {PlayerName}, Error: {Error}", request.Name, result.ErrorMessage);
            return Results.BadRequest(ErrorResponse.From(result.ErrorMessage));
        }

        PlayerSummaryDto player = result.Value!;
        logger.LogInformation("Player created. PlayerId: {PlayerId}, Name: {PlayerName}", player.PlayerId, player.Name);

        return Results.Created($"/players/{player.PlayerId}", player);
    }

    private static async Task<IResult> UpdatePlayer(
        Guid id,
        UpdatePlayerRequest request,
        PlayerApplication playerApplication,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        ILogger logger = loggerFactory.CreateLogger("PlayerEndpoints");
        Result result = await playerApplication.UpdatePlayerAsync(id, request, cancellationToken);

        if (result.IsFailure)
        {
            logger.LogWarning("Update player failed. PlayerId: {PlayerId}, Error: {Error}", id, result.ErrorMessage);
            return result.ErrorMessage.Contains("不存在", StringComparison.OrdinalIgnoreCase)
                ? Results.NotFound(ErrorResponse.From(result.ErrorMessage))
                : Results.BadRequest(ErrorResponse.From(result.ErrorMessage));
        }

        logger.LogInformation("Player updated. PlayerId: {PlayerId}", id);
        return Results.NoContent();
    }

    private static async Task<IResult> DeletePlayer(
        Guid id,
        PlayerApplication playerApplication,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        ILogger logger = loggerFactory.CreateLogger("PlayerEndpoints");
        Result result = await playerApplication.RemoveByIdAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            logger.LogWarning("Delete player failed. PlayerId: {PlayerId}, Error: {Error}", id, result.ErrorMessage);
            return Results.NotFound(ErrorResponse.From(result.ErrorMessage));
        }

        logger.LogInformation("Player deleted. PlayerId: {PlayerId}", id);
        return Results.NoContent();
    }

    private static async Task<IResult> DisablePlayer(
        Guid id,
        PlayerApplication playerApplication,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        ILogger logger = loggerFactory.CreateLogger("PlayerEndpoints");
        Result result = await playerApplication.DisableByIdAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            logger.LogWarning("Disable player failed. PlayerId: {PlayerId}, Error: {Error}", id, result.ErrorMessage);
            return result.ErrorMessage.Contains("不存在", StringComparison.OrdinalIgnoreCase)
                ? Results.NotFound(ErrorResponse.From(result.ErrorMessage))
                : Results.BadRequest(ErrorResponse.From(result.ErrorMessage));
        }

        logger.LogInformation("Player disabled. PlayerId: {PlayerId}", id);
        return Results.NoContent();
    }

    private static async Task<IResult> GetPlayersByRegion(
        string region,
        PlayerApplication playerApplication,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        ILogger logger = loggerFactory.CreateLogger("PlayerEndpoints");
        logger.LogInformation("Players by region requested. Region: {Region}", region);

        return Results.Ok(await playerApplication.GetPlayersByRegionAsync(region, cancellationToken));
    }

    private static async Task<IResult> SearchPlayers(
        string? keyword,
        PlayerApplication playerApplication,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        ILogger logger = loggerFactory.CreateLogger("PlayerEndpoints");
        string searchKeyword = keyword ?? string.Empty;

        logger.LogInformation("Player search requested. Keyword: {Keyword}", searchKeyword);

        return Results.Ok(await playerApplication.SearchPlayersByNameAsync(searchKeyword, cancellationToken));
    }
}
