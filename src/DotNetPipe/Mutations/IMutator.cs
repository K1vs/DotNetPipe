namespace K1vs.DotNetPipe.Mutations;

/// <summary>
/// Defines a mutator interface for a specific space.
/// This interface allows defining mutators that can modify the behavior of steps in a space.
/// </summary>
/// <typeparam name="TSpace">The type of the space to which the mutator will be applied.</typeparam>
public interface IMutator<TSpace>
{
    /// <summary>
    /// Describes the mutation to be applied to the specified space.
    /// You can mutate all steps in the space, or just a specific step in single mutator.
    /// </summary>
    /// <param name="space">The space to which the mutation will be applied.</param>
    void Mutate(TSpace space);
}