using System.Threading;

namespace K1vs.DotNetPipe.ReturningAsyncCancellable;

/// <summary>
/// Represents a switch step in a returning cancellable pipeline.
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
    Task<TResult> Handle(TInput input, IReadOnlyDictionary<string, Handler<TCaseInput, TCaseResult>> cases, Handler<TDefaultInput, TDefaultResult> defaultNext, CancellationToken ct = default);

    /// <summary>
    /// Builds pipelines for the case branches.
    /// </summary>
    IReadOnlyDictionary<string, OpenPipeline<TCaseInput, TCaseResult, TNextInput, TNextResult>> BuildCasesPipelines(Space space);

    /// <summary>
    /// Builds a pipeline for the default branch.
    /// </summary>
    OpenPipeline<TDefaultInput, TDefaultResult, TNextInput, TNextResult> BuildDefaultPipeline(Space space);
}



