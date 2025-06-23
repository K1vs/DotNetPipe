namespace K1vs.DotNetPipe.Universal.Steps;

public record StepName(string Name, string PipelineName)
{
    public override string ToString()
    {
        return $"{PipelineName}_{Name}";
    }
}