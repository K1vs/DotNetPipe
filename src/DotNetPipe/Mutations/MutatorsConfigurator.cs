namespace K1vs.DotNetPipe.Mutations;

/// <summary>
/// Configures mutators for a specific space.
/// This class allows you to incrementally define mutators that will be applied to the space.
/// </summary>
/// typeparam name="TSpace">The type of the space to which mutators will be applied.</typeparam>
public class MutatorsConfigurator<TSpace>
{
    private readonly List<Action<TSpace>> _mutatorsConfigurator = [];

    /// <summary>
    /// Configures a mutator for the specified space.
    /// This method allows you to add a mutator that will be applied to the space when registered.
    /// The mutator is defined as an action that takes the space as a parameter.
    /// This method can be called multiple times to add multiple mutators to the configurator.
    /// The mutators will be registered in the order they are added, allowing for a flexible and customizable configuration of the space's behavior.
    /// </summary>
    /// <param name="mutator">An action that defines the mutator(s) to be applied to the space.</param>
    /// <returns>The current instance of the <see cref="MutatorsConfigurator{TSpace}"/> class, allowing for method chaining.</returns>
    public MutatorsConfigurator<TSpace> Configure(Action<TSpace> mutator)
    {
        _mutatorsConfigurator.Add(mutator);
        return this;
    }

    /// <summary>
    /// Registers all configured mutators for the specified space.
    /// </summary>
    /// <param name="space">The space to which the mutators will be applied.</param>
    internal void RegisterMutators(TSpace space)
    {
        foreach (var mutator in _mutatorsConfigurator)
        {
            mutator(space);
        }
    }
}