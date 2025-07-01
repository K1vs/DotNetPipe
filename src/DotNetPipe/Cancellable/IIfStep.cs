namespace K1vs.DotNetPipe.Cancellable;

/// <summary>
/// Represents an if step in a pipeline.
/// </summary>
/// <typeparam name="TInput">The type of input for this step.</typeparam>
/// <typeparam name="TIfInput">The type of input for the if branch.</typeparam>
/// <typeparam name="TNextInput">The type of input for the next step in the pipeline.</typeparam>
public interface IIfStep<TInput, TIfInput, TNextInput> : IStep
{
    /// <summary>
    /// Handles the input for this step and conditionally executes the if branch or continues to the next step.
    /// </summary>
    /// <param name="input">The input for this step.</param>
    /// <param name="ifNext">The handler for the if branch.</param>
    /// <param name="next">The next step in the pipeline to which the input will be passed.</param>
    /// <param name="ct">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask Handle(TInput input, Handler<TIfInput> ifNext, Handler<TNextInput> next, CancellationToken ct = default);

    /// <summary>
    /// Builds a pipeline that executes the if branch when the condition is true.
    /// The pipeline will continue to the next step after executing the if branch.
    /// </summary>
    /// <param name="space">The space in which the pipeline is built.</param>
    /// <returns>An <see cref="OpenPipeline{TIfInput, TNextInput}"/> representing the pipeline that executes the if branch.</returns>
    OpenPipeline<TIfInput, TNextInput> BuildTruePipeline(Space space);
}
