# C# 基础学习专刊：课程十六

## 课程主题

Repository 模式与 EF Core 查询：创建 `IPlayerRepository` 和 `EfPlayerRepository`，用数据库查询替代内存字典，支持分页、按区服查询、按名称搜索。

课程十五中，我们已经完成了 EF Core + SQLite 入门：

- 创建 `PlayerDbContext`。
- 配置 SQLite 连接字符串。
- 使用迁移创建数据库。
- 初步实现 `EfPlayerStorage`。

但上一课的 `EfPlayerStorage` 仍然保留了 JSON 存储时代的思路：

```csharp
Task SaveAsync(IEnumerable<Player> players);
Task<List<Player>> LoadAsync();
```

这种“整体保存、整体加载”的接口不适合数据库。

课程十六的目标是进入更真实的数据访问方式：

```text
按需查询
单条新增
单条删除
分页查询
条件查询
统一 SaveChangesAsync
```

也就是 Repository 模式。

## 本课目标

完成本课后，你应该能做到：

- 理解 Repository 模式解决什么问题。
- 创建 `IPlayerRepository`。
- 实现 `EfPlayerRepository`。
- 使用 `AddAsync` 新增玩家。
- 使用 `GetByIdAsync` 按 ID 查询玩家。
- 使用 `GetPagedAsync` 分页查询玩家。
- 使用 `GetByRegionAsync` 按区服查询。
- 使用 `SearchByNameAsync` 按名称搜索。
- 使用 `AnyAsync` 检查重名。
- 使用 `AsNoTracking` 优化只读查询。
- 使用 `Skip`、`Take` 实现基础分页。
- 让业务层从内存字典逐步迁移到数据库查询。

## 第 1 步：为什么需要 Repository

之前的 `PlayerManager` 使用内存字典：

```csharp
private readonly Dictionary<Guid, Player> _playersById = new();
```

优点：

- 简单。
- 查询快。
- 适合学习集合。

缺点：

- 服务重启后要重新加载。
- 多实例部署时内存数据不共享。
- 数据量大时启动加载变慢。
- 分页、筛选、搜索都在内存做。
- 数据库没有真正发挥查询能力。

Repository 的目标是把数据访问封装起来：

```text
业务层只说我要什么数据。
Repository 负责怎么从数据库查。
```

例如业务层调用：

```csharp
Player? player = await repository.GetByIdAsync(id);
```

它不关心底层是：

- SQLite。
- MySQL。
- PostgreSQL。
- SQL Server。
- 测试用内存实现。

## 第 2 步：Repository 适合放在哪里

推荐结构：

```text
Repositories/
  IPlayerRepository.cs
  EfPlayerRepository.cs
```

也有人放在：

```text
Persistence/
  IPlayerRepository.cs
  EfPlayerRepository.cs
```

本课程建议：

```text
Repositories/
```

因为它能明确表达这是数据访问抽象。

当前项目结构会变成：

```text
Models/
  Player.cs
Dtos/
  PlayerSummaryDto.cs
  RankingPlayerDto.cs
  RegionStatDto.cs
Persistence/
  PlayerDbContext.cs
Repositories/
  IPlayerRepository.cs
  EfPlayerRepository.cs
Services/
  PlayerApplication.cs
Common/
  Result.cs
```

## 第 3 步：设计 `IPlayerRepository`

先创建：

```text
Repositories/IPlayerRepository.cs
```

代码：

```csharp
using gameC_.Models;

namespace gameC_.Repositories;

public interface IPlayerRepository
{
    Task AddAsync(Player player, CancellationToken cancellationToken = default);

    Task<Player?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Player?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<Player>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<List<Player>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<List<Player>> GetByRegionAsync(
        string region,
        CancellationToken cancellationToken = default);

    Task<List<Player>> SearchByNameAsync(
        string keyword,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    void Remove(Player player);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

说明：

- `GetByIdAsync` 用于只读查询。
- `GetTrackedByIdAsync` 用于修改或删除前查询。
- `GetPagedAsync` 用于分页。
- `ExistsByNameAsync` 用于重名检查。
- `Remove` 只标记删除，真正提交由 `SaveChangesAsync` 完成。

为什么有两个按 ID 查询？

```text
只读查询：AsNoTracking，更轻。
修改/删除：需要 EF Core 跟踪实体。
```

## 第 4 步：创建 `EfPlayerRepository`

创建：

```text
Repositories/EfPlayerRepository.cs
```

基础结构：

```csharp
using gameC_.Models;
using gameC_.Persistence;
using Microsoft.EntityFrameworkCore;

namespace gameC_.Repositories;

public sealed class EfPlayerRepository : IPlayerRepository
{
    private readonly PlayerDbContext _dbContext;

    public EfPlayerRepository(PlayerDbContext dbContext)
    {
        _dbContext = dbContext;
    }
}
```

Repository 通过构造函数注入：

```csharp
PlayerDbContext
```

这是 EF Core 数据访问的入口。

## 第 5 步：实现新增玩家

```csharp
public async Task AddAsync(
    Player player,
    CancellationToken cancellationToken = default)
{
    await _dbContext.Players.AddAsync(player, cancellationToken);
}
```

注意：

```csharp
AddAsync
```

只是把实体加入 EF Core 变更跟踪。

真正写入数据库要调用：

```csharp
await _dbContext.SaveChangesAsync(cancellationToken);
```

这和 EF Core 官方推荐的 SaveChanges 模式一致：先 Add / Update / Remove，再统一 `SaveChangesAsync`。

## 第 6 步：实现 `SaveChangesAsync`

```csharp
public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    return _dbContext.SaveChangesAsync(cancellationToken);
}
```

返回值是受影响的行数。

调用示例：

```csharp
await repository.AddAsync(player);
await repository.SaveChangesAsync();
```

好处：

- 一个业务流程可以包含多次 Add / Remove / 修改。
- 最后统一提交。
- 默认情况下，关系型数据库中的 `SaveChanges` 通常是事务性的。

## 第 7 步：实现按 ID 查询

只读查询：

```csharp
public async Task<Player?> GetByIdAsync(
    Guid id,
    CancellationToken cancellationToken = default)
{
    return await _dbContext.Players
        .AsNoTracking()
        .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
}
```

可跟踪查询：

```csharp
public async Task<Player?> GetTrackedByIdAsync(
    Guid id,
    CancellationToken cancellationToken = default)
{
    return await _dbContext.Players
        .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
}
```

区别：

| 方法 | 是否跟踪 | 适合场景 |
| --- | --- | --- |
| `GetByIdAsync` | 否 | 查询、展示 |
| `GetTrackedByIdAsync` | 是 | 修改、删除 |

`AsNoTracking` 适合只读查询，因为 EF Core 不需要跟踪实体变化，开销更小。

## 第 8 步：实现查询全部玩家

```csharp
public async Task<List<Player>> GetAllAsync(
    CancellationToken cancellationToken = default)
{
    return await _dbContext.Players
        .AsNoTracking()
        .OrderByDescending(p => p.Level)
        .ThenBy(p => p.Id)
        .ToListAsync(cancellationToken);
}
```

注意：

```csharp
ThenBy(p => p.Id)
```

它让排序更稳定。

数据库默认没有可靠顺序。只要你关心顺序，就应该显式 `OrderBy`。

## 第 9 步：实现分页查询

分页参数：

```text
pageNumber: 第几页，从 1 开始
pageSize: 每页多少条
```

实现：

```csharp
public async Task<List<Player>> GetPagedAsync(
    int pageNumber,
    int pageSize,
    CancellationToken cancellationToken = default)
{
    int skip = (pageNumber - 1) * pageSize;

    return await _dbContext.Players
        .AsNoTracking()
        .OrderByDescending(p => p.Level)
        .ThenBy(p => p.Id)
        .Skip(skip)
        .Take(pageSize)
        .ToListAsync(cancellationToken);
}
```

重要原则：

```text
分页必须有稳定排序。
```

不要写：

```csharp
_dbContext.Players.Skip(skip).Take(pageSize)
```

因为数据库不保证默认顺序。

更稳：

```csharp
OrderByDescending(p => p.Level)
ThenBy(p => p.Id)
```

EF Core 官方文档也提醒，分页排序应该尽量唯一，否则翻页时可能出现跳过或重复数据。

## 第 10 步：分页参数校验

Repository 可以假设参数已经被上层校验。

但为了安全，也可以做保护：

```csharp
pageNumber = Math.Max(pageNumber, 1);
pageSize = Math.Clamp(pageSize, 1, 100);
```

完整：

```csharp
public async Task<List<Player>> GetPagedAsync(
    int pageNumber,
    int pageSize,
    CancellationToken cancellationToken = default)
{
    pageNumber = Math.Max(pageNumber, 1);
    pageSize = Math.Clamp(pageSize, 1, 100);

    int skip = (pageNumber - 1) * pageSize;

    return await _dbContext.Players
        .AsNoTracking()
        .OrderByDescending(p => p.Level)
        .ThenBy(p => p.Id)
        .Skip(skip)
        .Take(pageSize)
        .ToListAsync(cancellationToken);
}
```

建议：

- API 层或应用服务层负责返回参数错误。
- Repository 层可以做底线保护，避免危险查询。

## 第 11 步：实现按区服查询

```csharp
public async Task<List<Player>> GetByRegionAsync(
    string region,
    CancellationToken cancellationToken = default)
{
    return await _dbContext.Players
        .AsNoTracking()
        .Where(p => p.Region == region)
        .OrderByDescending(p => p.Level)
        .ThenBy(p => p.Id)
        .ToListAsync(cancellationToken);
}
```

如果只查询活跃玩家：

```csharp
.Where(p => p.Region == region && p.IsActive)
```

后续可以根据接口需求决定是否只返回活跃玩家。

## 第 12 步：实现按名称搜索

基础写法：

```csharp
public async Task<List<Player>> SearchByNameAsync(
    string keyword,
    CancellationToken cancellationToken = default)
{
    return await _dbContext.Players
        .AsNoTracking()
        .Where(p => p.Name.Contains(keyword))
        .OrderByDescending(p => p.Level)
        .ThenBy(p => p.Id)
        .ToListAsync(cancellationToken);
}
```

如果关键字为空：

```csharp
if (string.IsNullOrWhiteSpace(keyword))
{
    return new List<Player>();
}
```

完整：

```csharp
public async Task<List<Player>> SearchByNameAsync(
    string keyword,
    CancellationToken cancellationToken = default)
{
    if (string.IsNullOrWhiteSpace(keyword))
    {
        return new List<Player>();
    }

    return await _dbContext.Players
        .AsNoTracking()
        .Where(p => p.Name.Contains(keyword))
        .OrderByDescending(p => p.Level)
        .ThenBy(p => p.Id)
        .ToListAsync(cancellationToken);
}
```

注意：

- `Contains` 通常会转换成 SQL `LIKE`。
- 不同数据库对大小写敏感规则不同。
- SQLite 默认排序和大小写规则与其他数据库可能不同。

如果要更精确控制搜索规则，后续需要学习 Collation、全文索引或数据库特定函数。

## 第 13 步：实现重名检查

```csharp
public async Task<bool> ExistsByNameAsync(
    string name,
    CancellationToken cancellationToken = default)
{
    return await _dbContext.Players
        .AsNoTracking()
        .AnyAsync(p => p.Name == name, cancellationToken);
}
```

如果要忽略大小写，初学阶段可以先写：

```csharp
string normalizedName = name.ToUpper();

return await _dbContext.Players
    .AsNoTracking()
    .AnyAsync(p => p.Name.ToUpper() == normalizedName, cancellationToken);
```

注意：

- 对列调用 `ToUpper()` 可能影响索引使用。
- 更专业的方式是数据库 Collation 或规范化字段。

本课先掌握 `AnyAsync`。

## 第 14 步：实现删除

```csharp
public void Remove(Player player)
{
    _dbContext.Players.Remove(player);
}
```

使用方式：

```csharp
Player? player = await repository.GetTrackedByIdAsync(id);

if (player is null)
{
    return Result.Failure("玩家不存在，无法删除");
}

repository.Remove(player);
await repository.SaveChangesAsync();
```

删除必须用 tracked entity 更自然。

如果你用 `AsNoTracking` 查出来，再 `Remove`，EF Core 也可以附加并删除，但初学阶段建议保持简单：

```text
要修改或删除，就查 tracked entity。
只展示，就 AsNoTracking。
```

## 第 15 步：完整 `EfPlayerRepository`

```csharp
using gameC_.Models;
using gameC_.Persistence;
using Microsoft.EntityFrameworkCore;

namespace gameC_.Repositories;

public sealed class EfPlayerRepository : IPlayerRepository
{
    private readonly PlayerDbContext _dbContext;

    public EfPlayerRepository(PlayerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        Player player,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Players.AddAsync(player, cancellationToken);
    }

    public async Task<Player?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Players
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Player?> GetTrackedByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Players
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<List<Player>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Players
            .AsNoTracking()
            .OrderByDescending(p => p.Level)
            .ThenBy(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Player>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        int skip = (pageNumber - 1) * pageSize;

        return await _dbContext.Players
            .AsNoTracking()
            .OrderByDescending(p => p.Level)
            .ThenBy(p => p.Id)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Player>> GetByRegionAsync(
        string region,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Players
            .AsNoTracking()
            .Where(p => p.Region == region)
            .OrderByDescending(p => p.Level)
            .ThenBy(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Player>> SearchByNameAsync(
        string keyword,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return new List<Player>();
        }

        return await _dbContext.Players
            .AsNoTracking()
            .Where(p => p.Name.Contains(keyword))
            .OrderByDescending(p => p.Level)
            .ThenBy(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Players
            .AsNoTracking()
            .AnyAsync(p => p.Name == name, cancellationToken);
    }

    public void Remove(Player player)
    {
        _dbContext.Players.Remove(player);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
```

## 第 16 步：注册 Repository

在 `Program.cs` 中：

```csharp
using gameC_.Repositories;
```

注册：

```csharp
builder.Services.AddScoped<IPlayerRepository, EfPlayerRepository>();
```

如果你准备彻底使用 Repository，可以逐步移除：

```csharp
IPlayerStorage
EfPlayerStorage
JsonPlayerStorage
PlayerManager 的内存字典
```

但迁移建议分两步：

1. 先新增 Repository，不删除旧代码。
2. 再改 `PlayerApplication` 使用 Repository。

这样更稳。

## 第 17 步：创建数据库版 `PlayerApplication`

当前 `PlayerApplication` 可能依赖：

```csharp
PlayerManager
IPlayerStorage
```

数据库版可以改成依赖：

```csharp
IPlayerRepository
```

示例：

```csharp
using gameC_.Common;
using gameC_.Dtos;
using gameC_.Models;
using gameC_.Repositories;

namespace gameC_.Services;

public sealed class PlayerApplication
{
    private readonly IPlayerRepository _repository;

    public PlayerApplication(IPlayerRepository repository)
    {
        _repository = repository;
    }
}
```

这一步会改变应用服务的核心逻辑。

## 第 18 步：实现新增玩家

```csharp
public async Task<Result<Player>> AddPlayerAsync(
    Player player,
    CancellationToken cancellationToken = default)
{
    if (string.IsNullOrWhiteSpace(player.Name))
    {
        return Result<Player>.Failure("玩家名称不能为空");
    }

    bool nameExists = await _repository.ExistsByNameAsync(
        player.Name,
        cancellationToken);

    if (nameExists)
    {
        return Result<Player>.Failure("玩家名称已存在");
    }

    await _repository.AddAsync(player, cancellationToken);
    await _repository.SaveChangesAsync(cancellationToken);

    return Result<Player>.Success(player);
}
```

注意：

- 方法变成 `Async`。
- 重名检查走数据库。
- 新增后立即 `SaveChangesAsync`。
- 返回 `Result<Player>`，成功时带回新玩家。

## 第 19 步：实现按 ID 查询

```csharp
public async Task<Result<PlayerSummaryDto>> GetPlayerAsync(
    Guid id,
    CancellationToken cancellationToken = default)
{
    Player? player = await _repository.GetByIdAsync(id, cancellationToken);

    if (player is null)
    {
        return Result<PlayerSummaryDto>.Failure("玩家不存在");
    }

    return Result<PlayerSummaryDto>.Success(ToSummaryDto(player));
}
```

辅助方法：

```csharp
private static PlayerSummaryDto ToSummaryDto(Player player)
{
    return new PlayerSummaryDto
    {
        PlayerId = player.Id,
        Name = player.Name,
        RegionName = player.GetRegionName(),
        Level = player.Level,
        IsActive = player.IsActive
    };
}
```

## 第 20 步：实现分页查询

先创建分页结果 DTO：

```text
Dtos/PagedResult.cs
```

代码：

```csharp
namespace gameC_.Dtos;

public sealed class PagedResult<T>
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public IReadOnlyList<T> Items { get; init; } = [];
}
```

应用服务：

```csharp
public async Task<PagedResult<PlayerSummaryDto>> GetPlayersAsync(
    int pageNumber,
    int pageSize,
    CancellationToken cancellationToken = default)
{
    pageNumber = Math.Max(pageNumber, 1);
    pageSize = Math.Clamp(pageSize, 1, 100);

    List<Player> players = await _repository.GetPagedAsync(
        pageNumber,
        pageSize,
        cancellationToken);

    return new PagedResult<PlayerSummaryDto>
    {
        PageNumber = pageNumber,
        PageSize = pageSize,
        Items = players.Select(ToSummaryDto).ToList()
    };
}
```

这里暂时不返回总数。

如果需要总数，可以再给 Repository 增加：

```csharp
Task<int> CountAsync(CancellationToken cancellationToken = default);
```

## 第 21 步：实现按区服查询

```csharp
public async Task<List<PlayerSummaryDto>> GetPlayersByRegionAsync(
    string region,
    CancellationToken cancellationToken = default)
{
    List<Player> players = await _repository.GetByRegionAsync(
        region,
        cancellationToken);

    return players
        .Select(ToSummaryDto)
        .ToList();
}
```

如果区服为空：

```csharp
if (string.IsNullOrWhiteSpace(region))
{
    return new List<PlayerSummaryDto>();
}
```

## 第 22 步：实现按名称搜索

```csharp
public async Task<List<PlayerSummaryDto>> SearchPlayersByNameAsync(
    string keyword,
    CancellationToken cancellationToken = default)
{
    List<Player> players = await _repository.SearchByNameAsync(
        keyword,
        cancellationToken);

    return players
        .Select(ToSummaryDto)
        .ToList();
}
```

接口可以设计成：

```text
GET /players/search?keyword=ali
```

或：

```text
GET /players?keyword=ali
```

本课建议先用单独接口，容易理解。

## 第 23 步：实现删除玩家

```csharp
public async Task<Result> RemoveByIdAsync(
    Guid id,
    CancellationToken cancellationToken = default)
{
    Player? player = await _repository.GetTrackedByIdAsync(
        id,
        cancellationToken);

    if (player is null)
    {
        return Result.Failure("玩家不存在，无法删除");
    }

    _repository.Remove(player);
    await _repository.SaveChangesAsync(cancellationToken);

    return Result.Success();
}
```

这里必须使用：

```csharp
GetTrackedByIdAsync
```

因为删除需要 EF Core 跟踪实体状态。

## 第 24 步：实现禁用玩家

```csharp
public async Task<Result> DisableByIdAsync(
    Guid id,
    CancellationToken cancellationToken = default)
{
    Player? player = await _repository.GetTrackedByIdAsync(
        id,
        cancellationToken);

    if (player is null)
    {
        return Result.Failure("玩家不存在，无法禁用");
    }

    if (!player.IsActive)
    {
        return Result.Failure("玩家已经是禁用状态");
    }

    player.IsActive = false;

    await _repository.SaveChangesAsync(cancellationToken);

    return Result.Success();
}
```

EF Core 会跟踪 `player.IsActive` 的变化。

调用 `SaveChangesAsync` 后，修改会写入数据库。

## 第 25 步：Endpoint 改造方向

旧接口：

```csharp
Result result = playerApp.AddPlayer(player);
await playerApp.SaveAsync();
```

新接口：

```csharp
Result<Player> result = await playerApp.AddPlayerAsync(player);
```

因为保存已经在应用服务内部完成，不需要 endpoint 再调用 `SaveAsync`。

新增玩家 endpoint 示例：

```csharp
private static async Task<IResult> CreatePlayer(
    CreatePlayerRequest request,
    PlayerApplication playerApp,
    CancellationToken cancellationToken)
{
    Player player = new Player
    {
        Name = request.Name,
        Level = request.Level,
        Region = request.Region,
        Gold = request.Gold
    };

    Result<Player> result = await playerApp.AddPlayerAsync(
        player,
        cancellationToken);

    if (result.IsFailure)
    {
        return Results.BadRequest(ErrorResponse.From(result.ErrorMessage));
    }

    Player createdPlayer = result.Value!;

    return Results.Created($"/players/{createdPlayer.Id}", createdPlayer);
}
```

分页查询 endpoint：

```csharp
private static async Task<IResult> GetPlayers(
    int? pageNumber,
    int? pageSize,
    PlayerApplication playerApp,
    CancellationToken cancellationToken)
{
    PagedResult<PlayerSummaryDto> result = await playerApp.GetPlayersAsync(
        pageNumber.GetValueOrDefault(1),
        pageSize.GetValueOrDefault(20),
        cancellationToken);

    return Results.Ok(result);
}
```

## 第 26 步：完整练习

练习目标：使用 Repository 替代内存字典作为主要数据来源。

要求：

1. 创建：

```text
Repositories/IPlayerRepository.cs
Repositories/EfPlayerRepository.cs
```

2. `IPlayerRepository` 至少包含：

```csharp
AddAsync
GetByIdAsync
GetTrackedByIdAsync
GetPagedAsync
GetByRegionAsync
SearchByNameAsync
ExistsByNameAsync
Remove
SaveChangesAsync
```

3. `EfPlayerRepository` 使用 `PlayerDbContext` 实现。
4. 只读查询使用 `AsNoTracking`。
5. 分页查询使用：

```csharp
OrderBy
ThenBy
Skip
Take
```

6. 在 DI 中注册：

```csharp
builder.Services.AddScoped<IPlayerRepository, EfPlayerRepository>();
```

7. 改造 `PlayerApplication`，依赖 `IPlayerRepository`。
8. 改造新增、查询、删除、禁用为异步数据库操作。
9. 新增分页查询接口：

```text
GET /players?pageNumber=1&pageSize=20
```

10. 新增名称搜索接口：

```text
GET /players/search?keyword=ali
```

11. 新增区服查询接口：

```text
GET /players/by-region/CN
```

验收标准：

- 新增玩家直接写入数据库。
- 查询玩家从数据库读取。
- 删除玩家后数据库记录消失。
- 禁用玩家后数据库字段变化。
- 分页接口能限制返回数量。
- 名称搜索能返回匹配玩家。
- 按区服查询能返回对应玩家。
- 重启服务后数据仍然存在。

## 第 27 步：本课作业

### 作业 1：返回总数

给 `IPlayerRepository` 增加：

```csharp
Task<int> CountAsync(CancellationToken cancellationToken = default);
```

把 `PagedResult<T>` 改成：

```csharp
public sealed class PagedResult<T>
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public IReadOnlyList<T> Items { get; init; } = [];
}
```

要求：

- 分页接口返回总数。
- 前端可以根据总数计算总页数。

### 作业 2：只查询活跃玩家

给 Repository 增加：

```csharp
Task<List<Player>> GetActivePlayersAsync(CancellationToken cancellationToken = default);
```

要求：

- 只返回 `IsActive = true` 的玩家。
- 按等级降序。

### 作业 3：排行榜改为数据库查询

给 Repository 增加：

```csharp
Task<List<Player>> GetTopByPowerAsync(int count, CancellationToken cancellationToken = default);
```

注意：

`CalculatePower()` 是 C# 方法，EF Core 不一定能翻译成 SQL。

可以先写：

```csharp
.OrderByDescending(p => p.Level * 100 + p.Gold / 10)
```

要求：

- 只查询活跃玩家。
- 返回前 count 条。

### 作业 4：名称唯一索引

在 `OnModelCreating` 中增加：

```csharp
entity.HasIndex(p => p.Name)
    .IsUnique();
```

然后创建迁移。

思考：

- 应用层已经检查重名，为什么数据库层还要唯一索引？
- 如果两个请求同时创建同名玩家，会发生什么？

## 本课常见错误

### 1. 查询接口没有 `AsNoTracking`

只读查询推荐：

```csharp
_dbContext.Players.AsNoTracking()
```

否则 EF Core 会跟踪查询结果，增加不必要开销。

### 2. 分页没有排序

风险写法：

```csharp
_dbContext.Players.Skip(skip).Take(pageSize)
```

推荐：

```csharp
_dbContext.Players
    .OrderByDescending(p => p.Level)
    .ThenBy(p => p.Id)
    .Skip(skip)
    .Take(pageSize)
```

### 3. 修改实体时用了 NoTracking 查询

风险：

```csharp
Player? player = await repository.GetByIdAsync(id);
player.IsActive = false;
await repository.SaveChangesAsync();
```

如果 `GetByIdAsync` 使用 `AsNoTracking`，修改可能不会被保存。

推荐：

```csharp
Player? player = await repository.GetTrackedByIdAsync(id);
```

### 4. 忘记调用 `SaveChangesAsync`

新增：

```csharp
await repository.AddAsync(player);
```

还不够。

必须：

```csharp
await repository.SaveChangesAsync();
```

### 5. 把 `DbContext` 暴露到 Endpoint

不推荐：

```csharp
app.MapGet("/players", (PlayerDbContext db) => ...)
```

入门可以这样演示，但项目结构中更推荐：

```text
Endpoint -> Application -> Repository -> DbContext
```

## 本课复盘问题

学完后，尝试回答：

1. Repository 模式解决什么问题？
2. 为什么数据库项目不适合整体 `SaveAsync(IEnumerable<Player>)`？
3. `AsNoTracking` 适合什么场景？
4. 修改或删除实体时为什么需要 tracked entity？
5. `AddAsync` 后为什么还要 `SaveChangesAsync`？
6. 分页为什么必须有稳定排序？
7. `Skip` 和 `Take` 分别做什么？
8. `AnyAsync` 适合什么场景？
9. 为什么 Endpoint 不应该直接依赖 `DbContext`？
10. 应用层重名检查和数据库唯一索引分别解决什么问题？

## 下一课预告

课程十七建议学习：

- EF Core 关系建模。
- 新增 `MatchRecord` 战绩实体。
- 配置 Player 与 MatchRecord 一对多关系。
- 查询玩家战绩。
- 统计胜率、KDA 和总分。

## 参考资料

- Microsoft Learn: Basic SaveChanges
  - https://learn.microsoft.com/ef/core/saving/basic
- Microsoft Learn: Tracking vs. No-Tracking Queries
  - https://learn.microsoft.com/ef/core/querying/tracking
- Microsoft Learn: Pagination
  - https://learn.microsoft.com/ef/core/querying/pagination
- Microsoft Learn: Efficient Querying
  - https://learn.microsoft.com/ef/core/performance/efficient-querying
