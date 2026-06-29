using GamePlayerSystem.Api.Endpoints;
using GamePlayerSystem.Api.Persistence;
using GamePlayerSystem.Core;
using GamePlayerSystem.Core.Persistence;
using GamePlayerSystem.Core.Repositories;
using GamePlayerSystem.Core.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

string playerDatabaseConnectionString = builder.Configuration.GetConnectionString("PlayerDatabase")
    ?? throw new InvalidOperationException("缺少 ConnectionStrings:PlayerDatabase 配置");

string normalizedPlayerDatabaseConnectionString = SqliteDatabasePath.NormalizeConnectionString(
    playerDatabaseConnectionString,
    Directory.GetCurrentDirectory());

builder.Services.AddDbContext<PlayerDbContext>(options =>
    options.UseSqlite(
        normalizedPlayerDatabaseConnectionString,
        sqliteOptions => sqliteOptions.MigrationsAssembly(typeof(Program).Assembly.GetName().Name)));
builder.Services.AddScoped<IPlayerRepository, EfPlayerRepository>();
builder.Services.AddScoped<PlayerApplication>();

var app = builder.Build();

await ApplyDatabaseMigrationsAsync(app);

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

static async Task ApplyDatabaseMigrationsAsync(WebApplication app)
{
    using IServiceScope scope = app.Services.CreateScope();
    PlayerDbContext dbContext = scope.ServiceProvider.GetRequiredService<PlayerDbContext>();

    await dbContext.Database.MigrateAsync();
}
