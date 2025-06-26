using K1vs.DotNetPipe.Sync.Steps;

namespace K1vs.DotNetPipe.Sync;

/// <summary>
/// Represents a pipeline in the DotNetPipe framework.
/// A pipeline consists of a series of steps that process input data sequentially.
/// Each pipeline has an entry step where processing begins and a last step that concludes the processing.
/// Pipelines can be open, allowing integrate it in another pipeline as a sub-pipeline in if, ifelse or switch step.
/// Otherwise, it can be called directly or can be integrated in another pipeline as a sub-pipeline in fork or multifork step.
/// </summary>
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