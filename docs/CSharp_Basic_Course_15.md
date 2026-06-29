# C# 基础学习专刊：课程十五

## 课程主题

EF Core 与 SQLite 入门：把 JSON 文件存储升级成数据库存储，创建 `PlayerDbContext`，使用迁移创建数据库。

前面课程中，我们使用 JSON 文件保存玩家数据：

```text
data/players.json
```

JSON 文件适合入门，但真实项目里，随着数据量、查询需求和并发写入增加，通常会使用数据库。

课程十五的目标是把玩家系统从：

```text
JSON 文件存储
```

升级到：

```text
SQLite 数据库存储
```

并学习 EF Core 的基础使用方式。

## 本课目标

完成本课后，你应该能做到：

- 理解 EF Core 是什么。
- 理解 SQLite 适合什么场景。
- 安装 EF Core SQLite 相关包。
- 创建 `PlayerDbContext`。
- 使用 `DbSet<Player>` 映射玩家表。
- 在 `appsettings.json` 中配置数据库连接字符串。
- 在 DI 容器中注册 `DbContext`。
- 使用 EF Core Migration 创建数据库。
- 使用 `dotnet ef database update` 更新数据库。
- 初步实现数据库版玩家存储。
- 理解 JSON 存储和数据库存储的区别。

## 第 1 步：什么是 EF Core

EF Core 全称是 Entity Framework Core。

它是 .NET 官方常用 ORM 框架。

ORM 的意思是：

```text
Object Relational Mapping
对象关系映射
```

简单理解：

```text
C# 类        <-> 数据库表
C# 对象      <-> 数据库行
C# 属性      <-> 数据库列
```

例如：

```csharp
public sealed class Player
{
    public Guid Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
}
```

可以映射成数据库表：

```text
Players
  Id
  Name
  Level
```

这样你可以用 C# 写：

```csharp
dbContext.Players.Add(player);
await dbContext.SaveChangesAsync();
```

而不是手写 SQL：

```sql
INSERT INTO Players ...
```

## 第 2 步：什么是 SQLite

SQLite 是一个轻量级数据库。

它通常就是一个本地文件：

```text
data/players.db
```

优点：

- 不需要单独安装数据库服务器。
- 适合本地开发。
- 适合桌面工具、小项目、学习项目。
- 部署简单。

缺点：

- 不适合高并发大型服务。
- 不适合复杂分布式场景。
- 和 MySQL / PostgreSQL / SQL Server 相比，功能和运维能力更轻量。

本课程使用 SQLite，因为它非常适合从 JSON 文件过渡到数据库。

## 第 3 步：JSON 存储和数据库存储的区别

| 对比项 | JSON 文件 | SQLite 数据库 |
| --- | --- | --- |
| 保存方式 | 整个对象序列化成文件 | 数据按表和列保存 |
| 查询能力 | 通常要读全文件再 LINQ | 可以按条件查询 |
| 数据量 | 小数据更合适 | 中小数据更合适 |
| 并发写入 | 容易冲突 | 更可靠 |
| 结构变化 | 手动处理 | 使用迁移管理 |
| 学习成本 | 低 | 中 |

什么时候继续用 JSON：

- 配置文件。
- 小型本地工具。
- 数据量小。
- 不需要复杂查询。

什么时候使用数据库：

- 数据会增长。
- 需要分页查询。
- 需要条件筛选。
- 需要索引。
- 需要更可靠的持久化。

## 第 4 步：安装 EF Core 包

在 API 项目中安装 SQLite Provider：

```powershell
dotnet add GamePlayerSystem.Api package Microsoft.EntityFrameworkCore.Sqlite
```

安装设计时工具包：

```powershell
dotnet add GamePlayerSystem.Api package Microsoft.EntityFrameworkCore.Design
```

如果本机没有 `dotnet ef` 工具，可以安装：

```powershell
dotnet tool install --global dotnet-ef
```

如果已经安装过，可以更新：

```powershell
dotnet tool update --global dotnet-ef
```

检查：

```powershell
dotnet ef --version
```

注意：

- EF Core 包版本建议和你的 .NET / EF Core 版本保持一致。
- 如果项目使用 `net10.0`，优先选择与当前 SDK 匹配的 EF Core 版本。

## 第 5 步：连接字符串配置

在 API 项目的 `appsettings.json` 中添加：

```json
{
  "ConnectionStrings": {
    "PlayerDatabase": "Data Source=data/players.db"
  }
}
```

如果文件里已经有其他配置，可以合并：

```json
{
  "ConnectionStrings": {
    "PlayerDatabase": "Data Source=data/players.db"
  },
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

说明：

- `ConnectionStrings` 是 .NET 项目中常见的连接字符串节点。
- `PlayerDatabase` 是连接字符串名称。
- `Data Source=data/players.db` 表示 SQLite 数据库文件路径。

## 第 6 步：创建 `PlayerDbContext`

推荐目录：

```text
Persistence/
  PlayerDbContext.cs
```

代码：

```csharp
using gameC_.Models;
using Microsoft.EntityFrameworkCore;

namespace gameC_.Persistence;

public sealed class PlayerDbContext : DbContext
{
    public PlayerDbContext(DbContextOptions<PlayerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Player> Players => Set<Player>();
}
```

解释：

| 代码 | 含义 |
| --- | --- |
| `DbContext` | EF Core 的数据库上下文 |
| `DbSet<Player>` | 表示玩家表 |
| `DbContextOptions<PlayerDbContext>` | 数据库配置 |
| `Set<Player>()` | 获取 `Player` 对应的数据集 |

`DbContext` 可以理解成：

```text
一次数据库工作单元
```

在 Web API 中，通常每个请求使用一个 `DbContext` 实例。

## 第 7 步：注册 `DbContext`

在 API 项目的 `Program.cs` 中：

```csharp
using gameC_.Persistence;
using Microsoft.EntityFrameworkCore;
```

注册：

```csharp
string connectionString = builder.Configuration
    .GetConnectionString("PlayerDatabase")
    ?? throw new InvalidOperationException("Connection string 'PlayerDatabase' not found.");

builder.Services.AddDbContext<PlayerDbContext>(options =>
    options.UseSqlite(connectionString));
```

完整位置：

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

string connectionString = builder.Configuration
    .GetConnectionString("PlayerDatabase")
    ?? throw new InvalidOperationException("Connection string 'PlayerDatabase' not found.");

builder.Services.AddDbContext<PlayerDbContext>(options =>
    options.UseSqlite(connectionString));
```

注意：

- `AddDbContext` 默认注册为 Scoped。
- Web API 中 Scoped 通常表示每个请求一个实例。
- 不要把同一个 `DbContext` 实例跨线程共享。

## 第 8 步：让 SQLite 目录存在

如果连接字符串是：

```text
Data Source=data/players.db
```

而 `data` 目录不存在，创建数据库时可能失败。

可以在启动时确保目录存在：

```csharp
Directory.CreateDirectory("data");
```

更规范一点，可以从连接字符串解析路径，但入门阶段先简单处理。

放在 `Program.cs` 中：

```csharp
Directory.CreateDirectory("data");
```

后续可以把数据库路径也放进 Options 或使用更完整的路径处理。

## 第 9 步：配置实体映射

EF Core 可以根据约定自动映射。

例如 `Player.Id` 会成为主键。

但建议显式配置一些规则。

在 `PlayerDbContext` 中重写：

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Player>(entity =>
    {
        entity.ToTable("Players");

        entity.HasKey(p => p.Id);

        entity.Property(p => p.Name)
            .HasMaxLength(64)
            .IsRequired();

        entity.Property(p => p.Region)
            .HasMaxLength(16)
            .IsRequired();
    });
}
```

完整代码：

```csharp
using gameC_.Models;
using Microsoft.EntityFrameworkCore;

namespace gameC_.Persistence;

public sealed class PlayerDbContext : DbContext
{
    public PlayerDbContext(DbContextOptions<PlayerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Player> Players => Set<Player>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>(entity =>
        {
            entity.ToTable("Players");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(p => p.Region)
                .HasMaxLength(16)
                .IsRequired();
        });
    }
}
```

## 第 10 步：注意当前 `Player` 类的属性

课程前面为了封装规则，`Player` 中可能有：

```csharp
private int _level = 1;

public int Level
{
    get => _level;
    set => _level = value < 1 ? 1 : value;
}
```

以及：

```csharp
public Guid Id { get; init; } = Guid.NewGuid();
```

EF Core 可以处理很多属性映射场景，但入门阶段建议保持实体简单。

推荐当前 `Player` 至少具备：

```csharp
public Guid Id { get; set; } = Guid.NewGuid();
public string Name { get; set; } = "未命名玩家";
public int Level { get; set; } = 1;
public string Region { get; set; } = "CN";
public int Gold { get; set; }
public bool IsActive { get; set; } = true;
```

然后把校验规则放到：

```text
PlayerManager
```

或者专门的验证方法中。

原因：

- EF Core 映射更简单。
- 数据库实体更像数据结构。
- 业务规则集中在服务层，后续更好测试。

如果你保留完整属性也可以，但遇到迁移或反序列化问题时，优先检查实体属性是否容易被 EF Core 构造和设置。

## 第 11 步：创建第一次迁移

在项目根目录执行：

```powershell
dotnet ef migrations add InitialCreate --project GamePlayerSystem.Api
```

如果启动项目和迁移项目不同，可以写完整：

```powershell
dotnet ef migrations add InitialCreate `
  --project GamePlayerSystem.Api `
  --startup-project GamePlayerSystem.Api
```

执行后会生成：

```text
GamePlayerSystem.Api/
  Migrations/
    2026xxxxxxxxxx_InitialCreate.cs
    PlayerDbContextModelSnapshot.cs
```

迁移文件表示：

```text
当前 C# 模型应该如何创建或修改数据库结构。
```

建议打开迁移文件看一眼，理解 EF Core 生成了什么表和列。

## 第 12 步：更新数据库

执行：

```powershell
dotnet ef database update --project GamePlayerSystem.Api
```

或者：

```powershell
dotnet ef database update `
  --project GamePlayerSystem.Api `
  --startup-project GamePlayerSystem.Api
```

成功后会生成：

```text
data/players.db
```

数据库里会有：

```text
Players
__EFMigrationsHistory
```

其中：

- `Players` 是玩家表。
- `__EFMigrationsHistory` 是 EF Core 记录迁移历史的表。

## 第 13 步：常用迁移命令

创建迁移：

```powershell
dotnet ef migrations add InitialCreate --project GamePlayerSystem.Api
```

更新数据库：

```powershell
dotnet ef database update --project GamePlayerSystem.Api
```

查看迁移：

```powershell
dotnet ef migrations list --project GamePlayerSystem.Api
```

删除最后一次还未应用的迁移：

```powershell
dotnet ef migrations remove --project GamePlayerSystem.Api
```

生成 SQL 脚本：

```powershell
dotnet ef migrations script --project GamePlayerSystem.Api
```

注意：

- 本地开发可以直接 `database update`。
- 生产环境更常见的是生成脚本，经过审核后执行。

## 第 14 步：创建数据库版存储接口实现

课程十一中我们抽象了：

```csharp
public interface IPlayerStorage
{
    Task SaveAsync(IEnumerable<Player> players, CancellationToken cancellationToken = default);

    Task<List<Player>> LoadAsync(CancellationToken cancellationToken = default);
}
```

这个接口适合 JSON 文件的“整体保存 / 整体加载”模式。

数据库更适合：

```text
新增一条
更新一条
删除一条
按条件查询
分页查询
```

所以这里有两条路线：

| 路线 | 特点 |
| --- | --- |
| 兼容旧接口 | 改动小，但不够数据库化 |
| 新建 Repository | 更符合数据库项目结构 |

本课先用兼容旧接口，让迁移成本小一些。

## 第 15 步：实现 `EfPlayerStorage`

创建：

```text
Services/
  EfPlayerStorage.cs
```

代码：

```csharp
using gameC_.Models;
using gameC_.Persistence;
using Microsoft.EntityFrameworkCore;

namespace gameC_.Services;

public sealed class EfPlayerStorage : IPlayerStorage
{
    private readonly PlayerDbContext _dbContext;

    public EfPlayerStorage(PlayerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveAsync(
        IEnumerable<Player> players,
        CancellationToken cancellationToken = default)
    {
        List<Player> playerList = players.ToList();

        _dbContext.Players.RemoveRange(_dbContext.Players);
        await _dbContext.Players.AddRangeAsync(playerList, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Player>> LoadAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Players
            .AsNoTracking()
            .OrderByDescending(p => p.Level)
            .ToListAsync(cancellationToken);
    }
}
```

说明：

- `LoadAsync` 从数据库加载玩家。
- `SaveAsync` 为了兼容旧接口，采用“清空后重新写入”的方式。
- `AsNoTracking` 表示只读查询，不跟踪实体变化。

注意：

这种 `SaveAsync` 不是生产最佳实践。

原因：

- 每次保存都删全表再插入。
- 数据量大时性能差。
- 并发情况下风险大。

它只适合本课平滑过渡。

下一步应该学习 Repository，把新增、删除、修改变成真正的数据库操作。

## 第 16 步：替换 DI 注册

旧 JSON 注册：

```csharp
builder.Services.AddSingleton<IPlayerStorage, JsonPlayerStorage>();
```

数据库版注册：

```csharp
builder.Services.AddScoped<IPlayerStorage, EfPlayerStorage>();
```

为什么从 Singleton 改成 Scoped？

因为 `EfPlayerStorage` 依赖：

```csharp
PlayerDbContext
```

而 `DbContext` 默认是 Scoped。

不能把 Scoped 的 `DbContext` 注入 Singleton 服务。

所以要改：

```csharp
builder.Services.AddScoped<IPlayerStorage, EfPlayerStorage>();
```

同时注意：

```csharp
PlayerApplication
```

如果依赖 `IPlayerStorage`，也不能继续是 Singleton。

建议改成：

```csharp
builder.Services.AddScoped<PlayerApplication>();
```

`PlayerManager` 当前如果保存内存状态，和数据库模式会有冲突。

本课为了平滑过渡，有两个选择：

### 选择 A：继续内存管理，数据库只负责启动加载和保存

注册：

```csharp
builder.Services.AddSingleton<PlayerManager>();
builder.Services.AddScoped<IPlayerStorage, EfPlayerStorage>();
builder.Services.AddScoped<PlayerApplication>();
```

缺点：

- `PlayerApplication` Scoped，但内部使用 Singleton `PlayerManager`。
- 数据仍主要在内存，数据库只是持久化。

### 选择 B：下一课改成 Repository，逐步移除内存状态

这是更推荐的真实项目路线。

本课先完成 EF Core 入门，下一课再升级结构。

## 第 17 步：启动时应用迁移

开发阶段可以在启动时自动迁移：

```csharp
using gameC_.Persistence;
using Microsoft.EntityFrameworkCore;

using IServiceScope scope = app.Services.CreateScope();
PlayerDbContext dbContext = scope.ServiceProvider.GetRequiredService<PlayerDbContext>();
await dbContext.Database.MigrateAsync();
```

位置：

```csharp
var app = builder.Build();

Directory.CreateDirectory("data");

using (IServiceScope scope = app.Services.CreateScope())
{
    PlayerDbContext dbContext = scope.ServiceProvider.GetRequiredService<PlayerDbContext>();
    await dbContext.Database.MigrateAsync();
}
```

说明：

- `MigrateAsync` 会把未应用的迁移应用到数据库。
- 本地开发很方便。

生产环境注意：

- 自动迁移可能不适合生产。
- 生产通常要生成 SQL 脚本、审核、发布。

## 第 18 步：测试数据库是否工作

启动 API：

```powershell
dotnet run --project GamePlayerSystem.Api
```

新增玩家：

```powershell
curl -Method POST http://localhost:5000/players `
  -ContentType "application/json" `
  -Body '{"name":"Alice","level":10,"region":"CN","gold":500}'
```

查询玩家：

```powershell
curl http://localhost:5000/players
```

然后停止服务，再重新启动。

如果玩家仍然存在，说明数据库持久化生效。

## 第 19 步：使用 SQLite 工具查看数据库

可以使用：

- DB Browser for SQLite
- Rider / Visual Studio 数据库工具
- SQLite CLI

查看：

```text
data/players.db
```

表：

```text
Players
__EFMigrationsHistory
```

你应该能看到玩家数据。

## 第 20 步：更真实的 Repository 方向

当前 `IPlayerStorage` 是整体保存和加载：

```csharp
SaveAsync(IEnumerable<Player> players)
LoadAsync()
```

数据库项目更推荐设计：

```csharp
public interface IPlayerRepository
{
    Task AddAsync(Player player, CancellationToken cancellationToken = default);
    Task<Player?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Player>> GetAllAsync(CancellationToken cancellationToken = default);
    Task RemoveAsync(Player player, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

这样 API 就不需要先加载全量数据到内存。

下一课可以继续做：

```text
IPlayerRepository + EfPlayerRepository
```

这是从练习项目走向真实 Web API 的关键一步。

## 第 21 步：完整练习

练习目标：给玩家 API 增加 EF Core + SQLite 数据库支持。

要求：

1. 安装包：

```powershell
dotnet add GamePlayerSystem.Api package Microsoft.EntityFrameworkCore.Sqlite
dotnet add GamePlayerSystem.Api package Microsoft.EntityFrameworkCore.Design
```

2. 安装或更新 `dotnet ef`：

```powershell
dotnet tool install --global dotnet-ef
```

或：

```powershell
dotnet tool update --global dotnet-ef
```

3. 在 `appsettings.json` 添加：

```json
"ConnectionStrings": {
  "PlayerDatabase": "Data Source=data/players.db"
}
```

4. 创建：

```text
Persistence/PlayerDbContext.cs
```

5. 在 `Program.cs` 注册：

```csharp
builder.Services.AddDbContext<PlayerDbContext>(options =>
    options.UseSqlite(connectionString));
```

6. 创建迁移：

```powershell
dotnet ef migrations add InitialCreate --project GamePlayerSystem.Api
```

7. 更新数据库：

```powershell
dotnet ef database update --project GamePlayerSystem.Api
```

8. 创建：

```text
Services/EfPlayerStorage.cs
```

9. 注册：

```csharp
builder.Services.AddScoped<IPlayerStorage, EfPlayerStorage>();
```

10. 启动 API，新增玩家，重启后确认数据仍存在。

验收标准：

- `data/players.db` 被创建。
- `Migrations` 目录被创建。
- 数据库中有 `Players` 表。
- API 可以新增玩家。
- 重启服务后玩家仍然存在。
- `JsonPlayerStorage` 可以暂时保留，但 DI 当前使用 `EfPlayerStorage`。

## 第 22 步：本课作业

### 作业 1：给 `Player` 增加创建时间

给 `Player` 增加：

```csharp
public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
```

然后创建迁移：

```powershell
dotnet ef migrations add AddPlayerCreatedAt --project GamePlayerSystem.Api
dotnet ef database update --project GamePlayerSystem.Api
```

要求：

- 数据库表增加 `CreatedAt` 列。
- 新增玩家时自动有创建时间。

### 作业 2：给名称加索引

在 `OnModelCreating` 中增加：

```csharp
entity.HasIndex(p => p.Name);
```

创建迁移并更新数据库。

思考：

- 为什么名称查询可能需要索引？
- 如果名称不允许重复，索引应该怎么设计？

### 作业 3：改造为 Repository

设计接口：

```csharp
public interface IPlayerRepository
{
    Task AddAsync(Player player, CancellationToken cancellationToken = default);
    Task<Player?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Player>> GetAllAsync(CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

本课只要求写接口和设计说明，不强制完成实现。

### 作业 4：生成 SQL 脚本

执行：

```powershell
dotnet ef migrations script --project GamePlayerSystem.Api
```

观察生成的 SQL。

要求：

- 找到创建 `Players` 表的 SQL。
- 找到 `__EFMigrationsHistory` 相关 SQL。

## 本课常见错误

### 1. 没安装 EF Core Design 包

如果运行 `dotnet ef migrations add` 报设计时相关错误，检查是否安装：

```powershell
dotnet add GamePlayerSystem.Api package Microsoft.EntityFrameworkCore.Design
```

### 2. 没安装 `dotnet-ef`

如果提示找不到 `dotnet ef`，安装：

```powershell
dotnet tool install --global dotnet-ef
```

### 3. 连接字符串名称不一致

配置：

```json
"PlayerDatabase": "Data Source=data/players.db"
```

代码：

```csharp
GetConnectionString("DefaultConnection")
```

这样会读不到。

保持一致：

```csharp
GetConnectionString("PlayerDatabase")
```

### 4. 把 `DbContext` 注册成 Singleton

不推荐：

```csharp
builder.Services.AddSingleton<PlayerDbContext>();
```

推荐：

```csharp
builder.Services.AddDbContext<PlayerDbContext>(...);
```

`AddDbContext` 默认 Scoped，更适合 Web 请求。

### 5. Singleton 服务依赖 Scoped `DbContext`

错误组合：

```csharp
builder.Services.AddSingleton<IPlayerStorage, EfPlayerStorage>();
```

如果 `EfPlayerStorage` 依赖 `PlayerDbContext`，应该用：

```csharp
builder.Services.AddScoped<IPlayerStorage, EfPlayerStorage>();
```

同时检查依赖它的服务生命周期。

### 6. 忘记更新数据库

只创建迁移：

```powershell
dotnet ef migrations add InitialCreate
```

不会真正创建数据库。

还需要：

```powershell
dotnet ef database update
```

## 本课复盘问题

学完后，尝试回答：

1. EF Core 是什么？
2. ORM 解决什么问题？
3. SQLite 适合什么场景？
4. `DbContext` 是什么？
5. `DbSet<Player>` 表示什么？
6. `AddDbContext` 默认注册什么生命周期？
7. Migration 的作用是什么？
8. `dotnet ef migrations add` 和 `dotnet ef database update` 有什么区别？
9. 为什么 `DbContext` 不应该注册成 Singleton？
10. 为什么数据库项目更适合 Repository，而不是整体 `SaveAsync(IEnumerable<Player>)`？

## 下一课预告

课程十六建议学习：

- Repository 模式与 EF Core 查询。
- 创建 `IPlayerRepository`。
- 实现 `EfPlayerRepository`。
- 用数据库查询替代内存字典。
- 支持分页、按区服查询、按名称搜索。

## 参考资料

- Microsoft Learn: DbContext Lifetime, Configuration, and Initialization
  - https://learn.microsoft.com/ef/core/dbcontext-configuration/
- Microsoft Learn: EF Core database providers
  - https://learn.microsoft.com/ef/core/providers/
- Microsoft Learn: Migrations Overview
  - https://learn.microsoft.com/ef/core/managing-schemas/migrations/
- Microsoft Learn: Applying Migrations
  - https://learn.microsoft.com/ef/core/managing-schemas/migrations/applying
- Microsoft Learn: EF Core tools reference
  - https://learn.microsoft.com/ef/core/cli/dotnet
