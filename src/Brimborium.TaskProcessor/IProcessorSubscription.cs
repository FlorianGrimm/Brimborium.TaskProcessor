namespace Brimborium.TaskProcessor;

public interface IProcessorSubscription {    
    Task UnsubscribeAsync(CancellationToken cancellationToken);
    Task CompleteAsync(CancellationToken cancellationToken);
}

public class SingleProcessorSubscription : IProcessorSubscription {
    public SingleProcessorSubscription(){        
    }

    public Task CompleteAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task UnsubscribeAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

public class ProcessorSubscription : IProcessorSubscription {
    private readonly TaskCompletionSource<bool> _TaskCompletionSource;
    private IProcessorSubscription? _Child;
    private List<IProcessorSubscription>? _ListChild;

    public ProcessorSubscription() {
        this._TaskCompletionSource = new TaskCompletionSource<bool>();
    }

    public IProcessorSubscription Add(IProcessorSubscription child) {
        lock (this) {
            if (this._Child is null && this._ListChild is null) {
                this._Child = child;
                return this;
            }
            if (this._ListChild is null) {
                this._ListChild = new List<IProcessorSubscription>();
                this._ListChild.Add(this._Child!);
                this._Child = null;
            }
            this._ListChild.Add(child);
        }
        return this;
    }
    public Task UnsubscribeAsync(CancellationToken cancellationToken) {
        if (this._TaskCompletionSource.TrySetResult(true)) {
           return innerUnsubscribeAsync(cancellationToken); 
        } else {
            return this._TaskCompletionSource.Task;
        }

        async Task innerUnsubscribeAsync(CancellationToken cancellationToken){
            if (this._ListChild is not null) {
                foreach (var child in this._ListChild) {
                    await child.UnsubscribeAsync(cancellationToken);
                }
                this._ListChild = null;
            }
            if (this._Child is not null) {
                await this._Child.UnsubscribeAsync(cancellationToken);
            }
        }
    }

    public Task CompleteAsync(CancellationToken cancellationToken) {
        if (this._TaskCompletionSource.TrySetResult(true)) {
            return innerCompleteAsync(cancellationToken);
        } else {
            return this._TaskCompletionSource.Task;
        }

        async Task innerCompleteAsync(CancellationToken cancellationToken){
            if (this._ListChild is not null) {
                foreach (var child in this._ListChild) {
                    await child.CompleteAsync(cancellationToken).ConfigureAwait(false);
                }
            }
            if (this._Child is not null) {
                await this._Child.CompleteAsync(cancellationToken).ConfigureAwait(false);
            }
            if (this._ListChild is not null) {
                foreach (var child in this._ListChild) {
                    await child.UnsubscribeAsync(cancellationToken).ConfigureAwait(false);
                }
                this._ListChild = null;
            }
            if (this._Child is not null) {
                await this._Child.UnsubscribeAsync(cancellationToken).ConfigureAwait(false);
                this._Child = null;
            }
        }
    }
}
