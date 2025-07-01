namespace K1vs.DotNetPipe;

/// <summary>
/// Provides methods to create pipelines and spaces.
/// Pipelines are used to process data in a structured manner, allowing for the addition of steps
/// to the pipeline that can transform or handle the data in various ways.
/// Pipelines behavior can be modified using mutators, which can change the way steps are executed or how data is processed.
/// </summary>
public static class Pipelines
{
    /// <summary>
    /// Creates a new pipeline with the specified name.
    /// The pipeline is created within a new space, which is a container for pipelines and their steps.
    /// </summary>
    /// <typeparam name="TInput">The type of the input data for the pipeline.</typeparam>
    /// <param name="name">The name of the pipeline.</param>
    /// <returns>A new pipeline entry.</returns>
    public static Universal.PipelineEntry<TInput> CreatePipeline<TInput>(string name)
    {
        var space = new Universal.Space();
        var pipeline = space.CreatePipeline<TInput>(name);
        return pipeline;
    }

    /// <summary>
    /// Creates a new pipeline with the specified name.
    /// The pipeline is created within a new space, which is a container for pipelines and their steps.
    /// </summary>
    /// <typeparam name="TInput">The type of the input data for the pipeline.</typeparam>
    /// <param name="name">The name of the pipeline.</param>
    /// <returns>A new pipeline entry.</returns>
    public static Cancellable.PipelineEntry<TInput> CreateCancellablePipeline<TInput>(string name)
    {
        var space = new Cancellable.Space();
        var pipeline = space.CreatePipeline<TInput>(name);
        return pipeline;
    }

    /// <summary>
    /// Creates a new async pipeline with the specified name.
    /// The pipeline is created within a new space, which is a container for pipelines and their steps.
    /// </summary>
    /// <typeparam name="TInput">The type of the input data for the pipeline.</typeparam>
    /// <param name="name">The name of the pipeline.</param>
    /// <returns>A new async pipeline entry.</returns>
    public static Async.PipelineEntry<TInput> CreateAsyncPipeline<TInput>(string name)
    {
        var space = new Async.Space();
        var pipeline = space.CreatePipeline<TInput>(name);
        return pipeline;
    }

    /// <summary>
    /// Creates a new async cancellable pipeline with the specified name.
    /// The pipeline is created within a new space, which is a container for pipelines and their steps.
    /// </summary>
    /// <typeparam name="TInput">The type of the input data for the pipeline.</typeparam>
    /// <param name="name">The name of the pipeline.</param>
    /// <returns>A new async cancellable pipeline entry.</returns>
    public static AsyncCancellable.PipelineEntry<TInput> CreateAsyncCancellablePipeline<TInput>(string name)
    {
        var space = new AsyncCancellable.Space();
        var pipeline = space.CreatePipeline<TInput>(name);
        return pipeline;
    }

    /// <summary>
    /// Creates a new sync pipeline with the specified name.
    /// The pipeline is created within a new space, which is a container for pipelines and their steps.
    /// </summary>
    /// <typeparam name="TInput">The type of the input data for the pipeline.</typeparam>
    /// <param name="name">The name of the pipeline.</param>
    /// <returns>A new pipeline entry.</returns>
    public static Sync.PipelineEntry<TInput> CreateSyncPipeline<TInput>(string name)
    {
        var space = new Sync.Space();
        var pipeline = space.CreatePipeline<TInput>(name);
        return pipeline;
    }

    /// <summary>
    /// Creates a space for pipelines.
    /// A space is a container for pipelines and their steps, allowing for organized management of multiple pipelines.
    /// Each space can contain multiple pipelines, and each pipeline can have multiple steps.
    /// </summary>
    /// <returns>A new space instance.</returns>
    public static Universal.Space CreateSpace()
    {
        return new Universal.Space();
    }

    /// <summary>
    /// Creates a space for pipelines.
    /// A space is a container for pipelines and their steps, allowing for organized management of multiple pipelines.
    /// Each space can contain multiple pipelines, and each pipeline can have multiple steps.
    /// </summary>
    /// <returns>A new space instance.</returns>
    public static Cancellable.Space CreateCancellableSpace()
    {
        return new Cancellable.Space();
    }

    /// <summary>
    /// Creates a space for async pipelines.
    /// A space is a container for pipelines and their steps, allowing for organized management of multiple pipelines.
    /// Each space can contain multiple pipelines, and each pipeline can have multiple steps.
    /// </summary>
    /// <returns>A new async space instance.</returns>
    public static Async.Space CreateAsyncSpace()
    {
        return new Async.Space();
    }

    /// <summary>
    /// Creates a space for async cancellable pipelines.
    /// A space is a container for pipelines and their steps, allowing for organized management of multiple pipelines.
    /// Each space can contain multiple pipelines, and each pipeline can have multiple steps.
    /// </summary>
    /// <returns>A new async cancellable space instance.</returns>
    public static AsyncCancellable.Space CreateAsyncCancellableSpace()
    {
        return new AsyncCancellable.Space();
    }

    /// <summary>
    /// Creates a space for sync pipelines.
    /// A space is a container for pipelines and their steps, allowing for organized management of multiple pipelines.
    /// Each space can contain multiple pipelines, and each pipeline can have multiple steps.
    /// </summary>
    /// <returns>A new sync space instance.</returns>
    public static Sync.Space CreateSyncSpace()
    {
        return new Sync.Space();
    }
}