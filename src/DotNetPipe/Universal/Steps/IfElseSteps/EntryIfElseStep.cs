namespace K1vs.DotNetPipe.Universal.Steps.IfElseSteps;

public class EntryIfElseStep<TRootStepInput, TIfInput, TElseInput, TNextStepInput> : IfElseStep<TRootStepInput, TRootStepInput, TIfInput, TElseInput, TNextStepInput>
{
    internal EntryIfElseStep(string name, IfElseSelector<TRootStepInput, TIfInput, TElseInput> selector,
        Func<Space, OpenPipeline<TIfInput, TNextStepInput>> trueBuilder,
        Func<Space, OpenPipeline<TElseInput, TNextStepInput>> elseBuilder,
        PipelineBuilder builder)
        : base(name, selector, trueBuilder, elseBuilder, builder)
    {
    }

    internal override Handler<TRootStepInput> BuildHandler(Handler<TNextStepInput> handler)
    {
        var selector = CreateStepSelector();
        var trueHandler = TruePipeline.BuildHandler(handler);
        var elseHandler = ElsePipeline.BuildHandler(handler);
        ValueTask Handler(TRootStepInput input) => selector(input, trueHandler, elseHandler);
        return Handler;
    }
}
