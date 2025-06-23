using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DotNetPipe.Benchmarks.TestPipelines.WithoutMutatorsIfTwoStepsFast;

namespace DotNetPipe.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.Declared, MethodOrderPolicy.Declared)]
public class WithoutMutatorsIfTwoStepsFast
{
    private readonly NotVirtualClass _classHandler;

    private readonly VirtualClass _virtualClassHandler;

    private readonly DotNetPipePipeline _pipelineHandler;

    public WithoutMutatorsIfTwoStepsFast()
    {
        _classHandler = new NotVirtualClass();
        _pipelineHandler = new DotNetPipePipeline();
        _virtualClassHandler = new VirtualClass();
    }

    [Params(-25, 0, 100)]
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
