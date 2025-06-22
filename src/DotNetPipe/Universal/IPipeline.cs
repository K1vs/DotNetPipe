using K1vs.DotNetPipe.Universal.Steps;

namespace K1vs.DotNetPipe.Universal;

public interface IPipeline
{
    /// <summary>
    /// Name of the pipeline.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Entry step of the pipeline.
    /// </summary>
    Step EntryStep { get; }

    /// <summary>
    /// Last step of the pipeline.
    /// If the pipeline is open, this is the last step that can be extended.
    /// If the pipeline is closed, this is the step that handles the final input, e.g. a handler step.
    /// </summary>
    Step LastStep { get; }

    /// <summary>
    /// True if the pipeline is an open pipeline, meaning it can be extended with additional steps.
    /// </summary>
    bool IsOpenPipeline { get; }
}