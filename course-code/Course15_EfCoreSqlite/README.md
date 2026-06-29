# Course 15: EF Core SQLite

对应文档：`docs/CSharp_Basic_Course_15.md`

本课程示例演示：

- EF Core
- SQLite
- `DbContext`
- `DbSet<Player>`
- `EnsureCreatedAsync`
- 数据库版玩家保存和加载

## 运行

```powershell
dotnet run --project course-code/Course15_EfCoreSqlite/Course15_EfCoreSqlite.csproj
```

运行后会生成：

```text
data/course15-players.db
```

说明：这是阶段一的独立运行示例，使用 `EnsureCreatedAsync`。正式 migrations 会在二期阶段四落地。
