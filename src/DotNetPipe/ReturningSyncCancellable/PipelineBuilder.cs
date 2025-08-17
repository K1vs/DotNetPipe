using K1vs.DotNetPipe.ReturningSyncCancellable.Steps;

namespace K1vs.DotNetPipe.ReturningSyncCancellable;

/// <summary>
/// Represents a builder for creating a returning cancellable pipeline in a specific space.
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

    internal PipelineBuilder(Space space, string name)
    {
        Space = space;
        Name = name;
    }
}



