namespace K1vs.DotNetPipe.Universal.Steps.LinearSteps;

public sealed class PipeLinearStep<TRootStepInput, TInput, TNextInput>: LinearStep<TRootStepInput, TInput, TNextInput>
{
    public ReducedPipeStep<TRootStepInput, TInput> PreviousStep { get; }

    internal PipeLinearStep(ReducedPipeStep<TRootStepInput, TInput> previousStep,
        string name,
        Pipe<TInput, TNextInput> @delegate,
        PipelineBuilder builder) : base(name, @delegate, builder)
    {
        PreviousStep = previousStep;
    }

    internal override Handler<TRootStepInput> BuildHandler(Handler<TNextInput> handler)
    {
        var pipe = CreateStepPipe();
        var resultHandler = PreviousStep.BuildHandler((input) => pipe(input, handler));
        return resultHandler;
    }
}
