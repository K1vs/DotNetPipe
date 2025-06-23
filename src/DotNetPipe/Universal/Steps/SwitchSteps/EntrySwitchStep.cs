namespace K1vs.DotNetPipe.Universal.Steps.SwitchSteps;

public class EntrySwitchStep<TRootStepInput, TCaseInput, TDefaultInput, TNextStepInput> : SwitchStep<TRootStepInput, TRootStepInput, TCaseInput, TDefaultInput, TNextStepInput>
{
    public override bool IsEntryStep => true;

    internal EntrySwitchStep(string name,
        SwitchSelector<TRootStepInput, TCaseInput, TDefaultInput> selector,
        Func<Space, IReadOnlyDictionary<string, OpenPipeline<TCaseInput, TNextStepInput>>> caseBuilder,
        OpenPipeline<TDefaultInput, TNextStepInput> defaultPipeline,
        PipelineBuilder builder)
        : base(name, selector, caseBuilder, defaultPipeline, builder)
    {
    }

    internal override Handler<TRootStepInput> BuildHandler(Handler<TNextStepInput> handler)
    {
        var selector = CreateStepSelector();
        var casesHandlers = CasesPipelines.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.BuildHandler(handler)).AsReadOnly();
        var defaultHandler = DefaultPipeline.BuildHandler(handler);
        ValueTask Handler(TRootStepInput input) => selector(input, casesHandlers, defaultHandler);
        return Handler;
    }
}
