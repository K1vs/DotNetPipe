namespace K1vs.DotNetPipe;

public class StepWithNameAlreadyExistsException : Exception
{
    public string Name { get; }
    public StepWithNameAlreadyExistsException(string name) : base($"Step with name {name} already exists")
    {
        Name = name;
    }
}
