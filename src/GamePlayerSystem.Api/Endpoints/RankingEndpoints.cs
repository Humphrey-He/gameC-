using GamePlayerSystem.Api.Responses;
using GamePlayerSystem.Core.Dtos;
using GamePlayerSystem.Core.Services;

namespace GamePlayerSystem.Api.Endpoints;

public static class RankingEndpoints
{
    public static RouteGroupBuilder MapRankingEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/rankings")
            .WithTags("Rankings");

        group.MapGet("/top", GetTopRanking)
            .WithName("GetTopRanking")
            .WithSummary("获取战力排行榜")
            .Produces<List<RankingPlayerDto>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        return group;
    }

    private static IResult GetTopRanking(
        int? count,
        PlayerApplication playerApplication,
        ILoggerFactory loggerFactory)
    {
        ILogger logger = loggerFactory.CreateLogger("RankingEndpoints");
        int rankingCount = count.GetValueOrDefault(10);

        if (rankingCount <= 0)
        {
            logger.LogWarning("Invalid ranking count: {Count}", rankingCount);
            return Results.BadRequest(ErrorResponse.From("count 必须大于 0"));
        }

        logger.LogInformation("Top ranking requested. Count: {Count}", rankingCount);
        return Results.Ok(playerApplication.GetRanking(rankingCount));
    }
}
