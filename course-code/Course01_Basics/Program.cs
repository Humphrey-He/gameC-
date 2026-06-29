// Course 01: 基础语法、控制台输入输出、变量、方法。

Console.Write("请输入玩家名称：");
string playerName = ReadStringOrDefault(Console.ReadLine(), "未命名玩家");

Console.Write("请输入玩家等级：");
int level = ReadIntOrDefault(Console.ReadLine(), 1);

Console.Write("请输入区服：");
string region = ReadStringOrDefault(Console.ReadLine(), "CN");

string stage = GetPlayerStage(level);

Console.WriteLine();
Console.WriteLine("玩家信息");
Console.WriteLine($"名称：{playerName}");
Console.WriteLine($"等级：{level}");
Console.WriteLine($"区服：{region}");
Console.WriteLine($"阶段：{stage}");

static string ReadStringOrDefault(string? input, string defaultValue)
{
    // Console.ReadLine 可能返回 null，所以统一在这里兜底。
    return string.IsNullOrWhiteSpace(input)
        ? defaultValue
        : input;
}

static int ReadIntOrDefault(string? input, int defaultValue)
{
    // TryParse 比 Parse 更适合处理用户输入。
    return int.TryParse(input, out int value)
        ? value
        : defaultValue;
}

static string GetPlayerStage(int level)
{
    if (level >= 30)
    {
        return "高级玩家";
    }

    if (level >= 10)
    {
        return "中级玩家";
    }

    return "新手玩家";
}
