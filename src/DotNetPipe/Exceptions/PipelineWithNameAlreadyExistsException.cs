namespace K1vs.DotNetPipe.Exceptions;

/// <summary>
/// Exception thrown when a pipeline with the specified name already exists.
/// This is typically used to prevent duplicate pipeline names in the system.
/// </summary>
public class PipelineWithNameAlreadyExistsException : Exception
{
    /// <summary>
    /// Gets the name of the pipeline that already exists.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineWithNameAlreadyExistsException"/> class with a specified pipeline name.
    /// </summary>
    /// <param name="name">The name of the pipeline that already exists.</param>
    internal PipelineWithNameAlreadyExistsException(string name)
        : base($"Pipeline with name '{name}' already exists.")
    {
        Name = name;
    }
}