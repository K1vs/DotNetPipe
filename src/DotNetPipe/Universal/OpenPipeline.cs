using K1vs.DotNetPipe.Universal.Steps;

namespace K1vs.DotNetPipe.Universal;

public class OpenPipeline<TInput, TNextInput> : IPipeline
{
    public Space Space => EntryStep.Builder.Space;

    public string Name { get; }

    public Step EntryStep { get; }

    public Step LastStep => ReducedPipeStep;

    public bool IsOpenPipeline => true;

    public ReducedPipeStep<TInput, TNextInput> ReducedPipeStep { get; }

    internal OpenPipeline(string name, Step entryStep, ReducedPipeStep<TInput, TNextInput> lastStep)
    {
        Name = name;
        EntryStep = entryStep;
        ReducedPipeStep = lastStep;
    }

    public Handler<TInput> BuildHandler(Handler<TNextInput> handler)
    {
        return ReducedPipeStep.BuildHandler(handler);
    }
}
