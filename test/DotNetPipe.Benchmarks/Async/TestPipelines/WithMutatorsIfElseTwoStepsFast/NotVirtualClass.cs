using System.Threading.Tasks;
using BenchmarkDotNet.Engines;

namespace DotNetPipe.Benchmarks.Async.TestPipelines.WithMutatorsIfElseTwoStepsFast;

internal class NotVirtualClass
{
    private const int _multiplier = -2;

    private readonly Consumer _consumer = new();

    public async Task Run(int input)
    {
        var result = input;

        // CheckNegative with mutation: input <= 0 (instead of < 0)
        if (input <= 0)
        {
            // MultiplyByConstant with mutation: add 1 before multiplying
            result = (input + 1) * _multiplier;
        }

        // ConsumeNumber handler (no mutation)
        _consumer.Consume(result);
        await Task.CompletedTask;
    }
}
