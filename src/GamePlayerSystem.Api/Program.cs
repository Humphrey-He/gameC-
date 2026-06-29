using GamePlayerSystem.Core;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    name = ProjectInfo.Name,
    status = ProjectInfo.Status
}));

app.Run();
