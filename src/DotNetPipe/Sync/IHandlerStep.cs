namespace K1vs.DotNetPipe.Sync;

/// <summary>
/// Represents a handler step in a pipeline.
/// </summary>
/// <typeparam name="TInput">The type of input for this step.</typeparam>
public interface IHandlerStep<TInput> : IStep
{
    /// <summary>
    /// Handles the input for this step. This is typically the final step in a pipeline.
    /// </summary>
    /// <param name="input">The input for this step.</param>
    void Handle(TInput input);
}
