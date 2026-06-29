# C# 基础学习专刊：课程五

## 课程主题

LINQ 进阶：学习 `Where`、`Select`、`GroupBy`、`Any`、`All`，统计玩家区服分布，并生成排行榜 DTO。

课程三和课程四中，我们已经用过一些 LINQ：

```csharp
List<Player> topPlayers = players
    .OrderByDescending(p => p.CalculatePower())
    .Take(10)
    .ToList();
```

这一课会系统整理 LINQ 的高频用法，让你能更自然地处理集合数据。

## 本课目标

完成本课后，你应该能做到：

- 使用 `Where` 筛选数据。
- 使用 `Select` 转换数据。
- 使用 `GroupBy` 分组统计。
- 使用 `Any` 判断是否存在。
- 使用 `All` 判断是否全部满足条件。
- 理解 LINQ 的延迟执行。
- 统计玩家区服分布。
- 创建排行榜 DTO。
- 把 `Player` 实体转换成展示用数据。

## 第 1 步：什么是 LINQ

LINQ 全称是 Language Integrated Query，意思是语言集成查询。

简单说，它让你用统一写法处理集合：

```csharp
List<Player> cnPlayers = players
    .Where(p => p.Region == "CN")
    .ToList();
```

你可以把 LINQ 理解成 C# 里的集合查询工具。

常见使用场景：

- 筛选玩家。
- 排序玩家。
- 统计数据。
- 分组统计。
- 把实体对象转换成 DTO。
- 判断集合中是否存在某类数据。

## 第 2 步：准备 `Player` 类

本课继续使用之前的 `Player`：

```csharp
namespace gameC_.Models;

public sealed class Player
{
    private int _level = 1;
    private int _gold;

    public Guid Id { get; init; } = Guid.NewGuid();

    public string Name { get; set; } = "未命名玩家";

    public int Level
    {
        get => _level;
        set => _level = value < 1 ? 1 : value;
    }

    public string Region { get; set; } = "CN";

    public int Gold
    {
        get => _gold;
        set => _gold = value < 0 ? 0 : value;
    }

    public bool IsActive { get; set; } = true;

    public int CalculatePower()
    {
        return Level * 100 + Gold / 10;
    }

    public string GetRegionName()
    {
        return Region switch
        {
            "CN" => "国服",
            "NA" => "北美服",
            "EU" => "欧洲服",
            _ => "未知区服"
        };
    }
}
```

准备测试数据：

```csharp
List<Player> players =
[
    new Player { Name = "Alice", Level = 10, Region = "CN", Gold = 500 },
    new Player { Name = "Bob", Level = 25, Region = "NA", Gold = 1200 },
    new Player { Name = "Cindy", Level = 35, Region = "EU", Gold = 3000 },
    new Player { Name = "David", Level = 18, Region = "CN", Gold = 900 },
    new Player { Name = "Ellen", Level = 42, Region = "CN", Gold = 5000, IsActive = false }
];
```

## 第 3 步：`Where` 筛选数据

`Where` 用于筛选符合条件的数据。

### 筛选国服玩家

```csharp
List<Player> cnPlayers = players
    .Where(p => p.Region == "CN")
    .ToList();
```

### 筛选活跃玩家

```csharp
List<Player> activePlayers = players
    .Where(p => p.IsActive)
    .ToList();
```

### 筛选等级大于等于 20 的玩家

```csharp
List<Player> highLevelPlayers = players
    .Where(p => p.Level >= 20)
    .ToList();
```

### 多条件筛选

```csharp
List<Player> result = players
    .Where(p => p.Region == "CN" && p.IsActive && p.Level >= 10)
    .ToList();
```

`Where` 的核心是：

```text
保留满足条件的元素。
```

## 第 4 步：`Select` 转换数据

`Select` 用于把一种数据转换成另一种数据。

### 只取玩家名称

```csharp
List<string> names = players
    .Select(p => p.Name)
    .ToList();
```

原来是：

```text
List<Player>
```

转换后是：

```text
List<string>
```

### 转换成摘要字符串

```csharp
List<string> summaries = players
    .Select(p => $"{p.Name} Lv.{p.Level} {p.GetRegionName()}")
    .ToList();
```

### 转换成匿名对象

```csharp
var playerViews = players
    .Select(p => new
    {
        p.Name,
        p.Level,
        Power = p.CalculatePower()
    })
    .ToList();
```

匿名对象适合临时使用，不适合跨方法、跨层长期传递。

如果数据要作为接口返回、页面展示、日志输出，建议定义 DTO。

## 第 5 步：什么是 DTO

DTO 全称是 Data Transfer Object，数据传输对象。

它的作用是：

```text
只携带当前场景需要的数据。
```

例如 `Player` 是业务实体，字段可能很多：

```text
Id, Name, Level, Region, Gold, IsActive, CreatedAt, UpdatedAt ...
```

排行榜页面可能只需要：

```text
Rank, PlayerId, Name, RegionName, Level, Power
```

这时候就可以创建 `RankingPlayerDto`。

## 第 6 步：创建排行榜 DTO

推荐目录：

```text
Dtos/
  RankingPlayerDto.cs
```

代码：

```csharp
namespace gameC_.Dtos;

public sealed class RankingPlayerDto
{
    public int Rank { get; init; }
    public Guid PlayerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string RegionName { get; init; } = string.Empty;
    public int Level { get; init; }
    public int Power { get; init; }
}
```

说明：

- DTO 通常只保存数据。
- `init` 表示对象初始化后不能再修改。
- DTO 不应该塞太多业务逻辑。

## 第 7 步：生成排行榜 DTO

```csharp
List<RankingPlayerDto> ranking = players
    .Where(p => p.IsActive)
    .OrderByDescending(p => p.CalculatePower())
    .Select((p, index) => new RankingPlayerDto
    {
        Rank = index + 1,
        PlayerId = p.Id,
        Name = p.Name,
        RegionName = p.GetRegionName(),
        Level = p.Level,
        Power = p.CalculatePower()
    })
    .ToList();
```

这里有一个新写法：

```csharp
.Select((p, index) => ...)
```

它可以同时拿到：

- `p`：当前玩家。
- `index`：当前索引，从 0 开始。

所以排名可以写成：

```csharp
Rank = index + 1
```

## 第 8 步：输出排行榜

```csharp
foreach (RankingPlayerDto item in ranking)
{
    Console.WriteLine($"{item.Rank}. {item.Name} {item.RegionName} Lv.{item.Level} 战力:{item.Power}");
}
```

输出示例：

```text
1. Cindy 欧洲服 Lv.35 战力:3800
2. Bob 北美服 Lv.25 战力:2620
3. David 国服 Lv.18 战力:1890
4. Alice 国服 Lv.10 战力:1050
```

注意：`Ellen` 被设置为 `IsActive = false`，所以不会出现在排行榜中。

## 第 9 步：`Any` 判断是否存在

`Any` 用于判断集合中是否存在至少一个满足条件的元素。

### 是否有玩家

```csharp
bool hasPlayers = players.Any();
```

### 是否有国服玩家

```csharp
bool hasCnPlayers = players.Any(p => p.Region == "CN");
```

### 是否有重名玩家

```csharp
string name = "Alice";

bool exists = players.Any(p =>
    string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
```

适合用在新增玩家前检查：

```csharp
if (exists)
{
    Console.WriteLine("玩家名称已存在");
}
```

## 第 10 步：`All` 判断是否全部满足

`All` 用于判断集合中所有元素是否都满足条件。

### 是否全部活跃

```csharp
bool allActive = players.All(p => p.IsActive);
```

### 是否所有玩家等级都合法

```csharp
bool allLevelValid = players.All(p => p.Level >= 1);
```

### 是否所有玩家都有名称

```csharp
bool allHasName = players.All(p => !string.IsNullOrWhiteSpace(p.Name));
```

注意一个容易忽略的点：

```csharp
List<Player> emptyPlayers = new();
bool result = emptyPlayers.All(p => p.IsActive);
```

空集合调用 `All` 会返回 `true`。

这在数学上叫“空真”，但业务上你要根据场景判断是否合理。

如果必须至少有一个玩家并且全部活跃：

```csharp
bool hasPlayersAndAllActive = players.Any() && players.All(p => p.IsActive);
```

## 第 11 步：`GroupBy` 分组

`GroupBy` 用于按某个 key 把数据分组。

### 按区服分组

```csharp
var groups = players
    .GroupBy(p => p.Region);
```

遍历：

```csharp
foreach (var group in groups)
{
    Console.WriteLine($"区服：{group.Key}");

    foreach (Player player in group)
    {
        Console.WriteLine(player.Name);
    }
}
```

其中：

- `group.Key` 是分组 key，例如 `CN`。
- `group` 本身是一组玩家。

## 第 12 步：统计玩家区服分布

目标输出：

```text
国服：3 人
北美服：1 人
欧洲服：1 人
```

写法：

```csharp
var regionStats = players
    .GroupBy(p => p.Region)
    .Select(g => new
    {
        Region = g.Key,
        Count = g.Count()
    })
    .OrderByDescending(x => x.Count)
    .ToList();
```

输出：

```csharp
foreach (var item in regionStats)
{
    Console.WriteLine($"{item.Region}：{item.Count} 人");
}
```

如果想显示中文区服名，可以写一个辅助方法：

```csharp
static string GetRegionName(string region)
{
    return region switch
    {
        "CN" => "国服",
        "NA" => "北美服",
        "EU" => "欧洲服",
        _ => "未知区服"
    };
}
```

然后：

```csharp
foreach (var item in regionStats)
{
    Console.WriteLine($"{GetRegionName(item.Region)}：{item.Count} 人");
}
```

## 第 13 步：创建区服统计 DTO

匿名对象临时用可以，但正式项目中建议定义 DTO。

推荐：

```text
Dtos/
  RegionStatDto.cs
```

代码：

```csharp
namespace gameC_.Dtos;

public sealed class RegionStatDto
{
    public string Region { get; init; } = string.Empty;
    public string RegionName { get; init; } = string.Empty;
    public int PlayerCount { get; init; }
    public int ActivePlayerCount { get; init; }
}
```

生成统计：

```csharp
List<RegionStatDto> regionStats = players
    .GroupBy(p => p.Region)
    .Select(g => new RegionStatDto
    {
        Region = g.Key,
        RegionName = GetRegionName(g.Key),
        PlayerCount = g.Count(),
        ActivePlayerCount = g.Count(p => p.IsActive)
    })
    .OrderByDescending(x => x.PlayerCount)
    .ToList();
```

输出：

```csharp
foreach (RegionStatDto item in regionStats)
{
    Console.WriteLine($"{item.RegionName}：总人数 {item.PlayerCount}，活跃 {item.ActivePlayerCount}");
}
```

## 第 14 步：组合 LINQ

LINQ 可以链式组合。

例如：获取国服活跃玩家战力 Top 3，并转换成 DTO：

```csharp
List<RankingPlayerDto> cnTopPlayers = players
    .Where(p => p.Region == "CN")
    .Where(p => p.IsActive)
    .OrderByDescending(p => p.CalculatePower())
    .Take(3)
    .Select((p, index) => new RankingPlayerDto
    {
        Rank = index + 1,
        PlayerId = p.Id,
        Name = p.Name,
        RegionName = p.GetRegionName(),
        Level = p.Level,
        Power = p.CalculatePower()
    })
    .ToList();
```

也可以把多个条件合在一个 `Where`：

```csharp
List<RankingPlayerDto> cnTopPlayers = players
    .Where(p => p.Region == "CN" && p.IsActive)
    .OrderByDescending(p => p.CalculatePower())
    .Take(3)
    .Select((p, index) => new RankingPlayerDto
    {
        Rank = index + 1,
        PlayerId = p.Id,
        Name = p.Name,
        RegionName = p.GetRegionName(),
        Level = p.Level,
        Power = p.CalculatePower()
    })
    .ToList();
```

两种都可以。条件复杂时，拆成多个 `Where` 有时更容易读。

## 第 15 步：延迟执行

LINQ 中很多方法是延迟执行的。

例如：

```csharp
IEnumerable<Player> activePlayers = players
    .Where(p => p.IsActive);
```

这行代码不会立刻遍历 `players`。

真正遍历发生在：

```csharp
foreach (Player player in activePlayers)
{
    Console.WriteLine(player.Name);
}
```

或者调用：

```csharp
List<Player> result = activePlayers.ToList();
```

### 延迟执行的影响

```csharp
IEnumerable<Player> activePlayers = players
    .Where(p => p.IsActive);

players.Add(new Player { Name = "Frank", IsActive = true });

foreach (Player player in activePlayers)
{
    Console.WriteLine(player.Name);
}
```

`Frank` 也会被输出，因为查询是在遍历时才真正执行。

如果你想固定当时结果，使用：

```csharp
List<Player> activePlayers = players
    .Where(p => p.IsActive)
    .ToList();
```

一句话记忆：

```text
不调用 ToList / ToArray 前，很多 LINQ 查询只是一个查询计划。
```

## 第 16 步：把 LINQ 放进 `PlayerManager`

可以把统计和排行榜逻辑放进 `PlayerManager`：

```csharp
using gameC_.Dtos;
using gameC_.Models;

namespace gameC_.Services;

public sealed class PlayerManager
{
    private readonly Dictionary<Guid, Player> _playersById = new();

    public IReadOnlyCollection<Player> Players => _playersById.Values;

    public bool AddPlayer(Player player)
    {
        bool exists = _playersById.Values.Any(p =>
            string.Equals(p.Name, player.Name, StringComparison.OrdinalIgnoreCase));

        if (exists)
        {
            return false;
        }

        return _playersById.TryAdd(player.Id, player);
    }

    public List<RankingPlayerDto> GetRanking(int count)
    {
        return _playersById.Values
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CalculatePower())
            .Take(count)
            .Select((p, index) => new RankingPlayerDto
            {
                Rank = index + 1,
                PlayerId = p.Id,
                Name = p.Name,
                RegionName = p.GetRegionName(),
                Level = p.Level,
                Power = p.CalculatePower()
            })
            .ToList();
    }

    public List<RegionStatDto> GetRegionStats()
    {
        return _playersById.Values
            .GroupBy(p => p.Region)
            .Select(g => new RegionStatDto
            {
                Region = g.Key,
                RegionName = GetRegionName(g.Key),
                PlayerCount = g.Count(),
                ActivePlayerCount = g.Count(p => p.IsActive)
            })
            .OrderByDescending(x => x.PlayerCount)
            .ToList();
    }

    public bool HasInactivePlayers()
    {
        return _playersById.Values.Any(p => !p.IsActive);
    }

    public bool AllPlayersHaveValidLevel()
    {
        return _playersById.Values.All(p => p.Level >= 1);
    }

    private static string GetRegionName(string region)
    {
        return region switch
        {
            "CN" => "国服",
            "NA" => "北美服",
            "EU" => "欧洲服",
            _ => "未知区服"
        };
    }
}
```

注意：

- `PlayerManager` 返回 DTO，不直接让 `Program.cs` 自己拼排行榜。
- `AddPlayer` 用 `Any` 检查名称是否重复。
- `GetRegionStats` 用 `GroupBy` 做区服统计。

## 第 17 步：完整练习

练习目标：在玩家管理系统中加入排行榜 DTO 和区服统计。

要求：

1. 创建目录：

```text
Dtos/
```

2. 创建：

```text
RankingPlayerDto.cs
RegionStatDto.cs
```

3. 在 `PlayerManager` 中增加：

```csharp
public List<RankingPlayerDto> GetRanking(int count)
public List<RegionStatDto> GetRegionStats()
public bool HasInactivePlayers()
public bool AllPlayersHaveValidLevel()
```

4. 菜单中增加：

```text
6. 显示区服统计
7. 检查是否有非活跃玩家
8. 检查所有玩家等级是否合法
```

5. 排行榜输出使用 `RankingPlayerDto`。

验收标准：

- 排行榜只显示活跃玩家。
- 排行榜包含排名、名称、区服、等级、战力。
- 区服统计包含总人数和活跃人数。
- 新增重名玩家时能拒绝。
- `Any`、`All`、`GroupBy`、`Select` 都至少使用一次。

## 第 18 步：本课作业

### 作业 1：生成玩家摘要 DTO

创建：

```csharp
public sealed class PlayerSummaryDto
{
    public Guid PlayerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string RegionName { get; init; } = string.Empty;
    public int Level { get; init; }
    public bool IsActive { get; init; }
}
```

在 `PlayerManager` 中增加：

```csharp
public List<PlayerSummaryDto> GetPlayerSummaries()
```

要求：

- 使用 `Select`。
- 按等级降序。

### 作业 2：统计等级段

统计玩家等级段：

```text
1-9：新手
10-29：中级
30+：高级
```

要求：

- 使用 `GroupBy`。
- 输出每个等级段的人数。

提示：

```csharp
static string GetLevelStage(int level)
{
    if (level >= 30)
    {
        return "高级";
    }

    if (level >= 10)
    {
        return "中级";
    }

    return "新手";
}
```

### 作业 3：检查异常数据

使用 `Any` 检查：

- 是否存在空名称玩家。
- 是否存在金币为 0 的玩家。
- 是否存在未知区服玩家。

### 作业 4：检查全部数据是否合法

使用 `All` 检查：

- 所有玩家等级是否大于等于 1。
- 所有玩家金币是否大于等于 0。
- 所有玩家名称是否不为空。

## 本课常见错误

### 1. 忘记 `ToList`

如果方法返回类型是：

```csharp
public List<Player> GetActivePlayers()
```

那么需要：

```csharp
return _playersById.Values
    .Where(p => p.IsActive)
    .ToList();
```

不能直接返回：

```csharp
return _playersById.Values.Where(p => p.IsActive);
```

因为后者类型是 `IEnumerable<Player>`。

### 2. 把实体直接当展示数据

不推荐排行榜直接返回完整 `Player`：

```csharp
public List<Player> GetRanking()
```

更推荐返回 DTO：

```csharp
public List<RankingPlayerDto> GetRanking()
```

这样展示层只拿到它需要的数据。

### 3. 对空集合使用 `All` 后误判

```csharp
bool allActive = players.All(p => p.IsActive);
```

如果 `players` 是空集合，结果是 `true`。

如果业务要求必须有玩家：

```csharp
bool allActive = players.Any() && players.All(p => p.IsActive);
```

### 4. 重复计算昂贵逻辑

如果 `CalculatePower()` 未来变复杂，下面可能重复计算：

```csharp
players
    .OrderByDescending(p => p.CalculatePower())
    .Select(p => new RankingPlayerDto
    {
        Power = p.CalculatePower()
    });
```

可以先投影：

```csharp
var ranking = players
    .Select(p => new
    {
        Player = p,
        Power = p.CalculatePower()
    })
    .OrderByDescending(x => x.Power)
    .Select((x, index) => new RankingPlayerDto
    {
        Rank = index + 1,
        PlayerId = x.Player.Id,
        Name = x.Player.Name,
        RegionName = x.Player.GetRegionName(),
        Level = x.Player.Level,
        Power = x.Power
    })
    .ToList();
```

### 5. LINQ 链太长难读

如果一条 LINQ 太长，可以拆变量：

```csharp
IEnumerable<Player> activePlayers = players.Where(p => p.IsActive);

IOrderedEnumerable<Player> orderedPlayers = activePlayers
    .OrderByDescending(p => p.CalculatePower());

List<Player> topPlayers = orderedPlayers
    .Take(10)
    .ToList();
```

清晰比炫技重要。

## 本课复盘问题

学完后，尝试回答：

1. `Where` 是做什么的？
2. `Select` 是做什么的？
3. `GroupBy` 的 `Key` 是什么？
4. `Any` 和 `All` 分别适合什么场景？
5. 空集合调用 `All` 会返回什么？
6. 什么是 DTO？
7. 为什么排行榜适合返回 DTO，而不是直接返回 `Player`？
8. `Select((p, index) => ...)` 中的 `index` 从几开始？
9. LINQ 延迟执行是什么意思？
10. 什么时候应该调用 `ToList()`？

## 下一课预告

课程六建议学习：

- 文件读写与 JSON。
- 使用 `System.Text.Json` 保存玩家数据。
- 从文件加载玩家列表。
- 给玩家管理系统增加持久化能力。
