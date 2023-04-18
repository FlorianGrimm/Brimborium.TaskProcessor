namespace Brimborium.TaskProcessor.Tests;

public class MapFilterProcessorTests
{
    [Fact]
    public void MapFilterProcessor_Test1()
    {
        var p1 = new MapFilterProcessor<int>(
            mapAsync: (value) => new ValueTask<int>(value + 1),
            filterAsync: default
        );

        var p2 = new MapFilterProcessor<int>(
            mapAsync: (value) => new ValueTask<int>(value * 10),
            filterAsync: default
        );

    }
}