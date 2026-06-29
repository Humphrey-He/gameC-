using GamePlayerSystem.Core;

namespace GamePlayerSystem.Tests;

public sealed class ProjectInfoTests
{
    [Fact]
    public void Name_ShouldBeGamePlayerSystem()
    {
        Assert.Equal("Game Player System", ProjectInfo.Name);
    }

    [Fact]
    public void Status_ShouldBeRunning()
    {
        Assert.Equal("running", ProjectInfo.Status);
    }
}
