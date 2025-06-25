using System.Collections.Immutable;
using K1vs.DotNetPipe.Exceptions;

namespace K1vs.DotNetPipe.Mutations;

/// <summary>
/// Represents a collection of mutators for a step in the pipeline.
/// This class allows adding, removing, and retrieving mutators based on their priority or name.
/// </summary>
/// <typeparam name="TStepDelegate">The type of the step delegate being mutated.</typeparam>
public class StepMutators<TStepDelegate>
{
    private readonly SortedDictionary<int, StepMutator<TStepDelegate>> _mutators = [];

    /// <summary>
    /// Gets the list of all mutators in the order of their priority.
    /// </summary>
    /// <typeparam name="TMutator">The type of mutators to retrieve.</typeparam>
    /// <returns>A read-only list of mutators of the specified type.</returns>
    public IReadOnlyList<TMutator> GetMutators<TMutator>()
    {
        return [.. _mutators.Values.Cast<TMutator>()];
    }

    /// <summary>
    /// Get mutator by its priority.
    /// Returns null if no mutator with the specified priority exists.
    /// </summary>
    public StepMutator<TStepDelegate>? GetMutator(int priority)
    {
        return _mutators.TryGetValue(priority, out var mutator) ? mutator : null;
    }

    /// <summary>
    /// Get mutator by its name.
    /// Returns null if no mutator with the specified name exists.
    /// </summary>
    public StepMutator<TStepDelegate>? GetMutator(string name)
    {
        return _mutators.Values.FirstOrDefault(m => m.Name == name);
    }

    /// <summary>
    /// Adds a mutator to the step mutators collection.
    /// This method allows you to add a mutator with a specific priority.
    /// If the priority slot is already occupied, the mutator adding behaves according to the specified adding mode:
    /// - ExactPlace: Adds the mutator at its priority slot, throwing an exception (<see cref="MutatorAlreadyExistsException"/>) if the slot is already occupied.
    /// - BeforeIfReserved: Adds the mutator before the first mutator with the specified priority, or at the specified priority slot if no such mutator exists.
    /// - AfterIfReserved: Adds the mutator after the first mutator with the specified priority, or at the specified priority slot if no such mutator exists.
    /// </summary>
    /// <param name="mutator">The mutator to add.</param>
    /// <param name="addingMode">The mode of adding the mutator, determining its placement in relation to existing mutators.</param>
    /// <returns>The added mutator.</returns>
    /// <exception cref="MutatorPriorityAlreadyExistsException">
    /// Thrown when trying to add a mutator at a priority slot that is already occupied
    /// by another mutator with the same priority.
    /// </exception>
    /// <exception cref="MutatorNameAlreadyExistsException">
    /// Thrown when trying to add a mutator with a name that already exists in the collection.
    /// </exception>
    public StepMutator<TStepDelegate> AddMutator(StepMutator<TStepDelegate> mutator, AddingMode addingMode)
    {
        if (addingMode == AddingMode.ExactPlace)
        {
            if(_mutators.TryGetValue(mutator.Priority, out var existingMutator))
            {
                throw new MutatorPriorityAlreadyExistsException(
                    mutator.Name, mutator.Priority,
                    existingMutator.Name, existingMutator.Priority
                );
            }
            if (_mutators.Values.Any(m => m.Name == mutator.Name))
            {
                throw new MutatorNameAlreadyExistsException(
                    mutator.Name, mutator.Priority,
                    _mutators.Values.First(m => m.Name == mutator.Name).Name,
                    _mutators.Values.First(m => m.Name == mutator.Name).Priority
                );
            }
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

    /// <summary>
    /// Removes a mutator from the step mutators collection by its priority.
    /// Returns the removed mutator if it exists, or null if no mutator with the specified priority exists.
    /// </summary>
    /// <param name="priority">The priority of the mutator to remove.</param>
    /// <returns>The removed mutator, or null if no mutator with the specified priority exists.</returns>
    public StepMutator<TStepDelegate>? RemoveMutator(int priority)
    {
        return _mutators.Remove(priority, out var mutator) ? mutator : null;
    }

    /// <summary>
    /// Replaces an existing mutator with a new one at the specified priority.
    /// If a mutator with the specified priority does not exist, it returns null and does not add the new mutator.
    /// </summary>
    /// <param name="priority">The priority of the mutator to replace.</param>
    /// <param name="mutator">The new mutator to replace the existing one.</param>
    /// <returns>The old mutator if it existed, or null if no mutator with the specified priority exists.</returns>
    public StepMutator<TStepDelegate>? ReplaceMutator(int priority, StepMutator<TStepDelegate> mutator)
    {
        if (_mutators.TryGetValue(priority, out var oldMutator))
        {
            _mutators[priority] = mutator;
            return oldMutator;
        }
        return null;
    }

    /// <summary>
    /// Adds or replaces a mutator in the step mutators collection.
    /// If a mutator with the same priority already exists, it replaces it and returns the old mutator.
    /// If no mutator with the specified priority exists, it adds the new mutator and returns null.
    /// </summary>
    /// <param name="mutator">The mutator to add or replace.</param>
    /// <returns>The old mutator if it existed, or null if no mutator with the specified priority exists.</returns>
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

    /// <summary>
    /// Moves a mutator from one priority to another.
    /// If the mutator with the specified priority exists, it is removed from that priority and added to the new priority.
    /// If the mutator does not exist at the specified priority, it returns null.
    /// </summary>
    /// <param name="priority">The current priority of the mutator to move.</param>
    /// <param name="newPriority">The new priority to which the mutator should be moved.</param>
    /// <returns>The moved mutator if it existed, or null if no mutator with the specified priority exists.</returns>
    public StepMutator<TStepDelegate>? MoveMutator(int priority, int newPriority)
    {
        if (_mutators.Remove(priority, out var mutator))
        {
            _mutators.Add(newPriority, mutator);
            return mutator;
        }
        return null;
    }

    /// <summary>
    /// Applies all mutators to the provided step delegate.
    /// </summary>
    /// <param name="delegate">The step delegate to be mutated.</param>
    /// <returns>The mutated step delegate after applying all mutators.</returns>
    internal TStepDelegate MutateDelegate(TStepDelegate @delegate)
    {
        foreach (var mutator in _mutators.Values)
        {
            @delegate = mutator.Mutator(@delegate);
        }
        return @delegate;
    }
}
