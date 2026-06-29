# C# 基础学习专刊：课程十四

## 课程主题

配置系统与 Options 模式：把 JSON 文件路径移到 `appsettings.json`，创建 `PlayerStorageOptions`，使用 `IOptions<T>` 注入配置。

课程十三中，我们在 `Program.cs` 里注册 JSON 存储时写了硬编码路径：

```csharp
builder.Services.AddSingleton<IPlayerStorage>(
    _ => new JsonPlayerStorage(Path.Combine("data", "players.json")));
```

这能运行，但不够规范。

真实项目里，路径、连接字符串、开关、分页大小、缓存时间等参数通常应该放在配置文件里，而不是写死在代码中。

课程十四的目标是把：

```text
data/players.json
```

迁移到：

```text
appsettings.json
```

并通过 Options 模式注入到 `JsonPlayerStorage`。

## 本课目标

完成本课后，你应该能做到：

- 理解为什么不要硬编码配置。
- 理解 ASP.NET Core 配置系统。
- 在 `appsettings.json` 中添加配置节点。
- 创建 `PlayerStorageOptions`。
- 使用 `builder.Services.Configure<TOptions>()` 绑定配置。
- 在服务中注入 `IOptions<PlayerStorageOptions>`。
- 理解 `IOptions<T>`、`IOptionsSnapshot<T>`、`IOptionsMonitor<T>` 的基础区别。
- 给 Options 添加基础校验。
- 改造 `JsonPlayerStorage`，让文件路径来自配置。

## 第 1 步：为什么不要硬编码

硬编码示例：

```csharp
new JsonPlayerStorage(Path.Combine("data", "players.json"));
```

问题：

- 开发环境、测试环境、生产环境可能路径不同。
- 改路径需要改代码。
- 不利于部署。
- 不利于测试。
- 参数散落在代码里，不容易统一管理。

更推荐：

```json
{
  "PlayerStorage": {
    "FilePath": "data/players.json"
  }
}
```

然后代码读取配置。

一句话：

```text
会因环境变化而变化的值，优先放进配置。
```

## 第 2 步：ASP.NET Core 配置来源

ASP.NET Core 可以从很多地方读取配置：

- `appsettings.json`
- `appsettings.Development.json`
- 环境变量
- 命令行参数
- 用户机密 Secret Manager
- Azure Key Vault
- 自定义配置源

常见加载顺序中，后加载的配置可以覆盖先加载的配置。

例如：

```text
appsettings.json
appsettings.Development.json
环境变量
命令行参数
```

开发环境可以用：

```text
appsettings.Development.json
```

覆盖默认配置。

## 第 3 步：在 `appsettings.json` 中添加配置

在 API 项目的：

```text
GamePlayerSystem.Api/appsettings.json
```

添加：

```json
{
  "PlayerStorage": {
    "FilePath": "data/players.json"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

如果原文件已经有 `Logging` 和 `AllowedHosts`，只需要加入 `PlayerStorage` 节点：

```json
"PlayerStorage": {
  "FilePath": "data/players.json"
}
```

注意 JSON 格式：

- 属性名用双引号。
- 节点之间用逗号。
- 最后一个属性后面不要多逗号。

## 第 4 步：创建 `PlayerStorageOptions`

推荐放在 API 项目或核心项目：

```text
Options/
  PlayerStorageOptions.cs
```

如果这个配置只服务于 API 项目，放 API 项目即可。

代码：

```csharp
namespace GamePlayerSystem.Api.Options;

public sealed class PlayerStorageOptions
{
    public const string SectionName = "PlayerStorage";

    public string FilePath { get; set; } = string.Empty;
}
```

说明：

- Options 类通常是普通 public 类。
- 属性需要 public `get; set;`，方便配置绑定。
- `SectionName` 用常量保存配置节点名称，避免到处写字符串。

为什么不用 `init`？

Options 绑定更常见、更稳妥的写法是 public settable properties：

```csharp
public string FilePath { get; set; } = string.Empty;
```

## 第 5 步：绑定配置到 Options

在 `Program.cs` 中：

```csharp
using GamePlayerSystem.Api.Options;
```

注册：

```csharp
builder.Services.Configure<PlayerStorageOptions>(
    builder.Configuration.GetSection(PlayerStorageOptions.SectionName));
```

完整片段：

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.Configure<PlayerStorageOptions>(
    builder.Configuration.GetSection(PlayerStorageOptions.SectionName));

builder.Services.AddSingleton<PlayerManager>();
builder.Services.AddSingleton<IPlayerStorage, JsonPlayerStorage>();
builder.Services.AddSingleton<PlayerApplication>();
```

注意这里变了：

旧写法：

```csharp
builder.Services.AddSingleton<IPlayerStorage>(
    _ => new JsonPlayerStorage(Path.Combine("data", "players.json")));
```

新写法：

```csharp
builder.Services.AddSingleton<IPlayerStorage, JsonPlayerStorage>();
```

因为 `JsonPlayerStorage` 自己会通过 `IOptions<PlayerStorageOptions>` 获取配置。

## 第 6 步：在 `JsonPlayerStorage` 中注入 `IOptions<T>`

需要命名空间：

```csharp
using Microsoft.Extensions.Options;
using GamePlayerSystem.Api.Options;
```

改造构造函数：

```csharp
public sealed class JsonPlayerStorage : IPlayerStorage
{
    private readonly string _filePath;

    public JsonPlayerStorage(IOptions<PlayerStorageOptions> options)
    {
        _filePath = options.Value.FilePath;
    }
}
```

完整示例：

```csharp
using System.Text.Json;
using gameC_.Exceptions;
using gameC_.Models;
using GamePlayerSystem.Api.Options;
using Microsoft.Extensions.Options;

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

    public JsonPlayerStorage(IOptions<PlayerStorageOptions> options)
    {
        _filePath = options.Value.FilePath;
    }

    public async Task SaveAsync(
        IEnumerable<Player> players,
        CancellationToken cancellationToken = default)
    {
        string? directory = Path.GetDirectoryName(_filePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(players, _jsonOptions);

        await File.WriteAllTextAsync(_filePath, json, cancellationToken);
    }

    public async Task<List<Player>> LoadAsync(
        CancellationToken cancellationToken = default)
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
}
```

说明：

- `IOptions<PlayerStorageOptions>` 由 DI 容器提供。
- `options.Value` 是绑定后的配置对象。
- `JsonPlayerStorage` 不再需要外部传入字符串路径。

## 第 7 步：命名空间放在哪里更合适

上面的示例中，`PlayerStorageOptions` 放在 API 项目：

```csharp
namespace GamePlayerSystem.Api.Options;
```

但 `JsonPlayerStorage` 在核心项目：

```csharp
namespace gameC_.Services;
```

这会让核心项目依赖 API 项目的 Options 类型，不太理想。

更推荐的结构是：

```text
核心项目/
  Options/
    PlayerStorageOptions.cs
  Services/
    JsonPlayerStorage.cs

API 项目/
  Program.cs
```

命名空间：

```csharp
namespace gameC_.Options;
```

这样 `JsonPlayerStorage` 引用：

```csharp
using gameC_.Options;
```

API 项目只负责绑定配置：

```csharp
builder.Services.Configure<PlayerStorageOptions>(
    builder.Configuration.GetSection(PlayerStorageOptions.SectionName));
```

建议本课采用：

```text
gameC_/Options/PlayerStorageOptions.cs
```

这样核心服务和配置对象在同一层，结构更干净。

## 第 8 步：改造后的推荐代码

`Options/PlayerStorageOptions.cs`：

```csharp
namespace gameC_.Options;

public sealed class PlayerStorageOptions
{
    public const string SectionName = "PlayerStorage";

    public string FilePath { get; set; } = string.Empty;
}
```

`Services/JsonPlayerStorage.cs`：

```csharp
using System.Text.Json;
using gameC_.Exceptions;
using gameC_.Models;
using gameC_.Options;
using Microsoft.Extensions.Options;

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

    public JsonPlayerStorage(IOptions<PlayerStorageOptions> options)
    {
        _filePath = options.Value.FilePath;
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

`Program.cs` 注册：

```csharp
using gameC_.Options;

builder.Services.Configure<PlayerStorageOptions>(
    builder.Configuration.GetSection(PlayerStorageOptions.SectionName));

builder.Services.AddSingleton<PlayerManager>();
builder.Services.AddSingleton<IPlayerStorage, JsonPlayerStorage>();
builder.Services.AddSingleton<PlayerApplication>();
```

## 第 9 步：配置校验

如果 `FilePath` 没配置，程序可能运行到保存时才失败。

更推荐启动时就发现配置错误。

可以使用 Options 验证：

```csharp
builder.Services
    .AddOptions<PlayerStorageOptions>()
    .Bind(builder.Configuration.GetSection(PlayerStorageOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.FilePath),
        "PlayerStorage:FilePath 不能为空")
    .ValidateOnStart();
```

这可以替代：

```csharp
builder.Services.Configure<PlayerStorageOptions>(...);
```

推荐写法：

```csharp
builder.Services
    .AddOptions<PlayerStorageOptions>()
    .Bind(builder.Configuration.GetSection(PlayerStorageOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.FilePath),
        "PlayerStorage:FilePath 不能为空")
    .ValidateOnStart();
```

说明：

- `Bind` 把配置节点绑定到 Options。
- `Validate` 添加校验规则。
- `ValidateOnStart` 尽量在启动时校验配置。

## 第 10 步：使用 DataAnnotations 校验

也可以使用特性校验。

需要命名空间：

```csharp
using System.ComponentModel.DataAnnotations;
```

Options：

```csharp
using System.ComponentModel.DataAnnotations;

namespace gameC_.Options;

public sealed class PlayerStorageOptions
{
    public const string SectionName = "PlayerStorage";

    [Required]
    public string FilePath { get; set; } = string.Empty;
}
```

注册：

```csharp
builder.Services
    .AddOptions<PlayerStorageOptions>()
    .Bind(builder.Configuration.GetSection(PlayerStorageOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

如果项目缺少扩展包，可添加：

```powershell
dotnet add GamePlayerSystem.Api package Microsoft.Extensions.Options.DataAnnotations
```

本课推荐先使用 `Validate(...)`，不额外引入 DataAnnotations 包也能理解主线。

## 第 11 步：`IOptions<T>`、`IOptionsSnapshot<T>`、`IOptionsMonitor<T>`

常见三种 Options 注入方式：

| 类型 | 生命周期特点 | 适合场景 |
| --- | --- | --- |
| `IOptions<T>` | 单例，可注入任意生命周期 | 启动后基本不变的配置 |
| `IOptionsSnapshot<T>` | Scoped，每次请求一个快照 | Web 请求中希望读取每次请求的配置快照 |
| `IOptionsMonitor<T>` | 单例，可监听变化 | 单例服务中需要读取最新配置或监听变化 |

当前项目中：

```csharp
JsonPlayerStorage
```

注册为：

```csharp
AddSingleton<IPlayerStorage, JsonPlayerStorage>()
```

所以推荐使用：

```csharp
IOptions<PlayerStorageOptions>
```

或者如果你想支持运行中配置更新，可以考虑：

```csharp
IOptionsMonitor<PlayerStorageOptions>
```

不推荐把 `IOptionsSnapshot<T>` 注入单例服务，因为 Snapshot 是 Scoped。

## 第 12 步：使用 `IOptionsMonitor<T>` 的版本

如果希望每次保存和加载时读取最新路径，可以改成：

```csharp
private readonly IOptionsMonitor<PlayerStorageOptions> _options;

public JsonPlayerStorage(IOptionsMonitor<PlayerStorageOptions> options)
{
    _options = options;
}

private string FilePath => _options.CurrentValue.FilePath;
```

保存时：

```csharp
string? directory = Path.GetDirectoryName(FilePath);
await File.WriteAllTextAsync(FilePath, json, cancellationToken);
```

这适合配置可能动态变化的场景。

但本项目初学阶段不需要复杂化。

推荐：

```csharp
IOptions<PlayerStorageOptions>
```

## 第 13 步：环境配置覆盖

`appsettings.json`：

```json
{
  "PlayerStorage": {
    "FilePath": "data/players.json"
  }
}
```

`appsettings.Development.json`：

```json
{
  "PlayerStorage": {
    "FilePath": "data/players.development.json"
  }
}
```

开发环境运行时，会使用 Development 配置覆盖默认配置。

好处：

- 开发环境用测试数据。
- 生产环境用正式路径。
- 不需要改代码。

## 第 14 步：环境变量覆盖配置

ASP.NET Core 支持通过环境变量覆盖配置。

层级配置通常用双下划线：

```text
PlayerStorage__FilePath
```

PowerShell 示例：

```powershell
$env:PlayerStorage__FilePath = "data/players.env.json"
dotnet run --project GamePlayerSystem.Api
```

这样可以覆盖：

```json
{
  "PlayerStorage": {
    "FilePath": "data/players.json"
  }
}
```

这在容器部署、CI/CD、服务器环境里很常见。

## 第 15 步：不要把敏感信息放进普通配置文件

本课的文件路径不是敏感信息，可以放 `appsettings.json`。

但下面这些不建议提交到代码仓库：

- 数据库密码。
- API Key。
- 访问令牌。
- 私钥。

开发环境可以使用：

- User Secrets。
- 环境变量。

生产环境可以使用：

- 环境变量。
- 云平台密钥服务。
- Key Vault。

一句话：

```text
普通配置可以进 appsettings，秘密配置不要进仓库。
```

## 第 16 步：日志中输出配置

启动时可以记录配置路径，方便排查。

```csharp
PlayerStorageOptions storageOptions = app.Services
    .GetRequiredService<IOptions<PlayerStorageOptions>>()
    .Value;

app.Logger.LogInformation("Player storage file path: {FilePath}",
    storageOptions.FilePath);
```

需要：

```csharp
using gameC_.Options;
using Microsoft.Extensions.Options;
```

注意：

- 文件路径可以记录。
- 密码、Token 这类敏感信息不要记录。

## 第 17 步：整理后的 `Program.cs`

示例：

```csharp
using gameC_.Exceptions;
using gameC_.Options;
using gameC_.Services;
using GamePlayerSystem.Api.Endpoints;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services
    .AddOptions<PlayerStorageOptions>()
    .Bind(builder.Configuration.GetSection(PlayerStorageOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.FilePath),
        "PlayerStorage:FilePath 不能为空")
    .ValidateOnStart();

builder.Services.AddSingleton<PlayerManager>();
builder.Services.AddSingleton<IPlayerStorage, JsonPlayerStorage>();
builder.Services.AddSingleton<PlayerApplication>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

PlayerStorageOptions storageOptions = app.Services
    .GetRequiredService<IOptions<PlayerStorageOptions>>()
    .Value;

app.Logger.LogInformation("Player storage file path: {FilePath}",
    storageOptions.FilePath);

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

## 第 18 步：完整练习

练习目标：把玩家 JSON 文件路径从硬编码迁移到 Options 配置。

要求：

1. 在核心项目创建：

```text
Options/PlayerStorageOptions.cs
```

2. 添加：

```csharp
public const string SectionName = "PlayerStorage";
public string FilePath { get; set; } = string.Empty;
```

3. 在 API 项目的 `appsettings.json` 添加：

```json
"PlayerStorage": {
  "FilePath": "data/players.json"
}
```

4. 在 `Program.cs` 注册 Options：

```csharp
builder.Services
    .AddOptions<PlayerStorageOptions>()
    .Bind(builder.Configuration.GetSection(PlayerStorageOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.FilePath),
        "PlayerStorage:FilePath 不能为空")
    .ValidateOnStart();
```

5. 修改服务注册：

```csharp
builder.Services.AddSingleton<IPlayerStorage, JsonPlayerStorage>();
```

6. 修改 `JsonPlayerStorage` 构造函数：

```csharp
public JsonPlayerStorage(IOptions<PlayerStorageOptions> options)
```

7. 删除 `new JsonPlayerStorage(Path.Combine(...))` 的硬编码写法。
8. 启动时输出当前文件路径日志。

验收标准：

- 项目可以正常启动。
- 新增玩家后仍能保存 JSON。
- 文件路径来自 `appsettings.json`。
- 把 `FilePath` 改成其他路径后，保存位置会变化。
- `FilePath` 为空时，启动阶段能提示配置错误。

## 第 19 步：本课作业

### 作业 1：开发环境配置覆盖

创建或修改：

```text
appsettings.Development.json
```

添加：

```json
{
  "PlayerStorage": {
    "FilePath": "data/players.development.json"
  }
}
```

要求：

- Development 环境下保存到 `players.development.json`。
- 日志输出实际使用的路径。

### 作业 2：增加自动保存配置

给 Options 增加：

```csharp
public bool AutoSave { get; set; } = true;
```

配置：

```json
"PlayerStorage": {
  "FilePath": "data/players.json",
  "AutoSave": true
}
```

要求：

- 新增、删除、禁用后，只有 `AutoSave = true` 才自动保存。
- 思考这个逻辑应该放在 Endpoint 还是 `PlayerApplication`。

### 作业 3：使用 DataAnnotations 校验

把 `FilePath` 改成：

```csharp
[Required]
public string FilePath { get; set; } = string.Empty;
```

注册改成：

```csharp
.ValidateDataAnnotations()
.ValidateOnStart();
```

如果缺包，添加：

```powershell
dotnet add GamePlayerSystem.Api package Microsoft.Extensions.Options.DataAnnotations
```

### 作业 4：环境变量覆盖

使用环境变量覆盖路径：

```powershell
$env:PlayerStorage__FilePath = "data/players.env.json"
dotnet run --project GamePlayerSystem.Api
```

要求：

- 观察日志中的路径。
- 新增玩家后确认数据保存到环境变量指定路径。

## 本课常见错误

### 1. 配置节点名称写错

`appsettings.json`：

```json
"PlayerStorage": {
  "FilePath": "data/players.json"
}
```

代码：

```csharp
GetSection("PlayerStore")
```

这样绑定不到。

推荐使用常量：

```csharp
PlayerStorageOptions.SectionName
```

### 2. Options 属性没有 public set

风险写法：

```csharp
public string FilePath { get; init; } = string.Empty;
```

推荐：

```csharp
public string FilePath { get; set; } = string.Empty;
```

### 3. 注册了 Options，但服务仍然手动传路径

不推荐：

```csharp
builder.Services.AddSingleton<IPlayerStorage>(
    _ => new JsonPlayerStorage("data/players.json"));
```

推荐：

```csharp
builder.Services.AddSingleton<IPlayerStorage, JsonPlayerStorage>();
```

### 4. 把 `IOptionsSnapshot<T>` 注入单例

不推荐：

```csharp
public JsonPlayerStorage(IOptionsSnapshot<PlayerStorageOptions> options)
```

因为 `JsonPlayerStorage` 是单例，而 `IOptionsSnapshot<T>` 是 scoped。

推荐：

```csharp
IOptions<PlayerStorageOptions>
```

或者：

```csharp
IOptionsMonitor<PlayerStorageOptions>
```

### 5. 把敏感信息提交到 `appsettings.json`

不要把密码、Token、密钥提交到仓库。

这类配置应使用：

- 环境变量。
- User Secrets。
- 密钥管理服务。

## 本课复盘问题

学完后，尝试回答：

1. 为什么不要把文件路径硬编码在代码里？
2. ASP.NET Core 常见配置来源有哪些？
3. `appsettings.Development.json` 有什么作用？
4. Options 模式解决什么问题？
5. `PlayerStorageOptions.SectionName` 有什么好处？
6. `Configure<TOptions>` 和 `AddOptions<TOptions>().Bind(...)` 有什么关系？
7. `IOptions<T>` 适合什么场景？
8. `IOptionsSnapshot<T>` 为什么不适合注入单例？
9. `IOptionsMonitor<T>` 适合什么场景？
10. 为什么敏感信息不应该放进普通配置文件？

## 下一课预告

课程十五建议学习：

- EF Core 与 SQLite 入门。
- 把 JSON 文件存储升级成数据库存储。
- 创建 `PlayerDbContext`。
- 使用迁移创建数据库。
- 实现 `SqlitePlayerStorage` 或 Repository。

## 参考资料

- Microsoft Learn: Configuration in ASP.NET Core
  - https://learn.microsoft.com/aspnet/core/fundamentals/configuration/
- Microsoft Learn: Options pattern in ASP.NET Core
  - https://learn.microsoft.com/aspnet/core/fundamentals/configuration/options
- Microsoft Learn: Options pattern in .NET
  - https://learn.microsoft.com/dotnet/core/extensions/options
