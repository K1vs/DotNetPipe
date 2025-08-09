namespace K1vs.DotNetPipe.Returning;

/// <summary>
/// Represents an if-else step in a pipeline.
/// </summary>
/// <typeparam name="TInput">The type of input for this step.</typeparam>
/// <typeparam name="TResult">The type of result for this step.</typeparam>
/// <typeparam name="TIfInput">The type of input for the if branch.</typeparam>
/// <typeparam name="TIfResult">The type of result for the if branch.</typeparam>
/// <typeparam name="TElseInput">The type of input for the else branch.</typeparam>
/// <typeparam name="TElseResult">The type of result for the else branch.</typeparam>
/// <typeparam name="TNextInput">The type of input for the next step.</typeparam>
/// <typeparam name="TNextResult">The type of result for the next step.</typeparam>
public interface IIfElseStep<TInput, TResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextInput, TNextResult> : IStep
{
    /// <summary>
    /// Handles the input for this step and conditionally executes either the if branch or the else branch.
    /// </summary>
    /// <param name="input">The input for this step.</param>
    /// <param name="ifNext">The handler for the if branch.</param>
    /// <param name="elseNext">The handler for the else branch.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask<TResult> Handle(TInput input, Handler<TIfInput, TIfResult> ifNext, Handler<TElseInput, TElseResult> elseNext);

    /// <summary>
    /// Builds a pipeline that executes the true branch when the condition is true.
    /// The pipeline will continue to the next step after executing the true branch.
    /// </summary>
    /// <param name="space">The space in which the pipeline is built.</param>
    /// <returns>An <see cref="OpenPipeline{TIfInput, TIfResult, TNextInput, TNextResult}"/> representing the pipeline that executes the true branch.</returns>
    OpenPipeline<TIfInput, TIfResult, TNextInput, TNextResult> BuildTruePipeline(Space space);

    /// <summary>
    /// Builds a pipeline that executes the false branch when the condition is false.
    /// The pipeline will continue to the next step after executing the false branch.
    /// </summary>
    /// <param name="space">The space in which the pipeline is built.</param>
    /// <returns>An <see cref="OpenPipeline{TElseInput, TElseResult, TNextInput, TNextResult}"/> representing the pipeline that executes the false branch.</returns>
    OpenPipeline<TElseInput, TElseResult, TNextInput, TNextResult> BuildFalsePipeline(Space space);
}
