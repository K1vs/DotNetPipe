namespace K1vs.DotNetPipe.Universal.Steps.IfSteps;

public sealed class PipeIfStep<TRootStepInput, TInput, TIfInput, TNextStepInput> : IfStep<TRootStepInput, TInput, TIfInput, TNextStepInput>
{
    public ReducedPipeStep<TRootStepInput, TInput> PreviousStep { get; }

    internal PipeIfStep(ReducedPipeStep<TRootStepInput, TInput> previousStep,
        string name,
        IfSelector<TInput, TIfInput, TNextStepInput> selector,
        Func<Space, OpenPipeline<TIfInput, TNextStepInput>> trueBuilder,
        PipelineBuilder builder)
        : base(name, selector, trueBuilder, builder)
    {
        PreviousStep = previousStep;
    }

    internal override Handler<TRootStepInput> BuildHandler(Handler<TNextStepInput> handler)
    {
        var trueHandler = TruePipeline.BuildHandler(handler);
        var selector = CreateStepSelector();
        ValueTask Handler(TInput input) => selector(input, trueHandler, handler);
        return PreviousStep.BuildHandler(Handler);
    }
}
