using K1vs.DotNetPipe.AsyncCancellable.Steps;

namespace K1vs.DotNetPipe.AsyncCancellable;

/// <summary>
/// Represents a DotNetPipe pipeline.
/// </summary>
/// <typeparam name="TInput">The type of the input data that the pipeline processes.</typeparam>
public interface IPipeline<TInput>
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
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task Run(TInput input, CancellationToken ct = default);
}