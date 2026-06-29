# C# 基础学习专刊：课程八

## 课程主题

项目结构与命名规范：拆分 `Models`、`Dtos`、`Services`、`Common`、`Exceptions`，使用 `.editorconfig` 统一风格，并整理 README。

前面七课里，我们逐步写出了玩家管理系统的核心能力：

- `Player` 玩家模型。
- `PlayerManager` 玩家管理。
- `Dictionary<Guid, Player>` 按 ID 管理玩家。
- LINQ 排行榜和区服统计。
- `PlayerStorage` JSON 保存与加载。
- `Result` 和自定义异常。

现在的问题是：代码功能越来越多，如果所有内容都随便放，项目会很快变乱。

课程八的目标是让你开始建立 C# 项目规范意识。

## 本课目标

完成本课后，你应该能做到：

- 理解为什么要拆目录。
- 知道 `Models`、`Dtos`、`Services`、`Common`、`Exceptions` 分别放什么。
- 按职责整理玩家管理系统代码。
- 理解 C# 常见命名规范。
- 创建 `.editorconfig` 统一代码风格。
- 编写基础 README。
- 知道什么代码不应该放进 `Program.cs`。

## 第 1 步：为什么需要项目结构

小练习可以只有一个文件：

```text
Program.cs
```

但项目一旦变大，所有代码都塞进 `Program.cs` 会出现问题：

- 文件越来越长。
- 找不到类在哪里。
- 输入输出、业务逻辑、文件读写混在一起。
- 修改一个功能容易影响其他功能。
- 很难写测试。
- 新人接手成本高。

项目结构的目的不是“看起来高级”，而是让代码职责清楚。

一句话：

```text
项目结构是为了降低理解成本和修改成本。
```

## 第 2 步：推荐目录结构

当前阶段建议整理成：

```text
gameC#/
  Common/
    Result.cs
    ResultT.cs
  Dtos/
    PlayerSummaryDto.cs
    RankingPlayerDto.cs
    RegionStatDto.cs
  Exceptions/
    PlayerStorageException.cs
  Models/
    Player.cs
  Services/
    PlayerManager.cs
    PlayerStorage.cs
  data/
    players.json
  docs/
    CSharp_Basic_Course_01.md
    CSharp_Basic_Course_02.md
    ...
  Program.cs
  gameC#.csproj
  .editorconfig
  README.md
```

后续项目变大后，可以升级为：

```text
src/
  GamePlayerSystem/
tests/
  GamePlayerSystem.Tests/
docs/
```

但现在先不要过度拆分。当前重点是把职责分开。

## 第 3 步：`Models` 放什么

`Models` 放业务实体，也就是系统中的核心对象。

本项目中：

```text
Models/
  Player.cs
```

`Player` 表示玩家本身：

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

适合放进 `Models` 的内容：

- 玩家。
- 战绩。
- 道具。
- 任务。
- 订单。
- 背包。

不适合放进 `Models` 的内容：

- 文件读写。
- 控制台菜单。
- JSON 序列化细节。
- HTTP 接口返回对象。
- 数据库连接逻辑。

## 第 4 步：`Dtos` 放什么

DTO 是 Data Transfer Object，数据传输对象。

它通常用于：

- 接口返回。
- 页面展示。
- 报表输出。
- 跨层传递数据。

示例：

```text
Dtos/
  RankingPlayerDto.cs
  RegionStatDto.cs
  PlayerSummaryDto.cs
```

排行榜 DTO：

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

DTO 和 Model 的区别：

| 类型 | 作用 |
| --- | --- |
| Model | 表示业务对象本身 |
| DTO | 表示某个场景需要传输或展示的数据 |

例如：

- `Player` 是完整玩家实体。
- `RankingPlayerDto` 是排行榜展示数据。

不要为了方便直接把所有 `Player` 信息都暴露出去。DTO 可以减少不必要的数据泄露。

## 第 5 步：`Services` 放什么

`Services` 放业务服务，也就是处理业务流程和操作的类。

本项目中：

```text
Services/
  PlayerManager.cs
  PlayerStorage.cs
```

`PlayerManager` 负责玩家管理：

- 新增玩家。
- 删除玩家。
- 查找玩家。
- 禁用玩家。
- 生成排行榜。
- 统计区服分布。

`PlayerStorage` 负责文件保存与加载：

- 保存玩家 JSON。
- 加载玩家 JSON。
- 处理文件读写异常。

注意职责边界：

```text
PlayerManager 不应该知道控制台怎么显示。
PlayerStorage 不应该知道排行榜怎么计算。
Player 不应该知道自己怎么保存到文件。
Program.cs 不应该承担大量业务逻辑。
```

## 第 6 步：`Common` 放什么

`Common` 放通用基础类。

本项目中：

```text
Common/
  Result.cs
  ResultT.cs
```

`Result` 表示无返回值操作的结果：

```csharp
Result result = manager.RemoveById(id);
```

`Result<T>` 表示成功时带数据的结果：

```csharp
Result<Player> result = manager.GetPlayer(id);
```

适合放进 `Common` 的内容：

- `Result`
- 通用常量
- 通用分页对象
- 通用时间工具

不建议把所有杂物都塞进 `Common`。

如果你发现 `Common` 变成一个垃圾桶，说明需要重新拆目录。

## 第 7 步：`Exceptions` 放什么

`Exceptions` 放自定义异常。

本项目中：

```text
Exceptions/
  PlayerStorageException.cs
```

示例：

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

什么时候创建自定义异常：

- 想明确表达错误来源。
- 想让调用方精确捕获。
- 想保留内部异常。
- 系统级错误需要统一处理。

不要为每一个小业务失败都创建异常。

例如：

```text
玩家不存在
玩家名称重复
玩家已经禁用
```

这些更适合 `Result`。

## 第 8 步：命名空间规范

C# 命名空间通常和目录结构对应。

如果项目根命名空间是：

```xml
<RootNamespace>gameC_</RootNamespace>
```

那么：

```text
Models/Player.cs
```

建议命名空间：

```csharp
namespace gameC_.Models;
```

```text
Services/PlayerManager.cs
```

建议命名空间：

```csharp
namespace gameC_.Services;
```

```text
Dtos/RankingPlayerDto.cs
```

建议命名空间：

```csharp
namespace gameC_.Dtos;
```

这样别人看到命名空间，就大概知道文件在哪里。

## 第 9 步：C# 常见命名规范

### 类名

使用 PascalCase：

```csharp
public sealed class PlayerManager
```

类名通常是名词或名词短语：

```text
Player
PlayerManager
PlayerStorage
RankingPlayerDto
```

### 方法名

使用 PascalCase，通常是动词或动词短语：

```csharp
AddPlayer
RemoveById
GetRanking
Load
Save
```

### 属性名

使用 PascalCase：

```csharp
public string Name { get; set; }
public int Level { get; set; }
```

### 私有字段

使用 `_camelCase`：

```csharp
private readonly Dictionary<Guid, Player> _playersById = new();
private readonly string _filePath;
```

### 局部变量和参数

使用 camelCase：

```csharp
public Result RemoveById(Guid id)
{
    bool removed = _playersById.Remove(id);
    return removed ? Result.Success() : Result.Failure("玩家不存在");
}
```

### 接口名

使用 `I` 开头：

```csharp
public interface IPlayerStorage
```

本阶段还不急着写接口，后面讲测试和依赖注入时再引入。

## 第 10 步：命名对照表

| 类型 | 规范 | 示例 |
| --- | --- | --- |
| 类 | PascalCase | `PlayerManager` |
| DTO | PascalCase + Dto | `RankingPlayerDto` |
| 异常 | PascalCase + Exception | `PlayerStorageException` |
| 方法 | PascalCase | `GetRanking` |
| 属性 | PascalCase | `IsActive` |
| 私有字段 | `_camelCase` | `_playersById` |
| 局部变量 | camelCase | `playerName` |
| 参数 | camelCase | `filePath` |
| 常量 | PascalCase | `DefaultRegion` |
| 接口 | I + PascalCase | `IPlayerRepository` |
| 异步方法 | PascalCase + Async | `SaveAsync` |

不推荐：

```csharp
public string player_name;
public int LEVEL;
public void do_something()
```

推荐：

```csharp
public string PlayerName { get; set; }
public int Level { get; set; }
public void DoSomething()
```

## 第 11 步：整理 `Program.cs`

`Program.cs` 应该负责：

- 创建对象。
- 加载数据。
- 显示菜单。
- 接收输入。
- 调用服务。
- 展示结果。

`Program.cs` 不应该负责：

- 玩家业务规则。
- 文件读写细节。
- JSON 配置细节。
- 排行榜计算细节。
- 复杂校验逻辑。

如果你看到 `Program.cs` 中出现大量类似代码：

```csharp
players
    .Where(...)
    .GroupBy(...)
    .Select(...)
```

说明这些逻辑可能应该移动到 `PlayerManager`。

如果你看到 `Program.cs` 中出现：

```csharp
File.ReadAllText(...)
JsonSerializer.Deserialize(...)
```

说明这些逻辑可能应该移动到 `PlayerStorage`。

## 第 12 步：创建 `.editorconfig`

`.editorconfig` 用于统一代码风格。

推荐在项目根目录创建：

```text
.editorconfig
```

基础内容：

```ini
root = true

[*]
charset = utf-8
end_of_line = crlf
insert_final_newline = true
indent_style = space
indent_size = 4
trim_trailing_whitespace = true

[*.cs]
csharp_style_namespace_declarations = file_scoped:suggestion
csharp_style_var_when_type_is_apparent = true:suggestion
csharp_style_var_elsewhere = false:suggestion
csharp_style_var_for_built_in_types = false:suggestion
dotnet_style_qualification_for_field = false:suggestion
dotnet_style_qualification_for_property = false:suggestion
dotnet_style_qualification_for_method = false:suggestion
dotnet_style_qualification_for_event = false:suggestion

dotnet_naming_rule.private_fields_should_be_camel_case.severity = suggestion
dotnet_naming_rule.private_fields_should_be_camel_case.symbols = private_fields
dotnet_naming_rule.private_fields_should_be_camel_case.style = private_field_style

dotnet_naming_symbols.private_fields.applicable_kinds = field
dotnet_naming_symbols.private_fields.applicable_accessibilities = private

dotnet_naming_style.private_field_style.required_prefix = _
dotnet_naming_style.private_field_style.capitalization = camel_case
```

说明：

- 缩进统一 4 个空格。
- 文件编码使用 UTF-8。
- C# 推荐文件范围命名空间。
- 私有字段建议 `_camelCase`。
- `var` 的使用保持克制。

## 第 13 步：README 应该写什么

README 是项目入口说明。

一个基础 README 至少包含：

```text
项目名称
项目介绍
技术栈
功能列表
目录结构
运行方式
学习目标
后续计划
```

示例：

```markdown
# Game Player System

这是一个用于复习 C# 基础和项目规范的玩家管理系统。

## 技术栈

- C# / .NET
- System.Text.Json
- LINQ

## 功能

- 新增玩家
- 删除玩家
- 禁用玩家
- 按 ID 查询玩家
- 生成排行榜
- 统计区服分布
- 保存和加载 JSON 数据

## 项目结构

```text
Common/
Dtos/
Exceptions/
Models/
Services/
docs/
Program.cs
```

## 运行

```powershell
dotnet run
```

## 数据文件

玩家数据保存在：

```text
data/players.json
```
```

README 不需要一开始写得特别长，但必须让别人知道怎么运行项目。

## 第 14 步：推荐整理后的文件职责

### `Models/Player.cs`

负责：

- 玩家属性。
- 玩家自身相关计算。
- 玩家自身状态。

不负责：

- 保存文件。
- 菜单输入。
- JSON 读写。

### `Dtos/RankingPlayerDto.cs`

负责：

- 排行榜展示数据。

不负责：

- 玩家业务逻辑。
- 排行榜计算。

### `Services/PlayerManager.cs`

负责：

- 玩家集合管理。
- 玩家查询。
- 业务校验。
- 排行榜生成。
- 区服统计。

不负责：

- 控制台输入输出。
- 文件读写细节。

### `Services/PlayerStorage.cs`

负责：

- 保存玩家数据。
- 加载玩家数据。
- JSON 序列化配置。

不负责：

- 玩家业务校验。
- 排行榜计算。

### `Common/Result.cs`

负责：

- 表达操作成功或失败。

不负责：

- 具体业务逻辑。

### `Exceptions/PlayerStorageException.cs`

负责：

- 表达玩家数据存储异常。

不负责：

- 玩家不存在这类业务失败。

## 第 15 步：完整练习

练习目标：把当前玩家管理系统整理成规范目录结构。

要求：

1. 创建目录：

```text
Models/
Dtos/
Services/
Common/
Exceptions/
data/
```

2. 移动或创建文件：

```text
Models/Player.cs
Dtos/RankingPlayerDto.cs
Dtos/RegionStatDto.cs
Dtos/PlayerSummaryDto.cs
Services/PlayerManager.cs
Services/PlayerStorage.cs
Common/Result.cs
Common/ResultT.cs
Exceptions/PlayerStorageException.cs
```

3. 确保命名空间和目录对应。
4. `Program.cs` 只保留菜单和调用逻辑。
5. 创建 `.editorconfig`。
6. 创建 `README.md`。
7. 运行：

```powershell
dotnet run
```

验收标准：

- 项目可以正常编译运行。
- 每个类文件只放一个主要类。
- 类名和文件名一致。
- 命名空间和目录对应。
- 私有字段使用 `_camelCase`。
- 公共类、方法、属性使用 PascalCase。
- README 能说明项目功能和运行方式。

## 第 16 步：本课作业

### 作业 1：创建 README

创建 `README.md`，包含：

- 项目简介。
- 当前功能。
- 项目结构。
- 如何运行。
- 数据文件位置。
- 下一步计划。

### 作业 2：补充 `.editorconfig`

创建 `.editorconfig`，至少包含：

- 缩进 4 空格。
- UTF-8。
- 文件末尾换行。
- 私有字段 `_camelCase`。
- 文件范围命名空间建议。

### 作业 3：整理 `Program.cs`

检查 `Program.cs`：

- 如果有玩家业务规则，移动到 `PlayerManager`。
- 如果有 JSON 读写，移动到 `PlayerStorage`。
- 如果有 DTO 构造，优先移动到 `PlayerManager`。

### 作业 4：写一份结构说明

在 `docs` 下创建：

```text
Project_Structure.md
```

说明每个目录放什么、不放什么。

## 本课常见错误

### 1. 为了拆目录而拆目录

目录不是越多越好。

当前阶段不需要：

```text
Factories/
Providers/
Handlers/
Processors/
Managers/
Helpers/
Utils/
```

先保持简单：

```text
Models
Dtos
Services
Common
Exceptions
```

### 2. `Common` 变成垃圾桶

不推荐：

```text
Common/
  PlayerHelper.cs
  FileHelper.cs
  StringHelper.cs
  EverythingHelper.cs
```

如果一个类名字叫 `Helper`，先想想它是否真的有明确职责。

### 3. DTO 里写复杂业务逻辑

不推荐：

```csharp
public sealed class RankingPlayerDto
{
    public int CalculatePower()
    {
        ...
    }
}
```

DTO 主要负责携带数据，不负责核心业务。

### 4. 命名空间和目录不一致

不推荐：

```text
Services/PlayerManager.cs
namespace gameC_.Models;
```

推荐：

```text
Services/PlayerManager.cs
namespace gameC_.Services;
```

### 5. README 只写标题

不够：

```markdown
# Game
```

至少要写运行方式：

```markdown
## 运行

```powershell
dotnet run
```
```

## 本课复盘问题

学完后，尝试回答：

1. 为什么项目需要目录结构？
2. `Models` 应该放什么？
3. `Dtos` 和 `Models` 有什么区别？
4. `Services` 应该负责什么？
5. `Common` 为什么不能乱放杂物？
6. 自定义异常为什么放在 `Exceptions`？
7. 命名空间为什么建议和目录对应？
8. C# 中类名、方法名、属性名分别用什么命名风格？
9. `.editorconfig` 解决什么问题？
10. README 最少应该包含哪些内容？

## 下一课预告

课程九建议学习：

- 单元测试基础。
- 使用 xUnit 测试 `Player` 和 `PlayerManager`。
- 测试新增、删除、查找、排行榜。
- 学习 Arrange、Act、Assert。
- 让项目从“能运行”升级成“可验证”。
