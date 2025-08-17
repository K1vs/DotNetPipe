namespace K1vs.DotNetPipe.ReturningSync;

/// <summary>
/// Represents a fork step in a pipeline.
/// </summary>
/// <typeparam name="TInput">The type of input for this step.</typeparam>
/// <typeparam name="TResult">The type of result for this step.</typeparam>
/// <typeparam name="TBranchAInput">The type of input for branch A.</typeparam>
/// <typeparam name="TBranchAResult">The type of result for branch A.</typeparam>
/// <typeparam name="TBranchBInput">The type of input for branch B.</typeparam>
/// <typeparam name="TBranchBResult">The type of result for branch B.</typeparam>
public interface IForkStep<TInput, TResult, TBranchAInput, TBranchAResult, TBranchBInput, TBranchBResult> : IStep
{
    /// <summary>
    /// Handles the input for this step and routes it to one of two branches.
    /// </summary>
    /// <param name="input">The input for this step.</param>
    /// <param name="branchANext">The handler for branch A.</param>
    /// <param name="branchBNext">The handler for branch B.</param>
    /// <returns>The result of the step.</returns>
    TResult Handle(TInput input, Handler<TBranchAInput, TBranchAResult> branchANext, Handler<TBranchBInput, TBranchBResult> branchBNext);

    /// <summary>
    /// Builds a pipeline for branch A.
    /// </summary>
    /// <param name="space">The space in which the pipeline is built.</param>
    /// <returns>A <see cref="Pipeline{TBranchAInput, TBranchAResult}"/> representing the pipeline for branch A.</returns>
    Pipeline<TBranchAInput, TBranchAResult> BuildBranchAPipeline(Space space);

    /// <summary>
    /// Builds a pipeline for branch B.
    /// </summary>
    /// <param name="space">The space in which the pipeline is built.</param>
    /// <returns>A <see cref="Pipeline{TBranchBInput, TBranchBResult}"/> representing the pipeline for branch B.</returns>
    Pipeline<TBranchBInput, TBranchBResult> BuildBranchBPipeline(Space space);
}

