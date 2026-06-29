// Course 14: Program.cs 中的 Options 注册示例。

using Course14_OptionsPattern.Options;
using Course14_OptionsPattern.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<PlayerStorageOptions>()
    .Bind(builder.Configuration.GetSection(PlayerStorageOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.FilePath),
        "PlayerStorage:FilePath 不能为空")
    .ValidateOnStart();

builder.Services.AddSingleton<JsonPlayerStorage>();
