namespace K1vs.DotNetPipe.ReturningSyncCancellable.Steps.SwitchSteps;

/// <summary>
/// Represents a entry switch step in a start of pipeline that allows branching based on a selector.
/// It processes input and routes it to different cases based on the selector.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TResult">The type of the result for the switch step.</typeparam>
/// <typeparam name="TCaseInput">The type of input for the case branches.</typeparam>
/// <typeparam name="TCaseResult">The type of result for the case branches.</typeparam>
/// <typeparam name="TDefaultInput">The type of input for the default branch.</typeparam>
/// <typeparam name="TDefaultResult">The type of result for the default branch.</typeparam>
/// <typeparam name="TNextStepInput">The type of input for the next step after the switch step.</typeparam>
/// <typeparam name="TNextStepResult">The type of result for the next step after the switch step.</typeparam>
public class EntrySwitchStep<TEntryStepInput, TResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextStepInput, TNextStepResult> : SwitchStep<TEntryStepInput, TResult, TEntryStepInput, TResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextStepInput, TNextStepResult>
{
    /// <inheritdoc/>
    public override bool IsEntryStep => true;

    internal EntrySwitchStep(string name,
        SwitchSelector<TEntryStepInput, TResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult> selector,
        Func<Space, IReadOnlyDictionary<string, OpenPipeline<TCaseInput, TCaseResult, TNextStepInput, TNextStepResult>>> caseBuilder,
        OpenPipeline<TDefaultInput, TDefaultResult, TNextStepInput, TNextStepResult> defaultPipeline,
        PipelineBuilder builder)
        : base(name, selector, caseBuilder, defaultPipeline, builder)
    {
    }

    /// <inheritdoc/>
    internal override Handler<TEntryStepInput, TResult> BuildHandler(Handler<TNextStepInput, TNextStepResult> handler)
    {
        var selector = CreateStepSelector();
        var casesHandlers = CasesPipelines.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.BuildHandler(handler));
        var defaultHandler = DefaultPipeline.BuildHandler(handler);
        TResult Handler(TEntryStepInput input, CancellationToken ct) => selector(input, casesHandlers, defaultHandler, ct);
        return Handler;
    }
}



