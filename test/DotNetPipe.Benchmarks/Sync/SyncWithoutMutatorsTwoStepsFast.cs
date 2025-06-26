using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DotNetPipe.Benchmarks.Sync.TestPipelines.WithoutMutatorsTwoStepsFast;

namespace DotNetPipe.Benchmarks.Sync;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.Declared, MethodOrderPolicy.Declared)]
public class SyncWithoutMutatorsTwoStepsFast
{
    private readonly NotVirtualClass _classHandler;

    private readonly VirtualClass _virtualClassHandler;

    private readonly DotNetPipePipeline _pipelineHandler;

    public SyncWithoutMutatorsTwoStepsFast()
    {
        _classHandler = new NotVirtualClass();
        _pipelineHandler = new DotNetPipePipeline();
        _virtualClassHandler = new VirtualClass();
    }

    [Params(-23, 0, 12)]
    public int Input { get; set; }

    [Benchmark]
    public void Pipeline()
    {
        _pipelineHandler.Run(Input);
    }

    [Benchmark]
    public void Class()
    {
        _classHandler.Run(Input);
    }

    [Benchmark]
    public void VirtualClass()
    {
        _virtualClassHandler.Run(Input);
    }
}
