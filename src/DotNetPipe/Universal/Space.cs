using K1vs.DotNetPipe.Universal.Steps;
using K1vs.DotNetPipe.Universal.Steps.HandlerSteps;
using K1vs.DotNetPipe.Universal.Steps.IfElseSteps;
using K1vs.DotNetPipe.Universal.Steps.IfSteps;
using K1vs.DotNetPipe.Universal.Steps.LinearSteps;
using K1vs.DotNetPipe.Universal.Steps.ForkSteps;
using K1vs.DotNetPipe.Universal.Steps.MultiForkSteps;
using K1vs.DotNetPipe.Universal.Steps.SwitchSteps;
using System.Collections.Immutable;

namespace K1vs.DotNetPipe.Universal;

public class Space
{
    private readonly Dictionary<StepName, Step> _steps = [];

    private readonly Dictionary<string, IPipeline> _pipelines = [];

    public IReadOnlyDictionary<StepName, Step> Steps => _steps.AsReadOnly();

    public IReadOnlyDictionary<string, IPipeline> Pipelines => _pipelines.AsReadOnly();

    public PipelineEntry<TInput> CreatePipeline<TInput>(string name)
    {
        var builder = new PipelineBuilder(this, name);
        return new PipelineEntry<TInput>(builder);
    }

    public IPipeline? GetPipeline(string name)
    {
        return _pipelines.GetValueOrDefault(name);
    }

    public IPipeline? GetRequiredPipeline(string name)
    {
        if (_pipelines.TryGetValue(name, out var pipeline))
        {
            return pipeline;
        }
        throw new PipelineNotFoundException(name);
    }

    public Pipeline<TInput>? GetPipeline<TInput>(string name)
    {
        if (_pipelines.TryGetValue(name, out var pipeline) && pipeline is Pipeline<TInput> closedPipeline)
        {
            return closedPipeline;
        }
        return null;
    }

    public Pipeline<TInput> GetRequiredPipeline<TInput>(string name)
    {
        if (!_pipelines.TryGetValue(name, out var pipeline))
        {
            throw new PipelineNotFoundException(name);
        }
        if (pipeline is not Pipeline<TInput> typedPipeline)
        {
            throw new UnexpectedPipelineTypeException(name, pipeline.GetType(), typeof(Pipeline<TInput>));
        }
        return typedPipeline;
    }

    public OpenPipeline<TInput, TNextInput>? GetOpenPipeline<TInput, TNextInput>(string name)
    {
        if (_pipelines.TryGetValue(name, out var pipeline) && pipeline is OpenPipeline<TInput, TNextInput> openPipeline)
        {
            return openPipeline;
        }
        return null;
    }

    public OpenPipeline<TInput, TNextInput> GetRequiredOpenPipeline<TInput, TNextInput>(string name)
    {
        if (!_pipelines.TryGetValue(name, out var pipeline))
        {
            throw new PipelineNotFoundException(name);
        }
        if (pipeline is not OpenPipeline<TInput, TNextInput> openPipeline)
        {
            throw new UnexpectedPipelineTypeException(name, pipeline.GetType(), typeof(OpenPipeline<TInput, TNextInput>));
        }
        return openPipeline;
    }

    public Step? GetStep(string pipeline, string name)
    {
        return _steps.GetValueOrDefault(new StepName(name, pipeline));
    }

    public Step GetRequiredStep(string pipeline, string name)
    {
        if (!_steps.TryGetValue(new StepName(name, pipeline), out var step))
        {
            throw new StepNotFoundException(name);
        }
        return step;
    }

    public LinearStep<TRootStepInput, TInput, TNextInput>? GetLinearStep<TRootStepInput, TInput, TNextInput>(string pipeline, string name)
    {
        if(_steps.TryGetValue(new StepName(name, pipeline), out var step) && step is LinearStep<TRootStepInput, TInput, TNextInput> pipelineStep)
        {
            return pipelineStep;
        }
        return null;
    }

    public LinearStep<TRootStepInput, TInput, TNextInput> GetRequiredLinearStep<TRootStepInput, TInput, TNextInput>(string pipeline, string name)
    {
        if (!_steps.TryGetValue(new StepName(name, pipeline), out var step))
        {
            throw new StepNotFoundException(name);
        }
        if (step is not LinearStep<TRootStepInput, TInput, TNextInput> pipelineStep)
        {
            throw new UnexpectedStepTypeException(name, step.GetType(), typeof(LinearStep<TRootStepInput, TInput, TNextInput>));
        }
        return pipelineStep;
    }

    public HandlerStep<TRootStepInput, TInput>? GetHandlerStep<TRootStepInput, TInput>(string pipeline, string name)
    {
        if(_steps.TryGetValue(new StepName(name, pipeline), out var step) && step is HandlerStep<TRootStepInput, TInput> pipelineTerminator)
        {
            return pipelineTerminator;
        }
        return null;
    }

    public HandlerStep<TRootStepInput, TInput> GetRequiredHandlerStep<TRootStepInput, TInput>(string pipeline, string name)
    {
        if (!_steps.TryGetValue(new StepName(name, pipeline), out var step))
        {
            throw new StepNotFoundException(name);
        }
        if (step is not HandlerStep<TRootStepInput, TInput> pipelineTerminator)
        {
            throw new UnexpectedStepTypeException(name, step.GetType(), typeof(HandlerStep<TRootStepInput, TInput>));
        }
        return pipelineTerminator;
    }

    public IfStep<TRootStepInput, TNextInput, TIfInput, TNextInput>? GetIfStep<TRootStepInput, TInput, TNextInput, TIfInput>(string pipeline, string name)
    {
        if(_steps.TryGetValue(new StepName(name, pipeline), out var step) && step is IfStep<TRootStepInput, TNextInput, TIfInput, TNextInput> pipelineIfStep)
        {
            return pipelineIfStep;
        }
        return null;
    }

    public IfStep<TRootStepInput, TNextInput, TIfInput, TNextInput> GetRequiredIfStep<TRootStepInput, TInput, TNextInput, TIfInput>(string pipeline, string name)
    {
        if (!_steps.TryGetValue(new StepName(name, pipeline), out var step))
        {
            throw new StepNotFoundException(name);
        }
        if (step is not IfStep<TRootStepInput, TNextInput, TIfInput, TNextInput> pipelineIfStep)
        {
            throw new UnexpectedStepTypeException(name, step.GetType(), typeof(IfStep<TRootStepInput, TNextInput, TIfInput, TNextInput>));
        }
        return pipelineIfStep;
    }

    public IfElseStep<TRootStepInput, TNextInput, TIfInput, TElseInput, TNextInput>? GetIfElseStep<TRootStepInput, TInput, TNextInput, TIfInput, TElseInput>(string pipeline, string name)
    {
        if(_steps.TryGetValue(new StepName(name, pipeline), out var step) && step is IfElseStep<TRootStepInput, TNextInput, TIfInput, TElseInput, TNextInput> pipelineIfElseStep)
        {
            return pipelineIfElseStep;
        }
        return null;
    }

    public IfElseStep<TRootStepInput, TNextInput, TIfInput, TElseInput, TNextInput> GetRequiredIfElseStep<TRootStepInput, TInput, TNextInput, TIfInput, TElseInput>(string pipeline, string name)
    {
        if (!_steps.TryGetValue(new StepName(name, pipeline), out var step))
        {
            throw new StepNotFoundException(name);
        }
        if (step is not IfElseStep<TRootStepInput, TNextInput, TIfInput, TElseInput, TNextInput> pipelineIfElseStep)
        {
            throw new UnexpectedStepTypeException(name, step.GetType(), typeof(IfElseStep<TInput, TNextInput, TIfInput, TElseInput, TNextInput>));
        }
        return pipelineIfElseStep;
    }

    public ForkStep<TRootStepInput, TInput, TBranchAInput, TBranchBInput>? GetForkStep<TRootStepInput, TInput, TBranchAInput, TBranchBInput>(string pipeline, string name)
    {
        if (_steps.TryGetValue(new StepName(name, pipeline), out var step) && step is ForkStep<TRootStepInput, TInput, TBranchAInput, TBranchBInput> forkStep)
        {
            return forkStep;
        }
        return null;
    }

    public ForkStep<TRootStepInput, TInput, TBranchAInput, TBranchBInput> GetRequiredForkStep<TRootStepInput, TInput, TBranchAInput, TBranchBInput>(string pipeline, string name)
    {
        if (!_steps.TryGetValue(new StepName(name, pipeline), out var step))
        {
            throw new StepNotFoundException(name);
        }
        if (step is not ForkStep<TRootStepInput, TInput, TBranchAInput, TBranchBInput> forkStep)
        {
            throw new UnexpectedStepTypeException(name, step.GetType(), typeof(ForkStep<TRootStepInput, TInput, TBranchAInput, TBranchBInput>));
        }
        return forkStep;
    }

    public MultiForkStep<TRootStepInput, TInput, TBranchesInput, TDefaultInput>? GetMultiForkStep<TRootStepInput, TInput, TBranchesInput, TDefaultInput>(string pipeline, string name)
    {
        if (_steps.TryGetValue(new StepName(name, pipeline), out var step) && step is MultiForkStep<TRootStepInput, TInput, TBranchesInput, TDefaultInput> multiForkStep)
        {
            return multiForkStep;
        }
        return null;
    }

    public MultiForkStep<TRootStepInput, TInput, TBranchesInput, TDefaultInput> GetRequiredMultiForkStep<TRootStepInput, TInput, TBranchesInput, TDefaultInput>(string pipeline, string name)
    {
        if (!_steps.TryGetValue(new StepName(name, pipeline), out var step))
        {
            throw new StepNotFoundException(name);
        }
        if (step is not MultiForkStep<TRootStepInput, TInput, TBranchesInput, TDefaultInput> multiForkStep)
        {
            throw new UnexpectedStepTypeException(name, step.GetType(), typeof(MultiForkStep<TRootStepInput, TInput, TBranchesInput, TDefaultInput>));
        }
        return multiForkStep;
    }

    public SwitchStep<TRootStepInput, TInput, TCaseInput, TDefaultInput, TNextStepInput>? GetSwitchStep<TRootStepInput, TInput, TCaseInput, TDefaultInput, TNextStepInput>(string pipeline, string name)
    {
        if (_steps.TryGetValue(new StepName(name, pipeline), out var step) && step is SwitchStep<TRootStepInput, TInput, TCaseInput, TDefaultInput, TNextStepInput> switchStep)
        {
            return switchStep;
        }
        return null;
    }

    public SwitchStep<TRootStepInput, TInput, TCaseInput, TDefaultInput, TNextStepInput> GetRequiredSwitchStep<TRootStepInput, TInput, TCaseInput, TDefaultInput, TNextStepInput>(string pipeline, string name)
    {
        if (!_steps.TryGetValue(new StepName(name, pipeline), out var step))
        {
            throw new StepNotFoundException(name);
        }
        if (step is not SwitchStep<TRootStepInput, TInput, TCaseInput, TDefaultInput, TNextStepInput> switchStep)
        {
            throw new UnexpectedStepTypeException(name, step.GetType(), typeof(SwitchStep<TRootStepInput, TInput, TCaseInput, TDefaultInput, TNextStepInput>));
        }
        return switchStep;
    }

    internal void AddPipeline(IPipeline pipeline)
    {
        if (!_pipelines.TryAdd(pipeline.Name, pipeline))
        {
            throw new PipelineWithNameAlreadyExistsException(pipeline.Name);
        }
    }

    internal void AddStep(Step step)
    {
        if (!_steps.TryAdd(step.Name, step))
        {
            throw new StepWithNameAlreadyExistsException(step.Name.ToString());
        }
    }
}
