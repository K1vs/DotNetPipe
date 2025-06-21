using K1vs.DotNetPipe.Universal.Steps;
using K1vs.DotNetPipe.Universal.Steps.HandlerSteps;
using K1vs.DotNetPipe.Universal.Steps.IfElseSteps;
using K1vs.DotNetPipe.Universal.Steps.IfSteps;
using K1vs.DotNetPipe.Universal.Steps.LinearSteps;
using System.Collections.Immutable;

namespace K1vs.DotNetPipe.Universal;

public class Space
{
    private readonly Dictionary<string, Step> _steps = [];

    public IReadOnlyDictionary<string, Step> Steps => _steps.AsReadOnly();

    public PipelineEntry<TInput> CreatePipeline<TInput>(string name)
    {
        var builder = new PipelineBuilder(this, name);
        return new PipelineEntry<TInput>(builder);
    }

    public Step? GetStep(string name)
    {
        return _steps.GetValueOrDefault(name);
    }

    public LinearStep<TRootStepInput, TInput, TNextInput>? GetPipeStep<TRootStepInput, TInput, TNextInput>(string name)
    {
        if(_steps.TryGetValue(name, out var step) && step is LinearStep<TRootStepInput, TInput, TNextInput> pipelineStep)
        {
            return pipelineStep;
        }
        return null;
    }

    public HandlerStep<TInput, TInput>? GetTerminatorStep<TInput>(string name)
    {
        if(_steps.TryGetValue(name, out var step) && step is HandlerStep<TInput, TInput> pipelineTerminator)
        {
            return pipelineTerminator;
        }
        return null;
    }

    public IfStep<TInput, TNextInput, TIfInput, TNextInput>? GetIfStep<TInput, TNextInput, TIfInput>(string name)
    {
        if(_steps.TryGetValue(name, out var step) && step is IfStep<TInput, TNextInput, TIfInput, TNextInput> pipelineIfStep)
        {
            return pipelineIfStep;
        }
        return null;
    }

    public IfElseStep<TInput, TNextInput, TIfInput, TElseInput, TNextInput>? GetIfElseStep<TInput, TNextInput, TIfInput, TElseInput>(string name)
    {
        if(_steps.TryGetValue(name, out var step) && step is IfElseStep<TInput, TNextInput, TIfInput, TElseInput, TNextInput> pipelineIfElseStep)
        {
            return pipelineIfElseStep;
        }
        return null;
    }

    internal void AddStep(Step step)
    {
        if(!_steps.TryAdd(step.Name, step))
        {
            throw new StepWithNameAlreadyExistsException(step.Name);
        }
    }
}
