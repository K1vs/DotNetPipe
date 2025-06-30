namespace K1vs.DotNetPipe.Universal;

/// <summary>
/// Represents a switch step in a pipeline.
/// </summary>
/// <typeparam name="TInput">The type of input for this step.</typeparam>
/// <typeparam name="TCaseInput">The type of input for the case branches.</typeparam>
/// <typeparam name="TDefaultInput">The type of input for the default branch.</typeparam>
/// <typeparam name="TNextInput">The type of input for the next step.</typeparam>
public interface ISwitchStep<TInput, TCaseInput, TDefaultInput, TNextInput> : IStep
{
    /// <summary>
    /// Handles the input for this step and routes it to one of the case branches or the default branch.
    /// </summary>
    /// <param name="input">The input for this step.</param>
    /// <param name="cases">A dictionary of case handlers where the key is the case identifier.</param>
    /// <param name="defaultNext">The handler for the default branch when no case matches.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask Handle(TInput input, IReadOnlyDictionary<string, Handler<TCaseInput>> cases, Handler<TDefaultInput> defaultNext);

    /// <summary>
    /// Builds pipelines for the case branches.
    /// </summary>
    /// <param name="space">The space in which the pipelines are built.</param>
    /// <returns>A dictionary of case pipelines where the key is the case identifier.</returns>
    IReadOnlyDictionary<string, OpenPipeline<TCaseInput, TNextInput>> BuildCasesPipelines(Space space);

    /// <summary>
    /// Builds a pipeline for the default branch.
    /// </summary>
    /// <param name="space">The space in which the pipeline is built.</param>
    /// <returns>An <see cref="OpenPipeline{TDefaultInput, TNextInput}"/> representing the default pipeline.</returns>
    OpenPipeline<TDefaultInput, TNextInput> BuildDefaultPipeline(Space space);
}
