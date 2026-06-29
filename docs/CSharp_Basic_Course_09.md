# C# 基础学习专刊：课程九

## 课程主题

单元测试基础：使用 xUnit 测试 `Player` 和 `PlayerManager`，学习 Arrange、Act、Assert。

前面八课中，我们已经把玩家管理系统整理出了基础项目结构：

- `Models/Player.cs`
- `Services/PlayerManager.cs`
- `Services/PlayerStorage.cs`
- `Dtos/`
- `Common/Result.cs`
- `Exceptions/PlayerStorageException.cs`

代码能运行是一回事，代码是否稳定、是否符合预期，是另一回事。

课程九的目标是让你开始写单元测试，让核心逻辑变成“可验证”的。

## 本课目标

完成本课后，你应该能做到：

- 理解什么是单元测试。
- 理解为什么测试核心业务逻辑。
- 创建 xUnit 测试项目。
- 添加项目引用。
- 编写第一个 `[Fact]` 测试。
- 理解 Arrange、Act、Assert。
- 测试 `Player` 的等级、金币和战力规则。
- 测试 `PlayerManager` 的新增、删除、查找、重名校验和排行榜。
- 使用 `dotnet test` 运行测试。

## 第 1 步：什么是单元测试

单元测试是对一个很小的代码单元进行验证。

这个单元通常是：

- 一个方法。
- 一个类。
- 一个明确的业务规则。

例如：

```text
当玩家等级设置为 -10 时，实际等级应该变成 1。
```

这就是一个适合单元测试的规则。

单元测试的目的不是证明代码永远没 bug，而是让你在修改代码时更有底气。

## 第 2 步：哪些代码适合先测试

优先测试：

- 有明确输入和输出的方法。
- 核心业务规则。
- 容易改坏的逻辑。
- 边界条件。
- 曾经出过 bug 的地方。

本项目优先测试：

```text
Player.CalculatePower()
Player.Level
Player.Gold
PlayerManager.AddPlayer()
PlayerManager.GetPlayer()
PlayerManager.RemoveById()
PlayerManager.GetRanking()
```

暂时不优先测试：

- 控制台输入输出。
- 菜单显示。
- 简单属性 get / set。
- 直接调用系统 API 的薄包装。

## 第 3 步：什么是 xUnit

xUnit 是 .NET 中常用的测试框架之一。

常见测试框架：

- xUnit
- NUnit
- MSTest

本课程使用 xUnit，因为它在 .NET 项目里很常见，写法也比较清爽。

测试方法通常长这样：

```csharp
[Fact]
public void CalculatePower_ShouldReturnExpectedValue()
{
    Player player = new Player
    {
        Level = 10,
        Gold = 500
    };

    int power = player.CalculatePower();

    Assert.Equal(1050, power);
}
```

## 第 4 步：推荐测试项目结构

如果当前项目根目录是：

```text
gameC#/
  gameC#.csproj
  Program.cs
  Models/
  Services/
  docs/
```

推荐在上一级或当前根目录旁边创建测试项目。初学阶段可以先放在当前目录下：

```text
gameC#/
  gameC#.csproj
  Models/
  Services/
  tests/
    gameC#.Tests/
      gameC#.Tests.csproj
      PlayerTests.cs
      PlayerManagerTests.cs
```

更正式的解决方案结构通常是：

```text
ProjectRoot/
  src/
    GamePlayerSystem/
  tests/
    GamePlayerSystem.Tests/
```

本课先按当前项目继续，不强行大迁移。

## 第 5 步：创建 xUnit 测试项目

在项目根目录执行：

```powershell
dotnet new xunit -o tests/gameC#.Tests
```

这会创建：

```text
tests/
  gameC#.Tests/
    gameC#.Tests.csproj
    UnitTest1.cs
```

然后给测试项目引用主项目：

```powershell
dotnet add tests/gameC#.Tests/gameC#.Tests.csproj reference gameC#.csproj
```

运行测试：

```powershell
dotnet test tests/gameC#.Tests/gameC#.Tests.csproj
```

如果看到测试通过，说明测试项目已经能跑。

## 第 6 步：整理测试文件

默认生成的：

```text
UnitTest1.cs
```

可以删除或改名为：

```text
PlayerTests.cs
```

测试类命名建议：

```text
被测试类名 + Tests
```

例如：

```text
PlayerTests
PlayerManagerTests
PlayerStorageTests
```

测试方法命名建议：

```text
被测方法_场景_期望结果
```

例如：

```csharp
CalculatePower_WithLevelAndGold_ReturnsExpectedPower
SetLevel_WithNegativeValue_UsesDefaultLevel
AddPlayer_WithDuplicateName_ReturnsFailure
```

方法名可以稍长一点，测试代码的可读性比短更重要。

## 第 7 步：Arrange、Act、Assert

单元测试常用三段式：

```text
Arrange：准备数据和环境
Act：执行被测试的动作
Assert：验证结果
```

示例：

```csharp
[Fact]
public void CalculatePower_WithLevelAndGold_ReturnsExpectedPower()
{
    // Arrange
    Player player = new Player
    {
        Level = 10,
        Gold = 500
    };

    // Act
    int power = player.CalculatePower();

    // Assert
    Assert.Equal(1050, power);
}
```

这三个注释不是必须的，但初学阶段建议保留，有助于养成测试结构感。

## 第 8 步：测试 `Player.CalculatePower`

`Player` 规则：

```csharp
public int CalculatePower()
{
    return Level * 100 + Gold / 10;
}
```

测试：

```csharp
using gameC_.Models;

namespace gameC_.Tests;

public sealed class PlayerTests
{
    [Fact]
    public void CalculatePower_WithLevelAndGold_ReturnsExpectedPower()
    {
        // Arrange
        Player player = new Player
        {
            Level = 10,
            Gold = 500
        };

        // Act
        int power = player.CalculatePower();

        // Assert
        Assert.Equal(1050, power);
    }
}
```

解释：

```text
等级 10 -> 10 * 100 = 1000
金币 500 -> 500 / 10 = 50
战力 = 1050
```

## 第 9 步：测试等级校验

`Player.Level` 规则：

```text
如果设置小于 1 的等级，自动变成 1。
```

测试：

```csharp
[Fact]
public void Level_WithNegativeValue_UsesDefaultLevel()
{
    // Arrange
    Player player = new Player();

    // Act
    player.Level = -10;

    // Assert
    Assert.Equal(1, player.Level);
}
```

再测试正常值：

```csharp
[Fact]
public void Level_WithValidValue_UsesAssignedValue()
{
    // Arrange
    Player player = new Player();

    // Act
    player.Level = 20;

    // Assert
    Assert.Equal(20, player.Level);
}
```

测试不只测错误场景，也要测正常场景。

## 第 10 步：测试金币校验

`Player.Gold` 规则：

```text
如果设置小于 0 的金币，自动变成 0。
```

测试：

```csharp
[Fact]
public void Gold_WithNegativeValue_UsesZero()
{
    // Arrange
    Player player = new Player();

    // Act
    player.Gold = -100;

    // Assert
    Assert.Equal(0, player.Gold);
}
```

正常值：

```csharp
[Fact]
public void Gold_WithValidValue_UsesAssignedValue()
{
    // Arrange
    Player player = new Player();

    // Act
    player.Gold = 300;

    // Assert
    Assert.Equal(300, player.Gold);
}
```

## 第 11 步：测试 `GetRegionName`

```csharp
[Theory]
[InlineData("CN", "国服")]
[InlineData("NA", "北美服")]
[InlineData("EU", "欧洲服")]
[InlineData("UNKNOWN", "未知区服")]
public void GetRegionName_WithRegionCode_ReturnsRegionName(
    string region,
    string expectedRegionName)
{
    // Arrange
    Player player = new Player
    {
        Region = region
    };

    // Act
    string regionName = player.GetRegionName();

    // Assert
    Assert.Equal(expectedRegionName, regionName);
}
```

这里用了两个新特性：

```csharp
[Theory]
[InlineData(...)]
```

`[Fact]` 适合无参数测试。

`[Theory]` 适合多组输入输出测试。

上面一个测试方法会运行 4 次。

## 第 12 步：准备 `PlayerManagerTests`

创建：

```text
tests/gameC#.Tests/PlayerManagerTests.cs
```

基本结构：

```csharp
using gameC_.Common;
using gameC_.Models;
using gameC_.Services;

namespace gameC_.Tests;

public sealed class PlayerManagerTests
{
    [Fact]
    public void AddPlayer_WithValidPlayer_ReturnsSuccess()
    {
        // Arrange
        PlayerManager manager = new PlayerManager();
        Player player = new Player
        {
            Name = "Alice",
            Level = 10,
            Gold = 500,
            Region = "CN"
        };

        // Act
        Result result = manager.AddPlayer(player);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(manager.Players);
    }
}
```

`Assert.Single(manager.Players)` 表示集合里应该只有一个元素。

## 第 13 步：测试重名玩家

规则：

```text
新增玩家时，名称不能重复，忽略大小写。
```

测试：

```csharp
[Fact]
public void AddPlayer_WithDuplicateName_ReturnsFailure()
{
    // Arrange
    PlayerManager manager = new PlayerManager();

    manager.AddPlayer(new Player
    {
        Name = "Alice",
        Level = 10
    });

    Player duplicatePlayer = new Player
    {
        Name = "alice",
        Level = 20
    };

    // Act
    Result result = manager.AddPlayer(duplicatePlayer);

    // Assert
    Assert.True(result.IsFailure);
    Assert.Equal("玩家名称已存在", result.ErrorMessage);
    Assert.Single(manager.Players);
}
```

这个测试验证了三件事：

- 操作失败。
- 错误信息正确。
- 玩家数量没有增加。

## 第 14 步：测试按 ID 查找

```csharp
[Fact]
public void GetPlayer_WithExistingId_ReturnsPlayer()
{
    // Arrange
    PlayerManager manager = new PlayerManager();
    Player player = new Player
    {
        Name = "Alice",
        Level = 10
    };

    manager.AddPlayer(player);

    // Act
    Result<Player> result = manager.GetPlayer(player.Id);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(player.Id, result.Value!.Id);
}
```

测试找不到：

```csharp
[Fact]
public void GetPlayer_WithMissingId_ReturnsFailure()
{
    // Arrange
    PlayerManager manager = new PlayerManager();
    Guid missingId = Guid.NewGuid();

    // Act
    Result<Player> result = manager.GetPlayer(missingId);

    // Assert
    Assert.True(result.IsFailure);
    Assert.Equal("玩家不存在", result.ErrorMessage);
}
```

## 第 15 步：测试删除玩家

```csharp
[Fact]
public void RemoveById_WithExistingId_RemovesPlayer()
{
    // Arrange
    PlayerManager manager = new PlayerManager();
    Player player = new Player
    {
        Name = "Alice"
    };

    manager.AddPlayer(player);

    // Act
    Result result = manager.RemoveById(player.Id);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Empty(manager.Players);
}
```

删除不存在：

```csharp
[Fact]
public void RemoveById_WithMissingId_ReturnsFailure()
{
    // Arrange
    PlayerManager manager = new PlayerManager();

    // Act
    Result result = manager.RemoveById(Guid.NewGuid());

    // Assert
    Assert.True(result.IsFailure);
    Assert.Equal("玩家不存在，无法删除", result.ErrorMessage);
}
```

## 第 16 步：测试禁用玩家

```csharp
[Fact]
public void DisableById_WithExistingActivePlayer_DisablesPlayer()
{
    // Arrange
    PlayerManager manager = new PlayerManager();
    Player player = new Player
    {
        Name = "Alice",
        IsActive = true
    };

    manager.AddPlayer(player);

    // Act
    Result result = manager.DisableById(player.Id);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.False(player.IsActive);
}
```

测试重复禁用：

```csharp
[Fact]
public void DisableById_WithAlreadyDisabledPlayer_ReturnsFailure()
{
    // Arrange
    PlayerManager manager = new PlayerManager();
    Player player = new Player
    {
        Name = "Alice",
        IsActive = false
    };

    manager.AddPlayer(player);

    // Act
    Result result = manager.DisableById(player.Id);

    // Assert
    Assert.True(result.IsFailure);
    Assert.Equal("玩家已经是禁用状态", result.ErrorMessage);
}
```

## 第 17 步：测试排行榜

规则：

- 只显示活跃玩家。
- 按战力降序。
- 返回指定数量。
- 排名从 1 开始。

测试：

```csharp
[Fact]
public void GetRanking_WithPlayers_ReturnsActivePlayersOrderedByPower()
{
    // Arrange
    PlayerManager manager = new PlayerManager();

    manager.AddPlayer(new Player
    {
        Name = "Low",
        Level = 5,
        Gold = 0,
        IsActive = true
    });

    manager.AddPlayer(new Player
    {
        Name = "High",
        Level = 30,
        Gold = 1000,
        IsActive = true
    });

    manager.AddPlayer(new Player
    {
        Name = "Disabled",
        Level = 99,
        Gold = 9999,
        IsActive = false
    });

    // Act
    List<RankingPlayerDto> ranking = manager.GetRanking(10);

    // Assert
    Assert.Equal(2, ranking.Count);
    Assert.Equal("High", ranking[0].Name);
    Assert.Equal(1, ranking[0].Rank);
    Assert.Equal("Low", ranking[1].Name);
    Assert.Equal(2, ranking[1].Rank);
}
```

记得添加：

```csharp
using gameC_.Dtos;
```

## 第 18 步：运行测试

执行：

```powershell
dotnet test tests/gameC#.Tests/gameC#.Tests.csproj
```

如果成功，会看到类似：

```text
Passed!  - Failed: 0, Passed: 10, Skipped: 0
```

如果失败，测试输出会告诉你：

- 哪个测试失败。
- 期望值是什么。
- 实际值是什么。
- 失败位置在哪一行。

测试失败不是坏事。测试失败是在提醒你：代码行为和预期不一致。

## 第 19 步：测试应该怎么写才健康

建议：

- 一个测试只验证一个主要行为。
- 测试名称要表达场景和期望。
- 不要依赖测试执行顺序。
- 每个测试自己准备数据。
- 不要让测试共享可变状态。
- 核心业务先测，菜单输入输出后测。
- 失败场景和边界条件要测。

不推荐：

```csharp
[Fact]
public void Test1()
```

推荐：

```csharp
[Fact]
public void AddPlayer_WithDuplicateName_ReturnsFailure()
```

## 第 20 步：完整练习

练习目标：给 `Player` 和 `PlayerManager` 添加基础单元测试。

要求：

1. 创建 xUnit 测试项目：

```powershell
dotnet new xunit -o tests/gameC#.Tests
```

2. 引用主项目：

```powershell
dotnet add tests/gameC#.Tests/gameC#.Tests.csproj reference gameC#.csproj
```

3. 创建：

```text
PlayerTests.cs
PlayerManagerTests.cs
```

4. `PlayerTests` 至少测试：

- `CalculatePower`
- `Level` 小于 1 时变成 1
- `Gold` 小于 0 时变成 0
- `GetRegionName`

5. `PlayerManagerTests` 至少测试：

- 新增成功。
- 重名失败。
- 按 ID 查找成功。
- 按 ID 查找失败。
- 删除成功。
- 删除不存在玩家失败。
- 禁用成功。
- 重复禁用失败。
- 排行榜排序正确。
- 排行榜不包含禁用玩家。

6. 运行：

```powershell
dotnet test tests/gameC#.Tests/gameC#.Tests.csproj
```

验收标准：

- 至少 10 个测试。
- 测试全部通过。
- 每个测试使用 Arrange、Act、Assert。
- 测试方法名表达清楚场景。

## 第 21 步：本课作业

### 作业 1：测试区服统计

给 `PlayerManager.GetRegionStats()` 写测试。

要求：

- 添加多个不同区服玩家。
- 包含活跃和非活跃玩家。
- 验证总人数。
- 验证活跃人数。

### 作业 2：测试批量导入

给 `AddPlayers(IEnumerable<Player> players)` 写测试。

要求：

- 批量添加 3 个玩家。
- 验证返回成功添加数量。
- 验证 `manager.Players.Count`。

### 作业 3：测试空数据排行榜

当没有任何玩家时：

```csharp
List<RankingPlayerDto> ranking = manager.GetRanking(10);
```

要求：

- 返回空列表。
- 不抛异常。

### 作业 4：测试错误消息

如果你给 `Result` 增加了 `ErrorCode`，测试：

- 重名玩家返回 `PLAYER_NAME_EXISTS`。
- 玩家不存在返回 `PLAYER_NOT_FOUND`。

## 本课常见错误

### 1. 测试项目没有引用主项目

如果测试项目找不到 `Player`，通常是忘了引用：

```powershell
dotnet add tests/gameC#.Tests/gameC#.Tests.csproj reference gameC#.csproj
```

### 2. 忘记添加命名空间

如果找不到类型，检查：

```csharp
using gameC_.Models;
using gameC_.Services;
using gameC_.Common;
using gameC_.Dtos;
```

### 3. 一个测试测太多东西

不推荐一个测试覆盖新增、删除、排行榜、保存文件全部流程。

单元测试应该小而清晰。

### 4. 测试依赖上一个测试的数据

不推荐：

```text
测试 A 新增玩家
测试 B 依赖测试 A 新增的玩家
```

每个测试都应该自己准备数据。

### 5. 只测试成功，不测试失败

失败路径更容易藏 bug。

至少测试：

- 找不到玩家。
- 重名玩家。
- 非法等级。
- 禁用已禁用玩家。
- 空列表排行榜。

## 本课复盘问题

学完后，尝试回答：

1. 什么是单元测试？
2. 为什么要测试核心业务逻辑？
3. xUnit 中 `[Fact]` 是什么？
4. `[Theory]` 和 `[InlineData]` 适合什么场景？
5. Arrange、Act、Assert 分别是什么意思？
6. 为什么测试不应该依赖执行顺序？
7. 为什么每个测试应该自己准备数据？
8. `Assert.Equal`、`Assert.True`、`Assert.Empty` 分别验证什么？
9. 为什么失败场景也要测试？
10. 哪些代码暂时不适合优先写单元测试？

## 下一课预告

课程十建议学习：

- 异步编程基础。
- `Task`、`async`、`await`。
- 把文件保存和加载改成异步。
- 使用 `File.ReadAllTextAsync` 和 `File.WriteAllTextAsync`。
- 理解什么时候需要异步，什么时候不需要。
