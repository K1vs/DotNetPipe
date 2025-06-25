using System.Threading.Tasks;

namespace DotNetPipe.Benchmarks.TestPipelines.WithMutatorsIfElseTwoStepsFast;

internal class NotVirtualClass
{
    private const int _multiplier = -2;

    public async ValueTask Run(int input)
    {
        var result = input;

        // CheckNegative with mutation: input <= 0 (instead of < 0)
        if (input <= 0)
        {
            // MultiplyByConstant with mutation: add 1 before multiplying
            result = (input + 1) * _multiplier;
        }

        // ConsumeNumber handler (no mutation)
        await ValueTask.CompletedTask;
    }
}
