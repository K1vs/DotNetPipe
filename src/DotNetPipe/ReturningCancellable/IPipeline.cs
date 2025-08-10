using System.Threading;

namespace K1vs.DotNetPipe.ReturningCancellable;

/// <summary>
/// Represents a DotNetPipe returning pipeline with cancellation support.
/// </summary>
/// <typeparam name="TInput">The type of the input data that the pipeline processes.</typeparam>
/// <typeparam name="TResult">The type of the result data that the pipeline produces.</typeparam>
public interface IPipeline<TInput, TResult>
{
    /// <summary>
    /// Gets the name of the pipeline.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Runs the pipeline with the specified input.
    /// </summary>
    /// <param name="input">The input data to process through the pipeline.</param>
    /// <param name="ct">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> representing the asynchronous operation.</returns>
    ValueTask<TResult> Run(TInput input, CancellationToken ct = default);
}


