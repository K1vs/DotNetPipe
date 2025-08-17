namespace K1vs.DotNetPipe.ReturningSync.Steps.SwitchSteps;

/// <summary>
/// Represents a step inside a pipeline that processes input and routes it to different cases based on a selector.
/// If no case matches, it routes to a default pipeline.
/// After branching, it continues to the next step in the pipeline.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TEntryStepResult">The type of the result for the entry step.</typeparam>
/// <typeparam name="TInput">The type of the input for the switch step.</typeparam>
/// <typeparam name="TResult">The type of the result for the switch step.</typeparam>
/// <typeparam name="TCaseInput">The type of input for the case branches.</typeparam>
/// <typeparam name="TCaseResult">The type of result for the case branches.</typeparam>
/// <typeparam name="TDefaultInput">The type of input for the default branch.</typeparam>
/// <typeparam name="TDefaultResult">The type of result for the default branch.</typeparam>
/// <typeparam name="TNextStepInput">The type of input for the next step after the switch step.</typeparam>
/// <typeparam name="TNextStepResult">The type of result for the next step after the switch step.</typeparam>
public sealed class PipeSwitchStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextStepInput, TNextStepResult> : SwitchStep<TEntryStepInput, TEntryStepResult, TInput, TResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult, TNextStepInput, TNextStepResult>
{
    /// <summary>
    /// Gets the previous step in the pipeline before this switch step.
    /// </summary>
    public ReducedPipeStep<TEntryStepInput, TEntryStepResult, TInput, TResult> PreviousStep { get; }

    /// <inheritdoc/>
    internal PipeSwitchStep(ReducedPipeStep<TEntryStepInput, TEntryStepResult, TInput, TResult> previousStep,
        string name,
        SwitchSelector<TInput, TResult, TCaseInput, TCaseResult, TDefaultInput, TDefaultResult> selector,
        Func<Space, IReadOnlyDictionary<string, OpenPipeline<TCaseInput, TCaseResult, TNextStepInput, TNextStepResult>>> caseBuilder,
        OpenPipeline<TDefaultInput, TDefaultResult, TNextStepInput, TNextStepResult> defaultPipeline,
        PipelineBuilder builder)
        : base(name, selector, caseBuilder, defaultPipeline, builder)
    {
        PreviousStep = previousStep;
    }

    /// <inheritdoc/>
    internal override Handler<TEntryStepInput, TEntryStepResult> BuildHandler(Handler<TNextStepInput, TNextStepResult> handler)
    {
        var selector = CreateStepSelector();
        var casesHandlers = CasesPipelines.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.BuildHandler(handler));
        var defaultHandler = DefaultPipeline.BuildHandler(handler);
        TResult Handler(TInput input) => selector(input, casesHandlers, defaultHandler);
        return PreviousStep.BuildHandler(Handler);
    }
}

