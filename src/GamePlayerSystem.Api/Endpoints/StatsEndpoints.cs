using GamePlayerSystem.Core.Dtos;
using GamePlayerSystem.Core.Services;

namespace GamePlayerSystem.Api.Endpoints;

public static class StatsEndpoints
{
    public static RouteGroupBuilder MapStatsEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/stats")
            .WithTags("Stats");

        group.MapGet("/regions", GetRegionStats)
            .WithName("GetRegionStats")
            .WithSummary("获取区服统计")
            .Produces<List<RegionStatDto>>(StatusCodes.Status200OK);

        return group;
    }

    private static IResult GetRegionStats(
        PlayerApplication playerApplication,
        ILoggerFactory loggerFactory)
    {
        ILogger logger = loggerFactory.CreateLogger("StatsEndpoints");
        logger.LogInformation("Region stats requested");

        return Results.Ok(playerApplication.GetRegionStats());
    }
}
