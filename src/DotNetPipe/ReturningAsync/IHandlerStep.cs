namespace K1vs.DotNetPipe.ReturningAsync;

/// <summary>
/// Represents a handler step in a pipeline.
/// </summary>
/// <typeparam name="TInput">The type of input for this step.</typeparam>
/// <typeparam name="TResult">The type of result for this step.</typeparam>
public interface IHandlerStep<TInput, TResult> : IStep
{
    /// <summary>
    /// Handles the input for this step. This is typically the final step in a pipeline.
    /// </summary>
    /// <param name="input">The input for this step.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<TResult> Handle(TInput input);
}

