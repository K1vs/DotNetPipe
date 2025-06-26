using K1vs.DotNetPipe.Mutations;

namespace K1vs.DotNetPipe.Sync.Steps.LinearSteps;

/// <summary>
/// Represents a linear step in a pipeline that processes input and produces output.
/// This step can be used to create a sequence of operations where each step processes the output of the previous step.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input for the entry step.</typeparam>
/// <typeparam name="TInput">The type of the input for this step.</typeparam>
/// <typeparam name="TNextInput">The type of the input for the next step after this step.</typeparam>
public abstract class LinearStep<TEntryStepInput, TInput, TNextInput> : ReducedPipeStep<TEntryStepInput, TNextInput>
{
    private readonly Pipe<TInput, TNextInput> _originalPipe;

    /// <summary>
    /// Mutators for the step pipe.
    /// These allow for modifying the behavior of the step dynamically.
    /// </summary>
    public StepMutators<Pipe<TInput, TNextInput>> Mutators { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LinearStep{TEntryStepInput, TInput, TNextInput}"/> class.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="originalPipe"></param>
    /// <param name="builder"></param>
    private protected LinearStep(string name, Pipe<TInput, TNextInput> originalPipe, PipelineBuilder builder)
        : base(name, builder)
    {
        _originalPipe = originalPipe;
        Mutators = new StepMutators<Pipe<TInput, TNextInput>>();
    }

    /// <summary>
    /// Builds the pipe based on the original pipe and any mutations defined in the Mutators collection.
    /// </summary>
    /// <returns>Mutated pipe that processes the input and produces the output.</returns>
    private protected Pipe<TInput, TNextInput> CreateStepPipe() => Mutators.MutateDelegate(_originalPipe);
}
