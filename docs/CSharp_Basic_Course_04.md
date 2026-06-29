# C# 基础学习专刊：课程四

## 课程主题

`Dictionary<TKey, TValue>`：使用 `Dictionary<Guid, Player>` 提升按 ID 查找效率，并比较 `List` 和 `Dictionary` 的适用场景。

课程三中，我们使用 `List<Player>` 管理多个玩家：

```csharp
List<Player> players = new();
```

查找玩家时，通常写：

```csharp
Player? player = players.FirstOrDefault(p => p.Id == id);
```

这种写法简单直观，但它需要从头到尾扫描列表。玩家数量少时没问题；如果玩家数量很多，并且经常按 ID 查找，就应该考虑使用字典：

```csharp
Dictionary<Guid, Player> playersById = new();
```

## 本课目标

完成本课后，你应该能做到：

- 理解 `Dictionary<TKey, TValue>` 的基本用途。
- 使用 `Dictionary<Guid, Player>` 保存玩家。
- 根据玩家 ID 快速查找玩家。
- 使用 `Add`、索引器、`TryAdd`、`TryGetValue`、`Remove`、`ContainsKey`。
- 理解 key 不能重复。
- 理解 `List<Player>` 和 `Dictionary<Guid, Player>` 的区别。
- 改造 `PlayerManager`，让按 ID 查找更高效。
- 知道什么时候用 `List`，什么时候用 `Dictionary`。

## 第 1 步：为什么需要 `Dictionary`

假设有 10 个玩家，用 `List<Player>` 查找某个 ID：

```csharp
Player? player = players.FirstOrDefault(p => p.Id == id);
```

这很舒服。

但如果有 100 万个玩家，每次都从第一个玩家开始找，最差情况可能要比较 100 万次。

`Dictionary<TKey, TValue>` 的设计目标是：

```text
通过 key 快速找到 value
```

在玩家系统里：

```text
玩家 ID -> 玩家对象
```

对应 C#：

```csharp
Dictionary<Guid, Player> playersById = new();
```

其中：

- `Guid` 是 key。
- `Player` 是 value。
- 一个 key 对应一个 value。
- key 不能重复。

## 第 2 步：什么是 key 和 value

字典可以理解成一张映射表：

```text
Key                         Value
------------------------------------------------
玩家 ID 1                    Alice 玩家对象
玩家 ID 2                    Bob 玩家对象
玩家 ID 3                    Cindy 玩家对象
```

代码：

```csharp
Dictionary<Guid, Player> playersById = new();

Player player = new Player
{
    Name = "Alice",
    Level = 10,
    Region = "CN",
    Gold = 500
};

playersById.Add(player.Id, player);
```

之后只要有 ID，就可以快速拿到玩家：

```csharp
Player player = playersById[player.Id];
```

## 第 3 步：创建字典

```csharp
Dictionary<Guid, Player> playersById = new();
```

也可以初始化：

```csharp
Player alice = new Player { Name = "Alice", Level = 10 };
Player bob = new Player { Name = "Bob", Level = 20 };

Dictionary<Guid, Player> playersById = new()
{
    [alice.Id] = alice,
    [bob.Id] = bob
};
```

这种写法里：

```csharp
[alice.Id] = alice
```

表示把 `alice.Id` 作为 key，把 `alice` 作为 value。

## 第 4 步：新增数据

### 使用 `Add`

```csharp
playersById.Add(player.Id, player);
```

如果 key 已经存在，`Add` 会抛异常。

### 使用索引器

```csharp
playersById[player.Id] = player;
```

如果 key 不存在，会新增。

如果 key 已存在，会覆盖。

### 使用 `TryAdd`

```csharp
bool added = playersById.TryAdd(player.Id, player);

if (!added)
{
    Console.WriteLine("玩家 ID 已存在");
}
```

建议：

- 明确不允许重复时，用 `TryAdd`。
- 明确允许覆盖时，用索引器。
- 初学阶段少用直接 `Add`，因为重复 key 时容易抛异常。

## 第 5 步：查找数据

### 不推荐直接索引访问未知 key

```csharp
Player player = playersById[id];
```

如果 `id` 不存在，会抛 `KeyNotFoundException`。

### 推荐 `TryGetValue`

```csharp
bool found = playersById.TryGetValue(id, out Player? player);

if (!found)
{
    Console.WriteLine("未找到玩家");
    return;
}

Console.WriteLine(player.GetSummary());
```

也可以简写：

```csharp
if (playersById.TryGetValue(id, out Player? player))
{
    Console.WriteLine(player.GetSummary());
}
else
{
    Console.WriteLine("未找到玩家");
}
```

`TryGetValue` 是字典查找中非常常用的安全写法。

## 第 6 步：判断 key 是否存在

```csharp
if (playersById.ContainsKey(id))
{
    Console.WriteLine("玩家存在");
}
```

但如果你判断后还要取值，不建议这样写：

```csharp
if (playersById.ContainsKey(id))
{
    Player player = playersById[id];
}
```

更推荐：

```csharp
if (playersById.TryGetValue(id, out Player? player))
{
    Console.WriteLine(player.GetSummary());
}
```

原因：

- `ContainsKey` 查一次。
- 索引器取值又查一次。
- `TryGetValue` 一次完成判断和取值。

## 第 7 步：删除数据

```csharp
bool removed = playersById.Remove(id);

Console.WriteLine(removed ? "删除成功" : "未找到玩家");
```

如果想删除前拿到被删除的玩家，新版本 .NET 支持：

```csharp
bool removed = playersById.Remove(id, out Player? removedPlayer);

if (removed)
{
    Console.WriteLine($"删除成功：{removedPlayer!.GetSummary()}");
}
```

如果不确定当前目标框架是否支持，可以先查再删：

```csharp
if (playersById.TryGetValue(id, out Player? player))
{
    playersById.Remove(id);
    Console.WriteLine($"删除成功：{player.GetSummary()}");
}
else
{
    Console.WriteLine("未找到玩家");
}
```

## 第 8 步：遍历字典

遍历 key 和 value：

```csharp
foreach (KeyValuePair<Guid, Player> pair in playersById)
{
    Console.WriteLine($"{pair.Key} | {pair.Value.GetSummary()}");
}
```

也可以用解构：

```csharp
foreach ((Guid id, Player player) in playersById)
{
    Console.WriteLine($"{id} | {player.GetSummary()}");
}
```

只遍历 value：

```csharp
foreach (Player player in playersById.Values)
{
    Console.WriteLine(player.GetSummary());
}
```

只遍历 key：

```csharp
foreach (Guid id in playersById.Keys)
{
    Console.WriteLine(id);
}
```

## 第 9 步：字典的 key 不能重复

字典中每个 key 只能出现一次。

```csharp
playersById.TryAdd(player.Id, player);
playersById.TryAdd(player.Id, player);
```

第二次会返回 `false`。

这就是字典的核心规则：

```text
一个 key 只能对应一个 value
```

如果你希望一个 key 对应多个 value，比如：

```text
区服 CN -> 多个玩家
```

那就不能用：

```csharp
Dictionary<string, Player>
```

而应该用：

```csharp
Dictionary<string, List<Player>>
```

或者先用 LINQ 分组，后续课程会讲。

## 第 10 步：改造 `PlayerManager`

课程三中的 `PlayerManager` 使用 `List<Player>`：

```csharp
private readonly List<Player> _players = new();
```

现在改成按 ID 存储：

```csharp
private readonly Dictionary<Guid, Player> _playersById = new();
```

完整示例：

```csharp
using gameC_.Models;

namespace gameC_.Services;

public sealed class PlayerManager
{
    private readonly Dictionary<Guid, Player> _playersById = new();

    public IReadOnlyCollection<Player> Players => _playersById.Values;

    public bool AddPlayer(Player player)
    {
        return _playersById.TryAdd(player.Id, player);
    }

    public bool RemoveById(Guid id)
    {
        return _playersById.Remove(id);
    }

    public Player? FindById(Guid id)
    {
        return _playersById.TryGetValue(id, out Player? player)
            ? player
            : null;
    }

    public List<Player> FindByName(string keyword)
    {
        return _playersById.Values
            .Where(p => p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public List<Player> GetTopByPower(int count)
    {
        return _playersById.Values
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CalculatePower())
            .Take(count)
            .ToList();
    }

    public List<Player> GetByRegion(string region)
    {
        return _playersById.Values
            .Where(p => p.Region == region)
            .ToList();
    }

    public bool DisableById(Guid id)
    {
        Player? player = FindById(id);

        if (player is null)
        {
            return false;
        }

        player.IsActive = false;
        return true;
    }
}
```

这里的变化：

- 按 ID 查找从扫描列表变成字典查找。
- `Players` 返回 `_playersById.Values`。
- 查找名称、排行榜、区服筛选仍然需要遍历所有 value。

## 第 11 步：`List` 和 `Dictionary` 的效率区别

简单理解：

| 操作 | `List<Player>` | `Dictionary<Guid, Player>` |
| --- | --- | --- |
| 按顺序遍历 | 很适合 | 可以，但顺序不应依赖 |
| 按索引访问 | 很适合 | 不适合 |
| 按 ID 查找 | 需要遍历 | 很适合 |
| 按 ID 删除 | 需要先查找 | 很适合 |
| 保持插入顺序 | 更直观 | 不应该依赖 |
| 排序 | 很适合 | 通常对 Values 转 List 后排序 |
| key 唯一约束 | 不自带 | 天然支持 |

时间复杂度的直观比较：

| 操作 | `List` | `Dictionary` |
| --- | --- | --- |
| 按 ID 查找 | O(n) | 平均 O(1) |
| 新增到末尾 | 平均 O(1) | 平均 O(1) |
| 删除指定 ID | O(n) | 平均 O(1) |
| 遍历全部 | O(n) | O(n) |
| 排序 | O(n log n) | 通常转列表后 O(n log n) |

不用死背复杂度，但要记住一句话：

```text
经常按唯一 key 查找，用 Dictionary。
经常按顺序处理一组数据，用 List。
```

## 第 12 步：什么时候用 `List<Player>`

适合 `List<Player>` 的场景：

- 玩家数量不大。
- 主要是展示列表。
- 经常按顺序遍历。
- 经常排序。
- 需要通过下标访问。
- 查询条件不是固定 ID，而是名称、等级、区服等复杂条件。

示例：

```csharp
List<Player> topPlayers = players
    .OrderByDescending(p => p.CalculatePower())
    .Take(10)
    .ToList();
```

## 第 13 步：什么时候用 `Dictionary<Guid, Player>`

适合 `Dictionary<Guid, Player>` 的场景：

- 每个玩家有唯一 ID。
- 经常通过 ID 查找。
- 经常通过 ID 删除。
- 不允许 ID 重复。
- 需要维护一张“ID 到对象”的映射表。

示例：

```csharp
Player? player = manager.FindById(id);
```

## 第 14 步：可不可以两个都用

可以，但要小心一致性。

例如：

```csharp
private readonly List<Player> _players = new();
private readonly Dictionary<Guid, Player> _playersById = new();
```

优点：

- `List` 负责顺序。
- `Dictionary` 负责快速查找。

缺点：

- 新增时两个都要加。
- 删除时两个都要删。
- 如果漏掉一个，数据就不一致。

初学阶段建议：

- 先只用一个主存储结构。
- 如果主要按 ID 查找，用 `Dictionary`。
- 如果主要列表展示和排序，用 `List`。
- 等需求真的复杂了，再考虑组合使用。

## 第 15 步：字典 key 的选择

好的 key 应该：

- 唯一。
- 稳定。
- 不容易改变。
- 比较成本低。

玩家系统中适合作 key：

```csharp
Guid Id
```

不太适合作 key：

```csharp
string Name
```

原因：

- 玩家可能改名。
- 名称可能重复。
- 大小写比较容易产生规则问题。

如果确实要按名称快速查找，可以额外维护：

```csharp
Dictionary<string, Guid> playerIdsByName = new(StringComparer.OrdinalIgnoreCase);
```

但这已经属于进阶设计，本课先理解主字典即可。

## 第 16 步：完整练习

练习目标：把课程三的 `PlayerManager` 从 `List<Player>` 改成 `Dictionary<Guid, Player>`。

要求：

1. `PlayerManager` 内部使用：

```csharp
private readonly Dictionary<Guid, Player> _playersById = new();
```

2. 提供只读玩家集合：

```csharp
public IReadOnlyCollection<Player> Players => _playersById.Values;
```

3. 实现新增：

```csharp
public bool AddPlayer(Player player)
```

4. 实现按 ID 查找：

```csharp
public Player? FindById(Guid id)
```

5. 实现按 ID 删除：

```csharp
public bool RemoveById(Guid id)
```

6. 实现按名称搜索：

```csharp
public List<Player> FindByName(string keyword)
```

7. 实现排行榜：

```csharp
public List<Player> GetTopByPower(int count)
```

8. 修改 `Program.cs`，保持菜单功能正常。

验收标准：

- 新增玩家成功。
- 显示全部玩家正常。
- 复制某个玩家 ID 后，可以按 ID 删除。
- 删除不存在的 ID 时不会崩溃。
- 排行榜仍然按战力降序。
- `FindById` 内部使用 `TryGetValue`。

## 第 17 步：本课作业

### 作业 1：按 ID 查询玩家

新增菜单：

```text
6. 按 ID 查询玩家
```

要求：

- 输入玩家 ID。
- 使用 `Guid.TryParse` 转换。
- 使用 `manager.FindById(id)` 查找。
- 找到则输出玩家摘要。
- 找不到则输出 `未找到玩家`。

### 作业 2：检查名称重复

在 `PlayerManager` 中增加：

```csharp
public bool ExistsByName(string name)
```

要求：

- 忽略大小写。
- 新增玩家前检查。
- 重名时拒绝新增。

### 作业 3：统计区服人数

增加方法：

```csharp
public int CountByRegion(string region)
```

要求：

- 返回指定区服玩家数量。
- 只统计活跃玩家。

### 作业 4：批量导入玩家

增加方法：

```csharp
public int AddPlayers(IEnumerable<Player> players)
```

要求：

- 批量添加玩家。
- 已存在 ID 的玩家跳过。
- 返回成功添加的数量。

## 本课常见错误

### 1. 直接用索引器读取未知 key

风险写法：

```csharp
Player player = playersById[id];
```

如果 ID 不存在，会抛异常。

推荐：

```csharp
if (playersById.TryGetValue(id, out Player? player))
{
    Console.WriteLine(player.GetSummary());
}
```

### 2. 以为字典是排序结构

不要依赖 `Dictionary` 的遍历顺序。

如果需要排序：

```csharp
List<Player> sortedPlayers = playersById.Values
    .OrderByDescending(p => p.CalculatePower())
    .ToList();
```

### 3. 使用会变化的字段作为 key

不推荐：

```csharp
Dictionary<string, Player> playersByName = new();
```

如果玩家可以改名，这个 key 就不稳定。

更推荐：

```csharp
Dictionary<Guid, Player> playersById = new();
```

### 4. 同时维护 `List` 和 `Dictionary` 却忘记同步

风险：

```csharp
_players.Remove(player);
// 忘记 _playersById.Remove(player.Id)
```

初学阶段先用一个主数据结构，减少一致性问题。

### 5. `TryAdd` 失败时不处理

不推荐：

```csharp
_playersById.TryAdd(player.Id, player);
```

推荐：

```csharp
bool added = _playersById.TryAdd(player.Id, player);

if (!added)
{
    Console.WriteLine("玩家已存在");
}
```

## 本课复盘问题

学完后，尝试回答：

1. `Dictionary<TKey, TValue>` 解决什么问题？
2. `Guid` 在 `Dictionary<Guid, Player>` 中是什么角色？
3. `Player` 在 `Dictionary<Guid, Player>` 中是什么角色？
4. 为什么 key 不能重复？
5. `Add`、索引器、`TryAdd` 有什么区别？
6. 为什么推荐用 `TryGetValue`？
7. `List<Player>` 按 ID 查找为什么是 O(n)？
8. `Dictionary<Guid, Player>` 按 ID 查找为什么通常更快？
9. 为什么不应该依赖字典遍历顺序？
10. 什么场景继续用 `List`，什么场景应该用 `Dictionary`？

## 下一课预告

课程五建议学习：

- LINQ 进阶。
- `Where`、`Select`、`GroupBy`、`Any`、`All`。
- 统计玩家区服分布。
- 生成排行榜 DTO。
- 从实体对象转换成展示对象。
