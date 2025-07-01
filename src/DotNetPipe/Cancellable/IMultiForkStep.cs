namespace K1vs.DotNetPipe.Cancellable;

/// <summary>
/// Represents a multi-fork step in a pipeline.
/// </summary>
/// <typeparam name="TInput">The type of input for this step.</typeparam>
/// <typeparam name="TBranchesInput">The type of input for the branch handlers.</typeparam>
/// <typeparam name="TDefaultInput">The type of input for the default branch.</typeparam>
public interface IMultiForkStep<TInput, TBranchesInput, TDefaultInput> : IStep
{
    /// <summary>
    /// Handles the input for this step and routes it to one of multiple branches or the default branch.
    /// </summary>
    /// <param name="input">The input for this step.</param>
    /// <param name="branches">A dictionary of branch handlers where the key is the branch identifier.</param>
    /// <param name="defaultNext">The handler for the default branch when no branch matches.</param>
    /// <param name="ct">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask Handle(TInput input, IReadOnlyDictionary<string, Handler<TBranchesInput>> branches, Handler<TDefaultInput> defaultNext, CancellationToken ct = default);

    /// <summary>
    /// Builds pipelines for the branches.
    /// </summary>
    /// <param name="space">The space in which the pipelines are built.</param>
    /// <returns>A dictionary of branch pipelines where the key is the branch identifier.</returns>
    IReadOnlyDictionary<string, Pipeline<TBranchesInput>> BuildBranchesPipelines(Space space);

    /// <summary>
    /// Builds a pipeline for the default branch.
    /// </summary>
    /// <param name="space">The space in which the pipeline is built.</param>
    /// <returns>A <see cref="Pipeline{TDefaultInput}"/> representing the default pipeline.</returns>
    Pipeline<TDefaultInput> BuildDefaultPipeline(Space space);
}
