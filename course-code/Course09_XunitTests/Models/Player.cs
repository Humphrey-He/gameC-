namespace Course09_XunitTests.Models;

public sealed class Player
{
    public string Region { get; set; } = "CN";
    public int Level { get; set; } = 1;
    public int Gold { get; set; }

    public int CalculatePower()
    {
        return Level * 100 + Gold / 10;
    }

    public string GetRegionName()
    {
        return Region switch
        {
            "CN" => "国服",
            "NA" => "北美服",
            "EU" => "欧洲服",
            _ => "未知区服"
        };
    }
}
