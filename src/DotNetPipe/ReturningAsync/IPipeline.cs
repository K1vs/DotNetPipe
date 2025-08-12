namespace K1vs.DotNetPipe.ReturningAsync;

/// <summary>
/// Represents a DotNetPipe pipeline.
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
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation.</returns>
    public Task<TResult> Run(TInput input);
}
