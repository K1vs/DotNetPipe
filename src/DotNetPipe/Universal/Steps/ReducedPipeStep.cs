using K1vs.DotNetPipe.Universal.Steps.ForkSteps;
using K1vs.DotNetPipe.Universal.Steps.HandlerSteps;
using K1vs.DotNetPipe.Universal.Steps.IfElseSteps;
using K1vs.DotNetPipe.Universal.Steps.IfSteps;
using K1vs.DotNetPipe.Universal.Steps.LinearSteps;
using K1vs.DotNetPipe.Universal.Steps.MultiForkSteps;
using K1vs.DotNetPipe.Universal.Steps.SwitchSteps;

namespace K1vs.DotNetPipe.Universal.Steps;

public abstract class ReducedPipeStep<TRootStepInput, TNextInput>: Step
{
    protected ReducedPipeStep(string name, PipelineBuilder builder)
        : base(name, builder)
    {
    }

    public OpenPipeline<TRootStepInput, TNextInput> BuildOpenPipeline()
    {
        if(Builder.EntryStep is null)
        {
            throw new InvalidOperationException("Entry step is not set");
        }
        var openPipeline = new OpenPipeline<TRootStepInput, TNextInput>(Builder.Name, Builder.EntryStep, this);
        Builder.Space.AddPipeline(openPipeline);
        return openPipeline;
    }

    public LinearStep<TRootStepInput, TNextInput, TNextStepNextInput> ThenLinear<TNextStepNextInput>(string name,
        Pipe<TNextInput, TNextStepNextInput> next)
    {
        return new PipeLinearStep<TRootStepInput, TNextInput, TNextStepNextInput>(this, name, next, Builder);
    }

    public IfStep<TRootStepInput, TNextInput, TIfInput, TNextStepNextInput> ThenIf<TIfInput, TNextStepNextInput>(string name,
        IfSelector<TNextInput, TIfInput, TNextStepNextInput> selector,
        Func<Space, OpenPipeline<TIfInput, TNextStepNextInput>> trueBuilder)
    {
        return new PipeIfStep<TRootStepInput, TNextInput, TIfInput, TNextStepNextInput>(this, name, selector, trueBuilder, Builder);
    }

    public IfElseStep<TRootStepInput, TNextInput, TIfInput, TElseInput, TNextStepNextInput> ThenIfElse<TIfInput, TElseInput, TNextStepNextInput>(string name,
        IfElseSelector<TNextInput, TIfInput, TElseInput> selector,
        Func<Space, OpenPipeline<TIfInput, TNextStepNextInput>> trueBuilder,
        Func<Space, OpenPipeline<TElseInput, TNextStepNextInput>> falseBuilder)
    {
        return new PipeIfElseStep<TRootStepInput, TNextInput, TIfInput, TElseInput, TNextStepNextInput>(this, name, selector, trueBuilder, falseBuilder, Builder);
    }

    public PipeSwitchStep<TRootStepInput, TNextInput, TCaseInput, TDefaultInput, TNextStepNextInput> ThenSwitch<TCaseInput, TDefaultInput, TNextStepNextInput>(string name,
        SwitchSelector<TNextInput, TCaseInput, TDefaultInput> selector,
        Func<Space, IReadOnlyDictionary<string, OpenPipeline<TCaseInput, TNextStepNextInput>>> caseBuilder,
        OpenPipeline<TDefaultInput, TNextStepNextInput> defaultBuilder)
    {
        return new PipeSwitchStep<TRootStepInput, TNextInput, TCaseInput, TDefaultInput, TNextStepNextInput>(this, name, selector, caseBuilder, defaultBuilder, Builder);
    }

    public PipeForkStep<TRootStepInput, TNextInput, TBranchAInput, TBranchBInput> ThenFork<TBranchAInput, TBranchBInput>(string name,
        ForkSelector<TNextInput, TBranchAInput, TBranchBInput> selector,
        Func<Space, Pipeline<TBranchAInput>> branchABuilder,
        Func<Space, Pipeline<TBranchBInput>> branchBBuilder)
    {
        return new PipeForkStep<TRootStepInput, TNextInput, TBranchAInput, TBranchBInput>(this, name, selector, branchABuilder, branchBBuilder, Builder);
    }

    public PipeMultiForkStep<TRootStepInput, TNextInput, TBranchesInput, TDefaultInput> ThenMultiFork<TBranchesInput, TDefaultInput>(string name,
        MultiForkSelector<TNextInput, TBranchesInput, TDefaultInput> selector,
        Func<Space, IReadOnlyDictionary<string, Pipeline<TBranchesInput>>> branchesBuilder,
        Func<Space, Pipeline<TDefaultInput>> defaultBuilder)
    {
        return new PipeMultiForkStep<TRootStepInput, TNextInput, TBranchesInput, TDefaultInput>(this, name, selector, branchesBuilder, defaultBuilder, Builder);
    }

    public PipeHandlerStep<TRootStepInput, TNextInput> HandleWith(string name, Handler<TNextInput> @delegate)
    {
        return new PipeHandlerStep<TRootStepInput, TNextInput>(this, name, @delegate, Builder);
    }

    internal abstract Handler<TRootStepInput> BuildHandler(Handler<TNextInput> handler);
}
