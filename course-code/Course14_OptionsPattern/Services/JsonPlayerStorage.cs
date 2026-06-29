using System.Text.Json;
using Course14_OptionsPattern.Models;
using Course14_OptionsPattern.Options;
using Microsoft.Extensions.Options;

namespace Course14_OptionsPattern.Services;

public sealed class JsonPlayerStorage
{
    private readonly string _filePath;

    public JsonPlayerStorage(IOptions<PlayerStorageOptions> options)
    {
        // Options 模式把配置绑定成强类型对象。
        _filePath = options.Value.FilePath;
    }

    public async Task SaveAsync(IEnumerable<Player> players)
    {
        string? directory = Path.GetDirectoryName(_filePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(players, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await File.WriteAllTextAsync(_filePath, json);
    }
}
