using System.Threading;

namespace K1vs.DotNetPipe.ReturningCancellable;

/// <summary>
/// Represents a handler step in a returning cancellable pipeline.
/// </summary>
/// <typeparam name="TInput">The type of input for this step.</typeparam>
/// <typeparam name="TResult">The type of result for this step.</typeparam>
public interface IHandlerStep<TInput, TResult> : IStep
{
    /// <summary>
    /// Handles the input for this step and produces a result.
    /// </summary>
    /// <param name="input">The input for this step.</param>
    /// <param name="ct">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> representing the asynchronous operation.</returns>
    ValueTask<TResult> Handle(TInput input, CancellationToken ct = default);
}


