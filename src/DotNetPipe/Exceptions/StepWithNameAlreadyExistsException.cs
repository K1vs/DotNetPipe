namespace K1vs.DotNetPipe.Exceptions;

/// <summary>
/// Exception thrown when a step with the specified name already exists.
/// This is typically used to prevent duplicate step names in the system.
/// </summary>
public class StepWithNameAlreadyExistsException : Exception
{
    /// <summary>
    /// Gets the name of the step that already exists.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StepWithNameAlreadyExistsException"/> class with a specified step name.
    /// </summary>
    /// <param name="name">The name of the step that already exists.</param>
    internal StepWithNameAlreadyExistsException(string name) : base($"Step with name {name} already exists")
    {
        Name = name;
    }
}
