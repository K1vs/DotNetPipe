using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DotNetPipe.Benchmarks.Universal.TestPipelines.WithoutMutatorsTwoStepsFast;

namespace DotNetPipe.Benchmarks.Universal;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.Declared, MethodOrderPolicy.Declared)]
public class WithoutMutatorsTwoStepsFast
{
    private readonly NotVirtualClass _classHandler;

    private readonly VirtualClass _virtualClassHandler;

    private readonly DotNetPipePipeline _pipelineHandler;

    public WithoutMutatorsTwoStepsFast()
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
