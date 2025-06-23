namespace K1vs.DotNetPipe.Universal.Steps;

public abstract class Step
{
    public StepName Name { get; }

    public PipelineBuilder Builder { get; }

    public virtual bool IsEntryStep { get; } = false;

    protected Step(string name, PipelineBuilder builder)
    {
        Name = new StepName(name, builder.Name);
        Builder = builder;
        builder.Space.AddStep(this);
        if (IsEntryStep)
        {
            if (builder.EntryStep != null)
            {
                throw new InvalidOperationException($"Entry step is already set to '{builder.EntryStep.Name}'. Cannot set it to '{name}'.");
            }
            builder.EntryStep = this;
        }
    }
}
