# C# 基础学习专刊：课程三

## 课程主题

集合基础：用 `List<Player>` 管理多个玩家，支持新增、删除、查找和排序。

课程二中，我们已经把单个玩家的信息整理成了 `Player` 类：

```csharp
Player player = new Player
{
    Name = "Alice",
    Level = 10,
    Region = "CN",
    Gold = 500
};
```

但真实项目里很少只处理一个玩家。你通常会面对多个玩家：

```csharp
List<Player> players = new();
```

课程三的目标是让你熟悉 C# 中最常用的集合类型之一：`List<T>`。

## 本课目标

完成本课后，你应该能做到：

- 理解 `List<T>` 的基本用途。
- 用 `List<Player>` 保存多个玩家。
- 新增玩家。
- 删除玩家。
- 根据 ID、名称查找玩家。
- 遍历玩家列表。
- 按等级、金币、战力排序。
- 理解 `FirstOrDefault`、`Where`、`OrderBy` 等基础 LINQ 用法。
- 初步把玩家列表管理逻辑拆成 `PlayerManager`。

## 第 1 步：为什么需要集合

如果没有集合，多个玩家可能会写成这样：

```csharp
Player player1 = new Player { Name = "Alice", Level = 10 };
Player player2 = new Player { Name = "Bob", Level = 20 };
Player player3 = new Player { Name = "Cindy", Level = 30 };
```

问题很明显：

- 玩家数量固定死了。
- 不方便遍历。
- 不方便查找。
- 不方便排序。
- 新增和删除都很麻烦。

使用 `List<Player>` 后：

```csharp
List<Player> players = new();

players.Add(new Player { Name = "Alice", Level = 10 });
players.Add(new Player { Name = "Bob", Level = 20 });
players.Add(new Player { Name = "Cindy", Level = 30 });
```

这样就可以用统一方式管理任意数量的玩家。

## 第 2 步：什么是 `List<T>`

`List<T>` 是 C# 中最常用的动态数组。

这里的 `T` 表示元素类型：

```csharp
List<int> numbers = new();
List<string> names = new();
List<Player> players = new();
```

常用操作：

| 操作 | 示例 |
| --- | --- |
| 新增 | `players.Add(player)` |
| 删除 | `players.Remove(player)` |
| 按条件删除 | `players.RemoveAll(p => p.Level < 1)` |
| 数量 | `players.Count` |
| 清空 | `players.Clear()` |
| 是否包含 | `players.Contains(player)` |
| 索引访问 | `players[0]` |

`List<T>` 适合：

- 数据量不特别大。
- 经常按顺序遍历。
- 经常追加元素。
- 需要按索引访问。

不适合：

- 高频按 key 查询大量数据，此时更适合 `Dictionary<TKey, TValue>`。
- 高频判断是否存在，此时可能更适合 `HashSet<T>`。

## 第 3 步：准备 `Player` 类

课程三继续使用课程二的 `Player` 类。

建议 `Models/Player.cs`：

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

    public string GetStage()
    {
        if (Level >= 30)
        {
            return "高级玩家";
        }

        if (Level >= 10)
        {
            return "中级玩家";
        }

        return "新手玩家";
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

    public string GetSummary()
    {
        return $"{Name} Lv.{Level} {GetRegionName()} 战力:{CalculatePower()}";
    }
}
```

## 第 4 步：创建玩家列表

在 `Program.cs` 中：

```csharp
using gameC_.Models;

List<Player> players = new();
```

也可以初始化时直接放几个玩家：

```csharp
List<Player> players =
[
    new Player { Name = "Alice", Level = 10, Region = "CN", Gold = 500 },
    new Player { Name = "Bob", Level = 25, Region = "NA", Gold = 1200 },
    new Player { Name = "Cindy", Level = 35, Region = "EU", Gold = 3000 }
];
```

这是现代 C# 的集合表达式写法。

如果你的项目版本不支持，也可以写成：

```csharp
List<Player> players = new()
{
    new Player { Name = "Alice", Level = 10, Region = "CN", Gold = 500 },
    new Player { Name = "Bob", Level = 25, Region = "NA", Gold = 1200 },
    new Player { Name = "Cindy", Level = 35, Region = "EU", Gold = 3000 }
};
```

## 第 5 步：新增玩家

```csharp
Player player = new Player
{
    Name = "David",
    Level = 15,
    Region = "CN",
    Gold = 800
};

players.Add(player);
```

也可以直接写：

```csharp
players.Add(new Player
{
    Name = "David",
    Level = 15,
    Region = "CN",
    Gold = 800
});
```

新增后查看数量：

```csharp
Console.WriteLine($"当前玩家数量：{players.Count}");
```

## 第 6 步：遍历玩家

最常见的是 `foreach`：

```csharp
foreach (Player player in players)
{
    Console.WriteLine(player.GetSummary());
}
```

也可以使用 `for`：

```csharp
for (int i = 0; i < players.Count; i++)
{
    Player player = players[i];
    Console.WriteLine($"{i + 1}. {player.GetSummary()}");
}
```

选择建议：

- 只是遍历，用 `foreach`。
- 需要索引，用 `for`。

## 第 7 步：根据 ID 查找玩家

每个 `Player` 都有一个 `Guid Id`。

查找指定 ID 的玩家：

```csharp
Guid targetId = players[0].Id;

Player? player = players.FirstOrDefault(p => p.Id == targetId);

if (player is null)
{
    Console.WriteLine("未找到玩家");
}
else
{
    Console.WriteLine($"找到玩家：{player.GetSummary()}");
}
```

说明：

- `FirstOrDefault` 会返回第一个满足条件的元素。
- 如果找不到，会返回 `null`。
- 所以返回类型是 `Player?`。

这里的：

```csharp
p => p.Id == targetId
```

叫 Lambda 表达式，可以理解为一个临时条件函数。

## 第 8 步：根据名称查找玩家

```csharp
Console.Write("请输入要查找的玩家名称：");
string? nameInput = Console.ReadLine();

string name = string.IsNullOrWhiteSpace(nameInput)
    ? string.Empty
    : nameInput;

Player? player = players.FirstOrDefault(p => p.Name == name);

if (player is null)
{
    Console.WriteLine("未找到玩家");
}
else
{
    Console.WriteLine(player.GetSummary());
}
```

如果希望忽略大小写：

```csharp
Player? player = players.FirstOrDefault(p =>
    string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
```

如果希望查找包含关键字的玩家：

```csharp
List<Player> matchedPlayers = players
    .Where(p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
    .ToList();
```

说明：

- `Where` 返回所有满足条件的元素。
- `ToList` 把结果转成 `List<Player>`。

## 第 9 步：删除玩家

### 先查找再删除

```csharp
Player? player = players.FirstOrDefault(p => p.Name == "Alice");

if (player is not null)
{
    players.Remove(player);
}
```

### 按条件删除

```csharp
int removedCount = players.RemoveAll(p => p.Name == "Alice");

Console.WriteLine($"删除数量：{removedCount}");
```

`RemoveAll` 适合按条件批量删除。

注意：不要在 `foreach` 遍历 `List<T>` 时直接删除元素。

错误示例：

```csharp
foreach (Player player in players)
{
    if (player.Level < 10)
    {
        players.Remove(player);
    }
}
```

推荐：

```csharp
players.RemoveAll(p => p.Level < 10);
```

## 第 10 步：修改玩家

因为 `Player` 是引用类型，查找到对象后可以直接修改：

```csharp
Player? player = players.FirstOrDefault(p => p.Name == "Alice");

if (player is not null)
{
    player.Level += 1;
    player.Gold += 100;
}
```

修改后，列表里的对象也已经变了，因为列表保存的是对象引用。

## 第 11 步：排序

### 按等级升序

```csharp
List<Player> sortedPlayers = players
    .OrderBy(p => p.Level)
    .ToList();
```

### 按等级降序

```csharp
List<Player> sortedPlayers = players
    .OrderByDescending(p => p.Level)
    .ToList();
```

### 按战力降序

```csharp
List<Player> sortedPlayers = players
    .OrderByDescending(p => p.CalculatePower())
    .ToList();
```

### 先按区服，再按等级

```csharp
List<Player> sortedPlayers = players
    .OrderBy(p => p.Region)
    .ThenByDescending(p => p.Level)
    .ToList();
```

注意：

- `OrderBy` 不会修改原列表。
- 它会返回一个新的排序结果。
- 如果要覆盖原列表，需要重新赋值。

```csharp
players = players
    .OrderByDescending(p => p.CalculatePower())
    .ToList();
```

## 第 12 步：筛选玩家

### 查询高级玩家

```csharp
List<Player> highLevelPlayers = players
    .Where(p => p.Level >= 30)
    .ToList();
```

### 查询国服玩家

```csharp
List<Player> cnPlayers = players
    .Where(p => p.Region == "CN")
    .ToList();
```

### 查询活跃玩家

```csharp
List<Player> activePlayers = players
    .Where(p => p.IsActive)
    .ToList();
```

### 组合条件

```csharp
List<Player> result = players
    .Where(p => p.Region == "CN" && p.Level >= 10)
    .OrderByDescending(p => p.CalculatePower())
    .ToList();
```

这就是 LINQ 最常见的写法之一：筛选、排序、转成列表。

## 第 13 步：统计

常用统计：

```csharp
int totalCount = players.Count;
int activeCount = players.Count(p => p.IsActive);
int maxLevel = players.Max(p => p.Level);
int minLevel = players.Min(p => p.Level);
double averageLevel = players.Average(p => p.Level);
int totalGold = players.Sum(p => p.Gold);
```

注意：如果列表为空，`Max`、`Min`、`Average` 会抛异常。

稳一点：

```csharp
if (players.Count > 0)
{
    int maxLevel = players.Max(p => p.Level);
    Console.WriteLine($"最高等级：{maxLevel}");
}
```

或者：

```csharp
int maxLevel = players.Count == 0
    ? 0
    : players.Max(p => p.Level);
```

## 第 14 步：把列表逻辑拆成 `PlayerManager`

当 `Program.cs` 中出现很多玩家管理逻辑时，就应该拆类。

创建：

```text
Services/
  PlayerManager.cs
```

示例：

```csharp
using gameC_.Models;

namespace gameC_.Services;

public sealed class PlayerManager
{
    private readonly List<Player> _players = new();

    public IReadOnlyList<Player> Players => _players;

    public void AddPlayer(Player player)
    {
        _players.Add(player);
    }

    public bool RemoveById(Guid id)
    {
        Player? player = FindById(id);

        if (player is null)
        {
            return false;
        }

        return _players.Remove(player);
    }

    public Player? FindById(Guid id)
    {
        return _players.FirstOrDefault(p => p.Id == id);
    }

    public List<Player> FindByName(string keyword)
    {
        return _players
            .Where(p => p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public List<Player> GetTopByPower(int count)
    {
        return _players
            .OrderByDescending(p => p.CalculatePower())
            .Take(count)
            .ToList();
    }

    public List<Player> GetByRegion(string region)
    {
        return _players
            .Where(p => p.Region == region)
            .ToList();
    }
}
```

说明：

- `_players` 是私有字段，外部不能随便改。
- `Players` 返回 `IReadOnlyList<Player>`，外部只能读。
- 增删查排序由 `PlayerManager` 管理。
- `Program.cs` 只负责输入输出和调用。

## 第 15 步：在 `Program.cs` 中使用 `PlayerManager`

```csharp
using gameC_.Models;
using gameC_.Services;

PlayerManager manager = new PlayerManager();

manager.AddPlayer(new Player { Name = "Alice", Level = 10, Region = "CN", Gold = 500 });
manager.AddPlayer(new Player { Name = "Bob", Level = 25, Region = "NA", Gold = 1200 });
manager.AddPlayer(new Player { Name = "Cindy", Level = 35, Region = "EU", Gold = 3000 });

Console.WriteLine("全部玩家：");

foreach (Player player in manager.Players)
{
    Console.WriteLine(player.GetSummary());
}

Console.WriteLine();
Console.WriteLine("战力 Top 2：");

List<Player> topPlayers = manager.GetTopByPower(2);

foreach (Player player in topPlayers)
{
    Console.WriteLine(player.GetSummary());
}
```

## 第 16 步：做一个菜单程序

本课完整练习是做一个简单菜单：

```text
1. 新增玩家
2. 显示全部玩家
3. 按名称查找玩家
4. 删除玩家
5. 显示战力排行榜
0. 退出
```

主循环示例：

```csharp
using gameC_.Models;
using gameC_.Services;

PlayerManager manager = new PlayerManager();

while (true)
{
    Console.WriteLine();
    Console.WriteLine("玩家管理系统");
    Console.WriteLine("1. 新增玩家");
    Console.WriteLine("2. 显示全部玩家");
    Console.WriteLine("3. 按名称查找玩家");
    Console.WriteLine("4. 删除玩家");
    Console.WriteLine("5. 显示战力排行榜");
    Console.WriteLine("0. 退出");
    Console.Write("请选择：");

    string? command = Console.ReadLine();

    if (command == "0")
    {
        break;
    }

    switch (command)
    {
        case "1":
            AddPlayer(manager);
            break;
        case "2":
            ShowAllPlayers(manager);
            break;
        case "3":
            FindPlayers(manager);
            break;
        case "4":
            RemovePlayer(manager);
            break;
        case "5":
            ShowTopPlayers(manager);
            break;
        default:
            Console.WriteLine("未知命令");
            break;
    }
}

static void AddPlayer(PlayerManager manager)
{
    Console.Write("名称：");
    string name = ReadStringOrDefault(Console.ReadLine(), "未命名玩家");

    Console.Write("等级：");
    int level = ReadIntOrDefault(Console.ReadLine(), 1);

    Console.Write("金币：");
    int gold = ReadIntOrDefault(Console.ReadLine(), 0);

    Console.Write("区服：");
    string region = ReadStringOrDefault(Console.ReadLine(), "CN");

    Player player = new Player
    {
        Name = name,
        Level = level,
        Gold = gold,
        Region = region
    };

    manager.AddPlayer(player);

    Console.WriteLine($"新增成功：{player.GetSummary()}");
}

static void ShowAllPlayers(PlayerManager manager)
{
    if (manager.Players.Count == 0)
    {
        Console.WriteLine("暂无玩家");
        return;
    }

    foreach (Player player in manager.Players)
    {
        Console.WriteLine($"{player.Id} | {player.GetSummary()}");
    }
}

static void FindPlayers(PlayerManager manager)
{
    Console.Write("请输入名称关键字：");
    string keyword = ReadStringOrDefault(Console.ReadLine(), string.Empty);

    List<Player> players = manager.FindByName(keyword);

    if (players.Count == 0)
    {
        Console.WriteLine("未找到玩家");
        return;
    }

    foreach (Player player in players)
    {
        Console.WriteLine($"{player.Id} | {player.GetSummary()}");
    }
}

static void RemovePlayer(PlayerManager manager)
{
    Console.Write("请输入玩家 ID：");
    string idInput = ReadStringOrDefault(Console.ReadLine(), string.Empty);

    if (!Guid.TryParse(idInput, out Guid id))
    {
        Console.WriteLine("玩家 ID 格式错误");
        return;
    }

    bool removed = manager.RemoveById(id);

    Console.WriteLine(removed ? "删除成功" : "未找到玩家");
}

static void ShowTopPlayers(PlayerManager manager)
{
    List<Player> players = manager.GetTopByPower(10);

    if (players.Count == 0)
    {
        Console.WriteLine("暂无玩家");
        return;
    }

    for (int i = 0; i < players.Count; i++)
    {
        Console.WriteLine($"{i + 1}. {players[i].GetSummary()}");
    }
}

static string ReadStringOrDefault(string? input, string defaultValue)
{
    return string.IsNullOrWhiteSpace(input)
        ? defaultValue
        : input;
}

static int ReadIntOrDefault(string? input, int defaultValue)
{
    return int.TryParse(input, out int value)
        ? value
        : defaultValue;
}
```

## 第 17 步：本课作业

### 作业 1：按区服显示玩家

新增菜单：

```text
6. 按区服显示玩家
```

要求：

- 输入区服代码。
- 显示该区服所有玩家。
- 如果没有玩家，输出 `该区服暂无玩家`。

### 作业 2：按等级区间查找

给 `PlayerManager` 增加方法：

```csharp
public List<Player> FindByLevelRange(int minLevel, int maxLevel)
```

要求：

- 返回等级在 `[minLevel, maxLevel]` 范围内的玩家。
- 结果按等级降序排列。

### 作业 3：禁用玩家

不要删除玩家，而是增加禁用功能：

```csharp
public bool DisableById(Guid id)
```

要求：

- 找到玩家后设置 `IsActive = false`。
- 显示全部玩家时标记是否活跃。
- 战力排行榜只显示活跃玩家。

### 作业 4：防止重名

新增玩家时，检查是否已有相同名称的玩家。

要求：

- 忽略大小写。
- 如果重名，拒绝新增。
- 输出 `玩家名称已存在`。

## 本课常见错误

### 1. 找不到玩家时没有处理 `null`

错误：

```csharp
Player player = players.FirstOrDefault(p => p.Name == "Alice");
Console.WriteLine(player.Name);
```

推荐：

```csharp
Player? player = players.FirstOrDefault(p => p.Name == "Alice");

if (player is null)
{
    Console.WriteLine("未找到玩家");
    return;
}

Console.WriteLine(player.Name);
```

### 2. 在 `foreach` 中删除元素

错误：

```csharp
foreach (Player player in players)
{
    players.Remove(player);
}
```

推荐：

```csharp
players.RemoveAll(p => p.Level < 10);
```

### 3. 误以为 `OrderBy` 会修改原列表

错误理解：

```csharp
players.OrderByDescending(p => p.Level);
```

这行代码不会改变 `players`。

正确：

```csharp
players = players
    .OrderByDescending(p => p.Level)
    .ToList();
```

或者：

```csharp
List<Player> sortedPlayers = players
    .OrderByDescending(p => p.Level)
    .ToList();
```

### 4. 忽略空列表统计问题

风险写法：

```csharp
int maxLevel = players.Max(p => p.Level);
```

当 `players` 为空时会抛异常。

推荐：

```csharp
int maxLevel = players.Count == 0
    ? 0
    : players.Max(p => p.Level);
```

### 5. `Program.cs` 越写越大

如果你发现 `Program.cs` 已经超过 200 行，并且全是玩家管理逻辑，就该拆出：

```text
Services/PlayerManager.cs
```

这是从脚本式代码走向项目式代码的重要一步。

## 本课复盘问题

学完后，尝试回答：

1. `List<T>` 适合什么场景？
2. `List<Player>` 和 `Player[]` 有什么区别？
3. `FirstOrDefault` 找不到时返回什么？
4. 为什么返回值要写 `Player?`？
5. `Where` 和 `FirstOrDefault` 有什么区别？
6. `OrderBy` 会不会修改原列表？
7. 为什么不建议在 `foreach` 中删除元素？
8. 为什么 `PlayerManager` 中 `_players` 要设为 `private`？
9. 为什么 `Players` 返回 `IReadOnlyList<Player>`？
10. 什么时候应该从 `Program.cs` 拆出服务类？

## 下一课预告

课程四建议学习：

- `Dictionary<TKey, TValue>`。
- 使用 `Dictionary<Guid, Player>` 提升按 ID 查找效率。
- 理解 key、value、哈希表。
- 比较 `List<Player>` 和 `Dictionary<Guid, Player>` 的适用场景。
