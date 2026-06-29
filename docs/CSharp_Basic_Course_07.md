# C# 基础学习专刊：课程七

## 课程主题

异常处理与结果模型：学习 `try`、`catch`、`finally`、自定义异常，并用 `Result` 返回操作成功或失败。

前几课中，我们已经完成了：

- 用 `Player` 表示玩家。
- 用 `PlayerManager` 管理玩家。
- 用 `Dictionary<Guid, Player>` 提升按 ID 查询效率。
- 用 LINQ 生成排行榜和区服统计。
- 用 `System.Text.Json` 保存和加载玩家数据。

但是实际项目里，很多操作都可能失败：

```text
玩家 ID 格式错误
玩家不存在
玩家名称重复
JSON 文件损坏
保存文件失败
加载文件失败
输入内容非法
```

课程七的目标是让你学会“如何表达失败”，并把错误处理写得更清楚。

## 本课目标

完成本课后，你应该能做到：

- 理解什么是异常。
- 使用 `try`、`catch`、`finally`。
- 知道什么时候捕获异常，什么时候不捕获。
- 创建自定义异常。
- 理解异常和业务失败的区别。
- 创建 `Result` 和 `Result<T>`。
- 用 `Result` 表达操作成功或失败。
- 改造 `PlayerManager`，让新增、删除、禁用等操作返回明确结果。
- 改造 `PlayerStorage`，让保存和加载失败时更容易处理。

## 第 1 步：什么是异常

异常是程序运行过程中出现的非正常情况。

例如：

```csharp
int number = int.Parse("abc");
```

这行代码会抛出异常，因为 `"abc"` 不能转换成整数。

常见异常：

| 异常类型 | 常见原因 |
| --- | --- |
| `FormatException` | 格式错误，例如数字转换失败 |
| `ArgumentException` | 参数不合法 |
| `ArgumentNullException` | 参数是 `null` |
| `InvalidOperationException` | 当前状态不允许执行操作 |
| `KeyNotFoundException` | 字典中 key 不存在 |
| `FileNotFoundException` | 文件不存在 |
| `JsonException` | JSON 格式错误 |
| `IOException` | 文件读写失败 |

异常如果没有被处理，程序通常会终止。

## 第 2 步：使用 `try` 和 `catch`

基本写法：

```csharp
try
{
    int number = int.Parse("abc");
    Console.WriteLine(number);
}
catch (FormatException ex)
{
    Console.WriteLine($"数字格式错误：{ex.Message}");
}
```

执行流程：

```text
进入 try
发生异常
跳转到匹配的 catch
执行 catch 中的处理逻辑
程序继续运行
```

如果没有异常，`catch` 不会执行。

## 第 3 步：捕获多个异常

```csharp
try
{
    string json = File.ReadAllText("data/players.json");
    List<Player>? players = JsonSerializer.Deserialize<List<Player>>(json);
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"文件不存在：{ex.Message}");
}
catch (JsonException ex)
{
    Console.WriteLine($"JSON 格式错误：{ex.Message}");
}
catch (IOException ex)
{
    Console.WriteLine($"文件读写错误：{ex.Message}");
}
```

建议：

- 能明确捕获具体异常时，优先捕获具体异常。
- 不要一上来就只写 `catch (Exception)`。
- `catch (Exception)` 可以放在最后兜底，但不要吞掉错误。

兜底写法：

```csharp
catch (Exception ex)
{
    Console.WriteLine($"未知错误：{ex.Message}");
}
```

## 第 4 步：`finally`

`finally` 表示无论是否发生异常，都会执行。

```csharp
try
{
    Console.WriteLine("开始保存");
}
catch (Exception ex)
{
    Console.WriteLine($"保存失败：{ex.Message}");
}
finally
{
    Console.WriteLine("保存流程结束");
}
```

常见用途：

- 释放资源。
- 关闭连接。
- 清理临时状态。
- 输出结束日志。

现代 C# 中，很多资源释放更推荐使用 `using`，后续课程会讲。

## 第 5 步：不要滥用异常

异常适合表示“不应该正常发生”的错误。

例如：

- JSON 文件损坏。
- 文件读写失败。
- 参数是 `null`。
- 程序状态不一致。

但是业务上的失败不一定要用异常。

例如：

- 玩家不存在。
- 玩家名称重复。
- 玩家已经被禁用。
- 金币不足。
- 等级不足。

这些属于业务可预期结果，更适合用 `Result` 返回。

一句话记忆：

```text
系统异常用 exception，业务失败用 Result。
```

## 第 6 步：为什么需要 `Result`

假设新增玩家：

```csharp
public bool AddPlayer(Player player)
{
    return _playersById.TryAdd(player.Id, player);
}
```

返回 `bool` 的问题是：

```text
只知道成功或失败，不知道为什么失败。
```

比如失败可能是：

- 玩家为 `null`。
- 玩家名称为空。
- 玩家名称重复。
- 玩家 ID 重复。

如果只返回 `false`，调用方只能猜。

所以我们定义：

```csharp
Result
```

让方法返回：

```text
是否成功
错误信息
```

## 第 7 步：创建 `Result`

推荐创建目录：

```text
Common/
  Result.cs
```

代码：

```csharp
namespace gameC_.Common;

public sealed class Result
{
    private Result(bool isSuccess, string errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public string ErrorMessage { get; }

    public static Result Success()
    {
        return new Result(true, string.Empty);
    }

    public static Result Failure(string errorMessage)
    {
        return new Result(false, errorMessage);
    }
}
```

使用：

```csharp
Result result = Result.Success();

if (result.IsSuccess)
{
    Console.WriteLine("操作成功");
}
```

失败：

```csharp
Result result = Result.Failure("玩家名称已存在");

if (result.IsFailure)
{
    Console.WriteLine(result.ErrorMessage);
}
```

## 第 8 步：创建 `Result<T>`

有些操作失败时要返回错误，成功时还要返回数据。

例如：

```csharp
查找玩家
```

成功时应该返回 `Player`，失败时返回错误信息。

创建：

```text
Common/
  ResultT.cs
```

代码：

```csharp
namespace gameC_.Common;

public sealed class Result<T>
{
    private Result(bool isSuccess, T? value, string errorMessage)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public T? Value { get; }

    public string ErrorMessage { get; }

    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, string.Empty);
    }

    public static Result<T> Failure(string errorMessage)
    {
        return new Result<T>(false, default, errorMessage);
    }
}
```

使用：

```csharp
Result<Player> result = manager.GetPlayer(id);

if (result.IsFailure)
{
    Console.WriteLine(result.ErrorMessage);
    return;
}

Player player = result.Value!;

Console.WriteLine(player.GetSummary());
```

注意：

- 只有 `IsSuccess` 为 `true` 时，才应该读取 `Value`。
- 这里用 `Value!` 是告诉编译器：我已经判断成功了。

后续可以继续优化成更严格的写法，本课先保持简单。

## 第 9 步：用 `Result` 改造新增玩家

原来的方法：

```csharp
public bool AddPlayer(Player player)
{
    return _playersById.TryAdd(player.Id, player);
}
```

改造后：

```csharp
using gameC_.Common;
using gameC_.Models;

namespace gameC_.Services;

public sealed class PlayerManager
{
    private readonly Dictionary<Guid, Player> _playersById = new();

    public IReadOnlyCollection<Player> Players => _playersById.Values;

    public Result AddPlayer(Player player)
    {
        if (string.IsNullOrWhiteSpace(player.Name))
        {
            return Result.Failure("玩家名称不能为空");
        }

        bool nameExists = _playersById.Values.Any(p =>
            string.Equals(p.Name, player.Name, StringComparison.OrdinalIgnoreCase));

        if (nameExists)
        {
            return Result.Failure("玩家名称已存在");
        }

        bool added = _playersById.TryAdd(player.Id, player);

        return added
            ? Result.Success()
            : Result.Failure("玩家 ID 已存在");
    }
}
```

调用：

```csharp
Result result = manager.AddPlayer(player);

if (result.IsFailure)
{
    Console.WriteLine(result.ErrorMessage);
    return;
}

Console.WriteLine("新增成功");
```

这样调用方就知道失败原因了。

## 第 10 步：用 `Result<T>` 改造查找玩家

原来的方法：

```csharp
public Player? FindById(Guid id)
{
    return _playersById.TryGetValue(id, out Player? player)
        ? player
        : null;
}
```

改造后：

```csharp
public Result<Player> GetPlayer(Guid id)
{
    return _playersById.TryGetValue(id, out Player? player)
        ? Result<Player>.Success(player)
        : Result<Player>.Failure("玩家不存在");
}
```

调用：

```csharp
Result<Player> result = manager.GetPlayer(id);

if (result.IsFailure)
{
    Console.WriteLine(result.ErrorMessage);
    return;
}

Player player = result.Value!;
Console.WriteLine(player.GetSummary());
```

这种写法比直接返回 `null` 更明确。

## 第 11 步：用 `Result` 改造删除玩家

```csharp
public Result RemoveById(Guid id)
{
    bool removed = _playersById.Remove(id);

    return removed
        ? Result.Success()
        : Result.Failure("玩家不存在，无法删除");
}
```

调用：

```csharp
Result result = manager.RemoveById(id);

Console.WriteLine(result.IsSuccess ? "删除成功" : result.ErrorMessage);
```

## 第 12 步：用 `Result` 改造禁用玩家

```csharp
public Result DisableById(Guid id)
{
    if (!_playersById.TryGetValue(id, out Player? player))
    {
        return Result.Failure("玩家不存在，无法禁用");
    }

    if (!player.IsActive)
    {
        return Result.Failure("玩家已经是禁用状态");
    }

    player.IsActive = false;

    return Result.Success();
}
```

这里就体现了业务失败：

- 玩家不存在。
- 玩家已禁用。

这些都不是程序异常，不需要抛 exception。

## 第 13 步：自定义异常

有些错误确实代表程序状态异常，可以自定义异常。

推荐创建：

```text
Exceptions/
  PlayerStorageException.cs
```

代码：

```csharp
namespace gameC_.Exceptions;

public sealed class PlayerStorageException : Exception
{
    public PlayerStorageException(string message)
        : base(message)
    {
    }

    public PlayerStorageException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
```

自定义异常的好处：

- 调用方可以精确捕获。
- 错误来源更清晰。
- 比直接抛 `Exception` 更有语义。

## 第 14 步：在 `PlayerStorage` 中使用自定义异常

原来的保存：

```csharp
public void Save(IEnumerable<Player> players)
{
    string json = JsonSerializer.Serialize(players, _jsonOptions);
    File.WriteAllText(_filePath, json);
}
```

改造：

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

    public void Save(IEnumerable<Player> players)
    {
        try
        {
            string? directory = Path.GetDirectoryName(_filePath);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonSerializer.Serialize(players, _jsonOptions);

            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            throw new PlayerStorageException("保存玩家数据失败", ex);
        }
    }
}
```

这里用了异常筛选：

```csharp
catch (Exception ex) when (...)
```

意思是只捕获指定类型的异常。

## 第 15 步：改造加载逻辑

```csharp
public List<Player> Load()
{
    try
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

文件不存在不是异常：

```csharp
return new List<Player>();
```

JSON 损坏才是异常：

```csharp
throw new PlayerStorageException("玩家数据文件格式错误", ex);
```

## 第 16 步：在 `Program.cs` 中处理异常和结果

新增玩家：

```csharp
Result result = manager.AddPlayer(player);

if (result.IsFailure)
{
    Console.WriteLine(result.ErrorMessage);
    return;
}

Console.WriteLine("新增成功");
```

保存玩家：

```csharp
try
{
    storage.Save(manager.Players);
    Console.WriteLine("保存成功");
}
catch (PlayerStorageException ex)
{
    Console.WriteLine(ex.Message);

    if (ex.InnerException is not null)
    {
        Console.WriteLine($"详细原因：{ex.InnerException.Message}");
    }
}
```

这里的区别：

- 新增失败是业务结果，用 `Result`。
- 保存失败是文件系统异常，用 `try/catch`。

## 第 17 步：什么时候用异常，什么时候用 `Result`

推荐规则：

| 场景 | 推荐方式 |
| --- | --- |
| 玩家不存在 | `Result` |
| 玩家名称重复 | `Result` |
| 玩家已禁用 | `Result` |
| 输入 ID 格式错误 | `Result` 或提前校验 |
| 文件不存在且允许自动创建 | 返回空数据 |
| JSON 文件损坏 | 异常 |
| 文件无权限 | 异常 |
| 参数为 `null` 且不允许 | 异常 |
| 程序内部状态不一致 | 异常 |

一句话：

```text
用户可以正常修正的问题，多用 Result。
系统环境或程序状态异常，多用 exception。
```

## 第 18 步：完整练习

练习目标：让玩家管理系统拥有清晰的错误处理方式。

要求：

1. 创建：

```text
Common/Result.cs
Common/ResultT.cs
Exceptions/PlayerStorageException.cs
```

2. 改造 `PlayerManager`：

```csharp
public Result AddPlayer(Player player)
public Result<Player> GetPlayer(Guid id)
public Result RemoveById(Guid id)
public Result DisableById(Guid id)
```

3. `AddPlayer` 至少处理：

- 名称为空。
- 名称重复。
- ID 重复。

4. `RemoveById` 找不到玩家时返回失败。
5. `DisableById` 找不到玩家或已禁用时返回失败。
6. 改造 `PlayerStorage`，保存和加载失败时抛出 `PlayerStorageException`。
7. `Program.cs` 调用业务方法时处理 `Result`。
8. `Program.cs` 调用保存和加载时使用 `try/catch`。

验收标准：

- 新增重名玩家时，输出明确错误。
- 删除不存在玩家时，输出明确错误。
- 禁用已禁用玩家时，输出明确错误。
- JSON 文件损坏时，输出“玩家数据文件格式错误”。
- 程序不会因为常见错误直接崩溃。

## 第 19 步：本课作业

### 作业 1：增加错误代码

给 `Result` 增加：

```csharp
public string ErrorCode { get; }
```

示例：

```text
PLAYER_NAME_EMPTY
PLAYER_NAME_EXISTS
PLAYER_NOT_FOUND
PLAYER_ALREADY_DISABLED
```

要求：

- 失败时同时返回错误码和错误消息。
- 输出时显示错误码。

### 作业 2：创建玩家校验方法

在 `PlayerManager` 中增加私有方法：

```csharp
private Result ValidatePlayer(Player player)
```

要求：

- 名称不能为空。
- 等级必须大于等于 1。
- 金币必须大于等于 0。
- 区服不能为空。

`AddPlayer` 中调用它。

### 作业 3：加载失败时保留当前数据

重新加载玩家数据时，如果文件损坏：

- 不要清空当前内存数据。
- 输出错误信息。
- 保持程序继续运行。

### 作业 4：保存失败时提示用户重试

保存失败时输出：

```text
保存失败，请检查文件权限或稍后重试
```

同时输出详细异常信息。

## 本课常见错误

### 1. 捕获异常后什么都不做

不推荐：

```csharp
try
{
    storage.Save(players);
}
catch
{
}
```

这会让错误消失，后面排查会很痛苦。

推荐：

```csharp
catch (PlayerStorageException ex)
{
    Console.WriteLine(ex.Message);
}
```

### 2. 所有失败都抛异常

不推荐：

```csharp
throw new Exception("玩家不存在");
```

玩家不存在是可预期业务结果，更推荐：

```csharp
return Result<Player>.Failure("玩家不存在");
```

### 3. 所有异常都用 `Exception`

不推荐：

```csharp
throw new Exception("保存失败");
```

推荐：

```csharp
throw new PlayerStorageException("保存玩家数据失败", ex);
```

### 4. 没判断 `Result` 就直接取值

风险写法：

```csharp
Player player = result.Value!;
```

推荐：

```csharp
if (result.IsFailure)
{
    Console.WriteLine(result.ErrorMessage);
    return;
}

Player player = result.Value!;
```

### 5. 把异常信息直接当用户提示

开发阶段可以输出 `ex.Message`。

真实项目里通常要区分：

- 给用户看的友好提示。
- 给开发者看的详细日志。

本阶段先会处理即可，后续日志课程再优化。

## 本课复盘问题

学完后，尝试回答：

1. 什么是异常？
2. `try`、`catch`、`finally` 分别做什么？
3. 为什么不建议所有地方都 `catch (Exception)`？
4. 为什么业务失败不一定要抛异常？
5. `Result` 解决了 `bool` 返回值的什么问题？
6. `Result<T>` 适合什么场景？
7. 自定义异常有什么好处？
8. 玩家不存在应该用异常还是 `Result`？
9. JSON 文件损坏应该用异常还是 `Result`？
10. `PlayerManager` 和 `PlayerStorage` 的错误处理有什么区别？

## 下一课预告

课程八建议学习：

- 项目结构与命名规范。
- 拆分 `Models`、`Dtos`、`Services`、`Common`、`Exceptions`。
- 使用 `.editorconfig` 统一风格。
- 整理 README。
- 把当前玩家管理系统整理成一个更像真实项目的结构。
