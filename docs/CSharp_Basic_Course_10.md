# C# 基础学习专刊：课程十

## 课程主题

异步编程基础：学习 `Task`、`async`、`await`，把文件保存和加载改成异步。

前面课程中，我们的 `PlayerStorage` 使用同步文件读写：

```csharp
File.ReadAllText(_filePath);
File.WriteAllText(_filePath, json);
```

同步代码容易理解，但当文件变大、操作变慢，或者未来升级成 Web API 时，同步 IO 可能会阻塞当前线程。

课程十的目标是理解 C# 异步编程的基础写法，并把玩家数据保存和加载升级成异步版本：

```csharp
await File.ReadAllTextAsync(_filePath);
await File.WriteAllTextAsync(_filePath, json);
```

## 本课目标

完成本课后，你应该能做到：

- 理解同步和异步的区别。
- 理解 `Task` 和 `Task<T>`。
- 使用 `async` 和 `await`。
- 写异步方法。
- 调用异步方法。
- 把 `PlayerStorage.Save` 改成 `SaveAsync`。
- 把 `PlayerStorage.Load` 改成 `LoadAsync`。
- 在 `Program.cs` 中使用异步加载和保存。
- 理解什么时候需要异步，什么时候不需要。

## 第 1 步：同步和异步的区别

同步代码像这样：

```csharp
string json = File.ReadAllText(_filePath);
```

执行这行代码时，当前线程会等待文件读取完成。

如果文件很小，几乎感觉不到。

如果文件很大，或者文件在网络磁盘上，等待时间就可能比较明显。

异步代码像这样：

```csharp
string json = await File.ReadAllTextAsync(_filePath);
```

它的意思是：

```text
开始文件读取
当前方法暂时让出执行权
文件读取完成后继续往下执行
```

异步不是让代码一定更快，而是减少等待时对线程的占用。

## 第 2 步：什么是 `Task`

`Task` 表示一个异步操作。

没有返回值的异步方法通常返回：

```csharp
Task
```

例如：

```csharp
public async Task SaveAsync()
{
    await File.WriteAllTextAsync("data.txt", "hello");
}
```

有返回值的异步方法通常返回：

```csharp
Task<T>
```

例如：

```csharp
public async Task<string> LoadAsync()
{
    return await File.ReadAllTextAsync("data.txt");
}
```

可以理解成：

```text
Task        -> 未来会完成的一件事
Task<T>     -> 未来会完成，并返回 T 的一件事
```

## 第 3 步：什么是 `async`

`async` 用来标记一个方法中可以使用 `await`。

示例：

```csharp
public async Task SaveAsync()
{
    await File.WriteAllTextAsync("data.txt", "hello");
}
```

注意：

- `async` 本身不会让方法自动变快。
- `async` 只是允许你在方法内部使用 `await`。
- 异步方法通常返回 `Task` 或 `Task<T>`。

不推荐：

```csharp
public async void Save()
{
}
```

除了事件处理器，普通异步方法不要返回 `void`。

推荐：

```csharp
public async Task SaveAsync()
{
}
```

## 第 4 步：什么是 `await`

`await` 用来等待一个异步操作完成。

```csharp
string json = await File.ReadAllTextAsync(_filePath);
```

可以理解成：

```text
等文件读取完成后，把结果赋值给 json。
```

如果没有 `await`：

```csharp
Task<string> jsonTask = File.ReadAllTextAsync(_filePath);
```

这时拿到的是一个任务，不是字符串。

如果需要结果，就要：

```csharp
string json = await jsonTask;
```

## 第 5 步：异步方法命名规范

C# 中异步方法通常以 `Async` 结尾：

```csharp
SaveAsync
LoadAsync
ReadFileAsync
WriteFileAsync
```

不推荐：

```csharp
public async Task Save()
```

推荐：

```csharp
public async Task SaveAsync()
```

这样调用方一眼就知道这是异步方法，需要 `await`。

## 第 6 步：把保存方法改成异步

同步版本：

```csharp
public void Save(IEnumerable<Player> players)
{
    string json = JsonSerializer.Serialize(players, _jsonOptions);
    File.WriteAllText(_filePath, json);
}
```

异步版本：

```csharp
public async Task SaveAsync(IEnumerable<Player> players)
{
    string? directory = Path.GetDirectoryName(_filePath);

    if (!string.IsNullOrWhiteSpace(directory))
    {
        Directory.CreateDirectory(directory);
    }

    string json = JsonSerializer.Serialize(players, _jsonOptions);

    await File.WriteAllTextAsync(_filePath, json);
}
```

变化：

- 返回值从 `void` 改成 `Task`。
- 方法名从 `Save` 改成 `SaveAsync`。
- 增加 `async`。
- `File.WriteAllText` 改成 `File.WriteAllTextAsync`。
- 前面加 `await`。

## 第 7 步：把加载方法改成异步

同步版本：

```csharp
public List<Player> Load()
{
    if (!File.Exists(_filePath))
    {
        return new List<Player>();
    }

    string json = File.ReadAllText(_filePath);
    List<Player>? players = JsonSerializer.Deserialize<List<Player>>(json, _jsonOptions);

    return players ?? new List<Player>();
}
```

异步版本：

```csharp
public async Task<List<Player>> LoadAsync()
{
    if (!File.Exists(_filePath))
    {
        return new List<Player>();
    }

    string json = await File.ReadAllTextAsync(_filePath);

    if (string.IsNullOrWhiteSpace(json))
    {
        return new List<Player>();
    }

    List<Player>? players = JsonSerializer.Deserialize<List<Player>>(json, _jsonOptions);

    return players ?? new List<Player>();
}
```

变化：

- 返回值从 `List<Player>` 改成 `Task<List<Player>>`。
- 方法名从 `Load` 改成 `LoadAsync`。
- 增加 `async`。
- `File.ReadAllText` 改成 `File.ReadAllTextAsync`。
- 使用 `await` 等待文件读取。

## 第 8 步：完整异步 `PlayerStorage`

```csharp
using System.Text.Json;
using gameC_.Exceptions;
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

    public async Task SaveAsync(IEnumerable<Player> players)
    {
        try
        {
            string? directory = Path.GetDirectoryName(_filePath);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonSerializer.Serialize(players, _jsonOptions);

            await File.WriteAllTextAsync(_filePath, json);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            throw new PlayerStorageException("保存玩家数据失败", ex);
        }
    }

    public async Task<List<Player>> LoadAsync()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                return new List<Player>();
            }

            string json = await File.ReadAllTextAsync(_filePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<Player>();
            }

            List<Player>? players = JsonSerializer.Deserialize<List<Player>>(json, _jsonOptions);

            return players ?? new List<Player>();
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

这里的异常处理规则和课程七保持一致：

- 文件不存在，返回空列表。
- JSON 损坏，抛 `PlayerStorageException`。
- 文件 IO 失败，抛 `PlayerStorageException`。

## 第 9 步：在 `Program.cs` 中使用异步

现代 C# 的顶级语句支持直接使用 `await`。

示例：

```csharp
using gameC_.Exceptions;
using gameC_.Models;
using gameC_.Services;

string dataFilePath = Path.Combine("data", "players.json");

PlayerManager manager = new PlayerManager();
PlayerStorage storage = new PlayerStorage(dataFilePath);

try
{
    List<Player> loadedPlayers = await storage.LoadAsync();
    manager.AddPlayers(loadedPlayers);

    Console.WriteLine($"已加载玩家数量：{manager.Players.Count}");
}
catch (PlayerStorageException ex)
{
    Console.WriteLine(ex.Message);
}

manager.AddPlayer(new Player
{
    Name = "Alice",
    Level = 10,
    Region = "CN",
    Gold = 500
});

try
{
    await storage.SaveAsync(manager.Players);
    Console.WriteLine("保存成功");
}
catch (PlayerStorageException ex)
{
    Console.WriteLine(ex.Message);
}
```

注意：

- 调用 `LoadAsync` 要使用 `await`。
- 调用 `SaveAsync` 要使用 `await`。
- 顶级语句中可以直接写 `await`。

## 第 10 步：在菜单中使用异步方法

课程六中的保存方法：

```csharp
static void SavePlayers(PlayerManager manager, PlayerStorage storage)
{
    storage.Save(manager.Players);
    Console.WriteLine("保存成功");
}
```

改成异步：

```csharp
static async Task SavePlayersAsync(PlayerManager manager, PlayerStorage storage)
{
    try
    {
        await storage.SaveAsync(manager.Players);
        Console.WriteLine("保存成功");
    }
    catch (PlayerStorageException ex)
    {
        Console.WriteLine(ex.Message);
    }
}
```

加载方法：

```csharp
static async Task LoadPlayersAsync(PlayerManager manager, PlayerStorage storage)
{
    try
    {
        List<Player> players = await storage.LoadAsync();

        manager.Clear();
        int count = manager.AddPlayers(players);

        Console.WriteLine($"加载成功：{count} 个玩家");
    }
    catch (PlayerStorageException ex)
    {
        Console.WriteLine(ex.Message);
    }
}
```

菜单调用：

```csharp
switch (command)
{
    case "9":
        await SavePlayersAsync(manager, storage);
        break;
    case "10":
        await LoadPlayersAsync(manager, storage);
        break;
}
```

如果一个方法内部用了 `await`，这个方法通常也要变成 `async Task`。

这叫异步传播。

## 第 11 步：`Task` 和 `Task<T>` 的区别

无返回值：

```csharp
public async Task SaveAsync()
{
    await File.WriteAllTextAsync("data.txt", "hello");
}
```

调用：

```csharp
await SaveAsync();
```

有返回值：

```csharp
public async Task<string> LoadAsync()
{
    return await File.ReadAllTextAsync("data.txt");
}
```

调用：

```csharp
string content = await LoadAsync();
```

对照表：

| 同步方法 | 异步方法 |
| --- | --- |
| `void Save()` | `Task SaveAsync()` |
| `List<Player> Load()` | `Task<List<Player>> LoadAsync()` |
| `Result AddPlayer()` | 通常保持同步 |
| `int CalculatePower()` | 通常保持同步 |

## 第 12 步：哪些方法不需要异步

不是所有方法都要加 `async`。

不需要异步的例子：

```csharp
public int CalculatePower()
{
    return Level * 100 + Gold / 10;
}
```

```csharp
public Result AddPlayer(Player player)
{
    ...
}
```

这些都是纯内存计算，速度很快，不涉及等待 IO。

适合异步的场景：

- 文件读写。
- 数据库访问。
- HTTP 请求。
- 网络通信。
- 等待外部系统。

一句话：

```text
IO 等待适合异步，普通内存计算通常不需要异步。
```

## 第 13 步：不要用 `.Result` 和 `.Wait()`

不推荐：

```csharp
List<Player> players = storage.LoadAsync().Result;
```

不推荐：

```csharp
storage.SaveAsync(manager.Players).Wait();
```

原因：

- 可能导致死锁。
- 异常处理更别扭。
- 破坏异步调用链。

推荐：

```csharp
List<Player> players = await storage.LoadAsync();
await storage.SaveAsync(manager.Players);
```

记住：

```text
异步方法尽量一路 await 下去。
```

## 第 14 步：异步异常处理

异步方法中的异常，也可以用 `try/catch` 捕获。

```csharp
try
{
    await storage.SaveAsync(manager.Players);
    Console.WriteLine("保存成功");
}
catch (PlayerStorageException ex)
{
    Console.WriteLine(ex.Message);
}
```

注意：

```csharp
storage.SaveAsync(manager.Players);
```

如果你忘记 `await`，异常可能不会在当前 `try/catch` 中按你预期被捕获。

所以调用异步方法时，要认真确认是否需要 `await`。

## 第 15 步：`CancellationToken` 简单了解

真实项目中，异步操作常常需要支持取消。

例如：

- 用户取消保存。
- Web 请求中断。
- 程序关闭。

C# 中常用：

```csharp
CancellationToken
```

示例：

```csharp
public async Task SaveAsync(
    IEnumerable<Player> players,
    CancellationToken cancellationToken = default)
{
    string json = JsonSerializer.Serialize(players, _jsonOptions);

    await File.WriteAllTextAsync(_filePath, json, cancellationToken);
}
```

加载：

```csharp
public async Task<List<Player>> LoadAsync(
    CancellationToken cancellationToken = default)
{
    string json = await File.ReadAllTextAsync(_filePath, cancellationToken);
    ...
}
```

本课先不强制加入取消机制，但你要知道它是异步代码里很重要的一环。

## 第 16 步：给 `PlayerStorage` 增加取消支持

进阶版保存：

```csharp
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
```

进阶版加载：

```csharp
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
```

注意：

- `OperationCanceledException` 通常不要包装成普通失败。
- 取消就是取消，交给上层决定如何处理。

## 第 17 步：单元测试异步方法

xUnit 支持异步测试。

同步测试：

```csharp
[Fact]
public void AddPlayer_WithValidPlayer_ReturnsSuccess()
{
}
```

异步测试：

```csharp
[Fact]
public async Task SaveAsync_WithPlayers_CreatesJsonFile()
{
}
```

示例：

```csharp
[Fact]
public async Task SaveAsync_WithPlayers_CreatesJsonFile()
{
    // Arrange
    string filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");
    PlayerStorage storage = new PlayerStorage(filePath);

    List<Player> players =
    [
        new Player { Name = "Alice", Level = 10 }
    ];

    try
    {
        // Act
        await storage.SaveAsync(players);

        // Assert
        Assert.True(File.Exists(filePath));
    }
    finally
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}
```

这里用 `finally` 清理临时文件，避免测试留下垃圾文件。

## 第 18 步：完整练习

练习目标：把玩家数据保存和加载升级成异步。

要求：

1. 修改 `PlayerStorage`：

```csharp
public Task SaveAsync(IEnumerable<Player> players)
public Task<List<Player>> LoadAsync()
```

2. 使用：

```csharp
File.WriteAllTextAsync
File.ReadAllTextAsync
```

3. 所有异步方法以 `Async` 结尾。
4. `Program.cs` 中启动时使用 `await storage.LoadAsync()`。
5. 菜单保存时使用 `await storage.SaveAsync(...)`。
6. 菜单加载时使用 `await storage.LoadAsync()`。
7. 不使用 `.Result`。
8. 不使用 `.Wait()`。
9. 保留 `PlayerStorageException`。
10. 可选：为保存和加载增加 `CancellationToken` 参数。

验收标准：

- 项目可以正常运行。
- 启动时能异步加载玩家数据。
- 保存时能异步写入 JSON。
- JSON 文件损坏时仍能提示错误。
- 代码中异步方法命名带 `Async`。
- 没有用 `.Result` 或 `.Wait()` 阻塞异步任务。

## 第 19 步：本课作业

### 作业 1：增加异步测试

给 `PlayerStorage.SaveAsync` 写测试：

- 保存一个玩家。
- 验证文件存在。
- 验证文件内容包含玩家名称。
- 测试结束删除临时文件。

### 作业 2：测试异步加载

给 `PlayerStorage.LoadAsync` 写测试：

- 先准备一个 JSON 文件。
- 调用 `LoadAsync`。
- 验证加载出的玩家数量。
- 验证玩家名称。

### 作业 3：加载不存在文件

测试：

```text
文件不存在时，LoadAsync 返回空列表。
```

要求：

- 不抛异常。
- 返回 `Count == 0`。

### 作业 4：加入取消参数

给 `SaveAsync` 和 `LoadAsync` 增加：

```csharp
CancellationToken cancellationToken = default
```

并把参数传给：

```csharp
File.WriteAllTextAsync
File.ReadAllTextAsync
```

## 本课常见错误

### 1. 写了 `async` 但没有 `await`

不推荐：

```csharp
public async Task SaveAsync()
{
    File.WriteAllText("data.txt", "hello");
}
```

如果没有异步操作，就不要加 `async`。

推荐：

```csharp
public async Task SaveAsync()
{
    await File.WriteAllTextAsync("data.txt", "hello");
}
```

### 2. 异步方法返回 `void`

不推荐：

```csharp
public async void SaveAsync()
```

推荐：

```csharp
public async Task SaveAsync()
```

### 3. 忘记 `await`

风险写法：

```csharp
storage.SaveAsync(manager.Players);
Console.WriteLine("保存成功");
```

这可能在保存完成前就输出成功。

推荐：

```csharp
await storage.SaveAsync(manager.Players);
Console.WriteLine("保存成功");
```

### 4. 用 `.Result` 或 `.Wait()`

不推荐：

```csharp
List<Player> players = storage.LoadAsync().Result;
```

推荐：

```csharp
List<Player> players = await storage.LoadAsync();
```

### 5. 把所有方法都改成异步

不需要：

```csharp
public async Task<int> CalculatePowerAsync()
```

纯计算方法保持同步就好：

```csharp
public int CalculatePower()
```

## 本课复盘问题

学完后，尝试回答：

1. 同步和异步有什么区别？
2. `Task` 表示什么？
3. `Task<T>` 表示什么？
4. `async` 的作用是什么？
5. `await` 的作用是什么？
6. 为什么异步方法通常以 `Async` 结尾？
7. 为什么普通异步方法不推荐返回 `void`？
8. 为什么不推荐使用 `.Result` 和 `.Wait()`？
9. 哪些场景适合异步？
10. 哪些方法不需要异步？

## 下一课预告

课程十一建议学习：

- 依赖注入基础。
- 抽象 `IPlayerStorage`。
- 让 `PlayerManager` 和存储实现解耦。
- 理解接口、实现类、构造函数注入。
- 为后续 ASP.NET Core Web API 做准备。
