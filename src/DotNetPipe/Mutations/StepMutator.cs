namespace K1vs.DotNetPipe.Mutations;

/// <summary>
/// Represents a mutator for a step in the pipeline.
/// This record holds the name of the mutator, its priority, and a function that modifies a step delegate.
/// </summary>
/// <typeparam name="TStepDelegate">The type of the step delegate being mutated.</typeparam
/// <param name="Name">The name of the mutator.</param
/// <param name="Priority">The priority of the mutator, which determines its order in the mutation process.</param
/// <param name="Mutator">A function that takes a step delegate and returns a modified step delegate.</param>
public record StepMutator<TStepDelegate>(string Name, int Priority, Func<TStepDelegate, TStepDelegate> Mutator);
