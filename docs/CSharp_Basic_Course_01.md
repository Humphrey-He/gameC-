# C# 基础学习专刊：课程一

## 课程主题

从零恢复 C# 手感：认识项目结构、入口程序、变量、类型、输入输出和方法。

你已经写过 Go 和 C++，所以这份课程不会把重点放在“什么是变量”这种通用编程概念上，而是重点讲：

- C# 项目是怎么组织的。
- C# 代码从哪里开始执行。
- C# 的类型、变量、方法和基础语法怎么写。
- C# 和 Go / C++ 在基础写法上的差异。
- 每一步怎么练习。

## 本课目标

完成本课后，你应该能做到：

- 看懂一个最小 C# 控制台项目。
- 理解 `.csproj`、`Program.cs`、`bin`、`obj` 的作用。
- 正确使用 `Console.WriteLine` 和 `Console.ReadLine`。
- 声明常用变量和基础类型。
- 写简单条件判断和循环。
- 写一个简单方法。
- 完成一个“玩家信息录入”小练习。

## 第 1 步：认识当前项目

你当前项目大致是这样：

```text
gameC#/
  gameC#.csproj
  Program.cs
  bin/
  obj/
  docs/
```

### `gameC#.csproj`

`.csproj` 是 C# 项目文件，负责描述：

- 这是一个什么类型的项目。
- 使用哪个 .NET 版本。
- 是否开启可空引用类型。
- 是否开启隐式 using。
- 引用了哪些 NuGet 包。

当前项目文件类似：

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <RootNamespace>gameC_</RootNamespace>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>
```

重点理解：

| 配置 | 含义 |
| --- | --- |
| `OutputType` | `Exe` 表示这是一个可执行程序 |
| `TargetFramework` | 目标 .NET 版本 |
| `RootNamespace` | 默认命名空间 |
| `ImplicitUsings` | 自动引入常用命名空间 |
| `Nullable` | 开启可空引用类型检查 |

### `Program.cs`

`Program.cs` 是程序入口。控制台程序运行时，会从这里开始执行。

最基础的 C# 程序：

```csharp
Console.WriteLine("Hello, World!");
```

注意：当前你的 `Program.cs` 里如果是：

```csharp
elConsole.WriteLine("Hello, World!");
```

这是错误的，应该改成：

```csharp
Console.WriteLine("Hello, World!");
```

### `bin` 和 `obj`

这两个目录是构建生成目录：

- `bin`：编译后的可执行文件。
- `obj`：中间构建文件。

平时不用手动修改它们。

## 第 2 步：运行第一个程序

在项目根目录执行：

```powershell
dotnet run
```

如果程序正确，会看到：

```text
Hello, World!
```

如果报错，先检查 `Program.cs` 是否写成了：

```csharp
Console.WriteLine("Hello, World!");
```

## 第 3 步：理解顶级语句

现代 C# 支持顶级语句，所以你可以直接写：

```csharp
Console.WriteLine("Hello, World!");
```

不需要像旧版本一样写完整类结构：

```csharp
using System;

namespace GameDemo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
        }
    }
}
```

两种写法都能用。学习初期建议先用顶级语句，等后面讲项目规范时再逐步切换到完整结构。

## 第 4 步：控制台输出

### 输出一行

```csharp
Console.WriteLine("玩家系统启动");
```

输出结果：

```text
玩家系统启动
```

### 输出但不换行

```csharp
Console.Write("请输入玩家名称：");
```

`WriteLine` 会换行，`Write` 不会换行。

### 字符串插值

C# 推荐使用 `$"..."` 做字符串插值：

```csharp
string playerName = "Alice";
int level = 10;

Console.WriteLine($"玩家：{playerName}，等级：{level}");
```

类似 Go 的 `fmt.Printf`，但写法更直接。

## 第 5 步：控制台输入

使用 `Console.ReadLine()` 读取用户输入：

```csharp
Console.Write("请输入玩家名称：");
string? playerName = Console.ReadLine();

Console.WriteLine($"你好，{playerName}");
```

注意这里是：

```csharp
string?
```

因为 `Console.ReadLine()` 可能返回 `null`。你的项目开启了：

```xml
<Nullable>enable</Nullable>
```

所以 C# 会提醒你注意空值风险。

更稳一点的写法：

```csharp
Console.Write("请输入玩家名称：");
string? input = Console.ReadLine();

string playerName = string.IsNullOrWhiteSpace(input)
    ? "未命名玩家"
    : input;

Console.WriteLine($"你好，{playerName}");
```

## 第 6 步：常用基础类型

C# 常见基础类型：

| 类型 | 说明 | 示例 |
| --- | --- | --- |
| `int` | 整数 | `100` |
| `long` | 长整数 | `10000000000L` |
| `float` | 单精度浮点数 | `1.5f` |
| `double` | 双精度浮点数 | `1.5` |
| `decimal` | 高精度小数，常用于金额 | `9.99m` |
| `bool` | 布尔值 | `true` / `false` |
| `char` | 单个字符 | `'A'` |
| `string` | 字符串 | `"hello"` |
| `DateTime` | 日期时间 | `DateTime.Now` |
| `Guid` | 全局唯一 ID | `Guid.NewGuid()` |

示例：

```csharp
Guid playerId = Guid.NewGuid();
string playerName = "Alice";
int level = 1;
long gold = 1000;
double winRate = 0.56;
bool isActive = true;
DateTime createdAt = DateTime.Now;

Console.WriteLine($"ID：{playerId}");
Console.WriteLine($"名称：{playerName}");
Console.WriteLine($"等级：{level}");
Console.WriteLine($"金币：{gold}");
Console.WriteLine($"胜率：{winRate}");
Console.WriteLine($"是否活跃：{isActive}");
Console.WriteLine($"创建时间：{createdAt}");
```

## 第 7 步：`var` 的使用

C# 可以使用 `var` 让编译器自动推断类型：

```csharp
var playerName = "Alice"; // string
var level = 10;           // int
var isActive = true;      // bool
```

但是 `var` 不是动态类型，编译后类型是固定的。

下面这样是错误的：

```csharp
var level = 10;
level = "ten";
```

建议：

- 右侧类型非常明显时，可以用 `var`。
- 右侧类型不明显时，写清楚类型。

推荐：

```csharp
var player = new Player();
Dictionary<string, int> scores = new();
```

不推荐：

```csharp
var result = GetData();
```

除非 `GetData()` 的返回值非常清楚。

## 第 8 步：数字转换

`Console.ReadLine()` 返回的是字符串。如果要读取数字，需要转换。

不推荐直接这样写：

```csharp
int level = int.Parse(Console.ReadLine());
```

因为用户输入非法内容会直接抛异常。

推荐：

```csharp
Console.Write("请输入玩家等级：");
string? input = Console.ReadLine();

bool success = int.TryParse(input, out int level);

if (!success)
{
    level = 1;
}

Console.WriteLine($"玩家等级：{level}");
```

`TryParse` 是 C# 中非常常见的安全转换方式。

## 第 9 步：条件判断

```csharp
int level = 15;

if (level >= 30)
{
    Console.WriteLine("高级玩家");
}
else if (level >= 10)
{
    Console.WriteLine("中级玩家");
}
else
{
    Console.WriteLine("新手玩家");
}
```

C# 的 `if` 和 C++ / Go 很像，但条件必须是 `bool`。

下面这种 C++ 风格在 C# 中不允许：

```csharp
int value = 1;

if (value)
{
    Console.WriteLine("不允许这样写");
}
```

必须写成：

```csharp
if (value != 0)
{
    Console.WriteLine("允许这样写");
}
```

## 第 10 步：`switch` 判断

传统写法：

```csharp
string region = "CN";

switch (region)
{
    case "CN":
        Console.WriteLine("国服");
        break;
    case "NA":
        Console.WriteLine("北美服");
        break;
    case "EU":
        Console.WriteLine("欧洲服");
        break;
    default:
        Console.WriteLine("未知区服");
        break;
}
```

表达式写法：

```csharp
string region = "CN";

string regionName = region switch
{
    "CN" => "国服",
    "NA" => "北美服",
    "EU" => "欧洲服",
    _ => "未知区服"
};

Console.WriteLine(regionName);
```

表达式写法更适合“根据输入得到一个结果”的场景。

## 第 11 步：循环

### `for`

```csharp
for (int i = 0; i < 5; i++)
{
    Console.WriteLine($"第 {i + 1} 次循环");
}
```

### `while`

```csharp
int count = 0;

while (count < 5)
{
    Console.WriteLine(count);
    count++;
}
```

### `foreach`

遍历集合时最常用：

```csharp
string[] players = ["Alice", "Bob", "Cindy"];

foreach (string player in players)
{
    Console.WriteLine(player);
}
```

## 第 12 步：数组和 `List<T>`

### 数组

数组长度固定：

```csharp
string[] players = new string[3];

players[0] = "Alice";
players[1] = "Bob";
players[2] = "Cindy";
```

简写：

```csharp
string[] players = ["Alice", "Bob", "Cindy"];
```

### `List<T>`

`List<T>` 是动态数组，更常用于业务代码：

```csharp
List<string> players = new();

players.Add("Alice");
players.Add("Bob");
players.Add("Cindy");

foreach (string player in players)
{
    Console.WriteLine(player);
}
```

常用方法：

```csharp
players.Add("David");
players.Remove("Bob");
bool hasAlice = players.Contains("Alice");
int count = players.Count;
```

## 第 13 步：方法

方法用于把重复逻辑封装起来。

```csharp
static int CalculatePower(int level, int attack)
{
    return level * 10 + attack;
}

int power = CalculatePower(10, 25);

Console.WriteLine($"战力：{power}");
```

在顶级语句中，本地方法通常写在文件底部。

完整示例：

```csharp
Console.Write("请输入等级：");
string? levelInput = Console.ReadLine();

Console.Write("请输入攻击力：");
string? attackInput = Console.ReadLine();

int level = ReadIntOrDefault(levelInput, 1);
int attack = ReadIntOrDefault(attackInput, 10);
int power = CalculatePower(level, attack);

Console.WriteLine($"战力：{power}");

static int ReadIntOrDefault(string? input, int defaultValue)
{
    return int.TryParse(input, out int value)
        ? value
        : defaultValue;
}

static int CalculatePower(int level, int attack)
{
    return level * 10 + attack;
}
```

## 第 14 步：本课完整练习

练习目标：写一个“玩家信息录入”控制台程序。

功能要求：

1. 提示用户输入玩家名称。
2. 提示用户输入玩家等级。
3. 提示用户输入区服。
4. 如果名称为空，默认使用 `未命名玩家`。
5. 如果等级输入错误，默认等级为 `1`。
6. 根据等级输出玩家阶段：
   - 1 到 9：新手玩家
   - 10 到 29：中级玩家
   - 30 及以上：高级玩家
7. 最后输出完整玩家信息。

参考代码：

```csharp
Console.Write("请输入玩家名称：");
string? nameInput = Console.ReadLine();

Console.Write("请输入玩家等级：");
string? levelInput = Console.ReadLine();

Console.Write("请输入区服：");
string? regionInput = Console.ReadLine();

string playerName = ReadStringOrDefault(nameInput, "未命名玩家");
int level = ReadIntOrDefault(levelInput, 1);
string region = ReadStringOrDefault(regionInput, "CN");
string stage = GetPlayerStage(level);

Console.WriteLine();
Console.WriteLine("玩家信息");
Console.WriteLine($"名称：{playerName}");
Console.WriteLine($"等级：{level}");
Console.WriteLine($"区服：{region}");
Console.WriteLine($"阶段：{stage}");

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

static string GetPlayerStage(int level)
{
    if (level >= 30)
    {
        return "高级玩家";
    }

    if (level >= 10)
    {
        return "中级玩家";
    }

    return "新手玩家";
}
```

## 第 15 步：本课作业

### 作业 1：补充金币输入

在玩家信息中增加金币字段：

- 提示用户输入金币。
- 如果输入错误，默认为 `0`。
- 输出玩家金币。

### 作业 2：增加战力计算

新增一个方法：

```csharp
static int CalculatePower(int level, int gold)
```

规则：

```text
战力 = 等级 * 100 + 金币 / 10
```

最后输出：

```text
战力：xxxx
```

### 作业 3：增加区服名称转换

输入区服代码：

```text
CN
NA
EU
```

输出中文区服名：

```text
CN -> 国服
NA -> 北美服
EU -> 欧洲服
其他 -> 未知区服
```

建议使用 `switch` 表达式。

## 本课常见错误

### 1. 拼错 `Console`

错误：

```csharp
elConsole.WriteLine("Hello");
```

正确：

```csharp
Console.WriteLine("Hello");
```

### 2. 忘记处理 `ReadLine` 的空值

不够稳：

```csharp
string name = Console.ReadLine();
```

推荐：

```csharp
string? input = Console.ReadLine();
string name = string.IsNullOrWhiteSpace(input) ? "默认名称" : input;
```

### 3. 直接 `Parse`

风险写法：

```csharp
int level = int.Parse(Console.ReadLine());
```

推荐：

```csharp
int level = int.TryParse(Console.ReadLine(), out int value)
    ? value
    : 1;
```

### 4. 把所有逻辑都写成一大坨

初期可以先写通，但写通后要开始拆方法：

- 输入字符串默认值：`ReadStringOrDefault`
- 输入数字默认值：`ReadIntOrDefault`
- 获取玩家阶段：`GetPlayerStage`
- 计算战力：`CalculatePower`

## 本课复盘问题

学完后，尝试回答：

1. `.csproj` 是干什么的？
2. `Program.cs` 在项目中是什么角色？
3. `Console.Write` 和 `Console.WriteLine` 有什么区别？
4. 为什么 `Console.ReadLine()` 返回 `string?`？
5. `int.Parse` 和 `int.TryParse` 有什么区别？
6. `if` 和 `switch` 分别适合什么场景？
7. 数组和 `List<T>` 有什么区别？
8. 为什么要把重复逻辑拆成方法？

## 下一课预告

课程二建议学习：

- 类和对象。
- 字段、属性、构造函数。
- `class` 与 `struct` 的区别。
- 创建 `Player` 类。
- 把本课的玩家信息从散落变量升级成对象。
