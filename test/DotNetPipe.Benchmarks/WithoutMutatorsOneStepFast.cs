using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using DotNetPipe.Benchmarks.TestPipelines.WithoutMutatorsOneStepFast;

namespace DotNetPipe.Benchmarks;

[MemoryDiagnoser]
public class WithoutMutatorsOneStepFast
{
    private readonly NotVirtualClass _classHandler;

    private readonly VirtualClass _virtualClassHandler = new();

    private readonly DotNetPipePipeline _pipelineHandler;

    public WithoutMutatorsOneStepFast()
    {
        _classHandler = new NotVirtualClass();
        _pipelineHandler = new DotNetPipePipeline();
        _virtualClassHandler = new VirtualClass();
    }

    [Params(-23, 0, 12)]
    public int Input { get; set; }

    [Benchmark]
    public async ValueTask Pipeline()
    {
        await _pipelineHandler.Run(Input);
    }

    [Benchmark]
    public async ValueTask Class()
    {
        await _classHandler.Run(Input);
    }

    [Benchmark]
    public async ValueTask VirtualClass()
    {
        await _virtualClassHandler.Run(Input);
    }
}
