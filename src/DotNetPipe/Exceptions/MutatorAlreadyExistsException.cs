namespace K1vs.DotNetPipe.Exceptions;

/// <summary>
/// Exception thrown when a mutator with the specified priority already exists.
/// </summary>
public abstract class MutatorAlreadyExistsException : Exception
{
    /// <summary>
    /// Gets the name of the mutator that already exists.
    /// </summary>
    public string ExistingName { get; }

    /// <summary>
    /// Gets the priority of the mutator that already exists.
    /// </summary>
    public int ExistingPriority { get; }

    /// <summary>
    /// Gets the name of the mutator that is being added.
    /// </summary>
    public string NewName { get; }

    /// <summary>
    /// Gets the priority of the mutator that is being added.
    /// </summary>
    public int NewPriority { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MutatorAlreadyExistsException"/> class with a specified mutator name and priority.
    /// </summary>
    /// <param name="newName">The name of the mutator that is being added.</param>
    /// <param name="newPriority">The priority of the mutator that is being added.</param>
    /// <param name="existingName">The name of the mutator that already exists.</param>
    /// <param name="existingPriority">The priority of the mutator that already exists.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    private protected MutatorAlreadyExistsException(string newName, int newPriority, string existingName, int existingPriority, string message)
        : base(message)
    {
        NewName = newName;
        NewPriority = newPriority;
        ExistingName = existingName;
        ExistingPriority = existingPriority;
    }
}
