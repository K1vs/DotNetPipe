namespace K1vs.DotNetPipe;

internal class PipelineWithNameAlreadyExistsException : Exception
{
    public PipelineWithNameAlreadyExistsException(string name)
        : base($"Pipeline with name '{name}' already exists.")
    {
    }
}