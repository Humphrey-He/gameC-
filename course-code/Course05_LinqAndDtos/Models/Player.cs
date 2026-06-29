namespace Course05_LinqAndDtos.Models;

public sealed class Player
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = "未命名玩家";
    public int Level { get; set; } = 1;
    public string Region { get; set; } = "CN";
    public int Gold { get; set; }
    public bool IsActive { get; set; } = true;

    public int CalculatePower()
    {
        return Level * 100 + Gold / 10;
    }

    public string GetRegionName()
    {
        return GetRegionName(Region);
    }

    public static string GetRegionName(string region)
    {
        return region switch
        {
            "CN" => "国服",
            "NA" => "北美服",
            "EU" => "欧洲服",
            _ => "未知区服"
        };
    }
}
