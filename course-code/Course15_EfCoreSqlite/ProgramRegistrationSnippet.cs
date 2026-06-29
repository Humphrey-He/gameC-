// Course 15: EF Core + SQLite 注册示例。

using Course15_EfCoreSqlite.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration
    .GetConnectionString("PlayerDatabase")
    ?? throw new InvalidOperationException("Connection string 'PlayerDatabase' not found.");

builder.Services.AddDbContext<PlayerDbContext>(options =>
    options.UseSqlite(connectionString));
