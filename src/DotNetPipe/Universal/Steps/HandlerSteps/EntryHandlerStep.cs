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
        return new Pipeline<TRootStepInput>(Builder.Name, Builder.EntryStep, this, BuildHandler);
    }

    internal override Handler<TRootStepInput> BuildHandler()
    {
        var handler = CreateStepHandler();
        return handler;
    }
}
