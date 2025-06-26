using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DotNetPipe.Benchmarks.Sync.TestPipelines.WithMutatorsForkFast;

namespace DotNetPipe.Benchmarks.Sync;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.Declared, MethodOrderPolicy.Declared)]
public class SyncWithMutatorsForkFast
{
    private readonly NotVirtualClass _classHandler;
    private readonly VirtualClassWithMutation _virtualClassHandler;
    private readonly DotNetPipePipeline _pipelineHandler;

    public SyncWithMutatorsForkFast()
    {
        _classHandler = new NotVirtualClass();
        _pipelineHandler = new DotNetPipePipeline();
        _virtualClassHandler = new VirtualClassWithMutation();
    }

    [Params("123", "  456  ", "abc123def", "hello", "", "!@#")]
    public string Input { get; set; }

    [Benchmark]
    public async ValueTask Pipeline()
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
