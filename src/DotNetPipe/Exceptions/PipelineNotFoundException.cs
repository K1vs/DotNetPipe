namespace K1vs.DotNetPipe.Exceptions;

/// <summary>
/// Exception thrown when a pipeline with the specified name is not found.
/// This is typically used when trying to access or manipulate a pipeline that does not exist.
/// </summary>
public class PipelineNotFoundException : KeyNotFoundException
{
    /// <summary>
    /// Gets the name of the pipeline that was not found.
    /// </summary>
    public string PipelineName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineNotFoundException"/> class with a specified pipeline name.
    /// </summary>
    /// <param name="name">The name of the pipeline that was not found.</param>
    internal PipelineNotFoundException(string name)
        : base($"Pipeline with name '{name}' not found.")
    {
        PipelineName = name;
    }
}