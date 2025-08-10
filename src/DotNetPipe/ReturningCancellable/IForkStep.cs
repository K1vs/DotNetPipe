using System.Threading;

namespace K1vs.DotNetPipe.ReturningCancellable;

/// <summary>
/// Represents a fork step in a returning cancellable pipeline.
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
    ValueTask<TResult> Handle(TInput input, Handler<TBranchAInput, TBranchAResult> branchANext, Handler<TBranchBInput, TBranchBResult> branchBNext, CancellationToken ct = default);

    /// <summary>
    /// Builds a pipeline for branch A.
    /// </summary>
    Pipeline<TBranchAInput, TBranchAResult> BuildBranchAPipeline(Space space);

    /// <summary>
    /// Builds a pipeline for branch B.
    /// </summary>
    Pipeline<TBranchBInput, TBranchBResult> BuildBranchBPipeline(Space space);
}


