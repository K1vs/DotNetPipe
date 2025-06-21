namespace K1vs.DotNetPipe;

public static class Pipelines
{
    public static Universal.PipelineEntry<TInput> Create<TInput>(string name)
    {
        var space = new Universal.Space();
        var pipeline = space.CreatePipeline<TInput>(name);
        return pipeline;
    }
}