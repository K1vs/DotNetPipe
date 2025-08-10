using System.Threading;

namespace K1vs.DotNetPipe.ReturningCancellable;

/// <summary>
/// Represents a multi-fork step in a returning cancellable pipeline.
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
    ValueTask<TResult> Handle(TInput input, IReadOnlyDictionary<string, Handler<TBranchesInput, TBranchesResult>> branches, Handler<TDefaultInput, TDefaultResult> defaultNext, CancellationToken ct = default);

    /// <summary>
    /// Builds pipelines for the branches.
    /// </summary>
    IReadOnlyDictionary<string, Pipeline<TBranchesInput, TBranchesResult>> BuildBranchesPipelines(Space space);

    /// <summary>
    /// Builds a pipeline for the default branch.
    /// </summary>
    Pipeline<TDefaultInput, TDefaultResult> BuildDefaultPipeline(Space space);
}


