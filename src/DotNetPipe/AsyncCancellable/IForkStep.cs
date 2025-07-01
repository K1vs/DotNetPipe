namespace K1vs.DotNetPipe.AsyncCancellable;

/// <summary>
/// Represents a fork step in a pipeline.
/// </summary>
/// <typeparam name="TInput">The type of input for this step.</typeparam>
/// <typeparam name="TBranchAInput">The type of input for branch A.</typeparam>
/// <typeparam name="TBranchBInput">The type of input for branch B.</typeparam>
public interface IForkStep<TInput, TBranchAInput, TBranchBInput> : IStep
{
    /// <summary>
    /// Handles the input for this step and routes it to one of two branches.
    /// </summary>
    /// <param name="input">The input for this step.</param>
    /// <param name="branchANext">The handler for branch A.</param>
    /// <param name="branchBNext">The handler for branch B.</param>
    /// <param name="ct">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task Handle(TInput input, Handler<TBranchAInput> branchANext, Handler<TBranchBInput> branchBNext, CancellationToken ct = default);

    /// <summary>
    /// Builds a pipeline for branch A.
    /// </summary>
    /// <param name="space">The space in which the pipeline is built.</param>
    /// <returns>A <see cref="Pipeline{TBranchAInput}"/> representing the pipeline for branch A.</returns>
    Pipeline<TBranchAInput> BuildBranchAPipeline(Space space);

    /// <summary>
    /// Builds a pipeline for branch B.
    /// </summary>
    /// <param name="space">The space in which the pipeline is built.</param>
    /// <returns>A <see cref="Pipeline{TBranchBInput}"/> representing the pipeline for branch B.</returns>
    Pipeline<TBranchBInput> BuildBranchBPipeline(Space space);
}
