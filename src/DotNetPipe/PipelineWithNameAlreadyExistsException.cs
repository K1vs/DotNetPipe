namespace K1vs.DotNetPipe;

public class PipelineWithNameAlreadyExistsException : Exception
{
    public PipelineWithNameAlreadyExistsException(string name)
        : base($"Pipeline with name '{name}' already exists.")
    {
    }
}