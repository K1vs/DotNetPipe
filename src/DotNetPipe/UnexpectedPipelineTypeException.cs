namespace K1vs.DotNetPipe.Universal;

public class UnexpectedPipelineTypeException : Exception
{
    public Type ActualType { get; }
    public Type ExpectedType { get; }

    public UnexpectedPipelineTypeException(string pipelineName, Type actualType, Type expectedType)
        : base($"Pipeline '{pipelineName}' has unexpected type '{actualType.FullName}', expected '{expectedType.FullName}'.")
    {
        ActualType = actualType;
        ExpectedType = expectedType;
    }
}