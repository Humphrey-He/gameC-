using System.Text.Json;
using Course06_JsonStorage.Models;

namespace Course06_JsonStorage.Services;

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

    public void Save(IEnumerable<Player> players)
    {
        string? directory = Path.GetDirectoryName(_filePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(players, _jsonOptions);
        File.WriteAllText(_filePath, json);
    }

    public List<Player> Load()
    {
        if (!File.Exists(_filePath))
        {
            return new List<Player>();
        }

        string json = File.ReadAllText(_filePath);

        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<Player>();
        }

        return JsonSerializer.Deserialize<List<Player>>(json, _jsonOptions)
            ?? new List<Player>();
    }
}
