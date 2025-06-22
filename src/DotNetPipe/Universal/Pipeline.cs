using K1vs.DotNetPipe.Universal.Steps;

namespace K1vs.DotNetPipe.Universal;

public class Pipeline<TInput>: IPipeline
{
    private readonly Func<Handler<TInput>> _buildHandler;

    public string Name { get; }

    public Step EntryStep { get; }

    public Step HandlerStep { get; }

    public Step LastStep => HandlerStep;

    public bool IsOpenPipeline => false;

    public Pipeline(string name, Step entryStep, Step handlerStep, Func<Handler<TInput>> buildHandler)
    {
        Name = name;
        EntryStep = entryStep;
        HandlerStep = handlerStep;
        _buildHandler = buildHandler;
    }

    public Handler<TInput> Compile()
    {
        return _buildHandler();
    }
}
