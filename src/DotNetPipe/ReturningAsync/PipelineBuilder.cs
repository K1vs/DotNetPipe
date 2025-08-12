using K1vs.DotNetPipe.ReturningAsync.Steps;

namespace K1vs.DotNetPipe.ReturningAsync;

/// <summary>
/// Represents a builder for creating a pipeline in a specific space.
/// </summary>
public class PipelineBuilder
{
    /// <summary>
    /// The space in which the pipeline is being built.
    /// </summary>
    public Space Space { get; }

    /// <summary>
    /// The name of the pipeline being built.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The entry step of the pipeline, which is the first step executed when the pipeline runs.
    /// </summary>
    public Step? EntryStep { get; internal set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineBuilder"/> class.
    /// </summary>
    /// <param name="space">The space in which the pipeline will be created.</param>
    /// <param name="name">The name of the pipeline.</param>
    internal PipelineBuilder(Space space, string name)
    {
        Space = space;
        Name = name;
    }
}

