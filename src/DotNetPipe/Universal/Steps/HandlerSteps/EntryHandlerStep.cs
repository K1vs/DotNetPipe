namespace K1vs.DotNetPipe.Universal.Steps.HandlerSteps;

public sealed class EntryHandlerStep<TRootStepInput>: HandlerStep<TRootStepInput, TRootStepInput>
{
    internal EntryHandlerStep(string name, Handler<TRootStepInput> handler, PipelineBuilder builder)
        : base(name, handler, builder)
    {
    }

    public override Pipeline<TRootStepInput> BuildPipeline()
    {
        if(Builder.EntryStep is null)
        {
            throw new InvalidOperationException("Entry step is not set");
        }
        var pipeline = new Pipeline<TRootStepInput>(Builder.Name, Builder.EntryStep, this, BuildHandler);
        Builder.Space.AddPipeline(pipeline);
        return pipeline;
    }

    internal override Handler<TRootStepInput> BuildHandler()
    {
        var handler = CreateStepHandler();
        return handler;
    }
}
