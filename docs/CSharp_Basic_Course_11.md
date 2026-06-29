# C# 基础学习专刊：课程十一

## 课程主题

依赖注入基础：抽象 `IPlayerStorage`，让 `PlayerManager` 和存储实现解耦，并为 ASP.NET Core Web API 做准备。

前面课程中，我们已经有了：

- `PlayerManager`：负责玩家管理。
- `PlayerStorage`：负责 JSON 文件保存和加载。
- `PlayerStorageException`：负责表达存储异常。
- `SaveAsync` / `LoadAsync`：异步保存和加载。

现在的问题是：如果未来不想用 JSON 文件，而是改成 SQLite、MySQL、Redis 或 Web API，业务代码会不会被迫大改？

课程十一的目标是学习依赖注入的基础思想：业务代码依赖抽象，而不是直接依赖具体实现。

## 本课目标

完成本课后，你应该能做到：

- 理解什么是依赖。
- 理解什么是依赖注入。
- 理解接口的作用。
- 创建 `IPlayerStorage` 接口。
- 让 `PlayerStorage` 实现 `IPlayerStorage`。
- 使用构造函数注入依赖。
- 理解“依赖抽象，不依赖具体类”。
- 为后续 ASP.NET Core 的内置依赖注入容器做准备。

## 第 1 步：什么是依赖

一个类需要另一个类才能工作，这就是依赖。

例如：

```csharp
PlayerStorage storage = new PlayerStorage("data/players.json");
```

如果 `Program.cs` 要保存玩家，就依赖 `PlayerStorage`。

如果某个服务内部直接写：

```csharp
private readonly PlayerStorage _storage = new PlayerStorage("data/players.json");
```

那么这个服务就强依赖 JSON 文件存储。

问题是：

- 换成数据库时要改这个服务。
- 测试时不好替换成假的存储。
- 类和类之间耦合变高。

## 第 2 步：什么是解耦

解耦不是让类之间完全没关系，而是减少对具体实现的依赖。

强耦合写法：

```csharp
PlayerStorage storage = new PlayerStorage("data/players.json");
```

更灵活的写法：

```csharp
IPlayerStorage storage = new JsonPlayerStorage("data/players.json");
```

业务代码只知道：

```text
我需要一个能保存和加载玩家数据的东西。
```

但它不关心这个东西是：

- JSON 文件。
- SQLite 数据库。
- MySQL 数据库。
- 内存假实现。
- 远程 HTTP 服务。

这就是接口的价值。

## 第 3 步：什么是接口

接口定义“能做什么”，不定义“怎么做”。

示例：

```csharp
public interface IPlayerStorage
{
    Task SaveAsync(IEnumerable<Player> players);

    Task<List<Player>> LoadAsync();
}
```

这表示任何实现了 `IPlayerStorage` 的类，都必须提供：

- 保存玩家。
- 加载玩家。

但接口不关心数据存在哪里。

## 第 4 步：创建 `IPlayerStorage`

推荐目录：

```text
Services/
  IPlayerStorage.cs
  JsonPlayerStorage.cs
```

接口代码：

```csharp
using gameC_.Models;

namespace gameC_.Services;

public interface IPlayerStorage
{
    Task SaveAsync(
        IEnumerable<Player> players,
        CancellationToken cancellationToken = default);

    Task<List<Player>> LoadAsync(
        CancellationToken cancellationToken = default);
}
```

说明：

- 接口名以 `I` 开头，这是 C# 常见规范。
- 方法保持 `Async` 结尾。
- 加入 `CancellationToken`，为异步取消做准备。
- 接口只描述能力，不写具体逻辑。

## 第 5 步：把 `PlayerStorage` 改名为 `JsonPlayerStorage`

课程十中的类叫：

```csharp
PlayerStorage
```

现在建议改成：

```csharp
JsonPlayerStorage
```

原因：

```text
PlayerStorage 这个名字太泛，不说明它是 JSON 文件存储。
JsonPlayerStorage 明确表达：这是基于 JSON 的玩家存储。
```

文件：

```text
Services/JsonPlayerStorage.cs
```

代码：

```csharp
using System.Text.Json;
using gameC_.Exceptions;
using gameC_.Models;

namespace gameC_.Services;

public sealed class JsonPlayerStorage : IPlayerStorage
{
    private readonly string _filePath;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public JsonPlayerStorage(string filePath)
    {
        _filePath = filePath;
    }

    public async Task SaveAsync(
        IEnumerable<Player> players,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string? directory = Path.GetDirectoryName(_filePath);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonSerializer.Serialize(players, _jsonOptions);

            await File.WriteAllTextAsync(_filePath, json, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            throw new PlayerStorageException("保存玩家数据失败", ex);
        }
    }

    public async Task<List<Player>> LoadAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                return new List<Player>();
            }

            string json = await File.ReadAllTextAsync(_filePath, cancellationToken);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<Player>();
            }

            List<Player>? players = JsonSerializer.Deserialize<List<Player>>(json, _jsonOptions);

            return players ?? new List<Player>();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (JsonException ex)
        {
            throw new PlayerStorageException("玩家数据文件格式错误", ex);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new PlayerStorageException("读取玩家数据失败", ex);
        }
    }
}
```

重点：

```csharp
public sealed class JsonPlayerStorage : IPlayerStorage
```

这表示 `JsonPlayerStorage` 实现了 `IPlayerStorage`。

## 第 6 步：接口变量可以接收实现类

因为 `JsonPlayerStorage` 实现了 `IPlayerStorage`，所以可以这样写：

```csharp
IPlayerStorage storage = new JsonPlayerStorage("data/players.json");
```

调用时：

```csharp
List<Player> players = await storage.LoadAsync();
await storage.SaveAsync(players);
```

调用方只依赖接口：

```text
IPlayerStorage
```

而不是具体类：

```text
JsonPlayerStorage
```

这就是面向接口编程。

## 第 7 步：什么是依赖注入

依赖注入就是：

```text
类需要什么依赖，不在类内部 new，而是从外部传进来。
```

不推荐：

```csharp
public sealed class PlayerApplication
{
    private readonly JsonPlayerStorage _storage = new JsonPlayerStorage("data/players.json");
}
```

推荐：

```csharp
public sealed class PlayerApplication
{
    private readonly IPlayerStorage _storage;

    public PlayerApplication(IPlayerStorage storage)
    {
        _storage = storage;
    }
}
```

外部创建：

```csharp
IPlayerStorage storage = new JsonPlayerStorage("data/players.json");
PlayerApplication app = new PlayerApplication(storage);
```

这样 `PlayerApplication` 不关心具体存储实现。

## 第 8 步：构造函数注入

最常见的依赖注入方式是构造函数注入。

示例：

```csharp
public sealed class PlayerApplication
{
    private readonly PlayerManager _manager;
    private readonly IPlayerStorage _storage;

    public PlayerApplication(PlayerManager manager, IPlayerStorage storage)
    {
        _manager = manager;
        _storage = storage;
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        List<Player> players = await _storage.LoadAsync(cancellationToken);

        _manager.Clear();
        _manager.AddPlayers(players);
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        await _storage.SaveAsync(_manager.Players, cancellationToken);
    }
}
```

这里：

- `PlayerApplication` 依赖 `PlayerManager`。
- `PlayerApplication` 依赖 `IPlayerStorage`。
- 依赖都从构造函数传入。

## 第 9 步：为什么不直接在 `PlayerManager` 中注入存储

你可能会想：

```text
让 PlayerManager 直接保存和加载，不就更方便了吗？
```

例如：

```csharp
public sealed class PlayerManager
{
    private readonly IPlayerStorage _storage;

    public PlayerManager(IPlayerStorage storage)
    {
        _storage = storage;
    }
}
```

这不是绝对错误，但当前阶段不推荐。

原因：

- `PlayerManager` 现在职责是管理玩家集合和业务规则。
- `IPlayerStorage` 是持久化能力。
- 如果放进去，`PlayerManager` 会同时负责业务管理和存储流程。

更清晰的方式是增加一个应用服务：

```text
PlayerApplication
```

它负责协调：

- `PlayerManager`
- `IPlayerStorage`

职责关系：

```text
Program.cs -> PlayerApplication -> PlayerManager
                            -> IPlayerStorage
```

这样每个类更专注。

## 第 10 步：创建 `PlayerApplication`

推荐目录：

```text
Services/
  PlayerApplication.cs
```

代码：

```csharp
using gameC_.Common;
using gameC_.Dtos;
using gameC_.Models;

namespace gameC_.Services;

public sealed class PlayerApplication
{
    private readonly PlayerManager _manager;
    private readonly IPlayerStorage _storage;

    public PlayerApplication(PlayerManager manager, IPlayerStorage storage)
    {
        _manager = manager;
        _storage = storage;
    }

    public IReadOnlyCollection<Player> Players => _manager.Players;

    public Result AddPlayer(Player player)
    {
        return _manager.AddPlayer(player);
    }

    public Result<Player> GetPlayer(Guid id)
    {
        return _manager.GetPlayer(id);
    }

    public Result RemoveById(Guid id)
    {
        return _manager.RemoveById(id);
    }

    public Result DisableById(Guid id)
    {
        return _manager.DisableById(id);
    }

    public List<RankingPlayerDto> GetRanking(int count)
    {
        return _manager.GetRanking(count);
    }

    public List<RegionStatDto> GetRegionStats()
    {
        return _manager.GetRegionStats();
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        List<Player> players = await _storage.LoadAsync(cancellationToken);

        _manager.Clear();
        _manager.AddPlayers(players);
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        await _storage.SaveAsync(_manager.Players, cancellationToken);
    }
}
```

说明：

- 对外保留玩家业务操作。
- 保存和加载交给 `IPlayerStorage`。
- 内部组合 `PlayerManager` 和存储实现。

## 第 11 步：在 `Program.cs` 中手动注入

暂时不用框架，先手动注入。

```csharp
using gameC_.Services;

string filePath = Path.Combine("data", "players.json");

PlayerManager manager = new PlayerManager();
IPlayerStorage storage = new JsonPlayerStorage(filePath);
PlayerApplication app = new PlayerApplication(manager, storage);
```

然后使用：

```csharp
await app.LoadAsync();

Result result = app.AddPlayer(new Player
{
    Name = "Alice",
    Level = 10,
    Region = "CN",
    Gold = 500
});

await app.SaveAsync();
```

这就是最朴素的依赖注入。

不是只有用了框架才叫依赖注入。手动把依赖传进去，也叫依赖注入。

## 第 12 步：替换成内存存储

接口的好处是可以轻松替换实现。

创建一个测试用或临时用的内存存储：

```text
Services/InMemoryPlayerStorage.cs
```

代码：

```csharp
using gameC_.Models;

namespace gameC_.Services;

public sealed class InMemoryPlayerStorage : IPlayerStorage
{
    private readonly List<Player> _players = new();

    public Task SaveAsync(
        IEnumerable<Player> players,
        CancellationToken cancellationToken = default)
    {
        _players.Clear();
        _players.AddRange(players);

        return Task.CompletedTask;
    }

    public Task<List<Player>> LoadAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_players.ToList());
    }
}
```

现在可以替换：

```csharp
IPlayerStorage storage = new InMemoryPlayerStorage();
PlayerApplication app = new PlayerApplication(new PlayerManager(), storage);
```

业务代码不需要改。

这就是依赖接口带来的灵活性。

## 第 13 步：为什么这对测试有帮助

如果测试 `PlayerApplication` 时直接使用 JSON 文件，会有几个问题：

- 测试变慢。
- 测试依赖文件路径。
- 测试结束要清理文件。
- 文件损坏或权限问题会干扰业务测试。

使用 `InMemoryPlayerStorage` 后：

```csharp
[Fact]
public async Task SaveAndLoadAsync_WithInMemoryStorage_RestoresPlayers()
{
    // Arrange
    IPlayerStorage storage = new InMemoryPlayerStorage();
    PlayerApplication app = new PlayerApplication(new PlayerManager(), storage);

    app.AddPlayer(new Player
    {
        Name = "Alice",
        Level = 10
    });

    // Act
    await app.SaveAsync();

    PlayerApplication newApp = new PlayerApplication(new PlayerManager(), storage);
    await newApp.LoadAsync();

    // Assert
    Assert.Single(newApp.Players);
}
```

测试不关心 JSON，只验证应用流程。

## 第 14 步：依赖倒置原则

依赖注入背后有一个重要设计原则：

```text
高层模块不应该依赖低层模块，二者都应该依赖抽象。
```

在本项目中：

- 高层模块：`PlayerApplication`
- 低层模块：`JsonPlayerStorage`
- 抽象：`IPlayerStorage`

不推荐：

```text
PlayerApplication -> JsonPlayerStorage
```

推荐：

```text
PlayerApplication -> IPlayerStorage <- JsonPlayerStorage
```

这样未来换数据库存储时：

```text
PlayerApplication -> IPlayerStorage <- SqlitePlayerStorage
```

`PlayerApplication` 不需要大改。

## 第 15 步：为 ASP.NET Core 做准备

ASP.NET Core 自带依赖注入容器。

未来 Web API 中，你会看到类似代码：

```csharp
builder.Services.AddSingleton<PlayerManager>();
builder.Services.AddSingleton<IPlayerStorage>(
    _ => new JsonPlayerStorage("data/players.json"));
builder.Services.AddSingleton<PlayerApplication>();
```

然后在 Controller 或 Minimal API 中使用：

```csharp
app.MapGet("/players", (PlayerApplication playerApp) =>
{
    return playerApp.Players;
});
```

这里的 `PlayerApplication` 不需要你手动 `new`。

框架会自动：

```text
创建 PlayerManager
创建 IPlayerStorage 的实现
创建 PlayerApplication
把依赖传进去
```

这就是依赖注入容器。

本课先理解手动注入，后面进入 ASP.NET Core 时就不会突然陌生。

## 第 16 步：服务生命周期简单了解

ASP.NET Core 里常见三种生命周期：

| 生命周期 | 含义 |
| --- | --- |
| Singleton | 整个程序只有一个实例 |
| Scoped | 每次请求一个实例 |
| Transient | 每次需要时创建新实例 |

简单理解：

- `Singleton`：适合无状态服务，或明确要共享状态的服务。
- `Scoped`：适合 Web 请求中的业务服务和数据库上下文。
- `Transient`：适合轻量、无状态、临时对象。

当前控制台项目里不用急着掌握生命周期细节。

先记住：

```text
依赖注入容器负责创建对象和传递依赖。
```

## 第 17 步：什么时候需要接口

不是所有类都要抽接口。

适合抽接口：

- 有多个实现。
- 需要替换实现。
- 需要隔离外部资源。
- 测试时需要 fake/mock。
- 依赖的是数据库、文件、网络、第三方服务。

本项目中适合抽接口：

```text
IPlayerStorage
```

因为存储可能变化：

- JSON 文件。
- SQLite。
- MySQL。
- 内存实现。

暂时不一定需要抽接口：

```text
Player
RankingPlayerDto
Result
```

这些是数据或基础类型，不需要为了抽象而抽象。

## 第 18 步：完整练习

练习目标：把玩家存储抽象成接口，并用构造函数注入到应用服务中。

要求：

1. 创建：

```text
Services/IPlayerStorage.cs
```

2. 定义接口：

```csharp
Task SaveAsync(IEnumerable<Player> players, CancellationToken cancellationToken = default);
Task<List<Player>> LoadAsync(CancellationToken cancellationToken = default);
```

3. 将 `PlayerStorage` 改名为：

```text
JsonPlayerStorage
```

4. 让 `JsonPlayerStorage` 实现 `IPlayerStorage`。
5. 创建：

```text
Services/InMemoryPlayerStorage.cs
```

6. 创建：

```text
Services/PlayerApplication.cs
```

7. `PlayerApplication` 通过构造函数接收：

```csharp
PlayerManager manager
IPlayerStorage storage
```

8. `Program.cs` 中手动组装：

```csharp
PlayerManager manager = new PlayerManager();
IPlayerStorage storage = new JsonPlayerStorage(filePath);
PlayerApplication app = new PlayerApplication(manager, storage);
```

9. 菜单操作尽量调用 `PlayerApplication`。

验收标准：

- 项目可以正常编译运行。
- `JsonPlayerStorage` 实现 `IPlayerStorage`。
- `Program.cs` 依赖 `IPlayerStorage` 类型。
- `PlayerApplication` 中没有直接 `new JsonPlayerStorage(...)`。
- 可以把 `JsonPlayerStorage` 换成 `InMemoryPlayerStorage`，业务代码基本不用改。

## 第 19 步：本课作业

### 作业 1：测试内存存储

给 `InMemoryPlayerStorage` 写测试：

- 保存 2 个玩家。
- 再加载。
- 验证数量为 2。
- 验证玩家名称正确。

### 作业 2：测试 `PlayerApplication`

使用 `InMemoryPlayerStorage` 测试：

- 新增玩家。
- 保存。
- 创建新的 `PlayerApplication`。
- 加载。
- 验证玩家仍然存在。

### 作业 3：抽象排行榜导出

思考是否需要：

```csharp
IRankingExporter
```

要求只写设计说明，不一定实现。

回答：

- 它可能有哪些实现？
- 什么情况下值得抽接口？
- 当前阶段是否有必要？

### 作业 4：阅读 ASP.NET Core DI 写法

提前熟悉这几行：

```csharp
builder.Services.AddSingleton<PlayerManager>();
builder.Services.AddSingleton<IPlayerStorage>(
    _ => new JsonPlayerStorage("data/players.json"));
builder.Services.AddSingleton<PlayerApplication>();
```

思考：

- 哪个是接口？
- 哪个是实现？
- 哪个服务依赖另一个服务？

## 本课常见错误

### 1. 接口里写实现逻辑

不推荐：

```csharp
public interface IPlayerStorage
{
    public async Task SaveAsync(...)
    {
        ...
    }
}
```

接口只定义能力。

推荐：

```csharp
public interface IPlayerStorage
{
    Task SaveAsync(...);
}
```

### 2. 类内部偷偷 new 具体实现

不推荐：

```csharp
public PlayerApplication()
{
    _storage = new JsonPlayerStorage("data/players.json");
}
```

推荐：

```csharp
public PlayerApplication(IPlayerStorage storage)
{
    _storage = storage;
}
```

### 3. 为所有类都创建接口

不需要：

```text
IPlayer
IResult
IRankingPlayerDto
```

接口要解决真实变化点，不是装饰品。

### 4. 接口命名不符合 C# 习惯

不推荐：

```csharp
public interface PlayerStorageInterface
```

推荐：

```csharp
public interface IPlayerStorage
```

### 5. 应用服务职责膨胀

`PlayerApplication` 是协调者，不应该把所有逻辑都写进去。

推荐：

- 玩家业务规则仍在 `PlayerManager`。
- 文件细节仍在 `JsonPlayerStorage`。
- `PlayerApplication` 负责调用和组合。

## 本课复盘问题

学完后，尝试回答：

1. 什么是依赖？
2. 什么是依赖注入？
3. 接口的作用是什么？
4. 为什么 `IPlayerStorage` 比直接依赖 `JsonPlayerStorage` 更灵活？
5. 什么是构造函数注入？
6. 为什么不要在类内部直接 `new` 具体依赖？
7. `JsonPlayerStorage` 和 `InMemoryPlayerStorage` 有什么关系？
8. 什么情况下值得抽接口？
9. 什么情况下不需要抽接口？
10. ASP.NET Core 的依赖注入容器大概负责什么？

## 下一课预告

课程十二建议学习：

- ASP.NET Core Web API 入门。
- 创建 Minimal API。
- 注册服务到 DI 容器。
- 提供玩家新增、查询、排行榜接口。
- 把控制台玩家系统升级成 HTTP API。
