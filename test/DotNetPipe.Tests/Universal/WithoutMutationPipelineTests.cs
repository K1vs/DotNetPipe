using NSubstitute;

namespace K1vs.DotNetPipe.Tests.Universal;

public class WithoutMutationPipelineTests
{
    [Theory]
    [InlineData(-4)]
    [InlineData(0)]
    [InlineData(2)]
    public async Task BuildAndRunPipeline_WhenOneHandlerStep_ShouldRun(int value)
    {
        // Arrange
        var handler = Substitute.For<Func<int, ValueTask>>();
        handler.Invoke(Arg.Is(value)).Returns(ValueTask.CompletedTask);
        var pipeline = Pipelines.Create<int>("TestPipeline")
            .StartWithHandler("TestHandler", async (input) => await handler(input))
            .BuildPipeline().Compile();
        // Act
        await pipeline(value);
        // Assert
        await handler.Received().Invoke(Arg.Is(value));
    }

    [Theory]
    [InlineData(-4, 5, 1)]
    [InlineData(0, 10, 10)]
    [InlineData(2, 3, 5)]
    public async Task BuildAndRunPipeline_WhenLinearStepThenHandlerStep_ShouldRun(int inputValue, int constantToAdd, int expectedHandlerInput)
    {
        // Arrange
        var handler = Substitute.For<Func<int, ValueTask>>();
        handler.Invoke(Arg.Is(expectedHandlerInput)).Returns(ValueTask.CompletedTask);

        var pipeline = Pipelines.Create<int>("TestTwoStepPipeline")
            .StartWithLinear<int>("AddConstant", async (input, next) =>
            {
                var result = input + constantToAdd;
                await next(result);
            })
            .HandleWith("TestHandler", async (input) => await handler(input))
            .BuildPipeline().Compile();

        // Act
        await pipeline(inputValue);

        // Assert
        await handler.Received().Invoke(Arg.Is(expectedHandlerInput));
    }

    [Theory]
    [InlineData(2, 3, 2, 10)]  // (2 + 3) * 2 = 10
    [InlineData(0, 5, 3, 15)]  // (0 + 5) * 3 = 15
    [InlineData(-1, 4, 2, 6)]  // (-1 + 4) * 2 = 6
    [InlineData(10, -5, 4, 20)] // (10 + (-5)) * 4 = 20
    public async Task BuildAndRunPipeline_WhenTwoLinearStepsThenHandlerStep_ShouldRun(int inputValue, int constantToAdd, int multiplier, int expectedHandlerInput)
    {
        // Arrange
        var handler = Substitute.For<Func<int, ValueTask>>();
        handler.Invoke(Arg.Is(expectedHandlerInput)).Returns(ValueTask.CompletedTask);

        var pipeline = Pipelines.Create<int>("TestThreeStepPipeline")
            .StartWithLinear<int>("AddConstant", async (input, next) =>
            {
                var result = input + constantToAdd;
                await next(result);
            })
            .ThenLinear<int>("MultiplyByCoefficient", async (input, next) =>
            {
                var result = input * multiplier;
                await next(result);
            })
            .HandleWith("TestHandler", async (input) => await handler(input))
            .BuildPipeline().Compile();

        // Act
        await pipeline(inputValue);

        // Assert
        await handler.Received().Invoke(Arg.Is(expectedHandlerInput));
    }

    [Theory]
    [InlineData("5", 3, 8)]    // "5" -> 5, 5 + 3 = 8
    [InlineData("10", -2, 8)]  // "10" -> 10, 10 + (-2) = 8
    [InlineData("0", 5, 5)]    // "0" -> 0, 0 + 5 = 5
    [InlineData("abc", 3, 0)]  // "abc" -> doesn't parse, handler not called
    [InlineData("", 10, 0)]    // "" -> doesn't parse, handler not called
    public async Task BuildAndRunPipeline_WhenStringParseAndAddConstant_ShouldCallHandlerOnlyOnSuccessfulParse(string inputValue, int constantToAdd, int expectedCallCount)
    {
        // Arrange
        var handler = Substitute.For<Func<int, ValueTask>>();
        handler.Invoke(Arg.Any<int>()).Returns(ValueTask.CompletedTask);

        var pipeline = Pipelines.Create<string>("TestStringParsingPipeline")
            .StartWithLinear<int>("ParseString", async (input, next) =>
            {
                if (int.TryParse(input, out var parsed))
                {
                    await next(parsed);
                }
                // If doesn't parse - don't call next
            })
            .ThenLinear<int>("AddConstant", async (input, next) =>
            {
                var result = input + constantToAdd;
                await next(result);
            })
            .HandleWith("TestHandler", async (input) => await handler(input))
            .BuildPipeline().Compile();

        // Act
        await pipeline(inputValue);

        // Assert
        if (expectedCallCount > 0)
        {
            await handler.Received().Invoke(Arg.Is(expectedCallCount));
        }
        else
        {
            await handler.DidNotReceive().Invoke(Arg.Any<int>());
        }
    }

    [Theory]
    [InlineData("  5  ", 3, 8)]     // "  5  " -> trim -> 5 (int) -> 5 + 3 = 8
    [InlineData(" 10 ", -2, 8)]     // " 10 " -> trim -> 10 (int) -> 10 + (-2) = 8
    [InlineData("3.7", 5, 9)]       // "3.7" -> trim -> 3.7 (float) -> round to 4 -> 4 + 5 = 9
    [InlineData(" 2.3 ", 1, 3)]     // " 2.3 " -> trim -> 2.3 (float) -> round to 2 -> 2 + 1 = 3
    [InlineData("5.5", 2, 8)]       // "5.5" -> trim -> 5.5 (float) -> round to 6 -> 6 + 2 = 8
    public async Task BuildAndRunPipeline_WhenIfStepHandlesIntAndFloat_ShouldProcessCorrectly(string inputValue, int constantToAdd, int expectedResult)
    {
        // Arrange
        var handler = Substitute.For<Func<int, ValueTask>>();
        handler.Invoke(Arg.Is(expectedResult)).Returns(ValueTask.CompletedTask);

        var pipeline = Pipelines.Create<string>("TestIfStepPipeline")
            .StartWithLinear<string>("TrimString", async (input, next) =>
            {
                var trimmed = input.Trim();
                await next(trimmed);
            })
            .ThenIf<string, int>("CheckIntOrFloat", async (input, conditionalNext, next) =>
            {
                // Try to parse as int first
                if (int.TryParse(input, out var intValue))
                {
                    // If it's an int, continue with main pipeline
                    await next(intValue);
                }
                else
                {
                    // If not an int, go to conditional pipeline (for float parsing)
                    await conditionalNext(input);
                }
            }, space => space.CreatePipeline<string>("FloatProcessing")
                .StartWithLinear<double>("ParseFloat", async (input, next) =>
                {
                    if (double.TryParse(input, out var floatValue))
                    {
                        await next(floatValue);
                    }
                })
                .ThenLinear<int>("RoundToInt", async (input, next) =>
                {
                    var rounded = (int)Math.Round(input);
                    await next(rounded);
                })
                .BuildOpenPipeline())
            .ThenLinear<int>("AddConstant", async (input, next) =>
            {
                var result = input + constantToAdd;
                await next(result);
            })
            .HandleWith("TestHandler", async (input) => await handler(input))
            .BuildPipeline().Compile();

        // Act
        await pipeline(inputValue);

        // Assert
        await handler.Received().Invoke(Arg.Is(expectedResult));
    }

    [Theory]
    [InlineData("  5  ", 3, 2, 13)]    // "  5  " -> trim -> 5 (int) -> false branch -> 5 * 2 = 10 -> 10 + 3 = 13
    [InlineData(" 10 ", -2, 4, 38)]    // " 10 " -> trim -> 10 (int) -> false branch -> 10 * 4 = 40 -> 40 + (-2) = 38
    [InlineData("3.7", 5, 3, 9)]       // "3.7" -> trim -> 3.7 (float) -> true branch -> round to 4 -> 4 + 5 = 9
    [InlineData(" 2.3 ", 1, 5, 3)]     // " 2.3 " -> trim -> 2.3 (float) -> true branch -> round to 2 -> 2 + 1 = 3
    [InlineData("5.5", 2, 7, 8)]       // "5.5" -> trim -> 5.5 (float) -> true branch -> round to 6 -> 6 + 2 = 8
    public async Task BuildAndRunPipeline_WhenIfElseStepHandlesIntFloatOrDefault_ShouldProcessCorrectly(string inputValue, int constantToAdd, int multiplier, int expectedResult)
    {
        // Arrange
        var handler = Substitute.For<Func<int, ValueTask>>();
        handler.Invoke(Arg.Is(expectedResult)).Returns(ValueTask.CompletedTask);

        var pipeline = Pipelines.Create<string>("TestIfElseStepPipeline")
            .StartWithLinear<string>("TrimString", async (input, next) =>
            {
                var trimmed = input.Trim();
                await next(trimmed);
            })
            .ThenIfElse<string, int, int>("CheckIntOrFloat", async (input, trueNext, falseNext) =>
            {
                // Try to parse as int first
                if (int.TryParse(input, out var intValue))
                {
                    // If it's an int, we actually want to bypass both branches and continue directly
                    // But since IfElse requires going through one of the branches, we'll use false branch for int
                    await falseNext(intValue);
                }
                else
                {
                    // If not an int, go to true branch (for float parsing)
                    await trueNext(input);
                }
            },
            // True branch - float processing
            space => space.CreatePipeline<string>("FloatProcessing")
                .StartWithLinear<double>("ParseFloat", async (input, next) =>
                {
                    if (double.TryParse(input, out var floatValue))
                    {
                        await next(floatValue);
                    }
                })
                .ThenLinear<int>("RoundToInt", async (input, next) =>
                {
                    var rounded = (int)Math.Round(input);
                    await next(rounded);
                })
                .BuildOpenPipeline(),
            // False branch - multiply by multiplier
            space => space.CreatePipeline<int>("IntOrDefaultProcessing")
                .StartWithLinear<int>("ParseIntOrDefault", async (input, next) =>
                {
                    // If we got here, it means we parsed as int in the false branch
                    // So we just pass it through
                    await next(input * multiplier);
                })
                .BuildOpenPipeline())
            .ThenLinear<int>("AddConstant", async (input, next) =>
            {
                var result = input + constantToAdd;
                await next(result);
            })
            .HandleWith("TestHandler", async (input) => await handler(input))
            .BuildPipeline().Compile();

        // Act
        await pipeline(inputValue);

        // Assert
        await handler.Received().Invoke(Arg.Is(expectedResult));
    }


}
