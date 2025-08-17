namespace K1vs.DotNetPipe.ReturningSync;

/// <summary>
/// Represents a switch step in a pipeline.
/// </summary>
/// <typeparam name="TInput">The type of input for this step.</typeparam>
/// <typeparam name="TResult">The type of result for this step.</typeparam>
/// <typeparam name="TCaseInput">The type of input for the case branches.</typeparam>
/// <typeparam name="TCaseResult">The type of result for the case branches.</typeparam>
/// <typeparam name="TDefaultInput">The type of input for the default branch.</typeparam>
/// <typeparam name="TDefaultResult">The type of result for the default branch.</typeparam>
/// <typeparam name="TNextInput">The type of input for the next step.</typeparam>
/// <typeparam name="TNextResult">The type of result for the next step.</typeparam>
public interface ISwitchStep<TInput, TResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextInput, TNextResult> : IStep
{
    /// <summary>
    /// Handles the input for this step and routes it to one of the case branches or the default branch.
    /// </summary>
    /// <param name="input">The input for this step.</param>
    /// <param name="cases">A dictionary of case handlers where the key is the case identifier.</param>
    /// <param name="defaultNext">The handler for the default branch when no case matches.</param>
    /// <returns>The result of the switch step.</returns>
    TResult Handle(TInput input, IReadOnlyDictionary<string, Handler<TCaseInput, TCaseResult>> cases, Handler<TDefaultInput, TDefaultResult> defaultNext);

    /// <summary>
    /// Builds pipelines for the case branches.
    /// </summary>
    /// <param name="space">The space in which the pipelines are built.</param>
    /// <returns>A dictionary of case pipelines where the key is the case identifier.</returns>
    IReadOnlyDictionary<string, OpenPipeline<TCaseInput, TCaseResult, TNextInput, TNextResult>> BuildCasesPipelines(Space space);

    /// <summary>
    /// Builds a pipeline for the default branch.
    /// </summary>
    /// <param name="space">The space in which the pipeline is built.</param>
    /// <returns>An <see cref="OpenPipeline{TDefaultInput, TDefaultResult, TNextInput, TNextResult}"/> representing the default pipeline.</returns>
    OpenPipeline<TDefaultInput, TDefaultResult, TNextInput, TNextResult> BuildDefaultPipeline(Space space);
}

