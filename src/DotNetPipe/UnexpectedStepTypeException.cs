namespace K1vs.DotNetPipe.Universal;

public class UnexpectedStepTypeException : Exception
{
    public Type ActualType { get; }
    public Type ExpectedType { get; }

    public UnexpectedStepTypeException(string stepName, Type actualType, Type expectedType)
        : base($"Step '{stepName}' has unexpected type '{actualType.FullName}', expected '{expectedType.FullName}'.")
    {
        ActualType = actualType;
        ExpectedType = expectedType;
    }
}
