namespace K1vs.DotNetPipe.Mutations;

/// <summary>
/// Specifies the mode for adding mutators to the pipeline step.
/// This enum defines how mutators should be added in relation to existing mutators.
/// </summary>
public enum AddingMode
{
    /// <summary>
    /// Adds the mutator at their priority slot.
    /// If priority slot is already occupied, the exception (<see cref="Exceptions.MutatorAlreadyExistsException"/>) will be thrown.
    /// </summary>
    ExactPlace,
    /// <summary>
    /// Adds the mutator before the first mutator with the specified priority.
    /// If no mutator with the specified priority exists, the mutator will be added at specified priority slot.
    /// </summary>
    BeforeIfReserved,
    /// <summary>
    /// Adds the mutator after the first mutator with the specified priority.
    /// If no mutator with the specified priority exists, the mutator will be added at specified priority slot.
    /// </summary>
    AfterIfReserved,
}
