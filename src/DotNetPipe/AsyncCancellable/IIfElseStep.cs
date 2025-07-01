namespace K1vs.DotNetPipe.AsyncCancellable;

/// <summary>
/// Represents an if-else step in a pipeline.
/// </summary>
/// <typeparam name="TInput">The type of input for this step.</typeparam>
/// <typeparam name="TIfInput">The type of input for the if branch.</typeparam>
/// <typeparam name="TElseInput">The type of input for the else branch.</typeparam>
/// <typeparam name="TNextInput">The type of input for the next step.</typeparam>
public interface IIfElseStep<TInput, TIfInput, TElseInput, TNextInput> : IStep
{
    /// <summary>
    /// Handles the input for this step and conditionally executes either the if branch or the else branch.
    /// </summary>
    /// <param name="input">The input for this step.</param>
    /// <param name="ifNext">The handler for the if branch.</param>
    /// <param name="elseNext">The handler for the else branch.</param>
    /// <param name="ct">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task Handle(TInput input, Handler<TIfInput> ifNext, Handler<TElseInput> elseNext, CancellationToken ct = default);

    /// <summary>
    /// Builds a pipeline that executes the true branch when the condition is true.
    /// The pipeline will continue to the next step after executing the true branch.
    /// </summary>
    /// <param name="space">The space in which the pipeline is built.</param>
    /// <returns>An <see cref="OpenPipeline{TIfInput, TNextInput}"/> representing the pipeline that executes the true branch.</returns>
    OpenPipeline<TIfInput, TNextInput> BuildTruePipeline(Space space);

    /// <summary>
    /// Builds a pipeline that executes the false branch when the condition is false.
    /// The pipeline will continue to the next step after executing the false branch.
    /// </summary>
    /// <param name="space">The space in which the pipeline is built.</param>
    /// <returns>An <see cref="OpenPipeline{TElseInput, TNextInput}"/> representing the pipeline that executes the false branch.</returns>
    OpenPipeline<TElseInput, TNextInput> BuildFalsePipeline(Space space);
}
