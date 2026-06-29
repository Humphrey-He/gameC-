# C# 基础学习专刊配套代码

这个目录按课程编号存放配套代码：

```text
course-code/
  Course01_Basics/
  Course02_PlayerClass/
  ...
  Course16_RepositoryEfCore/
```

说明：

- 每个课程文件夹对应 `docs/CSharp_Basic_Course_XX.md`。
- 代码以“学习示例”为主，重点展示本课核心知识点。
- 根项目 `gameC#.csproj` 已排除 `course-code/**/*.cs`，所以示例代码不会影响当前项目编译。
- 如果要运行某一课，可以把对应文件复制到练习项目中，或后续把该课程目录扩展成独立 `.csproj`。

## 阶段一已项目化课程

以下课程已具备独立 `.csproj` 和 README，可独立运行或测试：

| 课程 | 类型 | 验证命令 |
| --- | --- | --- |
| `Course01_Basics` | Console | `dotnet run --project course-code/Course01_Basics/Course01_Basics.csproj` |
| `Course02_PlayerClass` | Console | `dotnet run --project course-code/Course02_PlayerClass/Course02_PlayerClass.csproj` |
| `Course03_ListPlayerManager` | Console | `dotnet run --project course-code/Course03_ListPlayerManager/Course03_ListPlayerManager.csproj` |
| `Course06_JsonStorage` | Console | `dotnet run --project course-code/Course06_JsonStorage/Course06_JsonStorage.csproj` |
| `Course09_XunitTests` | xUnit | `dotnet test course-code/Course09_XunitTests/Course09_XunitTests.csproj` |
| `Course12_MinimalApi` | Minimal API | `dotnet run --project course-code/Course12_MinimalApi/Course12_MinimalApi.csproj` |
| `Course15_EfCoreSqlite` | Console + EF Core | `dotnet run --project course-code/Course15_EfCoreSqlite/Course15_EfCoreSqlite.csproj` |
| `Course16_RepositoryEfCore` | Console + EF Core | `dotnet run --project course-code/Course16_RepositoryEfCore/Course16_RepositoryEfCore.csproj` |

阶段一目标是让关键课程“可运行、可验证”，不是把全部课程一次性升级为完整项目。后续可以继续把其他课程目录补成独立项目。
