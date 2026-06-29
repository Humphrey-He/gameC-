# C# 基础学习专刊：课程十二

## 课程主题

ASP.NET Core Web API 入门：创建 Minimal API，注册服务到 DI 容器，并提供玩家新增、查询、排行榜接口。

前面十一课中，我们已经把玩家管理系统拆出了比较清晰的结构：

- `Player`
- `PlayerManager`
- `IPlayerStorage`
- `JsonPlayerStorage`
- `InMemoryPlayerStorage`
- `PlayerApplication`
- `Result`
- DTO
- 异步保存和加载

课程十二开始进入 Web API。目标是把控制台菜单升级成 HTTP 接口。

以前是用户在控制台输入：

```text
1. 新增玩家
2. 查询玩家
5. 显示排行榜
```

现在改成 HTTP 请求：

```text
POST /players
GET  /players/{id}
GET  /rankings/top?count=10
```

## 本课目标

完成本课后，你应该能做到：

- 理解什么是 Web API。
- 理解 Minimal API 的基本结构。
- 创建 ASP.NET Core Web 项目。
- 使用 `MapGet`、`MapPost` 定义接口。
- 把服务注册到 DI 容器。
- 在接口中注入 `PlayerApplication`。
- 创建请求 DTO。
- 返回合适的 HTTP 状态码。
- 提供玩家新增、查询、列表、排行榜接口。
- 理解控制台项目和 Web API 项目的差异。

## 第 1 步：什么是 Web API

Web API 是通过 HTTP 暴露功能的一种方式。

控制台程序是人通过键盘操作：

```text
输入命令 -> 程序执行 -> 控制台输出
```

Web API 是客户端通过 HTTP 请求操作：

```text
发送 HTTP 请求 -> 服务端执行 -> 返回 HTTP 响应
```

例如新增玩家：

```http
POST /players
Content-Type: application/json

{
  "name": "Alice",
  "level": 10,
  "region": "CN",
  "gold": 500
}
```

服务端返回：

```http
201 Created
```

查询玩家：

```http
GET /players/95b82b25-8df5-4b5e-84ef-b37cf64ba839
```

服务端返回：

```json
{
  "playerId": "95b82b25-8df5-4b5e-84ef-b37cf64ba839",
  "name": "Alice",
  "regionName": "国服",
  "level": 10,
  "isActive": true
}
```

## 第 2 步：什么是 Minimal API

ASP.NET Core 有两种常见 API 写法：

- Minimal API
- Controller API

Minimal API 代码更少，适合入门、小型服务、微服务和快速构建 HTTP API。

最小示例：

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
```

这段代码定义了一个接口：

```text
GET /
```

访问后返回：

```text
Hello World!
```

Microsoft 官方文档中也把 Minimal API 描述为构建 HTTP API 的简化方式。

## 第 3 步：创建 Web API 项目

建议先保留现有控制台学习代码，再新建一个 Web API 项目。

如果你已经准备把当前项目直接改成 Web 项目，也可以改 `.csproj`，但初学阶段更推荐新建项目。

在当前项目根目录执行：

```powershell
dotnet new web -o GamePlayerSystem.Api
```

目录大致是：

```text
GamePlayerSystem.Api/
  GamePlayerSystem.Api.csproj
  Program.cs
  appsettings.json
```

运行：

```powershell
dotnet run --project GamePlayerSystem.Api
```

启动后会看到类似：

```text
Now listening on: http://localhost:xxxx
```

浏览器访问：

```text
http://localhost:xxxx
```

如果 `Program.cs` 里有 `MapGet("/")`，就能看到返回内容。

## 第 4 步：Web 项目的 `Program.cs`

Minimal API 的 `Program.cs` 通常长这样：

```csharp
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => "Game Player API");

app.Run();
```

流程：

```text
创建 builder -> 注册服务 -> Build app -> 定义路由 -> Run
```

后面我们会在 `builder.Services` 中注册依赖：

```csharp
builder.Services.AddSingleton<PlayerManager>();
builder.Services.AddSingleton<IPlayerStorage>(...);
builder.Services.AddSingleton<PlayerApplication>();
```

## 第 5 步：项目引用

如果 Web API 项目要使用原来的类，需要让 API 项目引用主项目。

假设主项目是：

```text
gameC#.csproj
```

API 项目是：

```text
GamePlayerSystem.Api/GamePlayerSystem.Api.csproj
```

执行：

```powershell
dotnet add GamePlayerSystem.Api/GamePlayerSystem.Api.csproj reference gameC#.csproj
```

然后 API 项目就可以使用：

```csharp
using gameC_.Models;
using gameC_.Services;
using gameC_.Common;
using gameC_.Dtos;
```

注意：

如果主项目还是控制台 `Exe`，后续更规范的做法是把核心代码拆到类库项目中，例如：

```text
GamePlayerSystem.Core
```

本课先不强制重构，重点理解 Web API 流程。

## 第 6 步：注册服务到 DI 容器

课程十一中我们手动注入：

```csharp
PlayerManager manager = new PlayerManager();
IPlayerStorage storage = new JsonPlayerStorage(filePath);
PlayerApplication app = new PlayerApplication(manager, storage);
```

在 ASP.NET Core 中，可以注册到 DI 容器：

```csharp
builder.Services.AddSingleton<PlayerManager>();

builder.Services.AddSingleton<IPlayerStorage>(
    _ => new JsonPlayerStorage(Path.Combine("data", "players.json")));

builder.Services.AddSingleton<PlayerApplication>();
```

含义：

- `PlayerManager` 用单例。
- `IPlayerStorage` 对应 `JsonPlayerStorage`。
- `PlayerApplication` 由 DI 容器创建。

当接口需要 `PlayerApplication` 时，框架会自动把依赖传进去。

## 第 7 步：服务生命周期选择

当前项目里可以先用：

```csharp
AddSingleton
```

原因：

- 玩家数据目前保存在内存字典中。
- 希望整个服务运行期间共享同一份玩家数据。
- JSON 文件存储也可以作为单例使用。

但要知道真实 Web 项目中：

- 数据库上下文通常用 `Scoped`。
- 无状态轻量服务可以用 `Transient`。
- 全局共享缓存或配置服务可能用 `Singleton`。

本课先用 `Singleton`，后面接数据库时再调整。

## 第 8 步：创建请求 DTO

Web API 不建议直接把 `Player` 当请求体。

新增玩家请求可以创建：

```text
Dtos/
  CreatePlayerRequest.cs
```

代码：

```csharp
namespace gameC_.Dtos;

public sealed class CreatePlayerRequest
{
    public string Name { get; init; } = string.Empty;
    public int Level { get; init; }
    public string Region { get; init; } = "CN";
    public int Gold { get; init; }
}
```

原因：

- 请求 DTO 表示客户端允许提交什么。
- `Player` 是业务实体，不应该完全暴露给客户端。
- 客户端不应该随便提交 `Id`、`IsActive` 等字段。

## 第 9 步：根接口

先写一个健康检查接口：

```csharp
app.MapGet("/", () => TypedResults.Ok(new
{
    name = "Game Player API",
    status = "running"
}));
```

访问：

```text
GET /
```

返回：

```json
{
  "name": "Game Player API",
  "status": "running"
}
```

`TypedResults.Ok(...)` 表示返回 HTTP 200。

## 第 10 步：启动时加载数据

Web 服务启动时，可以从 JSON 加载玩家数据。

```csharp
var app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    PlayerApplication playerApp = scope.ServiceProvider
        .GetRequiredService<PlayerApplication>();

    await playerApp.LoadAsync();
}
```

注意：

- `app.Services` 是服务容器。
- `GetRequiredService<T>()` 获取服务。
- 启动时加载失败要根据项目需求处理。

简单版本：

```csharp
try
{
    using IServiceScope scope = app.Services.CreateScope();
    PlayerApplication playerApp = scope.ServiceProvider.GetRequiredService<PlayerApplication>();
    await playerApp.LoadAsync();
}
catch (PlayerStorageException ex)
{
    Console.WriteLine(ex.Message);
}
```

真实生产项目中应该使用日志，而不是只写 `Console.WriteLine`。

## 第 11 步：新增玩家接口

接口：

```text
POST /players
```

代码：

```csharp
app.MapPost("/players", async (
    CreatePlayerRequest request,
    PlayerApplication playerApp) =>
{
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
        return TypedResults.BadRequest(new
        {
            error = result.ErrorMessage
        });
    }

    await playerApp.SaveAsync();

    return TypedResults.Created($"/players/{player.Id}", new
    {
        player.Id,
        player.Name,
        player.Level,
        player.Region,
        player.Gold,
        player.IsActive
    });
});
```

说明：

- `CreatePlayerRequest` 从请求 JSON 自动绑定。
- `PlayerApplication` 从 DI 容器自动注入。
- 新增失败返回 `400 Bad Request`。
- 新增成功返回 `201 Created`。
- 成功后保存数据。

示例请求：

```http
POST /players
Content-Type: application/json

{
  "name": "Alice",
  "level": 10,
  "region": "CN",
  "gold": 500
}
```

## 第 12 步：查询全部玩家接口

接口：

```text
GET /players
```

代码：

```csharp
app.MapGet("/players", (PlayerApplication playerApp) =>
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

    return TypedResults.Ok(players);
});
```

这里为了演示，直接在接口里做了 DTO 转换。

更推荐后续把它移动到：

```csharp
PlayerApplication.GetPlayerSummaries()
```

或者：

```csharp
PlayerManager.GetPlayerSummaries()
```

接口层越薄越好。

## 第 13 步：按 ID 查询玩家接口

接口：

```text
GET /players/{id}
```

代码：

```csharp
app.MapGet("/players/{id:guid}", (Guid id, PlayerApplication playerApp) =>
{
    Result<Player> result = playerApp.GetPlayer(id);

    if (result.IsFailure)
    {
        return Results.NotFound(new
        {
            error = result.ErrorMessage
        });
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
});
```

这里使用：

```text
{id:guid}
```

表示路由参数必须是 Guid。

如果传入的不是 Guid，路由不会匹配。

注意：

这里用了 `Results.Ok` / `Results.NotFound`，因为两个分支返回的具体类型不同，写起来更简单。

后续可以学习更严格的 typed result 写法。

## 第 14 步：排行榜接口

接口：

```text
GET /rankings/top?count=10
```

代码：

```csharp
app.MapGet("/rankings/top", (int? count, PlayerApplication playerApp) =>
{
    int rankingCount = count.GetValueOrDefault(10);

    if (rankingCount <= 0)
    {
        return Results.BadRequest(new
        {
            error = "count 必须大于 0"
        });
    }

    List<RankingPlayerDto> ranking = playerApp.GetRanking(rankingCount);

    return Results.Ok(ranking);
});
```

访问：

```text
GET /rankings/top?count=5
```

如果不传 `count`：

```text
GET /rankings/top
```

默认返回 10 条。

## 第 15 步：删除玩家接口

接口：

```text
DELETE /players/{id}
```

代码：

```csharp
app.MapDelete("/players/{id:guid}", async (
    Guid id,
    PlayerApplication playerApp) =>
{
    Result result = playerApp.RemoveById(id);

    if (result.IsFailure)
    {
        return Results.NotFound(new
        {
            error = result.ErrorMessage
        });
    }

    await playerApp.SaveAsync();

    return Results.NoContent();
});
```

成功返回：

```text
204 No Content
```

删除不存在玩家返回：

```text
404 Not Found
```

## 第 16 步：禁用玩家接口

接口：

```text
POST /players/{id}/disable
```

代码：

```csharp
app.MapPost("/players/{id:guid}/disable", async (
    Guid id,
    PlayerApplication playerApp) =>
{
    Result result = playerApp.DisableById(id);

    if (result.IsFailure)
    {
        return Results.BadRequest(new
        {
            error = result.ErrorMessage
        });
    }

    await playerApp.SaveAsync();

    return Results.NoContent();
});
```

如果玩家不存在或已经禁用，都返回业务失败。

你也可以把“玩家不存在”映射成 404，把“已禁用”映射成 400。后续可以通过错误码进一步优化。

## 第 17 步：区服统计接口

接口：

```text
GET /stats/regions
```

代码：

```csharp
app.MapGet("/stats/regions", (PlayerApplication playerApp) =>
{
    return TypedResults.Ok(playerApp.GetRegionStats());
});
```

返回：

```json
[
  {
    "region": "CN",
    "regionName": "国服",
    "playerCount": 3,
    "activePlayerCount": 2
  }
]
```

## 第 18 步：完整 `Program.cs` 示例

```csharp
using gameC_.Common;
using gameC_.Dtos;
using gameC_.Exceptions;
using gameC_.Models;
using gameC_.Services;

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
    Console.WriteLine(ex.Message);
}

app.MapGet("/", () => TypedResults.Ok(new
{
    name = "Game Player API",
    status = "running"
}));

app.MapGet("/players", (PlayerApplication playerApp) =>
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

    return TypedResults.Ok(players);
});

app.MapGet("/players/{id:guid}", (Guid id, PlayerApplication playerApp) =>
{
    Result<Player> result = playerApp.GetPlayer(id);

    if (result.IsFailure)
    {
        return Results.NotFound(new
        {
            error = result.ErrorMessage
        });
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
});

app.MapPost("/players", async (
    CreatePlayerRequest request,
    PlayerApplication playerApp) =>
{
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
        return Results.BadRequest(new
        {
            error = result.ErrorMessage
        });
    }

    await playerApp.SaveAsync();

    return Results.Created($"/players/{player.Id}", new
    {
        player.Id,
        player.Name,
        player.Level,
        player.Region,
        player.Gold,
        player.IsActive
    });
});

app.MapDelete("/players/{id:guid}", async (
    Guid id,
    PlayerApplication playerApp) =>
{
    Result result = playerApp.RemoveById(id);

    if (result.IsFailure)
    {
        return Results.NotFound(new
        {
            error = result.ErrorMessage
        });
    }

    await playerApp.SaveAsync();

    return Results.NoContent();
});

app.MapPost("/players/{id:guid}/disable", async (
    Guid id,
    PlayerApplication playerApp) =>
{
    Result result = playerApp.DisableById(id);

    if (result.IsFailure)
    {
        return Results.BadRequest(new
        {
            error = result.ErrorMessage
        });
    }

    await playerApp.SaveAsync();

    return Results.NoContent();
});

app.MapGet("/rankings/top", (int? count, PlayerApplication playerApp) =>
{
    int rankingCount = count.GetValueOrDefault(10);

    if (rankingCount <= 0)
    {
        return Results.BadRequest(new
        {
            error = "count 必须大于 0"
        });
    }

    return Results.Ok(playerApp.GetRanking(rankingCount));
});

app.MapGet("/stats/regions", (PlayerApplication playerApp) =>
{
    return TypedResults.Ok(playerApp.GetRegionStats());
});

app.Run();
```

## 第 19 步：使用 HTTP 文件测试接口

可以创建：

```text
GamePlayerSystem.Api/GamePlayerSystem.Api.http
```

内容：

```http
@host = http://localhost:5000

GET {{host}}/

###

POST {{host}}/players
Content-Type: application/json

{
  "name": "Alice",
  "level": 10,
  "region": "CN",
  "gold": 500
}

###

GET {{host}}/players

###

GET {{host}}/rankings/top?count=10

###

GET {{host}}/stats/regions
```

如果端口不是 5000，改成实际启动端口。

也可以使用：

- 浏览器测试 GET。
- Postman。
- curl。
- Rider / Visual Studio / VS Code 的 HTTP Client。

## 第 20 步：用 curl 测试

新增玩家：

```powershell
curl -Method POST http://localhost:5000/players `
  -ContentType "application/json" `
  -Body '{"name":"Alice","level":10,"region":"CN","gold":500}'
```

查询全部玩家：

```powershell
curl http://localhost:5000/players
```

查询排行榜：

```powershell
curl http://localhost:5000/rankings/top?count=10
```

注意：

PowerShell 的 `curl` 实际上常常是 `Invoke-WebRequest` 的别名，参数写法和 Linux curl 不完全一样。

## 第 21 步：HTTP 状态码基础

常用状态码：

| 状态码 | 含义 | 场景 |
| --- | --- | --- |
| 200 | OK | 查询成功 |
| 201 | Created | 新增成功 |
| 204 | No Content | 删除成功，无响应体 |
| 400 | Bad Request | 请求参数错误或业务校验失败 |
| 404 | Not Found | 资源不存在 |
| 500 | Internal Server Error | 服务端未知错误 |

本项目建议：

- 新增成功：`201 Created`
- 查询成功：`200 OK`
- 删除成功：`204 No Content`
- 参数错误：`400 Bad Request`
- 玩家不存在：`404 Not Found`

## 第 22 步：接口层应该保持薄

Minimal API 里可以直接写很多逻辑，但不要把业务全部堆在路由里。

不推荐：

```csharp
app.MapGet("/rankings/top", (...) =>
{
    // 大量 LINQ
    // 大量校验
    // 大量业务规则
    // 大量文件读写
});
```

推荐：

```csharp
app.MapGet("/rankings/top", (int? count, PlayerApplication app) =>
{
    return Results.Ok(app.GetRanking(count.GetValueOrDefault(10)));
});
```

接口层负责：

- 接收请求。
- 做基础参数校验。
- 调用应用服务。
- 转换 HTTP 响应。

业务规则放在：

- `PlayerManager`
- `PlayerApplication`

存储细节放在：

- `IPlayerStorage`
- `JsonPlayerStorage`

## 第 23 步：完整练习

练习目标：把玩家管理系统升级成 Minimal API。

要求：

1. 创建 Web 项目：

```powershell
dotnet new web -o GamePlayerSystem.Api
```

2. 引用原项目：

```powershell
dotnet add GamePlayerSystem.Api/GamePlayerSystem.Api.csproj reference gameC#.csproj
```

3. 创建 `CreatePlayerRequest`。
4. 在 `Program.cs` 中注册：

```csharp
PlayerManager
IPlayerStorage
PlayerApplication
```

5. 启动时加载 JSON 数据。
6. 提供接口：

```text
GET    /
GET    /players
GET    /players/{id}
POST   /players
DELETE /players/{id}
POST   /players/{id}/disable
GET    /rankings/top?count=10
GET    /stats/regions
```

7. 新增、删除、禁用后保存 JSON。
8. 使用合适 HTTP 状态码。

验收标准：

- API 项目能启动。
- `GET /` 返回运行状态。
- `POST /players` 可以新增玩家。
- `GET /players` 可以看到新增玩家。
- `GET /rankings/top` 可以看到排行榜。
- 删除不存在玩家时返回 404。
- 新增重名玩家时返回 400。

## 第 24 步：本课作业

### 作业 1：增加修改玩家接口

新增：

```text
PUT /players/{id}
```

请求 DTO：

```csharp
public sealed class UpdatePlayerRequest
{
    public string Name { get; init; } = string.Empty;
    public int Level { get; init; }
    public string Region { get; init; } = "CN";
    public int Gold { get; init; }
}
```

要求：

- 玩家不存在返回 404。
- 修改成功返回 204。
- 修改后保存 JSON。

### 作业 2：增加按区服查询

新增：

```text
GET /players/by-region/{region}
```

要求：

- 返回指定区服玩家。
- 只返回活跃玩家。
- 按等级降序。

### 作业 3：增加错误响应 DTO

创建：

```csharp
public sealed class ErrorResponse
{
    public string Error { get; init; } = string.Empty;
}
```

把匿名错误对象：

```csharp
new { error = result.ErrorMessage }
```

替换成：

```csharp
new ErrorResponse { Error = result.ErrorMessage }
```

### 作业 4：整理 Endpoint

当 `Program.cs` 变长后，思考是否可以拆成：

```text
Endpoints/
  PlayerEndpoints.cs
  RankingEndpoints.cs
```

本课可以只写设计说明，下一阶段再实现。

## 本课常见错误

### 1. 忘记注册服务

如果接口中写：

```csharp
(PlayerApplication playerApp) => ...
```

但没有注册：

```csharp
builder.Services.AddSingleton<PlayerApplication>();
```

运行时会报依赖解析失败。

### 2. 接口直接依赖具体存储

不推荐：

```csharp
app.MapPost("/players", (JsonPlayerStorage storage) => ...)
```

推荐：

```csharp
builder.Services.AddSingleton<IPlayerStorage>(
    _ => new JsonPlayerStorage(...));
```

接口层依赖 `PlayerApplication`，存储细节藏在服务内部。

### 3. 路由里写太多业务逻辑

如果一个 `MapPost` 里超过几十行，要考虑拆到服务中。

接口层应该薄。

### 4. 忘记保存数据

新增、删除、禁用后，如果不调用：

```csharp
await playerApp.SaveAsync();
```

重启服务后数据会丢失。

### 5. 不区分 400 和 404

建议：

- 请求参数或业务校验失败：400
- 资源不存在：404

例如：

- 重名：400
- `count <= 0`：400
- 玩家 ID 不存在：404

## 本课复盘问题

学完后，尝试回答：

1. Web API 和控制台程序有什么区别？
2. Minimal API 的基本结构是什么？
3. `WebApplication.CreateBuilder(args)` 是做什么的？
4. `builder.Services` 是做什么的？
5. `MapGet` 和 `MapPost` 分别用于什么？
6. 为什么请求体建议使用 Request DTO？
7. 为什么接口层不应该写太多业务逻辑？
8. `201 Created` 适合什么场景？
9. 玩家不存在应该返回 400 还是 404？
10. ASP.NET Core 如何把 `PlayerApplication` 注入到接口里？

## 下一课预告

课程十三建议学习：

- Web API 项目规范。
- 拆分 Minimal API Endpoints。
- 增加 Swagger / OpenAPI。
- 统一错误响应。
- 使用日志 `ILogger`。
- 为后续数据库和真实项目结构做准备。

## 参考资料

- Microsoft Learn: Minimal APIs quick reference
  - https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis
- Microsoft Learn: Tutorial: Create a Minimal API with ASP.NET Core
  - https://learn.microsoft.com/aspnet/core/tutorials/min-web-api
- Microsoft Learn: Dependency injection in ASP.NET Core
  - https://learn.microsoft.com/aspnet/core/fundamentals/dependency-injection
- Microsoft Learn: Create responses in Minimal API applications
  - https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/responses
