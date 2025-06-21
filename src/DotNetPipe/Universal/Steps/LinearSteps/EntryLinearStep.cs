namespace K1vs.DotNetPipe.Universal.Steps.LinearSteps;

public sealed class EntryLinearStep<TRootStepInput, TNextInput>: LinearStep<TRootStepInput, TRootStepInput, TNextInput>
{
    internal EntryLinearStep(string name, Pipe<TRootStepInput, TNextInput> originalPipe, PipelineBuilder builder)
        : base(name, originalPipe, builder)
    {
    }

    internal override Handler<TRootStepInput> BuildHandler(Handler<TNextInput> handler)
    {
        var pipe = CreateStepPipe();
        return (input) => pipe(input, handler);
    }
}
