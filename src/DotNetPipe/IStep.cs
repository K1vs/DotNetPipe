namespace K1vs.DotNetPipe;

/// <summary>
/// Represents a step in a pipeline.
/// </summary>
public interface IStep
{
    /// <summary>
    /// The name of the step.
    /// </summary>
    string Name { get; }
}