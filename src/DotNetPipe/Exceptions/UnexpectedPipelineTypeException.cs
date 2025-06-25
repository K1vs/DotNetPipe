namespace K1vs.DotNetPipe.Exceptions;

/// <summary>
/// Exception thrown when a pipeline has an unexpected type.
/// This is typically used to ensure that the pipeline type matches the expected type.
/// </summary>
public class UnexpectedPipelineTypeException : Exception
{
    /// <summary>
    /// Gets the actual type of the pipeline that was encountered.
    /// </summary>
    public Type ActualType { get; }

    /// <summary>
    /// Gets the expected type of the pipeline that was anticipated.
    /// </summary>
    public Type ExpectedType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnexpectedPipelineTypeException"/> class with specified pipeline name, actual type, and expected type.
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline that has an unexpected type.</param>
    /// <param name="actualType">The actual type of the pipeline that was encountered.</param>
    /// <param name="expectedType">The expected type of the pipeline that was anticipated.</param>
    internal UnexpectedPipelineTypeException(string pipelineName, Type actualType, Type expectedType)
        : base($"Pipeline '{pipelineName}' has unexpected type '{actualType.FullName}', expected '{expectedType.FullName}'.")
    {
        ActualType = actualType;
        ExpectedType = expectedType;
    }
}