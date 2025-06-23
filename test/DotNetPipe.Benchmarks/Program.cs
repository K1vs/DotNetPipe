using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace DotNetPipe.Benchmarks;

public static class Program
{
    public static void Main(string[] args)
    {
        IConfig config = System.Diagnostics.Debugger.IsAttached ? new DebugInProcessConfig() : DefaultConfig.Instance;
        BenchmarkSwitcher
            .FromAssembly(typeof(Program).Assembly)
            .Run(args, config);
    }
}
