using System.Threading;

namespace K1vs.DotNetPipe.ReturningCancellable;

/// <summary>
/// Represents a linear step in a returning cancellable pipeline.
/// </summary>
/// <typeparam name="TInput">The type of input for this step.</typeparam>
/// <typeparam name="TResult">The type of result produced by this step.</typeparam>
/// <typeparam name="TNextInput">The type of input for the next step in the pipeline.</typeparam>
/// <typeparam name="TNextResult">The type of result produced by the next step in the pipeline.</typeparam>
public interface ILinearStep<TInput, TResult, TNextInput, TNextResult> : IStep
{
    /// <summary>
    /// Handles the input for this step and passes it to the next step.
    /// </summary>
    /// <param name="input">The input for this step.</param>
    /// <param name="next">The next step handler in the pipeline.</param>
    /// <param name="ct">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> representing the asynchronous operation.</returns>
    ValueTask<TResult> Handle(TInput input, Handler<TNextInput, TNextResult> next, CancellationToken ct = default);
}


