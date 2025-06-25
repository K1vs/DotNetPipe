namespace K1vs.DotNetPipe.Exceptions;

/// <summary>
/// Exception thrown when a step with the specified name is not found.
/// This is typically used when trying to access or manipulate a step that does not exist.
/// </summary>
public class StepNotFoundException : KeyNotFoundException
{
    /// <summary>
    /// Gets the name of the step that was not found.
    /// </summary>
    public string StepName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StepNotFoundException"/> class with a specified step name.
    /// </summary>
    /// <param name="name">The name of the step that was not found.</param>
    internal StepNotFoundException(string name)
        : base($"Step with name '{name}' not found.")
    {
        StepName = name;
    }
}
