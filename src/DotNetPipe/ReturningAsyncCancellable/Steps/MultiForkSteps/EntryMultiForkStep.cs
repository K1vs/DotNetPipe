namespace K1vs.DotNetPipe.ReturningAsyncCancellable.Steps.MultiForkSteps;

/// <summary>
/// Represents the entry multi-fork step of a pipeline.
/// Splits the pipeline into multiple named branches with a default branch, starting from the entry step.
/// </summary>
/// <typeparam name="TEntryStepInput">The type of the input of the pipeline entry step.</typeparam>
/// <typeparam name="TEntryStepResult">The type of the result of the pipeline entry step.</typeparam>
/// <typeparam name="TBranchesInput">The type of the input of the branch pipelines.</typeparam>
/// <typeparam name="TBranchesResult">The type of the result of the branch pipelines.</typeparam>
/// <typeparam name="TDefaultInput">The type of the input of the default pipeline.</typeparam>
/// <typeparam name="TDefaultResult">The type of the result of the default pipeline.</typeparam>
public sealed class EntryMultiForkStep<TEntryStepInput, TEntryStepResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> :
    MultiForkStep<TEntryStepInput, TEntryStepResult, TEntryStepInput, TEntryStepResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult>
{
    internal EntryMultiForkStep(string name,
        MultiForkSelector<TEntryStepInput, TEntryStepResult, TBranchesInput, TBranchesResult, TDefaultInput, TDefaultResult> selector,
        Func<Space, IReadOnlyDictionary<string, Pipeline<TBranchesInput, TBranchesResult>>> branchesBuilder,
        Func<Space, Pipeline<TDefaultInput, TDefaultResult>> defaultBuilder,
        PipelineBuilder builder)
        : base(name, selector, branchesBuilder, defaultBuilder, builder)
    {
    }

    /// <summary>
    /// Builds a closed pipeline starting from the entry step and ending at this multi-fork step.
    /// </summary>
    /// <returns>The closed pipeline instance.</returns>
    public override Pipeline<TEntryStepInput, TEntryStepResult> BuildPipeline()
    {
        if (Builder.EntryStep is null)
        {
            throw new InvalidOperationException("Entry step is not set");
        }
        var pipeline = new Pipeline<TEntryStepInput, TEntryStepResult>(Builder.Name, Builder.EntryStep, this, BuildHandler);
        Builder.Space.AddPipeline(pipeline);
        return pipeline;
    }

    /// <summary>
    /// Builds the handler for this step by compiling all branches and the default pipeline.
    /// </summary>
    internal override Handler<TEntryStepInput, TEntryStepResult> BuildHandler()
    {
        var selector = CreateStepSelector();
        var branches = BranchesPipelines.ToDictionary(kv => kv.Key, kv => kv.Value.Compile());
        var defaultHandler = DefaultPipeline.Compile();
        Task<TEntryStepResult> Handler(TEntryStepInput input, CancellationToken ct) => selector(input, branches, defaultHandler, ct);
        return Handler;
    }
}



