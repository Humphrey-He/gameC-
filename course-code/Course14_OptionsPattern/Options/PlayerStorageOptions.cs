namespace Course14_OptionsPattern.Options;

public sealed class PlayerStorageOptions
{
    public const string SectionName = "PlayerStorage";

    public string FilePath { get; set; } = string.Empty;
}
