namespace K1vs.DotNetPipe.Universal.Steps.IfElseSteps;

public class PipeIfElseStep<TRootStepInput, TInput, TIfInput, TElseInput, TNextStepInput> : IfElseStep<TRootStepInput, TInput, TIfInput, TElseInput, TNextStepInput>
{
    public ReducedPipeStep<TRootStepInput, TInput> PreviousStep { get; }

    internal PipeIfElseStep(ReducedPipeStep<TRootStepInput, TInput> previousStep,
        string name,
        IfElseSelector<TInput, TIfInput, TElseInput> selector,
        Func<Space, OpenPipeline<TIfInput, TNextStepInput>> trueBuilder,
        Func<Space, OpenPipeline<TElseInput, TNextStepInput>> elseBuilder,
        PipelineBuilder builder)
        : base(name, selector, trueBuilder, elseBuilder, builder)
    {
        PreviousStep = previousStep;
    }

    internal override Handler<TRootStepInput> BuildHandler(Handler<TNextStepInput> handler)
    {
        var selector = CreateStepSelector();
        var trueHandler = TruePipeline.BuildHandler(handler);
        var elseHandler = ElsePipeline.BuildHandler(handler);
        ValueTask Handler(TInput input) => selector(input, trueHandler, elseHandler);
        return PreviousStep.BuildHandler(Handler);
    }
}
