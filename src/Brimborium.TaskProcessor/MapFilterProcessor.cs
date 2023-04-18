namespace Brimborium.TaskProcessor;

public class MapFilterProcessor<TValue, TResult> 
//: IProcessor<Value> 
{
    private readonly Func<TValue, ValueTask<TResult>> _MapAsync;
    private readonly Func<TResult, ValueTask<bool>>? _FilterAsync;

    public MapFilterProcessor(
        Func<TValue, ValueTask<TResult>> mapAsync,
        Func<TResult, ValueTask<bool>>? filterAsync
    )
    {
        this._MapAsync = mapAsync;
        this._FilterAsync = filterAsync;
    }


}
