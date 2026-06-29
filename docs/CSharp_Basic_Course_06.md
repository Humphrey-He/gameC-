# C# 基础学习专刊：课程六

## 课程主题

文件读写与 JSON：使用 `System.Text.Json` 保存玩家数据，从文件加载玩家列表，给玩家管理系统增加持久化能力。

前几课中，玩家数据都保存在内存里：

```csharp
Dictionary<Guid, Player> _playersById = new();
```

程序运行时数据存在，程序关闭后数据就没了。

课程六的目标是给玩家管理系统增加最基础的持久化能力：

```text
内存中的玩家数据 -> 保存到 JSON 文件
JSON 文件 -> 加载回内存
```

## 本课目标

完成本课后，你应该能做到：

- 理解什么是持久化。
- 使用 `File` 读写文本文件。
- 使用 `Directory` 创建目录。
- 使用 `Path` 拼接路径。
- 使用 `System.Text.Json` 序列化对象。
- 使用 `System.Text.Json` 反序列化对象。
- 保存玩家列表到 JSON 文件。
- 从 JSON 文件加载玩家列表。
- 创建 `PlayerStorage`，把文件读写逻辑从 `PlayerManager` 中拆出来。
- 理解异常处理和默认空数据。

## 第 1 步：什么是持久化

持久化就是把内存中的数据保存到外部存储中。

常见外部存储：

- JSON 文件。
- XML 文件。
- CSV 文件。
- SQLite。
- MySQL。
- Redis。

本课先使用 JSON 文件，因为它简单、直观、适合学习。

内存数据：

```csharp
List<Player> players = new();
```

保存成文件：

```json
[
  {
    "id": "95b82b25-8df5-4b5e-84ef-b37cf64ba839",
    "name": "Alice",
    "level": 10,
    "region": "CN",
    "gold": 500,
    "isActive": true
  }
]
```

下次程序启动时，再从文件读取回来。

## 第 2 步：文件路径基础

推荐不要手写路径分隔符：

```csharp
string path = "data\\players.json";
```

更推荐使用 `Path.Combine`：

```csharp
string dataDirectory = "data";
string filePath = Path.Combine(dataDirectory, "players.json");
```

原因：

- Windows 路径分隔符是 `\`。
- Linux / macOS 路径分隔符是 `/`。
- `Path.Combine` 能更好地适配不同系统。

常用 API：

```csharp
Directory.Exists("data");
Directory.CreateDirectory("data");
File.Exists(filePath);
File.ReadAllText(filePath);
File.WriteAllText(filePath, content);
```

## 第 3 步：写入文本文件

```csharp
string dataDirectory = "data";
string filePath = Path.Combine(dataDirectory, "players.json");

Directory.CreateDirectory(dataDirectory);

File.WriteAllText(filePath, "Hello JSON");
```

执行后会生成：

```text
data/
  players.json
```

内容是：

```text
Hello JSON
```

`Directory.CreateDirectory` 的特点：

- 如果目录不存在，会创建。
- 如果目录已存在，不会报错。

所以可以放心调用。

## 第 4 步：读取文本文件

```csharp
string filePath = Path.Combine("data", "players.json");

if (!File.Exists(filePath))
{
    Console.WriteLine("文件不存在");
    return;
}

string content = File.ReadAllText(filePath);

Console.WriteLine(content);
```

读取文件前建议先判断是否存在。

如果不判断，文件不存在时会抛异常。

## 第 5 步：什么是序列化和反序列化

序列化：

```text
C# 对象 -> JSON 字符串
```

反序列化：

```text
JSON 字符串 -> C# 对象
```

示例对象：

```csharp
Player player = new Player
{
    Name = "Alice",
    Level = 10,
    Region = "CN",
    Gold = 500
};
```

序列化后：

```json
{
  "Name": "Alice",
  "Level": 10,
  "Region": "CN",
  "Gold": 500,
  "IsActive": true
}
```

C# 官方推荐优先使用：

```csharp
System.Text.Json
```

## 第 6 步：序列化单个对象

需要命名空间：

```csharp
using System.Text.Json;
```

代码：

```csharp
Player player = new Player
{
    Name = "Alice",
    Level = 10,
    Region = "CN",
    Gold = 500
};

string json = JsonSerializer.Serialize(player);

Console.WriteLine(json);
```

默认输出是一行。

如果想格式化：

```csharp
JsonSerializerOptions options = new()
{
    WriteIndented = true
};

string json = JsonSerializer.Serialize(player, options);
```

`WriteIndented = true` 表示输出带缩进，更适合人看。

## 第 7 步：反序列化单个对象

```csharp
string json = """
{
  "Name": "Alice",
  "Level": 10,
  "Region": "CN",
  "Gold": 500,
  "IsActive": true
}
""";

Player? player = JsonSerializer.Deserialize<Player>(json);

if (player is null)
{
    Console.WriteLine("反序列化失败");
    return;
}

Console.WriteLine(player.Name);
```

注意返回值是：

```csharp
Player?
```

因为 JSON 可能非法，或者内容无法转换成目标类型。

## 第 8 步：序列化玩家列表

```csharp
List<Player> players =
[
    new Player { Name = "Alice", Level = 10, Region = "CN", Gold = 500 },
    new Player { Name = "Bob", Level = 25, Region = "NA", Gold = 1200 }
];

JsonSerializerOptions options = new()
{
    WriteIndented = true
};

string json = JsonSerializer.Serialize(players, options);

Console.WriteLine(json);
```

输出大致是：

```json
[
  {
    "Id": "95b82b25-8df5-4b5e-84ef-b37cf64ba839",
    "Name": "Alice",
    "Level": 10,
    "Region": "CN",
    "Gold": 500,
    "IsActive": true
  },
  {
    "Id": "8ffbe5fa-e4b4-477f-bf47-653f5297a7a7",
    "Name": "Bob",
    "Level": 25,
    "Region": "NA",
    "Gold": 1200,
    "IsActive": true
  }
]
```

## 第 9 步：保存玩家列表到文件

```csharp
string dataDirectory = "data";
string filePath = Path.Combine(dataDirectory, "players.json");

Directory.CreateDirectory(dataDirectory);

JsonSerializerOptions options = new()
{
    WriteIndented = true
};

string json = JsonSerializer.Serialize(players, options);

File.WriteAllText(filePath, json);

Console.WriteLine("保存成功");
```

这里流程是：

```text
创建目录 -> 对象转 JSON -> 写入文件
```

## 第 10 步：从文件加载玩家列表

```csharp
string filePath = Path.Combine("data", "players.json");

if (!File.Exists(filePath))
{
    Console.WriteLine("没有存档文件，返回空列表");
    return;
}

string json = File.ReadAllText(filePath);

List<Player>? players = JsonSerializer.Deserialize<List<Player>>(json);

players ??= new List<Player>();
```

这里：

```csharp
players ??= new List<Player>();
```

意思是：

```text
如果 players 是 null，就给它一个空列表。
```

## 第 11 步：JSON 属性命名风格

C# 属性通常是 PascalCase：

```csharp
public string Name { get; set; }
```

默认 JSON 也会输出：

```json
{
  "Name": "Alice"
}
```

如果想输出 camelCase：

```json
{
  "name": "Alice"
}
```

可以配置：

```csharp
JsonSerializerOptions options = new()
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};
```

如果读取 JSON 时也希望大小写不敏感：

```csharp
JsonSerializerOptions options = new()
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true
};
```

建议本课使用这个配置。

## 第 12 步：注意 `init` 属性和 JSON

课程二中我们写过：

```csharp
public Guid Id { get; init; } = Guid.NewGuid();
```

`System.Text.Json` 可以在反序列化时设置 `init` 属性。

这意味着：

- 新建玩家时，如果不指定 `Id`，会自动生成。
- 从 JSON 加载时，会使用 JSON 里的 `Id`。

这正好符合我们的需求。

## 第 13 步：创建 `PlayerStorage`

文件读写逻辑不建议直接塞进 `Program.cs`，也不建议塞进 `PlayerManager`。

推荐创建：

```text
Services/
  PlayerStorage.cs
```

代码：

```csharp
using System.Text.Json;
using gameC_.Models;

namespace gameC_.Services;

public sealed class PlayerStorage
{
    private readonly string _filePath;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public PlayerStorage(string filePath)
    {
        _filePath = filePath;
    }

    public void Save(IEnumerable<Player> players)
    {
        string? directory = Path.GetDirectoryName(_filePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(players, _jsonOptions);

        File.WriteAllText(_filePath, json);
    }

    public List<Player> Load()
    {
        if (!File.Exists(_filePath))
        {
            return new List<Player>();
        }

        string json = File.ReadAllText(_filePath);

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

- `PlayerStorage` 只负责保存和加载。
- `PlayerManager` 继续负责玩家管理。
- `Program.cs` 负责调用二者。

这是职责分离。

## 第 14 步：让 `PlayerManager` 支持批量加载

课程四中的 `PlayerManager` 内部使用字典。

需要增加方法：

```csharp
public int AddPlayers(IEnumerable<Player> players)
{
    int count = 0;

    foreach (Player player in players)
    {
        if (_playersById.TryAdd(player.Id, player))
        {
            count++;
        }
    }

    return count;
}
```

也可以增加清空方法：

```csharp
public void Clear()
{
    _playersById.Clear();
}
```

这样加载文件时可以：

```csharp
manager.Clear();
manager.AddPlayers(storage.Load());
```

## 第 15 步：在 `Program.cs` 中保存和加载

示例：

```csharp
using gameC_.Models;
using gameC_.Services;

string dataFilePath = Path.Combine("data", "players.json");

PlayerManager manager = new PlayerManager();
PlayerStorage storage = new PlayerStorage(dataFilePath);

List<Player> loadedPlayers = storage.Load();
manager.AddPlayers(loadedPlayers);

Console.WriteLine($"已加载玩家数量：{manager.Players.Count}");

manager.AddPlayer(new Player
{
    Name = "Alice",
    Level = 10,
    Region = "CN",
    Gold = 500
});

storage.Save(manager.Players);

Console.WriteLine("保存成功");
```

程序流程：

```text
启动 -> 从文件加载 -> 放入 PlayerManager -> 操作玩家 -> 保存到文件
```

## 第 16 步：给菜单增加保存和加载

菜单增加：

```text
9. 保存玩家数据
10. 重新加载玩家数据
```

保存：

```csharp
static void SavePlayers(PlayerManager manager, PlayerStorage storage)
{
    storage.Save(manager.Players);
    Console.WriteLine("保存成功");
}
```

加载：

```csharp
static void LoadPlayers(PlayerManager manager, PlayerStorage storage)
{
    List<Player> players = storage.Load();

    manager.Clear();
    int count = manager.AddPlayers(players);

    Console.WriteLine($"加载成功：{count} 个玩家");
}
```

建议：

- 程序启动时自动加载。
- 用户手动选择保存。
- 程序退出前也可以自动保存。

## 第 17 步：异常处理

文件读写可能失败：

- 路径非法。
- 文件被占用。
- 没有权限。
- JSON 格式错误。
- 磁盘异常。

初学阶段可以先在 `Program.cs` 调用处捕获异常：

```csharp
try
{
    storage.Save(manager.Players);
    Console.WriteLine("保存成功");
}
catch (Exception ex)
{
    Console.WriteLine($"保存失败：{ex.Message}");
}
```

加载：

```csharp
try
{
    List<Player> players = storage.Load();
    manager.Clear();
    manager.AddPlayers(players);
    Console.WriteLine("加载成功");
}
catch (Exception ex)
{
    Console.WriteLine($"加载失败：{ex.Message}");
}
```

后续项目规范课会进一步讲：

- 自定义异常。
- 日志记录。
- 全局异常处理。
- 返回结果模型。

## 第 18 步：完整练习

练习目标：让玩家管理系统支持保存和加载 JSON 文件。

要求：

1. 创建：

```text
Services/PlayerStorage.cs
```

2. `PlayerStorage` 提供：

```csharp
public void Save(IEnumerable<Player> players)
public List<Player> Load()
```

3. 使用：

```csharp
System.Text.Json
```

4. 保存文件路径：

```text
data/players.json
```

5. JSON 使用 camelCase。
6. 程序启动时自动加载。
7. 菜单增加保存功能。
8. 菜单增加重新加载功能。
9. 程序退出前自动保存。
10. 文件不存在时返回空列表，不报错。

验收标准：

- 第一次运行时没有 JSON 文件，程序不会崩溃。
- 新增玩家后保存，会生成 `data/players.json`。
- 关闭程序再打开，能加载之前的玩家。
- JSON 文件格式清晰可读。
- JSON 文件损坏时，程序能提示加载失败，而不是直接崩溃。

## 第 19 步：本课作业

### 作业 1：保存前备份

保存玩家数据前，如果文件已经存在，先备份：

```text
data/players.backup.json
```

提示：

```csharp
File.Copy(sourcePath, backupPath, overwrite: true);
```

### 作业 2：记录保存时间

保存成功后输出：

```text
保存成功：2026-06-29 11:40:00
```

提示：

```csharp
DateTime.Now
```

### 作业 3：导出排行榜

把排行榜 DTO 保存到：

```text
data/ranking.json
```

要求：

- 使用 `PlayerManager.GetRanking(10)`。
- 保存 JSON。
- JSON 使用 camelCase。

### 作业 4：处理损坏 JSON

手动把 `players.json` 改坏，观察程序报错。

然后给加载逻辑加上异常处理：

```csharp
catch (JsonException ex)
{
    Console.WriteLine($"JSON 格式错误：{ex.Message}");
}
```

## 本课常见错误

### 1. 忘记创建目录

错误：

```csharp
File.WriteAllText("data/players.json", json);
```

如果 `data` 目录不存在，可能报错。

推荐：

```csharp
Directory.CreateDirectory("data");
File.WriteAllText("data/players.json", json);
```

### 2. 反序列化结果没有判空

风险写法：

```csharp
List<Player> players = JsonSerializer.Deserialize<List<Player>>(json);
```

推荐：

```csharp
List<Player>? players = JsonSerializer.Deserialize<List<Player>>(json);
return players ?? new List<Player>();
```

### 3. 把文件读写写进 `Player` 类

不推荐：

```csharp
player.SaveToFile();
```

`Player` 应该表示玩家本身，不应该负责文件系统。

推荐：

```csharp
PlayerStorage storage = new PlayerStorage(filePath);
storage.Save(manager.Players);
```

### 4. 程序退出忘记保存

如果用户新增了玩家但没有保存，数据会丢失。

可以在退出命令中：

```csharp
storage.Save(manager.Players);
break;
```

### 5. 路径到处硬编码

不推荐多处写：

```csharp
"data/players.json"
```

推荐集中定义：

```csharp
string dataFilePath = Path.Combine("data", "players.json");
```

## 本课复盘问题

学完后，尝试回答：

1. 什么是持久化？
2. 为什么程序关闭后内存数据会丢失？
3. `Path.Combine` 有什么好处？
4. `Directory.CreateDirectory` 在目录已存在时会怎样？
5. 什么是序列化？
6. 什么是反序列化？
7. `JsonSerializer.Serialize` 是做什么的？
8. `JsonSerializer.Deserialize<T>` 为什么可能返回 `null`？
9. 为什么文件读写逻辑不应该写进 `Player`？
10. `PlayerStorage` 和 `PlayerManager` 的职责分别是什么？

## 下一课预告

课程七建议学习：

- 异常处理与结果模型。
- `try`、`catch`、`finally`。
- 自定义异常。
- 用 `Result` 返回操作成功或失败。
- 让玩家管理系统的错误处理更规范。
