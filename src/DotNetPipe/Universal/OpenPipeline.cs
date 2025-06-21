using K1vs.DotNetPipe.Universal.Steps;

namespace K1vs.DotNetPipe.Universal;

public class OpenPipeline<TInput, TNextInput>
{
    public string Name { get; }

    public Step EntryStep { get; }

    public ReducedPipeStep<TInput, TNextInput> LastStep { get; }

    public OpenPipeline(string name, Step entryStep, ReducedPipeStep<TInput, TNextInput> lastStep)
    {
        Name = name;
        EntryStep = entryStep;
        LastStep = lastStep;
    }

    public Handler<TInput> BuildHandler(Handler<TNextInput> handler)
    {
        return LastStep.BuildHandler(handler);
    }
}
