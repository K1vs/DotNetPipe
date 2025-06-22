namespace K1vs.DotNetPipe.Universal.Steps.HandlerSteps;

public sealed class PipeHandlerStep<TRootStepInput, TInput> : HandlerStep<TRootStepInput, TInput>
{
    public ReducedPipeStep<TRootStepInput, TInput> PreviousStep { get; }

    internal PipeHandlerStep(ReducedPipeStep<TRootStepInput, TInput> previous,
        string name,
        Handler<TInput> handler,
        PipelineBuilder builder)
        : base(name, handler, builder)
    {
        PreviousStep = previous;
    }

    public override Pipeline<TRootStepInput> BuildPipeline()
    {
        if(PreviousStep is null)
        {
            throw new InvalidOperationException("Previous step is not set");
        }
        var pipeline = new Pipeline<TRootStepInput>(Builder.Name, PreviousStep, this, BuildHandler);
        Builder.Space.AddPipeline(pipeline);
        return pipeline;
    }

    internal override Handler<TRootStepInput> BuildHandler()
    {
        var handler = CreateStepHandler();
        var pipe = PreviousStep.BuildHandler(handler);
        return pipe;
    }
}
