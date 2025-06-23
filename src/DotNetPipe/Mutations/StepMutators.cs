using System.Collections.Immutable;

namespace K1vs.DotNetPipe.Mutations;

public class StepMutators<TStepDelegate>
{
    private readonly SortedDictionary<int, StepMutator<TStepDelegate>> _mutators = [];

    public IReadOnlyList<TMutator> GetMutators<TMutator>()
    {
        return [.. _mutators.Values.Cast<TMutator>()];
    }

    public StepMutator<TStepDelegate>? GetMutator(int priority)
    {
        return _mutators.TryGetValue(priority, out var mutator) ? mutator : null;
    }

    public StepMutator<TStepDelegate>? GetMutator(string name)
    {
        return _mutators.Values.FirstOrDefault(m => m.Name == name);
    }

    public StepMutator<TStepDelegate> AddMutator(StepMutator<TStepDelegate> mutator, AddingMode addingMode)
    {
        if(addingMode == AddingMode.ExactPlace)
        {
            _mutators.Add(mutator.Priority, mutator);
            return mutator;
        }
        else
        {
            int priority = mutator.Priority;
            while (_mutators.TryGetValue(priority, out _))
            {
                priority += addingMode == AddingMode.BeforeIfReserved ? -1 : 1;
            }
            var newMutator = new StepMutator<TStepDelegate>(mutator.Name, priority, mutator.Mutator);
            _mutators.Add(priority, newMutator);
            return newMutator;
        }
    }

    public StepMutator<TStepDelegate>? RemoveMutator(int priority)
    {
        return _mutators.Remove(priority, out var mutator) ? mutator : null;
    }

    public StepMutator<TStepDelegate>? ReplaceMutator(int priority, StepMutator<TStepDelegate> mutator)
    {
        if (_mutators.TryGetValue(priority, out var oldMutator))
        {
            _mutators[priority] = mutator;
            return oldMutator;
        }
        return null;
    }

    public StepMutator<TStepDelegate>? AddOrReplaceMutator(StepMutator<TStepDelegate> mutator)
    {
        if (_mutators.TryGetValue(mutator.Priority, out var oldMutator))
        {
            _mutators[mutator.Priority] = mutator;
            return oldMutator;
        }
        _mutators.Add(mutator.Priority, mutator);
        return null;
    }

    public StepMutator<TStepDelegate>? MoveMutator(int priority, int newPriority)
    {
        if (_mutators.Remove(priority, out var mutator))
        {
            _mutators.Add(newPriority, mutator);
            return mutator;
        }
        return null;
    }

    internal TStepDelegate MutateDelegate(TStepDelegate @delegate)
    {
        foreach (var mutator in _mutators.Values)
        {
            @delegate = mutator.Mutator(@delegate);
        }
        return @delegate;
    }
}
