# gameC# / C# Learning Project / C# 学習プロジェクト

## 中文

这是一个面向 C# 复习与实战训练的学习项目。项目从 C# 基础开始，逐步覆盖集合、LINQ、JSON、异常处理、单元测试、异步、依赖注入、ASP.NET Core Minimal API、Options 配置、EF Core、SQLite 和 Repository 模式。

### 内容

- `docs/`：课程文档与学习路线。
- `course-code/`：每节课程对应的配套代码。
- `Models/`：当前根项目中的基础示例模型。
- `Program.cs`：当前根项目的最小运行示例。

### 课程文档

课程文档位于 `docs/`：

- `CSharp_Three_Year_Roadmap.md`：三年 C# 能力目标与学习路线。
- `CSharp_Practical_Project_Plan.md`：C# 实战项目规划。
- `CSharp_Basic_Course_01.md` 到 `CSharp_Basic_Course_16.md`：基础学习专刊课程。

### 配套代码

配套代码位于 `course-code/`，每课一个文件夹：

```text
course-code/
  Course01_Basics/
  Course02_PlayerClass/
  Course03_ListPlayerManager/
  ...
  Course16_RepositoryEfCore/
```

这些代码是学习示例，不参与根项目编译。根项目已在 `gameC#.csproj` 中排除 `course-code/**/*.cs`。

### 运行

```powershell
dotnet build
dotnet run
```

### 学习建议

建议按课程顺序学习，每完成一课后：

- 阅读对应文档。
- 查看对应 `course-code`。
- 手写一遍核心代码。
- 完成本课作业。
- 用 Git 提交阶段成果。

---

## English

This is a C# learning and practice project. It starts from C# fundamentals and gradually covers collections, LINQ, JSON persistence, exception handling, unit testing, async programming, dependency injection, ASP.NET Core Minimal APIs, Options configuration, EF Core, SQLite, and the Repository pattern.

### Contents

- `docs/`: Course documents and learning roadmaps.
- `course-code/`: Companion code samples for each course.
- `Models/`: Basic sample model used by the root project.
- `Program.cs`: Minimal runnable example for the root project.

### Course Documents

Documents are stored in `docs/`:

- `CSharp_Three_Year_Roadmap.md`: Three-year C# skill goals and learning roadmap.
- `CSharp_Practical_Project_Plan.md`: Practical C# project plan.
- `CSharp_Basic_Course_01.md` to `CSharp_Basic_Course_16.md`: Step-by-step C# basic course series.

### Companion Code

Companion code lives in `course-code/`, one folder per course:

```text
course-code/
  Course01_Basics/
  Course02_PlayerClass/
  Course03_ListPlayerManager/
  ...
  Course16_RepositoryEfCore/
```

These files are learning samples and are excluded from the root project build through `gameC#.csproj`.

### Run

```powershell
dotnet build
dotnet run
```

### Suggested Workflow

For each course:

- Read the matching document.
- Review the matching `course-code` folder.
- Recreate the key code by hand.
- Finish the exercises.
- Commit your progress with Git.

---

## 日本語

このリポジトリは、C# の復習と実践練習のための学習プロジェクトです。C# の基礎から始めて、コレクション、LINQ、JSON 永続化、例外処理、単体テスト、非同期処理、依存性注入、ASP.NET Core Minimal API、Options 設定、EF Core、SQLite、Repository パターンまで段階的に学習します。

### 内容

- `docs/`：講義ドキュメントと学習ロードマップ。
- `course-code/`：各講義に対応するサンプルコード。
- `Models/`：ルートプロジェクトで使う基本サンプルモデル。
- `Program.cs`：ルートプロジェクトの最小実行サンプル。

### 講義ドキュメント

ドキュメントは `docs/` にあります。

- `CSharp_Three_Year_Roadmap.md`：3 年レベルの C# スキル目標と学習ロードマップ。
- `CSharp_Practical_Project_Plan.md`：C# 実践プロジェクト計画。
- `CSharp_Basic_Course_01.md` から `CSharp_Basic_Course_16.md`：C# 基礎講座シリーズ。

### サンプルコード

サンプルコードは `course-code/` にあり、講義ごとにフォルダを分けています。

```text
course-code/
  Course01_Basics/
  Course02_PlayerClass/
  Course03_ListPlayerManager/
  ...
  Course16_RepositoryEfCore/
```

これらは学習用サンプルです。ルートプロジェクトのビルドに影響しないよう、`gameC#.csproj` で `course-code/**/*.cs` を除外しています。

### 実行方法

```powershell
dotnet build
dotnet run
```

### 学習の進め方

各講義ごとに以下の流れをおすすめします。

- 対応するドキュメントを読む。
- 対応する `course-code` を確認する。
- 重要なコードを自分で書き直す。
- 課題を完成させる。
- Git で学習成果をコミットする。
