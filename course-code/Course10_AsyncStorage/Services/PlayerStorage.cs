using System.Text.Json;
using Course10_AsyncStorage.Models;

namespace Course10_AsyncStorage.Services;

public sealed class PlayerStorage
{
    private readonly string _filePath;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public PlayerStorage(string filePath)
    {
        _filePath = filePath;
    }

    public async Task SaveAsync(
        IEnumerable<Player> players,
        CancellationToken cancellationToken = default)
    {
        string? directory = Path.GetDirectoryName(_filePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(players, _jsonOptions);
        await File.WriteAllTextAsync(_filePath, json, cancellationToken);
    }

    public async Task<List<Player>> LoadAsync(
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            return new List<Player>();
        }

        string json = await File.ReadAllTextAsync(_filePath, cancellationToken);

        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<Player>();
        }

        return JsonSerializer.Deserialize<List<Player>>(json, _jsonOptions)
            ?? new List<Player>();
    }
}
