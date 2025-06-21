namespace K1vs.DotNetPipe.Universal.Steps;

public abstract class Step
{
    public PipelineBuilder Builder { get; }

    protected Step(string name, PipelineBuilder builder)
    {
        Name = name;
        Builder = builder;
    }

    public string Name { get; }
}
