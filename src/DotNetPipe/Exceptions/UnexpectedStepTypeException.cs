namespace K1vs.DotNetPipe.Exceptions;

/// <summary>
/// Exception thrown when a step has an unexpected type.
/// This is typically used to ensure that the step type matches the expected type.
/// </summary>
public class UnexpectedStepTypeException : Exception
{
    /// <summary>
    /// Gets the actual type of the step that was encountered.
    /// </summary>
    public Type ActualType { get; }

    /// <summary>
    /// Gets the expected type of the step that was anticipated.
    /// </summary>
    public Type ExpectedType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnexpectedStepTypeException"/> class with specified step name, actual type, and expected type.
    /// </summary>
    /// <param name="stepName">The name of the step that has an unexpected type.</param>
    /// <param name="actualType">The actual type of the step that was encountered.</param>
    /// <param name="expectedType">The expected type of the step that was anticipated.</param>
    internal UnexpectedStepTypeException(string stepName, Type actualType, Type expectedType)
        : base($"Step '{stepName}' has unexpected type '{actualType.FullName}', expected '{expectedType.FullName}'.")
    {
        ActualType = actualType;
        ExpectedType = expectedType;
    }
}
