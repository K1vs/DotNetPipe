using System.Threading;

namespace K1vs.DotNetPipe.ReturningCancellable;

/// <summary>
/// Represents an if-else step in a returning cancellable pipeline.
/// </summary>
/// <typeparam name="TInput">The type of input for this step.</typeparam>
/// <typeparam name="TResult">The type of result for this step.</typeparam>
/// <typeparam name="TIfInput">The type of input for the if branch.</typeparam>
/// <typeparam name="TIfResult">The type of result for the if branch.</typeparam>
/// <typeparam name="TElseInput">The type of input for the else branch.</typeparam>
/// <typeparam name="TElseResult">The type of result for the else branch.</typeparam>
/// <typeparam name="TNextInput">The type of input for the next step in the pipeline.</typeparam>
/// <typeparam name="TNextResult">The type of result for the next step in the pipeline.</typeparam>
public interface IIfElseStep<TInput, TResult, TIfInput, TIfResult, TElseInput, TElseResult, TNextInput, TNextResult> : IStep
{
    /// <summary>
    /// Handles the input for this step and routes it to true/false branch.
    /// </summary>
    ValueTask<TResult> Handle(TInput input,
        Handler<TIfInput, TIfResult> ifNext,
        Handler<TElseInput, TElseResult> elseNext,
        CancellationToken ct = default);

    /// <summary>
    /// Builds a pipeline for the true branch.
    /// </summary>
    /// <param name="space">The space in which the pipeline is built.</param>
    /// <returns>The open pipeline for the true branch.</returns>
    OpenPipeline<TIfInput, TIfResult, TNextInput, TNextResult> BuildTruePipeline(Space space);

    /// <summary>
    /// Builds a pipeline for the false branch.
    /// </summary>
    /// <param name="space">The space in which the pipeline is built.</param>
    /// <returns>The open pipeline for the false branch.</returns>
    OpenPipeline<TElseInput, TElseResult, TNextInput, TNextResult> BuildFalsePipeline(Space space);
}


