# C# 三年开发能力目标与学习路线

## 适用背景

你已有约 1 年 C# 基础经验，并且最近 2 年主要写 Go 和 C++。这意味着你不需要像零基础一样学习编程思维，重点应该放在：

- 重新熟悉 C# 语法、类型系统、标准库和 .NET 运行时习惯。
- 把 Go/C++ 中的工程经验迁移到 C# 的项目组织、测试、命名、异常处理和异步模型中。
- 用可运行的小项目验证知识，而不是只看语法。

本文档目标是帮助你达到“3 年 C# 开发者”应具备的能力水平，并给出一条可执行的复习路线。

## 三年 C# 开发者应达到的目标

### 1. 语言基础

应能熟练掌握并解释以下内容：

- 基本类型、值类型、引用类型、装箱与拆箱。
- `class`、`struct`、`record`、`interface`、`enum` 的使用场景。
- 访问修饰符：`public`、`private`、`protected`、`internal`、`protected internal`。
- 属性、字段、方法、构造函数、静态成员、扩展方法。
- 泛型、泛型约束、委托、事件、Lambda 表达式。
- 可空引用类型：`string?`、`nullable enable`、空值检查。
- 异常处理：`try`、`catch`、`finally`、自定义异常、异常边界。
- 模式匹配、表达式体成员、解构、元组、`using` 声明。

达到标准：

- 能看懂并维护中等复杂度的 C# 项目。
- 能写出类型清晰、空值处理明确、异常边界合理的业务代码。
- 能说明 C# 与 Go/C++ 在对象模型、内存管理、错误处理、泛型和并发模型上的差异。

### 2. .NET 基础与运行时理解

应掌握：

- .NET SDK、运行时、项目文件 `.csproj`、NuGet 包管理。
- `bin`、`obj`、Debug、Release、目标框架的含义。
- CLR、JIT、GC 的基本工作方式。
- 托管资源与非托管资源的区别。
- `IDisposable`、`using`、`await using` 的资源释放模式。
- 配置、日志、依赖注入的基本思想。

达到标准：

- 能独立创建、配置、运行和发布 C# 项目。
- 能排查常见构建错误、包依赖错误和运行时异常。
- 能识别内存泄漏风险、资源未释放风险和不必要的对象分配。

### 3. 数据结构与算法

应熟练使用：

- 数组：`T[]`
- 动态数组：`List<T>`
- 字典：`Dictionary<TKey, TValue>`
- 哈希集合：`HashSet<T>`
- 队列：`Queue<T>`
- 栈：`Stack<T>`
- 链表：`LinkedList<T>`
- 有序集合与映射：`SortedSet<T>`、`SortedDictionary<TKey, TValue>`
- 并发集合：`ConcurrentDictionary<TKey, TValue>`、`ConcurrentQueue<T>`

应理解：

- 时间复杂度和空间复杂度。
- 哈希冲突、相等比较器、`GetHashCode` 与 `Equals`。
- 值类型作为 key 时的注意事项。
- `IEnumerable<T>`、`ICollection<T>`、`IList<T>`、`IReadOnlyList<T>` 的区别。
- 何时返回接口，何时返回具体集合。

达到标准：

- 能根据场景选择合适集合，而不是默认全部使用 `List<T>`。
- 能写出常见算法：查找、排序、去重、分组、滑动窗口、递归、BFS、DFS。
- 能处理大集合性能问题，避免不必要的重复遍历和内存复制。

### 4. 常用函数与标准库

需要重点熟悉以下 API：

字符串：

- `string.IsNullOrEmpty`
- `string.IsNullOrWhiteSpace`
- `Substring`
- `Split`
- `Join`
- `Replace`
- `Contains`
- `StartsWith`
- `EndsWith`
- `Trim`
- `StringBuilder`

集合与 LINQ：

- `Where`
- `Select`
- `SelectMany`
- `Any`
- `All`
- `First` / `FirstOrDefault`
- `Single` / `SingleOrDefault`
- `OrderBy` / `ThenBy`
- `GroupBy`
- `ToList`
- `ToArray`
- `ToDictionary`
- `Distinct`
- `Aggregate`

日期与时间：

- `DateTime`
- `DateTimeOffset`
- `TimeSpan`
- `Stopwatch`

文件与路径：

- `File`
- `Directory`
- `Path`
- `Stream`
- `StreamReader`
- `StreamWriter`

异步与任务：

- `Task`
- `Task<T>`
- `async`
- `await`
- `Task.WhenAll`
- `Task.WhenAny`
- `CancellationToken`

序列化：

- `System.Text.Json`
- `JsonSerializer.Serialize`
- `JsonSerializer.Deserialize`

达到标准：

- 能不用频繁查文档完成常见字符串、集合、文件、JSON、异步操作。
- 能知道 LINQ 的延迟执行特性，避免重复枚举造成性能问题。
- 能在同步、异步、并发场景下选择合适 API。

### 5. 面向对象与设计能力

应掌握：

- 封装、继承、多态、组合。
- 接口与抽象类的选择。
- 依赖倒置、单一职责、开闭原则。
- DTO、Entity、Value Object、Service、Repository 的基本概念。
- 常见设计模式：工厂、策略、模板方法、观察者、适配器、装饰器。

达到标准：

- 能把业务逻辑拆成职责清晰的类。
- 能避免过度继承，优先使用组合。
- 能写出容易测试、容易替换依赖的代码。

### 6. 异步、并发与性能

应掌握：

- `async` / `await` 的执行模型。
- CPU 密集型与 IO 密集型任务的区别。
- `Task.Run` 的合理使用场景。
- `CancellationToken` 的传递与取消。
- `lock`、`Monitor`、`SemaphoreSlim`、并发集合。
- 死锁、线程安全、共享状态、不可变对象。
- 基础性能分析：分配、热点函数、重复查询、字符串拼接。

达到标准：

- 能写出可取消、可等待、异常可观察的异步代码。
- 能识别多线程共享状态风险。
- 能用简单压测或 Benchmark 判断优化是否有效。

### 7. 测试与调试

应掌握：

- 单元测试：xUnit、NUnit 或 MSTest。
- 断言、测试命名、测试数据组织。
- Mock 与 Stub 的基本使用。
- 断点调试、条件断点、观察变量、调用栈。
- 日志辅助定位问题。

达到标准：

- 能为核心业务逻辑写单元测试。
- 能用测试覆盖边界条件、异常分支和空值场景。
- 能通过调试工具定位大多数普通 bug。

### 8. 项目规范

一个成熟 C# 项目通常应具备：

- 清晰的解决方案结构：`src`、`tests`、`docs`。
- 明确的命名空间，与目录结构大体一致。
- 统一的代码风格，建议使用 `.editorconfig`。
- 统一的异常处理策略。
- 统一的日志策略。
- 统一的配置读取方式。
- 独立的测试项目。
- 必要的 README、开发说明和运行说明。

建议结构：

```text
ProjectName/
  src/
    ProjectName/
      Program.cs
      Services/
      Models/
      Options/
      Utilities/
  tests/
    ProjectName.Tests/
  docs/
    CSharp_Three_Year_Roadmap.md
  ProjectName.sln
  .editorconfig
  README.md
```

达到标准：

- 能让新成员快速理解项目入口、模块边界和运行方式。
- 能把业务代码、基础设施代码、测试代码分开。
- 能避免所有逻辑都堆在 `Program.cs` 或单个大类里。

### 9. 命名规范

推荐遵循 C# 常见命名约定：

| 类型 | 规范 | 示例 |
| --- | --- | --- |
| 类 | PascalCase，名词或名词短语 | `PlayerService` |
| 接口 | `I` + PascalCase | `IPlayerRepository` |
| 方法 | PascalCase，动词或动词短语 | `CalculateScore` |
| 属性 | PascalCase | `PlayerName` |
| 公共字段 | 尽量避免，必须使用时 PascalCase | `MaxHealth` |
| 私有字段 | `_camelCase` | `_currentHealth` |
| 局部变量 | camelCase | `totalScore` |
| 参数 | camelCase | `playerId` |
| 常量 | PascalCase | `DefaultTimeoutSeconds` |
| 异步方法 | 以 `Async` 结尾 | `LoadPlayerAsync` |
| 泛型参数 | 简短 `T` 或描述性名称 | `TItem` |
| 布尔变量 | 表达是或否 | `isActive`、`hasItems` |

注意事项：

- 不建议使用全大写常量名，例如 `MAX_COUNT`。
- 不建议使用匈牙利命名法，例如 `strName`、`iCount`。
- 类名用名词，方法名用动词。
- 方法名要表达意图，不要只写 `Do`、`Handle`、`Process` 这类过于泛的名字。
- 缩写保持可读，例如 `HttpClient`，不要写成 `HTTPClient`。

### 10. 三年水平的项目经验目标

建议至少完成以下项目：

1. 控制台工具项目
   - 目标：熟悉 C# 基础、文件 IO、JSON、命令行参数。
   - 示例：日志分析器、配置转换工具、批量文件整理工具。

2. 数据结构练习项目
   - 目标：熟悉集合、复杂度、泛型、测试。
   - 示例：实现 LRU 缓存、任务调度队列、排行榜系统。

3. Web API 项目
   - 目标：熟悉 ASP.NET Core、依赖注入、配置、日志、接口设计。
   - 示例：用户管理 API、库存 API、游戏战绩 API。

4. 持久化项目
   - 目标：熟悉数据库访问、Repository、事务、迁移。
   - 示例：使用 EF Core 或 Dapper 实现 CRUD 和分页查询。

5. 综合项目
   - 目标：模拟真实工作项目。
   - 示例：游戏玩家数据服务、任务系统、排行榜服务、后台管理 API。

完成标准：

- 有 README。
- 有清晰目录结构。
- 有单元测试。
- 有异常处理和日志。
- 有可运行的示例。
- 代码风格统一。

## 学习路线

### 第 1 阶段：恢复 C# 手感，1 到 2 周

目标：

- 重新熟悉语法和项目结构。
- 能独立创建、运行、调试控制台项目。

学习内容：

- 基础语法、类型系统、访问修饰符。
- `class`、`struct`、`record`、`interface`。
- 属性、构造函数、静态成员、扩展方法。
- 空值处理和异常处理。
- `.csproj`、NuGet、Debug / Release。

练习：

- 写一个控制台版“学生成绩管理”或“玩家属性管理”。
- 支持新增、删除、查询、排序、保存到 JSON 文件。

验收标准：

- 代码能运行。
- 至少拆出 `Models`、`Services`、`Utilities`。
- 不把全部逻辑写在 `Program.cs`。

### 第 2 阶段：集合、数据结构与 LINQ，2 到 3 周

目标：

- 熟练使用 C# 常用集合。
- 能根据业务场景选择合适数据结构。

学习内容：

- `List<T>`、`Dictionary<TKey, TValue>`、`HashSet<T>`。
- `Queue<T>`、`Stack<T>`、`LinkedList<T>`。
- `IEnumerable<T>`、`IReadOnlyList<T>`、`ICollection<T>`。
- LINQ 常用函数和延迟执行。
- `Equals`、`GetHashCode`、比较器。

练习：

- 实现一个游戏排行榜。
- 支持玩家分数更新、排名查询、Top N、按区服分组。
- 用单元测试覆盖排序、去重、边界值。

验收标准：

- 能解释每种集合的使用原因。
- LINQ 使用清晰，不重复枚举大集合。
- 有至少 10 个单元测试。

### 第 3 阶段：常用 API 与工程习惯，2 周

目标：

- 熟悉日常开发高频 API。
- 形成稳定的编码风格。

学习内容：

- 字符串、日期、文件、路径、JSON。
- `StringBuilder`、`Stopwatch`。
- 日志、配置、异常边界。
- `.editorconfig` 和命名规范。

练习：

- 写一个日志分析工具。
- 输入日志文件，输出错误统计、耗时统计、按日期分组结果。
- 支持 JSON 配置。

验收标准：

- 有清晰 README。
- 有错误输入处理。
- 有配置示例。
- 有基本测试。

### 第 4 阶段：异步、并发与资源管理，2 到 3 周

目标：

- 掌握 C# 异步模型。
- 能写出安全、可取消、可维护的异步代码。

学习内容：

- `Task`、`Task<T>`、`async`、`await`。
- `Task.WhenAll`、`Task.WhenAny`。
- `CancellationToken`。
- `lock`、`SemaphoreSlim`、并发集合。
- `IDisposable` 与资源释放。

练习：

- 写一个批量文件处理器。
- 并发读取多个文件，统计内容，支持取消任务。
- 限制最大并发数。

验收标准：

- 支持取消。
- 异常不会被吞掉。
- 资源能正确释放。
- 并发数量可配置。

### 第 5 阶段：测试、重构与设计模式，2 到 3 周

目标：

- 能写可测试代码。
- 能把混乱代码重构成职责清晰的结构。

学习内容：

- xUnit / NUnit / MSTest 任选一个。
- Mock、Stub、测试数据。
- 策略模式、工厂模式、观察者模式。
- 依赖倒置和接口隔离。

练习：

- 重构前面写过的排行榜或日志工具。
- 抽离接口、服务、配置、存储。
- 为核心逻辑补测试。

验收标准：

- 核心逻辑测试覆盖主要分支。
- 类职责清晰。
- 没有明显的超大方法和超大类。

### 第 6 阶段：Web API 或业务项目，4 到 6 周

目标：

- 完成一个接近真实工作的 C# 项目。
- 熟悉 ASP.NET Core 或你目标方向的主框架。

推荐项目：

- 游戏玩家数据 API。
- 玩家注册、登录可先简化。
- 玩家资料查询与修改。
- 战绩提交。
- 排行榜查询。
- 操作日志。
- 配置和环境区分。

学习内容：

- ASP.NET Core 路由、Controller 或 Minimal API。
- 依赖注入。
- 配置系统。
- 日志。
- 参数校验。
- 全局异常处理。
- EF Core 或 Dapper。
- 分层结构。

验收标准：

- 能本地启动。
- 有接口文档或 README 示例。
- 有测试。
- 有统一异常返回。
- 有基础日志。
- 目录结构清晰。

### 第 7 阶段：性能、源码阅读与面试准备，长期持续

目标：

- 从“会写”提升到“写得稳、能解释、能排查问题”。

学习内容：

- GC 与内存分配。
- LINQ 性能陷阱。
- 大对象、字符串拼接、装箱。
- BenchmarkDotNet 基础。
- 常见面试题：集合、异步、委托、事件、GC、异常、泛型、接口。

练习：

- 对前面项目做一次性能检查。
- 对热点逻辑写 Benchmark。
- 总结 20 个 C# 面试问题，并用自己的话回答。

验收标准：

- 能解释性能问题来自哪里。
- 能用数据证明优化有效。
- 能清晰讲出项目设计取舍。

## 每周学习节奏建议

如果工作日可投入 1 到 2 小时，周末可投入半天，可以按下面节奏执行：

| 时间 | 内容 |
| --- | --- |
| 周一 | 学习语法或 API，整理笔记 |
| 周二 | 写小练习 |
| 周三 | 补充测试或重构 |
| 周四 | 阅读官方文档或优秀项目代码 |
| 周五 | 总结本周问题 |
| 周六 | 完成一个小功能或阶段项目 |
| 周日 | 复盘、整理 README、准备下周任务 |

## 推荐复习顺序

优先级从高到低：

1. C# 类型系统、空值、异常。
2. 集合、LINQ、数据结构。
3. 常用 API：字符串、文件、JSON、日期。
4. 异步、并发、资源释放。
5. 测试与调试。
6. 项目结构、命名规范、工程规范。
7. ASP.NET Core 或目标业务框架。
8. 性能分析和源码阅读。

## 阶段性检查清单

### 一个月后

- 能熟练写控制台项目。
- 能使用常见集合和 LINQ。
- 能读写 JSON 文件。
- 能写基本单元测试。
- 能遵守基础命名规范。

### 三个月后

- 能独立完成一个中小型 C# 项目。
- 能使用异步和取消机制。
- 能做基本项目分层。
- 能写 README 和测试。
- 能解释常见集合、泛型、委托、事件、异常、GC 基础。

### 六个月后

- 能维护真实 C# 项目。
- 能设计清晰的服务类和接口。
- 能排查常见性能问题。
- 能写稳定的 Web API 或工具项目。
- 能在 Code Review 中发现命名、结构、异常处理和测试问题。

### 达到三年水平时

- 能独立负责一个模块。
- 能设计可维护的类结构和项目结构。
- 能在业务需求不完整时拆解任务并落地实现。
- 能评估集合、并发、数据库访问和异常处理的技术取舍。
- 能指导新人修正常见 C# 代码问题。

## 建议输出物

学习过程中建议保留以下材料：

- `notes/`：每周学习笔记。
- `practice/`：小练习代码。
- `src/`：阶段项目代码。
- `tests/`：单元测试。
- `docs/`：项目说明、接口说明、复盘总结。

每完成一个阶段，写一次简短复盘：

```text
本阶段学了什么：
遇到的问题：
解决方式：
还不熟的点：
下一阶段计划：
```

## 参考资料

- Microsoft Learn: C# coding conventions
  - https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions
- Microsoft Learn: C# identifier naming rules and conventions
  - https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/identifier-names
- Microsoft Learn: Collections
  - https://learn.microsoft.com/dotnet/csharp/programming-guide/concepts/collections
- Microsoft Learn: Framework design guidelines, naming guidelines
  - https://learn.microsoft.com/dotnet/standard/design-guidelines/naming-guidelines
