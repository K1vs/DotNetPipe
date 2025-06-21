using K1vs.DotNetPipe.Universal.Steps.ForkSteps;
using K1vs.DotNetPipe.Universal.Steps.HandlerSteps;
using K1vs.DotNetPipe.Universal.Steps.IfElseSteps;
using K1vs.DotNetPipe.Universal.Steps.IfSteps;
using K1vs.DotNetPipe.Universal.Steps.LinearSteps;
using K1vs.DotNetPipe.Universal.Steps.MultiForkSteps;
using K1vs.DotNetPipe.Universal.Steps.SwitchSteps;

namespace K1vs.DotNetPipe.Universal;

public class PipelineEntry<TPipelineInput>
{
    public PipelineBuilder Builder { get; }

    public PipelineEntry(PipelineBuilder builder)
    {
        Builder = builder;
    }

    public EntryLinearStep<TPipelineInput, TNextInput> StartWithLinear<TNextInput>(string name, Pipe<TPipelineInput, TNextInput> next)
    {
        var step = new EntryLinearStep<TPipelineInput, TNextInput>(name, next, Builder);
        Builder.EntryStep = step;
        return step;
    }

    public EntryIfStep<TPipelineInput, TIfInput, TNextInput> StartWithIf<TIfInput, TNextInput>(string name,
        IfSelector<TPipelineInput, TIfInput, TNextInput> selector,
        Func<Space, OpenPipeline<TIfInput, TNextInput>> trueBuilder)
    {
        var step = new EntryIfStep<TPipelineInput, TIfInput, TNextInput>(name, selector, trueBuilder, Builder);
        Builder.EntryStep = step;
        return step;
    }

    public EntryIfElseStep<TPipelineInput, TIfInput, TElseInput, TNextInput> StartWithIfElse<TIfInput, TElseInput, TNextInput>(string name,
        IfElseSelector<TPipelineInput, TIfInput, TElseInput> selector,
        Func<Space, OpenPipeline<TIfInput, TNextInput>> trueBuilder,
        Func<Space, OpenPipeline<TElseInput, TNextInput>> falseBuilder)
    {
        var step = new EntryIfElseStep<TPipelineInput, TIfInput, TElseInput, TNextInput>(name, selector, trueBuilder, falseBuilder, Builder);
        Builder.EntryStep = step;
        return step;
    }

    public EntrySwitchStep<TPipelineInput, TCaseInput, TDefaultInput, TNextInput> StartWithSwitch<TCaseInput, TDefaultInput, TNextInput>(string name,
        SwitchSelector<TPipelineInput, TCaseInput, TDefaultInput> selector,
        Func<Space, IReadOnlyDictionary<string, OpenPipeline<TCaseInput, TNextInput>>> caseBuilder,
        OpenPipeline<TDefaultInput, TNextInput> defaultBuilder)
    {
        var step = new EntrySwitchStep<TPipelineInput, TCaseInput, TDefaultInput, TNextInput>(name, selector, caseBuilder, defaultBuilder, Builder);
        Builder.EntryStep = step;
        return step;
    }

    public EntryForkStep<TPipelineInput, TBranchAInput, TBranchBInput> StartWithFork<TBranchAInput, TBranchBInput>(string name,
        ForkSelector<TPipelineInput, TBranchAInput, TBranchBInput> selector,
        Func<Space, Pipeline<TBranchAInput>> branchABuilder,
        Func<Space, Pipeline<TBranchBInput>> branchBBuilder)
    {
        var step = new EntryForkStep<TPipelineInput, TBranchAInput, TBranchBInput>(name, selector, branchABuilder, branchBBuilder, Builder);
        Builder.EntryStep = step;
        return step;
    }

    public EntryMultiForkStep<TPipelineInput, TBranchesInput, TDefaultInput> StartWithMultiFork<TBranchesInput, TDefaultInput>(string name,
        MultiForkSelector<TPipelineInput, TBranchesInput, TDefaultInput> selector,
        Func<Space, IReadOnlyDictionary<string, Pipeline<TBranchesInput>>> branchesBuilder,
        Func<Space, Pipeline<TDefaultInput>> defaultBuilder)
    {
        var step = new EntryMultiForkStep<TPipelineInput, TBranchesInput, TDefaultInput>(name, selector, branchesBuilder, defaultBuilder, Builder);
        Builder.EntryStep = step;
        return step;
    }

    public EntryHandlerStep<TPipelineInput> StartWithHandler(string name, Handler<TPipelineInput> handler)
    {
        var step = new EntryHandlerStep<TPipelineInput>(name, handler, Builder);
        Builder.EntryStep = step;
        return step;
    }
}
