namespace K1vs.DotNetPipe.Cancellable.Steps;

/// <summary>
/// Base class for all steps in a pipeline.
/// </summary>
public abstract class Step
{
    /// <summary>
    /// The name of the step, which consist of own name and the pipeline name it belongs to.
    /// It is used to uniquely identify the step within the pipeline and space.
    /// </summary>
    public StepName Name { get; }

    /// <summary>
    /// The builder that created this step, which contains the pipeline it belongs to.
    /// </summary>
    public PipelineBuilder Builder { get; }

    /// <summary>
    /// Indicates whether this step is the entry step of the pipeline.
    /// </summary>
    public virtual bool IsEntryStep { get; } = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="Step"/> class with the specified name and builder.
    /// </summary>
    /// <param name="name">The name of the step.</param>
    /// <param name="builder">The builder that created this step, which contains the pipeline it belongs to.</param>
    /// <exception cref="InvalidOperationException">Thrown if the entry step is already set in the builder.</exception>
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
