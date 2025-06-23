namespace K1vs.DotNetPipe.Mutations;

public class MutatorsConfigurator<TSpace>
{
    private readonly List<Action<TSpace>> _mutatorsConfigurator = [];

    public MutatorsConfigurator<TSpace> Configure(Action<TSpace> mutator)
    {
        _mutatorsConfigurator.Add(mutator);
        return this;
    }

    internal void RegisterMutators(TSpace space)
    {
        foreach (var mutator in _mutatorsConfigurator)
        {
            mutator(space);
        }
    }
}