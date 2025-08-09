namespace K1vs.DotNetPipe.Returning;

/// <summary>
/// Represents an if step in a pipeline.
/// </summary>
/// <typeparam name="TInput">The type of input for this step.</typeparam>
/// <typeparam name="TResult">The type of result for this step.</typeparam>
/// <typeparam name="TIfInput">The type of input for the if branch.</typeparam>
/// <typeparam name="TIfResult">The type of result for the if branch.</typeparam>
/// <typeparam name="TNextInput">The type of input for the next step in the pipeline.</typeparam>
/// <typeparam name="TNextResult">The type of result for the next step in the pipeline.</typeparam>
public interface IIfStep<TInput, TResult, TIfInput, TIfResult, TNextInput, TNextResult> : IStep
{
    /// <summary>
    /// Handles the input for this step and conditionally executes the if branch or continues to the next step.
    /// </summary>
    /// <param name="input">The input for this step.</param>
    /// <param name="ifNext">The handler for the if branch.</param>
    /// <param name="next">The next step in the pipeline to which the input will be passed.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask<TResult> Handle(TInput input, Handler<TIfInput, TIfResult> ifNext, Handler<TNextInput, TNextResult> next);

    /// <summary>
    /// Builds a pipeline that executes the if branch when the condition is true.
    /// The pipeline will continue to the next step after executing the if branch.
    /// </summary>
    /// <param name="space">The space in which the pipeline is built.</param>
    /// <returns>An <see cref="OpenPipeline{TIfInput, TIfResult, TNextInput, TNextResult}"/> representing the pipeline that executes the if branch.</returns>
    OpenPipeline<TIfInput, TIfResult, TNextInput, TNextResult> BuildTruePipeline(Space space);
}
