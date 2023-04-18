namespace Brimborium.TaskProcessor.Tests;

public class BufferProcessorTests
{
    [Fact]
    public async Task BufferProcessor_Test1()
    {
        var p1 = new BufferProcessor<int>();

        var p2 = new BufferProcessor<int>();

        
        await p2.Enqueue(1);

    }
}
