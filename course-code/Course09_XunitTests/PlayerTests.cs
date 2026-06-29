using Course09_XunitTests.Models;
using Xunit;

namespace Course09_XunitTests;

public sealed class PlayerTests
{
    [Fact]
    public void CalculatePower_WithLevelAndGold_ReturnsExpectedPower()
    {
        // Arrange
        Player player = new Player
        {
            Level = 10,
            Gold = 500
        };

        // Act
        int power = player.CalculatePower();

        // Assert
        Assert.Equal(1050, power);
    }

    [Theory]
    [InlineData("CN", "国服")]
    [InlineData("NA", "北美服")]
    [InlineData("EU", "欧洲服")]
    [InlineData("UNKNOWN", "未知区服")]
    public void GetRegionName_WithRegionCode_ReturnsRegionName(
        string region,
        string expected)
    {
        Player player = new Player
        {
            Region = region
        };

        Assert.Equal(expected, player.GetRegionName());
    }
}
