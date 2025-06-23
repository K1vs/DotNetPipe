namespace K1vs.DotNetPipe;

public class PipelineNotFoundException : KeyNotFoundException
{
    public string PipelineName { get; }

    public PipelineNotFoundException(string name)
        : base($"Pipeline with name '{name}' not found.")
    {
        PipelineName = name;
    }
}