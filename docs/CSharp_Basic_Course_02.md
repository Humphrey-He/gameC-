# C# 基础学习专刊：课程二

## 课程主题

类和对象：把玩家信息从散落变量升级成 `Player` 类。

课程一中，我们用多个变量保存玩家信息：

```csharp
string playerName = "Alice";
int level = 10;
string region = "CN";
int gold = 500;
```

这种写法在小练习里可以接受，但一旦字段变多、逻辑变复杂，就会很散。课程二的目标是把这些数据收拢到一个对象里：

```csharp
Player player = new Player();
```

然后通过对象管理玩家的名称、等级、区服、金币和战力。

## 本课目标

完成本课后，你应该能做到：

- 理解什么是类和对象。
- 创建一个 `Player` 类。
- 使用字段和属性保存数据。
- 理解构造函数的作用。
- 给类添加方法。
- 理解 `class` 和 `struct` 的基础区别。
- 把课程一的玩家信息录入程序改成对象版本。

## 第 1 步：为什么需要类

课程一的玩家信息可能长这样：

```csharp
string playerName = "Alice";
int level = 10;
string region = "CN";
int gold = 500;
bool isActive = true;
```

问题是：

- 这些变量只是碰巧放在一起，编译器不知道它们属于同一个玩家。
- 如果有多个玩家，就要复制很多变量。
- 方法参数会越来越多。
- 业务逻辑容易散落在各处。

例如：

```csharp
static int CalculatePower(int level, int gold)
{
    return level * 100 + gold / 10;
}
```

现在只有等级和金币还好。如果以后战力还要考虑装备、称号、VIP 等级，参数会越来越乱。

类的作用就是把“数据”和“相关行为”组织到一起。

## 第 2 步：创建第一个类

新建一个 `Player` 类：

```csharp
public sealed class Player
{
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public string Region { get; set; } = string.Empty;
    public int Gold { get; set; }
    public bool IsActive { get; set; }
}
```

解释：

| 代码 | 含义 |
| --- | --- |
| `public` | 其他代码可以访问这个类 |
| `sealed` | 不允许其他类继承它 |
| `class` | 定义引用类型 |
| `Name` | 玩家名称属性 |
| `get; set;` | 允许读取和修改属性 |
| `string.Empty` | 默认空字符串，避免 `null` |

使用方式：

```csharp
Player player = new Player();

player.Name = "Alice";
player.Level = 10;
player.Region = "CN";
player.Gold = 500;
player.IsActive = true;

Console.WriteLine(player.Name);
```

这就比散落变量更像一个完整实体。

## 第 3 步：字段和属性

C# 中常见两种成员：

- 字段：直接保存数据。
- 属性：对外暴露数据，内部可以控制读写。

字段写法：

```csharp
public string name = string.Empty;
```

属性写法：

```csharp
public string Name { get; set; } = string.Empty;
```

业务代码中更推荐使用属性。

原因：

- 属性可以控制读取和写入。
- 属性可以加校验。
- 属性更符合 C# 项目规范。
- 很多框架会默认识别属性，例如 JSON 序列化、ORM。

推荐：

```csharp
public int Level { get; set; }
```

不推荐：

```csharp
public int level;
```

私有字段通常用于封装内部状态：

```csharp
private int _level;
```

私有字段命名一般使用 `_camelCase`。

## 第 4 步：给属性添加校验

如果等级不能小于 1，可以这样写：

```csharp
public sealed class Player
{
    private int _level = 1;

    public string Name { get; set; } = string.Empty;

    public int Level
    {
        get => _level;
        set => _level = value < 1 ? 1 : value;
    }
}
```

使用：

```csharp
Player player = new Player();
player.Level = -100;

Console.WriteLine(player.Level); // 1
```

这里的逻辑是：

- 外部给 `Level` 赋值。
- `set` 捕获这个值。
- 如果小于 1，就改成 1。
- 最终保存到 `_level`。

这种封装方式能避免对象进入明显错误的状态。

## 第 5 步：自动属性和完整属性

自动属性：

```csharp
public int Gold { get; set; }
```

完整属性：

```csharp
private int _gold;

public int Gold
{
    get => _gold;
    set => _gold = value < 0 ? 0 : value;
}
```

什么时候用自动属性：

- 不需要特殊校验。
- 只是保存和读取数据。

什么时候用完整属性：

- 需要校验。
- 需要转换。
- 需要限制写入。
- 设置属性时需要触发额外逻辑。

## 第 6 步：构造函数

构造函数用于创建对象时初始化数据。

没有构造函数时：

```csharp
Player player = new Player();

player.Name = "Alice";
player.Level = 10;
player.Region = "CN";
player.Gold = 500;
```

有构造函数后：

```csharp
public sealed class Player
{
    public Player(string name, int level, string region, int gold)
    {
        Name = name;
        Level = level;
        Region = region;
        Gold = gold;
        IsActive = true;
    }

    public string Name { get; set; }
    public int Level { get; set; }
    public string Region { get; set; }
    public int Gold { get; set; }
    public bool IsActive { get; set; }
}
```

使用：

```csharp
Player player = new Player("Alice", 10, "CN", 500);
```

构造函数特点：

- 方法名和类名相同。
- 没有返回值类型。
- 创建对象时自动调用。

## 第 7 步：默认构造函数

如果你没有写任何构造函数，C# 会给你一个默认无参构造函数：

```csharp
Player player = new Player();
```

但是一旦你写了带参数构造函数，默认无参构造函数就不会自动存在。

例如：

```csharp
public sealed class Player
{
    public Player(string name)
    {
        Name = name;
    }

    public string Name { get; set; }
}
```

此时下面代码会报错：

```csharp
Player player = new Player();
```

如果你两个都想支持，需要手动写：

```csharp
public sealed class Player
{
    public Player()
    {
        Name = "未命名玩家";
    }

    public Player(string name)
    {
        Name = name;
    }

    public string Name { get; set; }
}
```

## 第 8 步：对象初始化器

C# 常用对象初始化器：

```csharp
Player player = new Player
{
    Name = "Alice",
    Level = 10,
    Region = "CN",
    Gold = 500,
    IsActive = true
};
```

这种写法很常见，适合 DTO、配置、测试数据和简单模型。

它和下面写法等价：

```csharp
Player player = new Player();

player.Name = "Alice";
player.Level = 10;
player.Region = "CN";
player.Gold = 500;
player.IsActive = true;
```

## 第 9 步：给类添加方法

类不只能保存数据，也可以包含和数据相关的行为。

例如计算战力：

```csharp
public sealed class Player
{
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public int Gold { get; set; }

    public int CalculatePower()
    {
        return Level * 100 + Gold / 10;
    }
}
```

使用：

```csharp
Player player = new Player
{
    Name = "Alice",
    Level = 10,
    Gold = 500
};

int power = player.CalculatePower();

Console.WriteLine(power);
```

这样写的好处是：

- 计算战力的逻辑跟玩家放在一起。
- 调用时不需要到处传 `level` 和 `gold`。
- 后面增加装备、称号等字段时，更容易维护。

## 第 10 步：只读属性

有些属性不希望外部随便改。

例如玩家 ID 创建后不应该变化：

```csharp
public sealed class Player
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
}
```

使用：

```csharp
Player player = new Player();

Console.WriteLine(player.Id);
```

下面这样会报错：

```csharp
player.Id = Guid.NewGuid();
```

如果只希望构造函数和对象初始化器能设置，可以使用 `init`：

```csharp
public Guid Id { get; init; } = Guid.NewGuid();
```

使用：

```csharp
Player player = new Player
{
    Id = Guid.NewGuid(),
    Name = "Alice"
};
```

对象创建完成后，`Id` 就不能再改。

## 第 11 步：`class` 和 `struct` 的基础区别

C# 中：

- `class` 是引用类型。
- `struct` 是值类型。

简单理解：

- `class` 变量保存的是对象引用。
- `struct` 变量通常保存的是值本身。

`class` 示例：

```csharp
Player player1 = new Player { Name = "Alice" };
Player player2 = player1;

player2.Name = "Bob";

Console.WriteLine(player1.Name); // Bob
```

因为 `player1` 和 `player2` 指向同一个对象。

`struct` 示例：

```csharp
PlayerPoint point1 = new PlayerPoint { X = 10, Y = 20 };
PlayerPoint point2 = point1;

point2.X = 99;

Console.WriteLine(point1.X); // 10
```

定义：

```csharp
public struct PlayerPoint
{
    public int X { get; set; }
    public int Y { get; set; }
}
```

初期建议：

- 业务实体优先用 `class`。
- 小型、不可变、表示一个值的对象可以考虑 `struct`。
- 玩家、订单、用户、战绩这类对象通常用 `class`。
- 坐标、颜色、范围、尺寸这类可以考虑 `struct`。

## 第 12 步：创建 `Player.cs`

现在开始把 `Player` 类从 `Program.cs` 中拆出去。

推荐创建目录：

```text
Models/
  Player.cs
```

`Player.cs` 内容：

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
}
```

说明：

- 命名空间使用当前项目的默认根命名空间 `gameC_`。
- `Player` 放在 `Models` 下，所以命名空间写 `gameC_.Models`。
- `Level` 和 `Gold` 使用完整属性，避免非法值。
- `CalculatePower`、`GetStage`、`GetRegionName` 都是和玩家自身相关的方法。

## 第 13 步：在 `Program.cs` 中使用 `Player`

`Program.cs` 示例：

```csharp
using gameC_.Models;

Console.Write("请输入玩家名称：");
string? nameInput = Console.ReadLine();

Console.Write("请输入玩家等级：");
string? levelInput = Console.ReadLine();

Console.Write("请输入玩家金币：");
string? goldInput = Console.ReadLine();

Console.Write("请输入区服：");
string? regionInput = Console.ReadLine();

Player player = new Player
{
    Name = ReadStringOrDefault(nameInput, "未命名玩家"),
    Level = ReadIntOrDefault(levelInput, 1),
    Gold = ReadIntOrDefault(goldInput, 0),
    Region = ReadStringOrDefault(regionInput, "CN")
};

Console.WriteLine();
Console.WriteLine("玩家信息");
Console.WriteLine($"ID：{player.Id}");
Console.WriteLine($"名称：{player.Name}");
Console.WriteLine($"等级：{player.Level}");
Console.WriteLine($"金币：{player.Gold}");
Console.WriteLine($"区服：{player.GetRegionName()}");
Console.WriteLine($"阶段：{player.GetStage()}");
Console.WriteLine($"战力：{player.CalculatePower()}");
Console.WriteLine($"是否活跃：{player.IsActive}");

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

这版代码相比课程一的变化：

- 玩家数据被集中到了 `Player` 对象。
- 战力、阶段、区服名称由 `Player` 自己负责。
- `Program.cs` 更像流程组织者，不再承担全部业务逻辑。

## 第 14 步：本课完整练习

练习目标：把课程一的“玩家信息录入”升级为面向对象版本。

要求：

1. 创建 `Models` 文件夹。
2. 创建 `Player.cs`。
3. 在 `Player` 类中添加以下属性：
   - `Id`
   - `Name`
   - `Level`
   - `Region`
   - `Gold`
   - `IsActive`
4. 给 `Level` 添加校验，小于 1 时自动变成 1。
5. 给 `Gold` 添加校验，小于 0 时自动变成 0。
6. 添加 `CalculatePower()` 方法。
7. 添加 `GetStage()` 方法。
8. 添加 `GetRegionName()` 方法。
9. 在 `Program.cs` 中读取输入，创建 `Player` 对象。
10. 输出完整玩家信息。

验收标准：

- `Program.cs` 中不再散落大量玩家字段。
- 玩家相关逻辑主要在 `Player` 类中。
- 输入错误的等级和金币时，程序不会崩溃。
- 输入空名称时，使用默认名称。
- 程序可以通过 `dotnet run` 运行。

## 第 15 步：本课作业

### 作业 1：增加经验值

给 `Player` 增加：

```csharp
public int Experience { get; set; }
```

要求：

- 经验值不能小于 0。
- 输出玩家经验。

### 作业 2：升级方法

给 `Player` 增加方法：

```csharp
public void AddExperience(int amount)
```

规则：

- 如果 `amount <= 0`，不处理。
- 每 100 点经验升 1 级。
- 升级后扣除对应经验。

示例：

```text
当前等级：1
当前经验：0
增加经验：250
结果等级：3
剩余经验：50
```

### 作业 3：显示摘要

给 `Player` 增加方法：

```csharp
public string GetSummary()
```

返回类似：

```text
Alice Lv.10 国服 战力:1050
```

然后在 `Program.cs` 中输出：

```csharp
Console.WriteLine(player.GetSummary());
```

## 本课常见错误

### 1. 类名和文件名不一致

推荐：

```text
Player.cs
public sealed class Player
```

文件名和类名保持一致，查找和维护更舒服。

### 2. 命名空间写错

如果 `Player.cs` 是：

```csharp
namespace gameC_.Models;
```

那么 `Program.cs` 需要：

```csharp
using gameC_.Models;
```

### 3. 忘记初始化字符串

不推荐：

```csharp
public string Name { get; set; }
```

在开启可空引用类型时，这会有警告。

推荐：

```csharp
public string Name { get; set; } = string.Empty;
```

或者：

```csharp
public string Name { get; set; } = "未命名玩家";
```

### 4. 业务逻辑仍然全写在 `Program.cs`

不推荐：

```csharp
int power = player.Level * 100 + player.Gold / 10;
```

更推荐：

```csharp
int power = player.CalculatePower();
```

和玩家高度相关的逻辑，优先放进 `Player`。

### 5. 过度设计

刚开始不要急着上接口、继承、抽象类、设计模式。

本课重点是：

- 会定义类。
- 会创建对象。
- 会用属性保存状态。
- 会用方法表达行为。

## 本课复盘问题

学完后，尝试回答：

1. 为什么要把玩家信息放进 `Player` 类？
2. 字段和属性有什么区别？
3. 为什么业务代码更推荐属性？
4. 构造函数什么时候执行？
5. 自动属性和完整属性分别适合什么场景？
6. `get`、`set`、`init` 有什么区别？
7. `class` 和 `struct` 的基本区别是什么？
8. 为什么玩家更适合用 `class`？
9. 哪些逻辑应该放在 `Player` 类里？
10. 哪些逻辑不应该放在 `Player` 类里？

## 下一课预告

课程三建议学习：

- 集合基础。
- `List<Player>` 管理多个玩家。
- 新增、删除、查找玩家。
- 遍历玩家列表。
- 按等级、金币、战力排序。
