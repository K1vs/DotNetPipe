namespace K1vs.DotNetPipe.Mutations;

public record StepMutator<TStepDelegate>(string Name, int Priority, Func<TStepDelegate, TStepDelegate> Mutator);
