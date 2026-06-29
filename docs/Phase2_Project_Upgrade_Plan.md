# 二期规划：从课程仓库升级为完整实战项目

## 当前定位

当前仓库已经完成了 C# 学习路线、课程文档、配套示例代码和基础 README。

现阶段更接近：

```text
C# 学习资料仓库 + 配套代码示例
```

二期目标是升级为：

```text
C# 学习资料仓库 + 可运行实战项目 + 测试 + 数据库 + CI + 双语言模块
```

## 当前主要缺口

1. `course-code/` 目前是代码示例，不是每课独立可运行项目，没有各自的 `.csproj`。
2. Web API、EF Core、Repository 目前主要是课程示例和片段，不是完整集成后的实际服务。
3. 没有真正的 `GamePlayerSystem.Api` 项目。
4. 没有真实 xUnit 测试项目。
5. 没有 EF Core migrations。
6. 没有 SQLite 数据库实际落地。
7. 没有 CI/CD，例如 GitHub Actions。
8. 还没有双语言模块，比如 C# + Rust / C++。
9. README 还没有课程目录跳转链接，可以继续优化。

## 二期总体目标

二期完成后，仓库应具备：

- 一个可运行的 ASP.NET Core Web API 项目。
- 一个独立的 Core 类库项目，承载 Models、Dtos、Services、Repositories。
- 一个真实 xUnit 测试项目。
- 一个 SQLite 数据库落地方案。
- EF Core migrations。
- GitHub Actions 自动构建和测试。
- 每课代码可独立运行或有明确运行方式。
- 一个双语言模块原型。
- 更完整的三语 README 和课程目录导航。

## 推荐目标结构

```text
gameC-/
  src/
    GamePlayerSystem.Core/
      Common/
      Dtos/
      Exceptions/
      Models/
      Options/
      Persistence/
      Repositories/
      Services/
    GamePlayerSystem.Api/
      Endpoints/
      Responses/
      Program.cs
      appsettings.json
  tests/
    GamePlayerSystem.Tests/
  course-code/
    Course01_Basics/
    Course02_PlayerClass/
    ...
    Course16_RepositoryEfCore/
  native/
    ranking-rust/
  docs/
  .github/
    workflows/
      dotnet.yml
  README.md
  GamePlayerSystem.sln
```

## 阶段一：课程代码项目化

### 目标

让 `course-code/` 不只是代码片段，而是更方便运行和验证的课程示例。

### 要求

- 为关键课程创建独立 `.csproj`。
- 每个课程目录保留课程编号和主题。
- 每个课程目录增加 `README.md`，说明运行方式。
- 简单课程可以是 Console 项目。
- Web API 课程可以是 Minimal API 项目。
- 测试课程可以是 xUnit 项目。

### 推荐处理方式

不是所有 16 课都必须立即完整项目化。优先处理：

- `Course01_Basics`
- `Course02_PlayerClass`
- `Course03_ListPlayerManager`
- `Course06_JsonStorage`
- `Course09_XunitTests`
- `Course12_MinimalApi`
- `Course15_EfCoreSqlite`
- `Course16_RepositoryEfCore`

### 验收标准

- 至少 8 个课程目录可以独立运行或测试。
- 每个可运行课程都有 `.csproj`。
- 每个可运行课程都有 README。
- 根项目构建不受 `course-code/` 影响。

## 阶段二：创建真实解决方案结构

### 目标

建立正式项目结构，不再只依赖根目录最小项目。

### 要求

创建解决方案：

```powershell
dotnet new sln -n GamePlayerSystem
```

创建 Core 类库：

```powershell
dotnet new classlib -o src/GamePlayerSystem.Core
```

创建 API 项目：

```powershell
dotnet new web -o src/GamePlayerSystem.Api
```

创建测试项目：

```powershell
dotnet new xunit -o tests/GamePlayerSystem.Tests
```

添加项目到解决方案：

```powershell
dotnet sln add src/GamePlayerSystem.Core
dotnet sln add src/GamePlayerSystem.Api
dotnet sln add tests/GamePlayerSystem.Tests
```

添加引用：

```powershell
dotnet add src/GamePlayerSystem.Api reference src/GamePlayerSystem.Core
dotnet add tests/GamePlayerSystem.Tests reference src/GamePlayerSystem.Core
```

### 验收标准

- 存在 `GamePlayerSystem.sln`。
- `src/GamePlayerSystem.Core` 可构建。
- `src/GamePlayerSystem.Api` 可启动。
- `tests/GamePlayerSystem.Tests` 可运行。
- 根目录旧项目可以保留，但不再是主项目。

## 阶段三：整合 Web API

### 目标

把课程 12-16 的示例整合成真实可运行 API 服务。

### 要求

API 至少提供：

```text
GET    /
GET    /players
GET    /players/{id}
POST   /players
PUT    /players/{id}
DELETE /players/{id}
POST   /players/{id}/disable
GET    /players/by-region/{region}
GET    /players/search?keyword=xxx
GET    /rankings/top?count=10
GET    /stats/regions
```

项目结构：

```text
src/GamePlayerSystem.Api/
  Endpoints/
    PlayerEndpoints.cs
    RankingEndpoints.cs
    StatsEndpoints.cs
  Responses/
    ErrorResponse.cs
  Program.cs
```

### 验收标准

- API 可以本地启动。
- OpenAPI 文档可访问。
- 主要接口可通过 HTTP 文件或 curl 调用。
- 错误响应格式统一。
- 使用 `ILogger` 记录关键操作。

## 阶段四：EF Core 与 SQLite 落地

### 目标

让玩家数据真正存入 SQLite，而不是只停留在文档和示例。

### 要求

Core 项目中包含：

```text
Persistence/
  PlayerDbContext.cs
Repositories/
  IPlayerRepository.cs
  EfPlayerRepository.cs
```

API 项目中配置：

```json
{
  "ConnectionStrings": {
    "PlayerDatabase": "Data Source=data/players.db"
  }
}
```

安装 EF Core 包：

```powershell
dotnet add src/GamePlayerSystem.Api package Microsoft.EntityFrameworkCore.Sqlite
dotnet add src/GamePlayerSystem.Api package Microsoft.EntityFrameworkCore.Design
```

创建迁移：

```powershell
dotnet ef migrations add InitialCreate --project src/GamePlayerSystem.Api
```

更新数据库：

```powershell
dotnet ef database update --project src/GamePlayerSystem.Api
```

### 验收标准

- 存在 `Migrations/`。
- 能生成 `data/players.db`。
- 玩家新增、查询、删除、禁用都操作数据库。
- API 重启后数据仍然存在。
- Repository 查询支持分页、区服、名称搜索。

## 阶段五：真实测试项目

### 目标

建立可运行的 xUnit 测试项目，覆盖核心业务。

### 要求

测试项目：

```text
tests/GamePlayerSystem.Tests/
```

至少覆盖：

- `Player` 基础规则。
- `Result` / `Result<T>`。
- `PlayerApplication` 新增、查询、删除、禁用。
- `EfPlayerRepository` 基础查询。
- 分页查询。
- 名称搜索。
- 区服查询。

可选：

- 使用 SQLite in-memory。
- 使用临时 SQLite 文件。
- 使用 WebApplicationFactory 做 API 集成测试。

### 验收标准

- `dotnet test` 可运行。
- 至少 20 个测试。
- 核心成功路径和失败路径都有覆盖。
- 测试不依赖固定本地数据文件。

## 阶段六：CI/CD

### 目标

使用 GitHub Actions 自动检查构建和测试。

### 要求

创建：

```text
.github/workflows/dotnet.yml
```

流程至少包含：

- Checkout
- Setup .NET
- Restore
- Build
- Test

示例：

```yaml
name: dotnet

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - run: dotnet restore GamePlayerSystem.sln
      - run: dotnet build GamePlayerSystem.sln --no-restore
      - run: dotnet test GamePlayerSystem.sln --no-build
```

### 验收标准

- GitHub Actions 能成功运行。
- PR 和 push 都会触发。
- 构建失败或测试失败会阻止合并。

## 阶段七：双语言模块原型

### 目标

实现一个最小 C# + Rust 或 C# + C++ 双语言模块。

### 推荐优先级

第一选择：

```text
C# + Rust
```

原因：

- 适合高性能计算模块。
- FFI 边界清晰。
- 跨平台能力较好。

备选：

```text
C# + C++
```

适合复用已有 C++ 代码或游戏底层能力。

### 模块建议

实现排行榜计算模块：

```text
native/ranking-rust/
```

功能：

- 输入玩家分数数组。
- 输出 Top N。
- C# 通过 P/Invoke 调用。

### C# 侧要求

定义接口：

```csharp
public interface IRankingEngine
{
    IReadOnlyList<RankingPlayerDto> GetTopPlayers(
        IReadOnlyList<Player> players,
        int count);
}
```

实现：

```text
ManagedRankingEngine
NativeRustRankingEngine
```

### 验收标准

- 有纯 C# 实现。
- 有 Rust 或 C++ native 实现。
- 两种实现输出一致。
- 有测试验证一致性。
- native 模块有 README 说明构建方式。

## 阶段八：README 与文档优化

### 目标

让仓库首页更像成熟项目。

### 要求

README 增加：

- 课程目录跳转链接。
- 二期项目结构说明。
- API 项目运行方式。
- 测试运行方式。
- EF Core migration 命令。
- GitHub Actions 状态徽章。
- 双语言模块说明。

三语 README 保留：

- 中文。
- English。
- 日本語。

建议增加目录：

```markdown
## Table of Contents
```

或三语对应：

```markdown
## 目录 / Table of Contents / 目次
```

### 验收标准

- 新用户打开 README 能知道如何学习、运行、测试项目。
- 所有关键文档都有入口链接。
- 课程 01-16 可以从 README 直接跳转。

## 建议实施顺序

推荐顺序：

1. 创建 `src/`、`tests/`、`.sln`。
2. 整合 Core 类库。
3. 整合 Web API。
4. 落地 EF Core SQLite 和 migrations。
5. 补真实 xUnit 测试。
6. 添加 GitHub Actions。
7. 优化 README。
8. 再做双语言模块。
9. 最后回头把 `course-code/` 独立项目化。

原因：

- 先有真实主项目，后续测试、CI、双语言都有落点。
- `course-code/` 项目化很有价值，但不应该阻塞主实战项目落地。

## 二期完成定义

二期完成时，应满足：

- `dotnet build GamePlayerSystem.sln` 成功。
- `dotnet test GamePlayerSystem.sln` 成功。
- API 可启动。
- SQLite 数据库可生成。
- migrations 可执行。
- GitHub Actions 通过。
- README 有课程跳转和运行说明。
- 至少一个双语言模块原型可运行。

## 风险与注意事项

- 不要一次性重构全部课程代码，容易失控。
- 不要让 API 直接依赖 `DbContext`，优先走 Application + Repository。
- 不要把 SQLite 数据库文件提交到仓库。
- 不要把敏感配置放入 `appsettings.json`。
- 不要在 CI 中依赖本地路径。
- 双语言模块要保持边界小，不要一开始就做复杂 FFI。
