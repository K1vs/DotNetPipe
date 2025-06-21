namespace K1vs.DotNetPipe.Universal.Steps.IfSteps;

public sealed class EntryIfStep<TRootStepInput, TIfInput, TNextStepInput> : IfStep<TRootStepInput, TRootStepInput, TIfInput, TNextStepInput>
{
    internal EntryIfStep(string name, IfSelector<TRootStepInput, TIfInput, TNextStepInput> selector,
        Func<Space, OpenPipeline<TIfInput, TNextStepInput>> trueBuilder,
        PipelineBuilder builder)
        : base(name, selector, trueBuilder, builder)
    {
    }

    internal override Handler<TRootStepInput> BuildHandler(Handler<TNextStepInput> handler)
    {
        var trueHandler = TruePipeline.BuildHandler(handler);
        var selector = CreateStepSelector();
        ValueTask Handler(TRootStepInput input) => selector(input, trueHandler, handler);
        return Handler;
    }
}
