namespace Brimborium.TaskProcessor;

public class EmptyProcessorSink<Value>
    : IProcessorSink<Value>
    , IProcessorSinkBound<Value>
{
    private static IProcessorSink<Value>? _Instance;
    public static IProcessorSink<Value> Instance => _Instance ??= new EmptyProcessorSink<Value>();

    // IProcessorSink<Value>

    public Task OnNextAsync(Value value, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task OnCompleteAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task OnErrorAsync(Exception error, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    // IProcessorSinkBound<Value>

    public (IProcessorSink<Value> nextSync, IProcessorSubscription processorSubscription) Add(IProcessorSink<Value> sink)
    {
        var nextSync = new SingleProcessorSink<Value>(sink);
        return (nextSync, nextSync);
    }
}

public class SingleProcessorSink<Value> 
    : IProcessorSink<Value>
    , IProcessorSubscription
    , IProcessorSinkBound<Value>
{
    private readonly IProcessorSink<Value> _Sink;

    // 1 = unsubscribed
    // 2 = completed    
    private int _State;

    public SingleProcessorSink(IProcessorSink<Value> sink)
    {
        this._Sink = sink;
    }

    // IProcessorSink<Value>

    public Task OnNextAsync(Value value, CancellationToken cancellationToken)
    {
        return this._Sink.OnNextAsync(value, cancellationToken);
    }

    public Task OnErrorAsync(Exception error, CancellationToken cancellationToken)
    {
        return this._Sink.OnErrorAsync(error, cancellationToken);
    }

    public Task OnCompleteAsync(CancellationToken cancellationToken)
    {
        this._State |= 2;
        return this._Sink.OnCompleteAsync(cancellationToken);
    }

    // IProcessorSubscription

    public Task UnsubscribeAsync(CancellationToken cancellationToken)
    {
        this._State |= 1;
        return Task.CompletedTask;
    }

    public Task CompleteAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }


    // IProcessorSinkBound<Value>
    
    public (IProcessorSink<Value> nextSync, IProcessorSubscription processorSubscription) Add(IProcessorSink<Value> sink)
    {
        var nextSync = new SingleProcessorSink<Value>(sink);
        return (nextSync, nextSync);
    }

}

public class MultipleProcessorSinkItem<Value>
  : IProcessorSink<Value>
  , IProcessorSubscription

{
    private readonly MultipleProcessorSink<Value> _Owner;
    private readonly IProcessorSink<Value> _Sink;

    // 1 = unsubscribed
    // 2 = completed    
    private int _State;

    public MultipleProcessorSinkItem(
        MultipleProcessorSink<Value> owner,
        IProcessorSink<Value> sink
    ){
        this._Owner = owner;
        this._Sink = sink;
    }

    public Task OnNextAsync(Value value, CancellationToken cancellationToken)
    {
        return this._Sink.OnNextAsync(value, cancellationToken);
    }

    public Task OnCompleteAsync(CancellationToken cancellationToken)
    {
        return this._Sink.OnCompleteAsync(cancellationToken);
    }

    public Task OnErrorAsync(Exception error, CancellationToken cancellationToken)
    {
        return this._Sink.OnErrorAsync(error, cancellationToken);
    }

    public Task CompleteAsync(CancellationToken cancellationToken)
    {
        this._State |= 2;
        return Task.CompletedTask;
    }

    public Task UnsubscribeAsync(CancellationToken cancellationToken)
    {
        this._State |= 1;
        this._Owner._Sinks.Remove(this);
        return Task.CompletedTask;
    }
}
public class MultipleProcessorSink<Value> 
    : IProcessorSink<Value>
    , IProcessorSinkBound<Value>
{
  
    internal readonly List<MultipleProcessorSinkItem<Value>> _Sinks;
    public MultipleProcessorSink(){
        this._Sinks = new List<MultipleProcessorSinkItem<Value>>();
    }

    public (IProcessorSink<Value> nextSync, IProcessorSubscription processorSubscription) Add(IProcessorSink<Value> sink)
    {
        var processorSubscription = new MultipleProcessorSinkItem<Value>(this, sink);
        this._Sinks.Add(processorSubscription);
        return (this, processorSubscription);        
    }

    public async Task OnNextAsync(Value value, CancellationToken cancellationToken)
    {
        foreach(var sink in this._Sinks) {
          await   sink.OnNextAsync(value, cancellationToken);
        }
    }

    public async Task OnCompleteAsync(CancellationToken cancellationToken)
    {
        foreach(var sink in this._Sinks) {
            await sink.OnCompleteAsync(cancellationToken);
        }
    }

    public async Task OnErrorAsync(Exception error, CancellationToken cancellationToken)
    {
        foreach(var sink in this._Sinks) {
            await sink.OnErrorAsync(error, cancellationToken);
        }
    }
}