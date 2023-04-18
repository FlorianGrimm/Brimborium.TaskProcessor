namespace Brimborium.TaskProcessor;
public class BufferProcessor<Value> 
    : IProcessor<Value>
    , IProcessorSource<Value>
{
    private IProcessorSink<Value> _Sink;
    private Channel<Value> _Channel;
    private ChannelWriter<Value> _ChannelWriter;

    public BufferProcessor()
    {
        this._Sink = EmptyProcessorSink<Value>.Instance;
        this._Channel = Channel.CreateUnbounded<Value>();
        this._ChannelWriter = this._Channel.Writer;
    }

    public BufferProcessor(IProcessorSink<Value> sink)
    : this()
    {
        this._Sink = sink;
    }


    public ValueTask<IProcessorSubscription> SubscribeAsync(IProcessorSink<Value> sink, CancellationToken cancellationToken = default)
    {
        var (nextSync, processorSubscription)
            = ((IProcessorSinkBound<Value>)this._Sink).Add(sink);
        this._Sink = nextSync;
        return ValueTask.FromResult<IProcessorSubscription>(processorSubscription);
    }

    public async Task Enqueue(Value value)
    {
        await this._ChannelWriter.WriteAsync(value);
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (await this._Channel.Reader.WaitToReadAsync(cancellationToken))
        {
            while (this._Channel.Reader.TryRead(out var value))
            {
                await this.ExecuteNextAsync(value, cancellationToken);
            }
        }
        await this._Sink.OnCompleteAsync(cancellationToken);
        this._Channel.Writer.Complete();
    }

    protected virtual async Task ExecuteNextAsync(Value value, CancellationToken cancellationToken)
    {
        try
        {
            await this._Sink.OnNextAsync(value, cancellationToken);
        }
        catch (Exception error)
        {
            await this._Sink.OnErrorAsync(error, cancellationToken);
        }
    }
}