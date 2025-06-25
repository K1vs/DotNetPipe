namespace K1vs.DotNetPipe.Exceptions;

/// <summary>
/// Exception thrown when a mutator with the specified priority already exists.
/// This is typically used to prevent undefined mutators order in the system.
/// </summary>
public class MutatorPriorityAlreadyExistsException : MutatorAlreadyExistsException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MutatorPriorityAlreadyExistsException"/> class with a specified mutator priority.
    /// </summary>
    /// <param name="name">The name of the mutator that already exists.</param>
    /// <param name="priority">The priority of the mutator that already exists.</param>
    internal MutatorPriorityAlreadyExistsException(string newName, int newPriority, string existingName, int existingPriority)
        : base(newName, newPriority, existingName, existingPriority,
        $"Mutator with priority {existingPriority} and name '{existingName}' already exists and new mutator with same priority {newPriority} can't be added.")
    {
    }
}