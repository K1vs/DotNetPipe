namespace K1vs.DotNetPipe.Sync;

/// <summary>
/// Represents a linear step in a pipeline.
/// </summary>
/// <typeparam name="TInput">The type of input for this step.</typeparam>
/// <typeparam name="TNextInput">The type of input for the next step in the pipeline.</typeparam>
public interface ILinearStep<TInput, TNextInput> : IStep
{
    /// <summary>
    /// Handles the input for this step and passes it to the next step in the pipeline.
    /// </summary>
    /// <param name="input">The input for this step.</param>
    /// <param name="next">The next step in the pipeline to which the input will be passed.</param>
    void Handle(TInput input, Handler<TNextInput> next);
}