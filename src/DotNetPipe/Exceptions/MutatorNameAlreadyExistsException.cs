namespace K1vs.DotNetPipe.Exceptions;

/// <summary>
/// Exception thrown when a mutator with the specified name already exists.
/// This is typically used to prevent duplicate mutator names in the system.
/// </summary>
public class MutatorNameAlreadyExistsException : MutatorAlreadyExistsException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MutatorNameAlreadyExistsException"/> class with a specified mutator name.
    /// </summary>
    /// <param name="newName">The name of the new mutator being added.</param>
    /// <param name="newPriority">The priority of the new mutator being added.</param>
    /// <param name="existingName">The name of the mutator that already exists.</param>
    /// <param name="existingPriority">The priority of the mutator that already exists.</param>
    internal MutatorNameAlreadyExistsException(string newName, int newPriority, string existingName, int existingPriority)
        : base(newName, newPriority, existingName, existingPriority,
        $"Mutator with name '{existingName}' and priority {existingPriority} already exists and new mutator with same name {newName} can't be added.")
    {
    }
}
