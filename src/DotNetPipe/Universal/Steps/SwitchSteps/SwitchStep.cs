namespace K1vs.DotNetPipe.Universal.Steps.SwitchSteps;

using K1vs.DotNetPipe.Mutations;
using K1vs.DotNetPipe.Universal;

public abstract class SwitchStep<TRootStepInput, TInput, TCaseInput, TDefaultInput, TNextStepInput> : ReducedPipeStep<TRootStepInput, TNextStepInput>
{
    private readonly SwitchSelector<TInput, TCaseInput, TDefaultInput> _selector;

    public StepMutators<SwitchSelector<TInput, TCaseInput, TDefaultInput>> _mutators;

    public IReadOnlyDictionary<string, OpenPipeline<TCaseInput, TNextStepInput>> CasesPipelines { get; }

    public OpenPipeline<TDefaultInput, TNextStepInput> DefaultPipeline { get; }

    private protected SwitchStep(string name,
        SwitchSelector<TInput, TCaseInput, TDefaultInput> selector,
        Func<Space, IReadOnlyDictionary<string, OpenPipeline<TCaseInput, TNextStepInput>>> caseBuilder,
        OpenPipeline<TDefaultInput, TNextStepInput> defaultPipeline,
        PipelineBuilder builder)
        : base(name, builder)
    {
        _selector = selector;
        _mutators = new StepMutators<SwitchSelector<TInput, TCaseInput, TDefaultInput>>();
        CasesPipelines = caseBuilder(builder.Space);
        DefaultPipeline = defaultPipeline;
    }

    private protected SwitchSelector<TInput, TCaseInput, TDefaultInput> CreateStepSelector() => _mutators.MutateDelegate(_selector);
}