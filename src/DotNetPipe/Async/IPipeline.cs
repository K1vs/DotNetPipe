namespace K1vs.DotNetPipe.Async;

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
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task Run(TInput input);
}