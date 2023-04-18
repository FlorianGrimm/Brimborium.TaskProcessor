namespace Brimborium.TaskProcessor;
public interface IProcessor {
}

public interface IProcessor<Value> {
//     ValueTask<IProcessorSubscription> SubscribeAsync(IProcessorSink<Value> sink, IProcessorSubscription? subscription, CancellationToken cancellationToken = default);
}

public interface IProcessorSource<Value> {
    ValueTask<IProcessorSubscription> SubscribeAsync(IProcessorSink<Value> sink, CancellationToken cancellationToken = default);
}

public interface IProcessorSink<Value> {
    Task OnNextAsync(Value value,CancellationToken cancellationToken);
    Task OnErrorAsync(Exception error,CancellationToken cancellationToken);
    Task OnCompleteAsync(CancellationToken cancellationToken);
}

public interface IProcessorSinkBound<Value>:IProcessorSink<Value> {
    (IProcessorSink<Value> nextSync, IProcessorSubscription processorSubscription) Add(IProcessorSink<Value> sink);
}

// public interface IProcessorPublisherSink<Value>:IProcessorSink<Value>{
//     ValueTask OnSubscribeAsync(IProcessorSubscription subscription);
// }
