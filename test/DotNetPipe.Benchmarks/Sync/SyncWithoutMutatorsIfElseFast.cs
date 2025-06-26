using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DotNetPipe.Benchmarks.Sync.TestPipelines.WithoutMutatorsIfElseFast;

namespace DotNetPipe.Benchmarks.Sync;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.Declared, MethodOrderPolicy.Declared)]
public class SyncWithoutMutatorsIfElseFast
{
    private readonly NotVirtualClass _classHandler;

    private readonly VirtualClass _virtualClassHandler;

    private readonly DotNetPipePipeline _pipelineHandler;

    public SyncWithoutMutatorsIfElseFast()
    {
        _classHandler = new NotVirtualClass();
        _pipelineHandler = new DotNetPipePipeline();
        _virtualClassHandler = new VirtualClass();
    }

    [Params(" 123.45 ", " 678 ", " 90.12 ", "24", "some random text")]
    public string Input { get; set; }

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
