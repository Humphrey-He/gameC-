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
