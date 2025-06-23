namespace K1vs.DotNetPipe;

public class StepNotFoundException : KeyNotFoundException
{
    public string StepName { get; }

    public StepNotFoundException(string name)
        : base($"Step with name '{name}' not found.")
    {
        StepName = name;
    }
}
