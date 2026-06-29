using Microsoft.Data.Sqlite;

namespace GamePlayerSystem.Api.Persistence;

internal static class SqliteDatabasePath
{
    public static string NormalizeConnectionString(string connectionString, string baseDirectory)
    {
        SqliteConnectionStringBuilder builder = new(connectionString);

        if (string.IsNullOrWhiteSpace(builder.DataSource) ||
            builder.DataSource.Equals(":memory:", StringComparison.OrdinalIgnoreCase) ||
            Path.IsPathRooted(builder.DataSource))
        {
            return connectionString;
        }

        string databasePath = Path.GetFullPath(Path.Combine(baseDirectory, builder.DataSource));
        string? databaseDirectory = Path.GetDirectoryName(databasePath);

        if (!string.IsNullOrWhiteSpace(databaseDirectory))
        {
            Directory.CreateDirectory(databaseDirectory);
        }

        builder.DataSource = databasePath;

        return builder.ToString();
    }
}
