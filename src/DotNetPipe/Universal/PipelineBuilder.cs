using K1vs.DotNetPipe.Universal.Steps;

namespace K1vs.DotNetPipe.Universal;

public class PipelineBuilder
{
    public Space Space { get; }

    public string Name { get; }

    public Step? EntryStep { get; internal set; }

    public PipelineBuilder(Space space, string name)
    {
        Space = space;
        Name = name;
    }
}
