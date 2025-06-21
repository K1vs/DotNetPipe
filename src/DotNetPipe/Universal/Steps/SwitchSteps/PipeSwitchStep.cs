namespace K1vs.DotNetPipe.Universal.Steps.SwitchSteps;

public sealed class PipeSwitchStep<TRootStepInput, TInput, TCaseInput, TDefaultInput, TNextStepInput> : SwitchStep<TRootStepInput, TInput, TCaseInput, TDefaultInput, TNextStepInput>
{
    public ReducedPipeStep<TRootStepInput, TInput> PreviousStep { get; }

    internal PipeSwitchStep(ReducedPipeStep<TRootStepInput, TInput> previousStep,
        string name,
        SwitchSelector<TInput, TCaseInput, TDefaultInput> selector,
        Func<Space, IReadOnlyDictionary<string, OpenPipeline<TCaseInput, TNextStepInput>>> caseBuilder,
        OpenPipeline<TDefaultInput, TNextStepInput> defaultPipeline,
        PipelineBuilder builder)
        : base(name, selector, caseBuilder, defaultPipeline, builder)
    {
        PreviousStep = previousStep;
    }

    internal override Handler<TRootStepInput> BuildHandler(Handler<TNextStepInput> handler)
    {
        var selector = CreateStepSelector();
        var casesHandlers = CasesPipelines.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.BuildHandler(handler)).AsReadOnly();
        var defaultHandler = DefaultPipeline.BuildHandler(handler);
        ValueTask Handler(TInput input) => selector(input, casesHandlers, defaultHandler);
        return PreviousStep.BuildHandler(Handler);
    }
}