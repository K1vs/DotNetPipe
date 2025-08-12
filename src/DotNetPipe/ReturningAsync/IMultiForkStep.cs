namespace K1vs.DotNetPipe.ReturningAsync;

/// <summary>
/// Represents a multi-fork step in a pipeline.
/// </summary>
/// <typeparam name="TInput">The type of input for this step.</typeparam>
/// <typeparam name="TResult">The type of result for this step.</typeparam>
/// <typeparam name="TBranchesInput">The type of input for the branch handlers.</typeparam>
/// <typeparam name="TBranchesResult">The type of result for the branch handlers.</typeparam>
/// <typeparam name="TDefaultInput">The type of input for the default branch.</typeparam>
/// <typeparam name="TDefaultResult">The type of result for the default branch.</typeparam>
public interface IMultiForkStep<TInput, TResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> : IStep
{
    /// <summary>
    /// Handles the input for this step and routes it to one of multiple branches or the default branch.
    /// </summary>
    /// <param name="input">The input for this step.</param>
    /// <param name="branches">A dictionary of branch handlers where the key is the branch identifier.</param>
    /// <param name="defaultNext">The handler for the default branch when no branch matches.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<TResult> Handle(TInput input, IReadOnlyDictionary<string, Handler<TBranchesInput, TBranchesResult>> branches, Handler<TDefaultInput, TDefaultResult> defaultNext);

    /// <summary>
    /// Builds pipelines for the branches.
    /// </summary>
    /// <param name="space">The space in which the pipelines are built.</param>
    /// <returns>A dictionary of branch pipelines where the key is the branch identifier.</returns>
    IReadOnlyDictionary<string, Pipeline<TBranchesInput, TBranchesResult>> BuildBranchesPipelines(Space space);

    /// <summary>
    /// Builds a pipeline for the default branch.
    /// </summary>
    /// <param name="space">The space in which the pipeline is built.</param>
    /// <returns>A <see cref="Pipeline{TDefaultInput, TDefaultResult}"/> representing the default pipeline.</returns>
    Pipeline<TDefaultInput, TDefaultResult> BuildDefaultPipeline(Space space);
}

