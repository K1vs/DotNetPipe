using K1vs.DotNetPipe.Universal.Steps;

namespace K1vs.DotNetPipe.Universal;

public class Pipeline<TInput>
{
    private readonly Func<Handler<TInput>> _buildHandler;

    public string Name { get; }

    public Step EntryStep { get; }

    public Step TerminatorStep { get; }

    public Pipeline(string name, Step entryStep, Step terminatorStep, Func<Handler<TInput>> buildHandler)
    {
        Name = name;
        EntryStep = entryStep;
        TerminatorStep = terminatorStep;
        _buildHandler = buildHandler;
    }

    public Handler<TInput> Compile()
    {
        return _buildHandler();
    }
}
