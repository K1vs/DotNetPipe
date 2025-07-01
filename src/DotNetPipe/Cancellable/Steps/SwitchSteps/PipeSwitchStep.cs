namespace K1vs.DotNetPipe.Cancellable.Steps.SwitchSteps;

/// <summary>
/// Represents a step inside a pipeline that processes input and routes it to different cases based on a selector.
/// If no case matches, it routes to a default pipeline.
/// After branching, it continues to the next step in the pipeline.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TInput">The type of the input for the switch step.</typeparam>
/// <typeparam name="TCaseInput">The type of input for the case branches.</typeparam>
/// <typeparam name="TDefaultInput">The type of input for the default branch.</typeparam>
/// <typeparam name="TNextStepInput">The type of input for the next step after the switch step.</typeparam>
public sealed class PipeSwitchStep<TEntryStepInput, TInput, TCaseInput, TDefaultInput, TNextStepInput> : SwitchStep<TEntryStepInput, TInput, TCaseInput, TDefaultInput, TNextStepInput>
{
    /// <summary>
    /// Gets the previous step in the pipeline before this switch step.
    /// </summary>
    public ReducedPipeStep<TEntryStepInput, TInput> PreviousStep { get; }

    /// <inheritdoc/>
    internal PipeSwitchStep(ReducedPipeStep<TEntryStepInput, TInput> previousStep,
        string name,
        SwitchSelector<TInput, TCaseInput, TDefaultInput> selector,
        Func<Space, IReadOnlyDictionary<string, OpenPipeline<TCaseInput, TNextStepInput>>> caseBuilder,
        OpenPipeline<TDefaultInput, TNextStepInput> defaultPipeline,
        PipelineBuilder builder)
        : base(name, selector, caseBuilder, defaultPipeline, builder)
    {
        PreviousStep = previousStep;
    }

    /// <inheritdoc/>
    internal override Handler<TEntryStepInput> BuildHandler(Handler<TNextStepInput> handler)
    {
        var selector = CreateStepSelector();
        var casesHandlers = CasesPipelines.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.BuildHandler(handler)).AsReadOnly();
        var defaultHandler = DefaultPipeline.BuildHandler(handler);
        ValueTask Handler(TInput input, CancellationToken ct) => selector(input, casesHandlers, defaultHandler, ct);
        return PreviousStep.BuildHandler(Handler);
    }
}
