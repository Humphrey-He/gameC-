# C# 基础学习专刊：课程十三

## 课程主题

Web API 项目规范：拆分 Minimal API Endpoints，增加 Swagger / OpenAPI，统一错误响应，并使用 `ILogger`。

课程十二中，我们已经把玩家管理系统升级成了 Minimal API：

```text
GET    /players
GET    /players/{id}
POST   /players
DELETE /players/{id}
GET    /rankings/top
GET    /stats/regions
```

但是所有接口都写在 `Program.cs` 里，随着接口增加，`Program.cs` 会越来越长。

课程十三的目标是整理 Web API 项目结构，让它更接近真实项目：

- Endpoints 拆分。
- OpenAPI / Swagger 文档。
- 统一错误响应。
- 使用 `ILogger` 记录日志。

## 本课目标

完成本课后，你应该能做到：

- 理解为什么要拆分 Endpoints。
- 使用扩展方法组织 Minimal API。
- 使用 `MapGroup` 给接口分组。
- 增加 OpenAPI 文档。
- 理解 OpenAPI 和 Swagger 的区别。
- 创建统一错误响应 `ErrorResponse`。
- 用 `ILogger` 记录请求、业务失败和异常。
- 让 `Program.cs` 更短、更像项目入口。

## 第 1 步：当前 `Program.cs` 的问题

课程十二的 `Program.cs` 可能已经包含：

- 服务注册。
- 启动加载数据。
- 根接口。
- 玩家接口。
- 排行榜接口。
- 区服统计接口。
- 错误响应。
- 保存逻辑。

这会让 `Program.cs` 变成一个大文件。

问题：

- 接口越多，越难找。
- 玩家接口和排行榜接口混在一起。
- 错误响应到处写匿名对象。
- 日志缺失，出问题不好排查。
- OpenAPI 文档不够清晰。

整理目标：

```text
Program.cs 负责启动和注册。
Endpoints 负责定义 HTTP 路由。
Services 负责业务。
Dtos 负责请求和响应对象。
```

## 第 2 步：推荐 Web API 目录结构

在 API 项目中建议整理成：

```text
GamePlayerSystem.Api/
  Endpoints/
    PlayerEndpoints.cs
    RankingEndpoints.cs
    StatsEndpoints.cs
  Responses/
    ErrorResponse.cs
  Program.cs
  appsettings.json
  GamePlayerSystem.Api.csproj
```

如果 DTO 已经放在核心项目：

```text
gameC#/Dtos/
```

可以继续复用。

如果 DTO 只服务于 Web API，也可以放在 API 项目中：

```text
GamePlayerSystem.Api/Requests/
GamePlayerSystem.Api/Responses/
```

本课为了简单，建议：

- 业务 DTO 继续放在核心项目 `Dtos`。
- API 专用响应放在 API 项目 `Responses`。
- Endpoints 放在 API 项目 `Endpoints`。

## 第 3 步：什么是 Endpoint 拆分

课程十二中，我们直接写：

```csharp
app.MapGet("/players", ...);
app.MapPost("/players", ...);
app.MapDelete("/players/{id:guid}", ...);
```

现在改成：

```csharp
app.MapPlayerEndpoints();
app.MapRankingEndpoints();
app.MapStatsEndpoints();
```

这样 `Program.cs` 更像启动配置：

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<PlayerManager>();
builder.Services.AddSingleton<IPlayerStorage>(
    _ => new JsonPlayerStorage(Path.Combine("data", "players.json")));
builder.Services.AddSingleton<PlayerApplication>();

var app = builder.Build();

app.MapPlayerEndpoints();
app.MapRankingEndpoints();
app.MapStatsEndpoints();

app.Run();
```

具体接口放到单独文件里。

## 第 4 步：创建 `PlayerEndpoints`

创建：

```text
Endpoints/
  PlayerEndpoints.cs
```

代码结构：

```csharp
using gameC_.Common;
using gameC_.Dtos;
using gameC_.Models;
using gameC_.Services;
using GamePlayerSystem.Api.Responses;

namespace GamePlayerSystem.Api.Endpoints;

public static class PlayerEndpoints
{
    public static RouteGroupBuilder MapPlayerEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/players")
            .WithTags("Players");

        group.MapGet("/", GetPlayers)
            .WithName("GetPlayers");

        group.MapGet("/{id:guid}", GetPlayerById)
            .WithName("GetPlayerById");

        group.MapPost("/", CreatePlayer)
            .WithName("CreatePlayer");

        group.MapDelete("/{id:guid}", DeletePlayer)
            .WithName("DeletePlayer");

        group.MapPost("/{id:guid}/disable", DisablePlayer)
            .WithName("DisablePlayer");

        return group;
    }

    private static IResult GetPlayers(PlayerApplication playerApp)
    {
        List<PlayerSummaryDto> players = playerApp.Players
            .OrderByDescending(p => p.Level)
            .Select(p => new PlayerSummaryDto
            {
                PlayerId = p.Id,
                Name = p.Name,
                RegionName = p.GetRegionName(),
                Level = p.Level,
                IsActive = p.IsActive
            })
            .ToList();

        return Results.Ok(players);
    }

    private static IResult GetPlayerById(Guid id, PlayerApplication playerApp)
    {
        Result<Player> result = playerApp.GetPlayer(id);

        if (result.IsFailure)
        {
            return Results.NotFound(ErrorResponse.From(result.ErrorMessage));
        }

        Player player = result.Value!;

        return Results.Ok(new PlayerSummaryDto
        {
            PlayerId = player.Id,
            Name = player.Name,
            RegionName = player.GetRegionName(),
            Level = player.Level,
            IsActive = player.IsActive
        });
    }

    private static async Task<IResult> CreatePlayer(
        CreatePlayerRequest request,
        PlayerApplication playerApp,
        ILoggerFactory loggerFactory)
    {
        ILogger logger = loggerFactory.CreateLogger("PlayerEndpoints");

        Player player = new Player
        {
            Name = request.Name,
            Level = request.Level,
            Region = request.Region,
            Gold = request.Gold
        };

        Result result = playerApp.AddPlayer(player);

        if (result.IsFailure)
        {
            logger.LogWarning("Create player failed. Name: {PlayerName}, Error: {Error}",
                request.Name,
                result.ErrorMessage);

            return Results.BadRequest(ErrorResponse.From(result.ErrorMessage));
        }

        await playerApp.SaveAsync();

        logger.LogInformation("Player created. PlayerId: {PlayerId}, Name: {PlayerName}",
            player.Id,
            player.Name);

        return Results.Created($"/players/{player.Id}", new
        {
            player.Id,
            player.Name,
            player.Level,
            player.Region,
            player.Gold,
            player.IsActive
        });
    }

    private static async Task<IResult> DeletePlayer(
        Guid id,
        PlayerApplication playerApp,
        ILoggerFactory loggerFactory)
    {
        ILogger logger = loggerFactory.CreateLogger("PlayerEndpoints");

        Result result = playerApp.RemoveById(id);

        if (result.IsFailure)
        {
            logger.LogWarning("Delete player failed. PlayerId: {PlayerId}, Error: {Error}",
                id,
                result.ErrorMessage);

            return Results.NotFound(ErrorResponse.From(result.ErrorMessage));
        }

        await playerApp.SaveAsync();

        logger.LogInformation("Player deleted. PlayerId: {PlayerId}", id);

        return Results.NoContent();
    }

    private static async Task<IResult> DisablePlayer(
        Guid id,
        PlayerApplication playerApp,
        ILoggerFactory loggerFactory)
    {
        ILogger logger = loggerFactory.CreateLogger("PlayerEndpoints");

        Result result = playerApp.DisableById(id);

        if (result.IsFailure)
        {
            logger.LogWarning("Disable player failed. PlayerId: {PlayerId}, Error: {Error}",
                id,
                result.ErrorMessage);

            return Results.BadRequest(ErrorResponse.From(result.ErrorMessage));
        }

        await playerApp.SaveAsync();

        logger.LogInformation("Player disabled. PlayerId: {PlayerId}", id);

        return Results.NoContent();
    }
}
```

说明：

- `MapGroup("/players")` 把玩家接口分组。
- `.WithTags("Players")` 让 OpenAPI 文档按 Players 分组。
- 私有方法负责具体接口逻辑。
- 错误响应统一使用 `ErrorResponse`。
- 日志使用结构化字段 `{PlayerId}`、`{Error}`。

## 第 5 步：创建统一错误响应

创建：

```text
Responses/
  ErrorResponse.cs
```

代码：

```csharp
namespace GamePlayerSystem.Api.Responses;

public sealed class ErrorResponse
{
    public string Error { get; init; } = string.Empty;

    public static ErrorResponse From(string error)
    {
        return new ErrorResponse
        {
            Error = error
        };
    }
}
```

以前到处写：

```csharp
new
{
    error = result.ErrorMessage
}
```

现在统一写：

```csharp
ErrorResponse.From(result.ErrorMessage)
```

好处：

- 错误响应格式统一。
- 后续增加 `ErrorCode`、`TraceId` 更方便。
- API 文档更清楚。

进阶版：

```csharp
public sealed class ErrorResponse
{
    public string Error { get; init; } = string.Empty;
    public string? ErrorCode { get; init; }
    public string? TraceId { get; init; }
}
```

本课先用简单版。

## 第 6 步：创建 `RankingEndpoints`

创建：

```text
Endpoints/
  RankingEndpoints.cs
```

代码：

```csharp
using gameC_.Dtos;
using gameC_.Services;
using GamePlayerSystem.Api.Responses;

namespace GamePlayerSystem.Api.Endpoints;

public static class RankingEndpoints
{
    public static RouteGroupBuilder MapRankingEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/rankings")
            .WithTags("Rankings");

        group.MapGet("/top", GetTopRanking)
            .WithName("GetTopRanking");

        return group;
    }

    private static IResult GetTopRanking(
        int? count,
        PlayerApplication playerApp,
        ILoggerFactory loggerFactory)
    {
        ILogger logger = loggerFactory.CreateLogger("RankingEndpoints");

        int rankingCount = count.GetValueOrDefault(10);

        if (rankingCount <= 0)
        {
            logger.LogWarning("Invalid ranking count: {Count}", rankingCount);

            return Results.BadRequest(ErrorResponse.From("count 必须大于 0"));
        }

        List<RankingPlayerDto> ranking = playerApp.GetRanking(rankingCount);

        logger.LogInformation("Top ranking requested. Count: {Count}", rankingCount);

        return Results.Ok(ranking);
    }
}
```

接口：

```text
GET /rankings/top?count=10
```

## 第 7 步：创建 `StatsEndpoints`

创建：

```text
Endpoints/
  StatsEndpoints.cs
```

代码：

```csharp
using gameC_.Services;

namespace GamePlayerSystem.Api.Endpoints;

public static class StatsEndpoints
{
    public static RouteGroupBuilder MapStatsEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/stats")
            .WithTags("Stats");

        group.MapGet("/regions", GetRegionStats)
            .WithName("GetRegionStats");

        return group;
    }

    private static IResult GetRegionStats(
        PlayerApplication playerApp,
        ILoggerFactory loggerFactory)
    {
        ILogger logger = loggerFactory.CreateLogger("StatsEndpoints");

        logger.LogInformation("Region stats requested");

        return Results.Ok(playerApp.GetRegionStats());
    }
}
```

接口：

```text
GET /stats/regions
```

## 第 8 步：让 `Program.cs` 变短

拆分前：

```csharp
app.MapGet("/players", ...);
app.MapPost("/players", ...);
app.MapDelete("/players/{id:guid}", ...);
app.MapGet("/rankings/top", ...);
app.MapGet("/stats/regions", ...);
```

拆分后：

```csharp
using gameC_.Exceptions;
using gameC_.Services;
using GamePlayerSystem.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<PlayerManager>();
builder.Services.AddSingleton<IPlayerStorage>(
    _ => new JsonPlayerStorage(Path.Combine("data", "players.json")));
builder.Services.AddSingleton<PlayerApplication>();

var app = builder.Build();

try
{
    using IServiceScope scope = app.Services.CreateScope();
    PlayerApplication playerApp = scope.ServiceProvider.GetRequiredService<PlayerApplication>();
    await playerApp.LoadAsync();
}
catch (PlayerStorageException ex)
{
    app.Logger.LogError(ex, "Load player data failed");
}

app.MapGet("/", () => Results.Ok(new
{
    name = "Game Player API",
    status = "running"
}))
.WithTags("Health");

app.MapPlayerEndpoints();
app.MapRankingEndpoints();
app.MapStatsEndpoints();

app.Run();
```

现在 `Program.cs` 的职责更清楚：

- 创建应用。
- 注册服务。
- 启动加载。
- 注册 endpoint 组。
- 启动服务。

## 第 9 步：OpenAPI 和 Swagger 的区别

经常有人把 OpenAPI 和 Swagger 混着说。

简单区分：

| 名称 | 含义 |
| --- | --- |
| OpenAPI | 一种描述 HTTP API 的规范 |
| Swagger | 一组围绕 OpenAPI 的工具，常见的是 Swagger UI |

OpenAPI 文档通常是 JSON：

```text
/openapi/v1.json
```

Swagger UI 是一个网页，可以在浏览器里查看和测试接口。

在 ASP.NET Core 中：

- .NET 提供第一方 OpenAPI 文档生成能力。
- Swagger UI 通常通过第三方包提供，例如 NSwag 或 Swashbuckle。

## 第 10 步：增加 OpenAPI 文档

在 .NET 10 Minimal API 中，可以使用第一方 OpenAPI：

```csharp
builder.Services.AddOpenApi();
```

然后：

```csharp
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
```

完整位置：

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<PlayerManager>();
builder.Services.AddSingleton<IPlayerStorage>(
    _ => new JsonPlayerStorage(Path.Combine("data", "players.json")));
builder.Services.AddSingleton<PlayerApplication>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
```

访问：

```text
GET /openapi/v1.json
```

如果项目模板没有自动引用 OpenAPI 包，可以添加：

```powershell
dotnet add GamePlayerSystem.Api package Microsoft.AspNetCore.OpenApi
```

注意：

- OpenAPI 文档是机器可读的接口描述。
- 它不一定自带漂亮的网页 UI。

## 第 11 步：增加 Swagger UI

如果想要浏览器里的交互式页面，可以使用 NSwag 或 Swashbuckle。

这里给一个 NSwag 示例。

安装：

```powershell
dotnet add GamePlayerSystem.Api package NSwag.AspNetCore
```

注册：

```csharp
builder.Services.AddOpenApiDocument();
```

启用：

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}
```

访问：

```text
/swagger
```

注意：

- `AddOpenApi` / `MapOpenApi` 是 ASP.NET Core 第一方 OpenAPI 路线。
- `AddOpenApiDocument` / `UseSwaggerUi` 是 NSwag 路线。
- 初学时二选一即可。

推荐本课选择：

```text
先用 AddOpenApi + MapOpenApi 理解 OpenAPI。
需要可视化页面时，再加 NSwag 或 Swashbuckle。
```

## 第 12 步：给 endpoint 增加 OpenAPI 元数据

Minimal API 可以加名称、标签、摘要和描述。

示例：

```csharp
group.MapPost("/", CreatePlayer)
    .WithName("CreatePlayer")
    .WithSummary("新增玩家")
    .WithDescription("创建一个新玩家，并保存到 JSON 文件。")
    .Produces(StatusCodes.Status201Created)
    .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);
```

查询玩家：

```csharp
group.MapGet("/{id:guid}", GetPlayerById)
    .WithName("GetPlayerById")
    .WithSummary("按 ID 查询玩家")
    .Produces<PlayerSummaryDto>(StatusCodes.Status200OK)
    .Produces<ErrorResponse>(StatusCodes.Status404NotFound);
```

这些信息会进入 OpenAPI 文档，让接口说明更清晰。

## 第 13 步：使用 `ILogger`

ASP.NET Core 内置日志系统。

常见写法：

```csharp
private static IResult GetPlayers(
    PlayerApplication playerApp,
    ILoggerFactory loggerFactory)
{
    ILogger logger = loggerFactory.CreateLogger("PlayerEndpoints");

    logger.LogInformation("Players requested");

    return Results.Ok(...);
}
```

也可以在类中使用：

```csharp
ILogger<PlayerApplication>
```

但 Minimal API 的静态 endpoint 方法里，使用 `ILoggerFactory` 比较直接。

日志级别：

| 级别 | 场景 |
| --- | --- |
| `Trace` | 极细调试信息 |
| `Debug` | 调试信息 |
| `Information` | 正常关键流程 |
| `Warning` | 可恢复问题或业务失败 |
| `Error` | 异常或操作失败 |
| `Critical` | 严重故障 |

示例：

```csharp
logger.LogInformation("Player created. PlayerId: {PlayerId}", player.Id);
logger.LogWarning("Create player failed. Error: {Error}", result.ErrorMessage);
logger.LogError(ex, "Save player data failed");
```

## 第 14 步：结构化日志

推荐：

```csharp
logger.LogInformation("Player created. PlayerId: {PlayerId}, Name: {PlayerName}",
    player.Id,
    player.Name);
```

不推荐：

```csharp
logger.LogInformation($"Player created. PlayerId: {player.Id}, Name: {player.Name}");
```

结构化日志的好处：

- 日志系统可以把 `PlayerId` 当字段处理。
- 后续接入日志平台更容易查询。
- 性能和格式控制更好。

一句话：

```text
日志模板用占位符，不要直接字符串插值。
```

## 第 15 步：配置日志级别

在 `appsettings.Development.json` 中可以配置：

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

含义：

- 默认日志级别是 `Information`。
- ASP.NET Core 框架日志只显示 `Warning` 及以上，避免太吵。

开发阶段可以保持：

```text
Information
```

生产阶段通常会更谨慎地配置日志级别。

## 第 16 步：统一异常日志

启动加载数据时：

```csharp
try
{
    using IServiceScope scope = app.Services.CreateScope();
    PlayerApplication playerApp = scope.ServiceProvider.GetRequiredService<PlayerApplication>();
    await playerApp.LoadAsync();
}
catch (PlayerStorageException ex)
{
    app.Logger.LogError(ex, "Load player data failed");
}
```

接口保存数据时：

```csharp
try
{
    await playerApp.SaveAsync();
}
catch (PlayerStorageException ex)
{
    logger.LogError(ex, "Save player data failed");

    return Results.Problem(
        title: "保存玩家数据失败",
        statusCode: StatusCodes.Status500InternalServerError);
}
```

业务失败：

```csharp
logger.LogWarning("Create player failed. Error: {Error}", result.ErrorMessage);
```

系统异常：

```csharp
logger.LogError(ex, "Save player data failed");
```

这和课程七的思想一致：

```text
业务失败用 Result，系统异常用 exception + log。
```

## 第 17 步：使用 `ProblemDetails`

ASP.NET Core 支持标准化错误响应 `ProblemDetails`。

简单返回：

```csharp
return Results.Problem(
    title: "保存玩家数据失败",
    detail: "请稍后重试",
    statusCode: StatusCodes.Status500InternalServerError);
```

输出类似：

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.6.1",
  "title": "保存玩家数据失败",
  "status": 500,
  "detail": "请稍后重试"
}
```

本课建议：

- 业务错误继续用 `ErrorResponse`。
- 未预期异常或系统错误用 `Results.Problem`。

后续可以统一成全局异常处理。

## 第 18 步：整理后的 `Program.cs` 完整示例

```csharp
using gameC_.Exceptions;
using gameC_.Services;
using GamePlayerSystem.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddSingleton<PlayerManager>();
builder.Services.AddSingleton<IPlayerStorage>(
    _ => new JsonPlayerStorage(Path.Combine("data", "players.json")));
builder.Services.AddSingleton<PlayerApplication>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

try
{
    using IServiceScope scope = app.Services.CreateScope();
    PlayerApplication playerApp = scope.ServiceProvider.GetRequiredService<PlayerApplication>();
    await playerApp.LoadAsync();

    app.Logger.LogInformation("Player data loaded");
}
catch (PlayerStorageException ex)
{
    app.Logger.LogError(ex, "Load player data failed");
}

app.MapGet("/", () => Results.Ok(new
{
    name = "Game Player API",
    status = "running"
}))
.WithTags("Health")
.WithSummary("服务健康检查");

app.MapPlayerEndpoints();
app.MapRankingEndpoints();
app.MapStatsEndpoints();

app.Run();
```

## 第 19 步：完整练习

练习目标：整理 Minimal API 项目结构，增加 OpenAPI 和日志。

要求：

1. 创建目录：

```text
Endpoints/
Responses/
```

2. 创建文件：

```text
Endpoints/PlayerEndpoints.cs
Endpoints/RankingEndpoints.cs
Endpoints/StatsEndpoints.cs
Responses/ErrorResponse.cs
```

3. 把 `/players` 相关接口移动到 `PlayerEndpoints`。
4. 把 `/rankings` 相关接口移动到 `RankingEndpoints`。
5. 把 `/stats` 相关接口移动到 `StatsEndpoints`。
6. 使用 `MapGroup` 分组。
7. 每组使用 `.WithTags(...)`。
8. 增加 `builder.Services.AddOpenApi()`。
9. 开发环境启用 `app.MapOpenApi()`。
10. 统一错误响应使用 `ErrorResponse`。
11. 使用 `ILogger` 或 `ILoggerFactory` 记录：
    - 新增玩家成功。
    - 新增玩家失败。
    - 删除玩家成功。
    - 删除玩家失败。
    - 查询排行榜。
    - 保存或加载异常。

验收标准：

- `Program.cs` 明显变短。
- API 可以正常启动。
- 原有接口仍然可用。
- `/openapi/v1.json` 可以访问。
- 业务失败返回统一错误格式。
- 控制台可以看到关键日志。

## 第 20 步：本课作业

### 作业 1：给接口补充 OpenAPI 元数据

给每个 endpoint 添加：

```csharp
.WithName(...)
.WithSummary(...)
.Produces(...)
```

至少覆盖：

- `GET /players`
- `GET /players/{id}`
- `POST /players`
- `DELETE /players/{id}`
- `GET /rankings/top`

### 作业 2：增加 Swagger UI

任选 NSwag 或 Swashbuckle。

如果选择 NSwag：

```powershell
dotnet add GamePlayerSystem.Api package NSwag.AspNetCore
```

并配置：

```csharp
builder.Services.AddOpenApiDocument();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}
```

要求：

- 浏览器能打开 Swagger UI。
- 能在页面上测试 `POST /players`。

### 作业 3：增强错误响应

把 `ErrorResponse` 改成：

```csharp
public sealed class ErrorResponse
{
    public string Error { get; init; } = string.Empty;
    public string? ErrorCode { get; init; }
    public string? TraceId { get; init; }
}
```

要求：

- 业务错误带 `ErrorCode`。
- `TraceId` 可暂时为空。

### 作业 4：日志分类

把字符串 logger：

```csharp
loggerFactory.CreateLogger("PlayerEndpoints");
```

改成类型 logger。

提示：

```csharp
ILogger<PlayerEndpointLogger>
```

可以创建一个空类型专门作为日志分类：

```csharp
public sealed class PlayerEndpointLogger
{
}
```

也可以继续保留字符串分类，本作业重点是理解日志 category。

## 本课常见错误

### 1. Endpoints 拆分后忘记 `using`

如果 `Program.cs` 找不到：

```csharp
app.MapPlayerEndpoints();
```

检查是否添加：

```csharp
using GamePlayerSystem.Api.Endpoints;
```

### 2. 扩展方法不是 `public static`

错误：

```csharp
class PlayerEndpoints
{
    RouteGroupBuilder MapPlayerEndpoints(...)
}
```

推荐：

```csharp
public static class PlayerEndpoints
{
    public static RouteGroupBuilder MapPlayerEndpoints(this IEndpointRouteBuilder app)
}
```

### 3. 混淆 OpenAPI 和 Swagger UI

记住：

- OpenAPI 是接口描述规范。
- Swagger UI 是可视化页面工具。
- `.NET` 第一方可以生成 OpenAPI 文档。
- Swagger UI 通常来自第三方包。

### 4. 日志用字符串插值

不推荐：

```csharp
logger.LogInformation($"Player created: {player.Id}");
```

推荐：

```csharp
logger.LogInformation("Player created: {PlayerId}", player.Id);
```

### 5. 错误响应格式到处不一致

不推荐：

```json
{ "error": "..." }
{ "message": "..." }
{ "msg": "..." }
```

推荐统一：

```json
{ "error": "..." }
```

或者统一使用 `ProblemDetails`。

## 本课复盘问题

学完后，尝试回答：

1. 为什么要拆分 Minimal API Endpoints？
2. `MapGroup` 解决什么问题？
3. `.WithTags` 对 OpenAPI 文档有什么帮助？
4. OpenAPI 和 Swagger 有什么区别？
5. `AddOpenApi` 和 `MapOpenApi` 分别做什么？
6. 为什么需要统一错误响应？
7. `ILogger` 有哪些常用日志级别？
8. 为什么推荐结构化日志？
9. 业务失败和系统异常应该分别怎么记录？
10. `Program.cs` 最理想应该负责哪些事情？

## 下一课预告

课程十四建议学习：

- 配置系统与 Options 模式。
- 把 JSON 文件路径移到 `appsettings.json`。
- 创建 `PlayerStorageOptions`。
- 使用 `IOptions<T>` 注入配置。
- 避免路径和参数硬编码。

## 参考资料

- Microsoft Learn: Route handlers in Minimal API apps
  - https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/route-handlers
- Microsoft Learn: OpenAPI support in ASP.NET Core API apps
  - https://learn.microsoft.com/aspnet/core/fundamentals/openapi/overview
- Microsoft Learn: Logging in .NET and ASP.NET Core
  - https://learn.microsoft.com/aspnet/core/fundamentals/logging/
- Microsoft Learn: Create responses in Minimal API applications
  - https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/responses
