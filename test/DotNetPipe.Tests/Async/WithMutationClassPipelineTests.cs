using K1vs.DotNetPipe.Mutations;
using K1vs.DotNetPipe.Async;
using NSubstitute;

namespace K1vs.DotNetPipe.Tests.Async;

public class WithMutationClassPipelineTests
{
    [Theory]
    [InlineData(-4, -3)]
    [InlineData(0, 1)]
    [InlineData(2, 3)]
    public async Task BuildAndRunPipeline_WhenOneHandlerStep_ShouldRun(int value, int expectedValue)
    {
        // Arrange
        var handler = Substitute.For<Func<int, Task>>();
        handler.Invoke(Arg.Is(expectedValue)).Returns(Task.CompletedTask);

        var mutator = new TestHandlerMutator();
        var testPipeline = new TestPipeline([mutator], handler);

        // Act
        await testPipeline.Run(value);

        // Assert
        await handler.Received().Invoke(Arg.Is(expectedValue));
    }

    [Theory]
    [InlineData(-4, 3)]   // ((-4 * 2) + 10) + 1 = (-8 + 10) + 1 = 3
    [InlineData(0, 11)]   // ((0 * 2) + 10) + 1 = (0 + 10) + 1 = 11
    [InlineData(2, 15)]   // ((2 * 2) + 10) + 1 = (4 + 10) + 1 = 15
    public async Task BuildAndRunPipeline_WhenLinearStepThenHandlerStep_ShouldRun(int inputValue, int expectedHandlerInput)
    {
        // Arrange
        var handler = Substitute.For<Func<int, Task>>();
        handler.Invoke(Arg.Is(expectedHandlerInput)).Returns(Task.CompletedTask);

        var linearMutator = new TestLinearMutator();
        var handlerMutator = new TestTwoStepHandlerMutator();
        var testPipeline = new TestTwoStepPipeline([linearMutator, handlerMutator], handler);

        // Act
        await testPipeline.Run(inputValue);

        // Assert
        await handler.Received().Invoke(Arg.Is(expectedHandlerInput));
    }

    [Theory]
    [InlineData(2, 45)]   // Step1: (2 + 5) + 10 = 17, Step2: (17 + 5) * 2 = 44, Handler: 44 + 1 = 45
    [InlineData(0, 41)]   // Step1: (0 + 5) + 10 = 15, Step2: (15 + 5) * 2 = 40, Handler: 40 + 1 = 41
    [InlineData(-1, 39)]  // Step1: (-1 + 5) + 10 = 14, Step2: (14 + 5) * 2 = 38, Handler: 38 + 1 = 39
    public async Task BuildAndRunPipeline_WhenTwoLinearStepsThenHandlerStep_ShouldRun(int inputValue, int expectedHandlerInput)
    {
        // Arrange
        var handler = Substitute.For<Func<int, Task>>();
        handler.Invoke(Arg.Is(expectedHandlerInput)).Returns(Task.CompletedTask);

        var firstLinearMutator = new TestFirstLinearMutator();
        var secondLinearMutator = new TestSecondLinearMutator();
        var handlerMutator = new TestThreeStepHandlerMutator();
        var testPipeline = new TestThreeStepPipeline([firstLinearMutator, secondLinearMutator, handlerMutator], handler);

        // Act
        await testPipeline.Run(inputValue);

        // Assert
        await handler.Received().Invoke(Arg.Is(expectedHandlerInput));
    }

    [Theory]
    [InlineData("5", 11)]     // "5" -> 5 + 2 = 7, 7 + 3 = 10, handler gets 10 + 1 = 11
    [InlineData("10", 16)]    // "10" -> 10 + 2 = 12, 12 + 3 = 15, handler gets 15 + 1 = 16
    [InlineData("0", 6)]      // "0" -> 0 + 2 = 2, 2 + 3 = 5, handler gets 5 + 1 = 6
    [InlineData("abc", 0)]    // "abc" -> doesn't parse, handler not called
    [InlineData("", 0)]       // "" -> doesn't parse, handler not called
    public async Task BuildAndRunPipeline_WhenStringParseAndAddConstant_ShouldCallHandlerOnlyOnSuccessfulParse(string inputValue, int expectedValue)
    {
        // Arrange
        var handler = Substitute.For<Func<int, Task>>();
        handler.Invoke(Arg.Any<int>()).Returns(Task.CompletedTask);

        var parseLinearMutator = new TestStringParseLinearMutator();
        var addConstantLinearMutator = new TestAddConstantLinearMutator();
        var handlerMutator = new TestStringParsingHandlerMutator();
        var testPipeline = new TestStringParsingPipeline([parseLinearMutator, addConstantLinearMutator, handlerMutator], handler);

        // Act
        await testPipeline.Run(inputValue);

        // Assert
        if (expectedValue > 0)
        {
            await handler.Received().Invoke(Arg.Is(expectedValue));
        }
        else
        {
            await handler.DidNotReceive().Invoke(Arg.Any<int>());
        }
    }

    [Theory]
    [InlineData(" 10 ", 12)]   // " 10 " -> trim -> 10 -> int path -> 10 + 2 (AddConstant) = 12
    [InlineData("3.7", 7)]     // "3.7" -> trim -> 3.7 -> float path -> 3.7 + 1 (mutation) = 4.7 -> round to 5 -> 5 + 2 (AddConstant) = 7
    [InlineData(" 2.3 ", 5)]   // " 2.3 " -> trim -> 2.3 -> float path -> 2.3 + 1 (mutation) = 3.3 -> round to 3 -> 3 + 2 (AddConstant) = 5
    [InlineData("5.5", 8)]     // "5.5" -> trim -> 5.5 -> float path -> 5.5 + 1 (mutation) = 6.5 -> round to 6 (banker's rounding) -> 6 + 2 (AddConstant) = 8
    public async Task BuildAndRunPipeline_WhenIfStepHandlesIntAndFloat_ShouldProcessCorrectly(string inputValue, int expectedResult)
    {
        // Arrange
        var handler = Substitute.For<Func<int, Task>>();
        handler.Invoke(Arg.Is(expectedResult)).Returns(Task.CompletedTask);

        var trimMutator = new TestIfStepTrimMutator();
        var ifStepMutator = new TestIfStepMutator();
        var parseFloatMutator = new TestIfStepParseFloatMutator();
        var roundToIntMutator = new TestIfStepRoundToIntMutator();
        var addConstantMutator = new TestIfStepAddConstantMutator();
        var handlerMutator = new TestIfStepHandlerMutator();

        var testPipeline = new TestIfStepPipeline([trimMutator, ifStepMutator, parseFloatMutator, roundToIntMutator, addConstantMutator, handlerMutator], handler);

        // Act
        await testPipeline.Run(inputValue);

        // Assert
        await handler.Received().Invoke(Arg.Is(expectedResult));
    }

    [Theory]
    [InlineData("  5  ", 10)]    // "  5  " -> trim -> 5 (int) -> true branch (float processing) -> parse 5.0 -> +1 = 6.0 -> round to 6 -> 6 + 3 = 9 -> +1 = 10
    [InlineData(" 10 ", 15)]     // " 10 " -> trim -> 10 (int) -> true branch (float processing) -> parse 10.0 -> +1 = 11.0 -> round to 11 -> 11 + 3 = 14 -> +1 = 15
    [InlineData("3.7", 8)]       // "3.7" -> trim -> 3.7 (float) -> false branch (int processing) -> 0 + 2 = 2 -> 2 * 2 = 4 -> 4 + 3 = 7 -> +1 = 8
    [InlineData(" 2.3 ", 8)]     // " 2.3 " -> trim -> 2.3 (float) -> false branch (int processing) -> 0 + 2 = 2 -> 2 * 2 = 4 -> 4 + 3 = 7 -> +1 = 8
    [InlineData("5.5", 8)]       // "5.5" -> trim -> 5.5 (float) -> false branch (int processing) -> 0 + 2 = 2 -> 2 * 2 = 4 -> 4 + 3 = 7 -> +1 = 8
    public async Task BuildAndRunPipeline_WhenIfElseStepHandlesIntFloatOrDefault_ShouldProcessCorrectly(string inputValue, int expectedResult)
    {
        // Arrange
        var handler = Substitute.For<Func<int, Task>>();
        handler.Invoke(Arg.Is(expectedResult)).Returns(Task.CompletedTask);

        var trimMutator = new TestIfElseStepTrimMutator();
        var ifElseStepMutator = new TestIfElseStepMutator();
        var parseFloatMutator = new TestIfElseStepParseFloatMutator();
        var roundToIntMutator = new TestIfElseStepRoundToIntMutator();
        var multiplyByTwoMutator = new TestIfElseStepMultiplyByTwoMutator();
        var addConstantMutator = new TestIfElseStepAddConstantMutator();
        var handlerMutator = new TestIfElseStepHandlerMutator();

        var testPipeline = new TestIfElseStepPipeline([trimMutator, ifElseStepMutator, parseFloatMutator, roundToIntMutator, multiplyByTwoMutator, addConstantMutator, handlerMutator], handler);

        // Act
        await testPipeline.Run(inputValue);

        // Assert
        await handler.Received().Invoke(Arg.Is(expectedResult));
    }

    [Theory]
    [InlineData(" 105 ", 318)]    // >50 -> GreaterThan100 -> *3 = 315 -> +3 = 318
    [InlineData(" 50 ", 55)]      // 0<x<=50 -> BetweenZeroAndHundred -> +2 = 52 -> +3 = 55
    [InlineData(" -5 ", -7)]      // <=0 -> LessThanZero -> *2 = -10 -> +3 = -7
    [InlineData(" 0 ", 3)]        // <=0 -> LessThanZero -> *2 = 0 -> +3 = 3
    [InlineData("abc", 6)]        // not a number -> string length = 3 -> +3 = 6
    [InlineData("hello", 8)]      // not a number -> string length = 5 -> +3 = 8
    [InlineData("", 3)]           // empty string -> length = 0 -> +3 = 3
    public async Task BuildAndRunPipeline_WhenSwitchStepRoutesByNumberRange_ShouldProcessCorrectly(string inputValue, int expectedResult)
    {
        // Arrange
        var handler = Substitute.For<Func<int, Task>>();
        handler.Invoke(Arg.Is(expectedResult)).Returns(Task.CompletedTask);

        var trimMutator = new TestSwitchStepTrimMutator();
        var switchMutator = new TestSwitchStepMutator();
        var multiplyByThreeMutator = new TestSwitchStepMultiplyByThreeMutator();
        var addTwoMutator = new TestSwitchStepAddTwoMutator();
        var multiplyByTwoMutator = new TestSwitchStepMultiplyByTwoMutator();
        var keepZeroMutator = new TestSwitchStepKeepZeroMutator();
        var stringLengthMutator = new TestSwitchStepStringLengthMutator();
        var handlerMutator = new TestSwitchStepHandlerMutator();

        var testPipeline = new TestSwitchStepPipeline(
            [trimMutator, switchMutator, multiplyByThreeMutator, addTwoMutator, multiplyByTwoMutator, keepZeroMutator, stringLengthMutator, handlerMutator],
            handler);

        // Act
        await testPipeline.Run(inputValue);

        // Assert
        await handler.Received().Invoke(Arg.Is(expectedResult));
    }

    [Theory]
    [InlineData("123", 0, "******")]
    [InlineData("  456  ", 0, "******")]
    [InlineData("abc123def", 128, "")]
    [InlineData("hello", 5, "")]
    [InlineData("", 0, "******")]
    [InlineData("!@#", 0, "***!@#***")]
    public async Task BuildAndRunPipeline_WhenForkSplitsByDigitContent_ShouldProcessCorrectly(string inputValue, int expectedIntResult, string? expectedStringResult)
    {
        // Arrange
        var intHandler = Substitute.For<Func<int, Task>>();
        var stringHandler = Substitute.For<Func<string, Task>>();
        intHandler.Invoke(Arg.Any<int>()).Returns(Task.CompletedTask);
        stringHandler.Invoke(Arg.Any<string>()).Returns(Task.CompletedTask);

        var mutators = new IMutator<Space>[]
        {
            new TestForkStepTrimMutator(),
            new TestForkStepMutator(),
            new TestForkStepRemoveNonDigitsMutator(),
            new TestForkStepParseToIntMutator(),
            new TestForkStepIntHandlerMutator(),
            new TestForkStepRemoveDigitsMutator(),
            new TestForkStepAddSpacesMutator(),
            new TestForkStepStringHandlerMutator()
        };

        var pipeline = new TestForkStepPipeline(mutators, intHandler, stringHandler);

        // Act
        await pipeline.Run(inputValue);

        // Assert
        if (expectedStringResult == "")
        {
            // Should call int handler (digit-only string)
            await intHandler.Received().Invoke(Arg.Is(expectedIntResult));
            await stringHandler.DidNotReceive().Invoke(Arg.Any<string>());
        }
        else
        {
            // Should call string handler (non-digit string)
            await stringHandler.Received().Invoke(Arg.Is(expectedStringResult!));
            await intHandler.DidNotReceive().Invoke(Arg.Any<int>());
        }
    }

    [Theory]
    [InlineData("123", 138, "", new char[0])]    // digits only -> DigitBranch -> parse to 123 -> +15 = 138
    [InlineData("  456  ", 471, "", new char[0])]    // digits only (trimmed) -> DigitBranch -> parse to 456 -> +15 = 471
    [InlineData("abc", 0, "", new char[] { 'a', 'b', 'c', '_' })]    // letters only -> SpecialCharBranch -> char array with underscore + unique
    [InlineData("xyz", 0, "", new char[] { 'x', 'y', 'z', '_' })]    // letters only -> SpecialCharBranch -> char array with underscore + unique
    [InlineData("!@#", 0, "", new char[] { '!', '@', '#', '_' })]    // special chars only -> SpecialCharBranch -> char array with underscore + unique
    [InlineData("@@@", 0, "", new char[] { '@', '_' })]    // repeated special chars -> SpecialCharBranch -> unique char array with underscore
    [InlineData("a1b2", 0, "", new char[] { 'a', '1', 'b', '2', '_' })]    // mixed content -> DefaultBranch -> SpecialCharBranch -> char array with underscore + unique
    [InlineData("hello123", 0, "", new char[] { 'h', 'e', 'l', 'o', '1', '2', '3', '_' })]    // mixed content -> DefaultBranch -> SpecialCharBranch -> unique char array with underscore
    [InlineData("12345abc", 0, "", new char[] { '1', '2', '3', '4', '5', 'a', 'b', 'c', '_' })]    // mixed content -> DefaultBranch -> SpecialCharBranch -> unique char array with underscore
    public async Task BuildAndRunPipeline_WhenMultiForkClassifiesStringContent_ShouldProcessCorrectly(
        string inputValue,
        int expectedIntResult,
        string expectedStringResult,
        char[] expectedCharArrayResult)
    {
        // Arrange
        var intHandler = Substitute.For<Func<int, Task>>();
        var stringHandler = Substitute.For<Func<string, Task>>();
        var charArrayHandler = Substitute.For<Func<char[], Task>>();
        intHandler.Invoke(Arg.Any<int>()).Returns(Task.CompletedTask);
        stringHandler.Invoke(Arg.Any<string>()).Returns(Task.CompletedTask);
        charArrayHandler.Invoke(Arg.Any<char[]>()).Returns(Task.CompletedTask);

        var mutators = new IMutator<Space>[]
        {
            new TestMultiForkStepTrimMutator(),
            new TestMultiForkStepMutator(),
            new TestMultiForkStepParseToIntMutator(),
            new TestMultiForkStepAddConstantMutator(),
            new TestMultiForkStepIntHandlerMutator(),
            new TestMultiForkStepAddSpacesMutator(),
            new TestMultiForkStepStringHandlerMutator(),
            new TestMultiForkStepRemoveWhitespaceMutator(),
            new TestMultiForkStepConvertToCharArrayMutator(),
            new TestMultiForkStepRemoveDuplicatesMutator(),
            new TestMultiForkStepCharArrayHandlerMutator(),
            new TestMultiForkStepCountDigitsAndLettersMutator(),
            new TestMultiForkStepCalculateRatioMutator(),
            new TestMultiForkStepDefaultIntHandlerMutator()
        };

        var pipeline = new TestMultiForkStepPipeline(mutators, intHandler, stringHandler, charArrayHandler);

        // Act
        await pipeline.Run(inputValue);

        // Assert
        if (expectedStringResult != "")
        {
            // Should call string handler (letter-only string) - but this won't happen anymore with new mutator
            await stringHandler.Received().Invoke(Arg.Is(expectedStringResult!));
            await intHandler.DidNotReceive().Invoke(Arg.Any<int>());
            await charArrayHandler.DidNotReceive().Invoke(Arg.Any<char[]>());
        }
        else if (expectedCharArrayResult.Length > 0)
        {
            // Should call char array handler (everything except digits-only strings)
            await charArrayHandler.Received().Invoke(Arg.Is<char[]>(arr => arr.SequenceEqual(expectedCharArrayResult)));
            await intHandler.DidNotReceive().Invoke(Arg.Any<int>());
            await stringHandler.DidNotReceive().Invoke(Arg.Any<string>());
        }
        else
        {
            // Should call int handler (digit-only strings)
            await intHandler.Received().Invoke(Arg.Is(expectedIntResult));
            await stringHandler.DidNotReceive().Invoke(Arg.Any<string>());
            await charArrayHandler.DidNotReceive().Invoke(Arg.Any<char[]>());
        }
    }
}

#region mutators

/// <summary>
/// Test mutator implementation that increments input by 1 for single handler step pipeline
/// </summary>
public class TestHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredHandlerStep<int, int>("TestPipeline", "TestHandler");
        var mutator = new StepMutator<Handler<int>>("TestHandlerMutator", 1, (handler) =>
        {
            return async (input) =>
            {
                input += 1;
                await handler(input);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator implementation that increments input by 1 for two-step pipeline handler
/// </summary>
public class TestTwoStepHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredHandlerStep<int, int>("TestTwoStepPipeline", "TestHandler");
        var mutator = new StepMutator<Handler<int>>("TestHandlerMutator", 1, (handler) =>
        {
            return async (input) =>
            {
                input += 1;
                await handler(input);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator implementation that multiplies input by 2 for TestTwoStepPipeline
/// </summary>
public class TestLinearMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<int, int, int>("TestTwoStepPipeline", "AddConstant");
        var mutator = new StepMutator<Pipe<int, int>>("AddConstantMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                // Apply mutation to input first - multiply by 2
                input *= 2;
                await pipe(input, next);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test first linear mutator implementation that adds 5 to the input for TestThreeStepPipeline
/// </summary>
public class TestFirstLinearMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<int, int, int>("TestThreeStepPipeline", "AddConstant");
        var mutator = new StepMutator<Pipe<int, int>>("AddConstantMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                // Apply mutation to input first - add 5
                input += 5;
                await pipe(input, next);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test second linear mutator implementation that multiplies the input by 2 for TestThreeStepPipeline
/// </summary>
public class TestSecondLinearMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<int, int, int>("TestThreeStepPipeline", "MultiplyByCoefficient");
        var mutator = new StepMutator<Pipe<int, int>>("MultiplyByCoefficient", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                // Apply mutation to input first - add 5 before multiplying
                input += 5;
                await pipe(input, next);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator implementation that increments input by 1 for three-step pipeline handler
/// </summary>
public class TestThreeStepHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredHandlerStep<int, int>("TestThreeStepPipeline", "TestHandler");
        var mutator = new StepMutator<Handler<int>>("TestHandlerMutator", 1, (handler) =>
        {
            return async (input) =>
            {
                input += 1;
                await handler(input);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for string parsing step - adds 2 to parsed value
/// </summary>
public class TestStringParseLinearMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, string, int>("TestStringParsingPipeline", "ParseString");
        var mutator = new StepMutator<Pipe<string, int>>("ParseStringMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                // Custom parsing logic with mutation
                if (int.TryParse(input, out var parsed))
                {
                    parsed += 2; // Apply mutation - add 2 to successfully parsed value
                    await next(parsed);
                }
                // If doesn't parse - don't call next
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for add constant step - pass through as-is
/// </summary>
public class TestAddConstantLinearMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, int, int>("TestStringParsingPipeline", "AddConstant");
        var mutator = new StepMutator<Pipe<int, int>>("AddConstantMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                // Apply original add constant logic as-is
                await pipe(input, next);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for string parsing handler step - adds 1
/// </summary>
public class TestStringParsingHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredHandlerStep<string, int>("TestStringParsingPipeline", "TestHandler");
        var mutator = new StepMutator<Handler<int>>("TestHandlerMutator", 1, (handler) =>
        {
            return async (input) =>
            {
                input += 1;
                await handler(input);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for trim string step - pass through as-is
/// </summary>
public class TestIfStepTrimMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, string, string>("TestIfStepPipeline", "TrimString");
        var mutator = new StepMutator<Pipe<string, string>>("TrimStringMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for if step selector - treat all strings as potential floats
/// </summary>
public class TestIfStepMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredIfStep<string, string, string, int>("TestIfStepPipeline", "CheckIntOrFloat");
        var mutator = new StepMutator<IfSelector<string, string, int>>("CheckIntOrFloatMutator", 1, (selector) =>
        {
            return async (input, ifNext, next) =>
            {
                // Apply original logic without modifications
                await selector(input, ifNext, next);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for parse float step - pass through as-is
/// </summary>
public class TestIfStepParseFloatMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, string, double>("FloatProcessing", "ParseFloat");
        var mutator = new StepMutator<Pipe<string, double>>("ParseFloatMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for round to int step - add 1 to input before rounding
/// </summary>
public class TestIfStepRoundToIntMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, double, int>("FloatProcessing", "RoundToInt");
        var mutator = new StepMutator<Pipe<double, int>>("RoundToIntMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                // Apply mutation - add 1 to input before rounding
                input += 1;
                await pipe(input, next);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for add constant step - pass through as-is
/// </summary>
public class TestIfStepAddConstantMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, int, int>("TestIfStepPipeline", "AddConstant");
        var mutator = new StepMutator<Pipe<int, int>>("AddConstantMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for handler step - pass through as-is
/// </summary>
public class TestIfStepHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredHandlerStep<string, int>("TestIfStepPipeline", "TestHandler");
        var mutator = new StepMutator<Handler<int>>("TestHandlerMutator", 1, (handler) =>
        {
            return async (input) =>
            {
                await handler(input);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for trim string step - pass through as-is
/// </summary>
public class TestIfElseStepTrimMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, string, string>("TestIfElseStepPipeline", "TrimString");
        var mutator = new StepMutator<Pipe<string, string>>("TrimStringMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for IfElse step selector - swap branches
/// </summary>
public class TestIfElseStepMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredIfElseStep<string, string, string, int, int>("TestIfElseStepPipeline", "CheckIntOrFloat");
        var mutator = new StepMutator<IfElseSelector<string, string, int>>("CheckIntOrFloatMutator", 1, (selector) =>
        {
            return async (input, ifNext, elseNext) =>
            {
                // Modified logic: swap the branches - what was false becomes true and vice versa
                if (int.TryParse(input, out var intValue))
                {
                    // If it's an int, go to true branch (was false branch before)
                    // Now int values go to float processing pipeline
                    await ifNext(input);
                }
                else
                {
                    // If not an int, go to false branch (was true branch before)
                    // Use 0 as default for non-parseable strings
                    await elseNext(0);
                }
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for parse float step - pass through as-is
/// </summary>
public class TestIfElseStepParseFloatMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, string, double>("FloatProcessing", "ParseFloat");
        var mutator = new StepMutator<Pipe<string, double>>("ParseFloatMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for round to int step - add 1 to input before rounding
/// </summary>
public class TestIfElseStepRoundToIntMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, double, int>("FloatProcessing", "RoundToInt");
        var mutator = new StepMutator<Pipe<double, int>>("RoundToIntMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                // Add 1 to the input before rounding
                input += 1;
                await pipe(input, next);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for multiply by two step - add 2 to input before multiplying
/// </summary>
public class TestIfElseStepMultiplyByTwoMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<int, int, int>("IntProcessing", "MultiplyByTwo");
        var mutator = new StepMutator<Pipe<int, int>>("MultiplyByTwoMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                // Add 2 to the input before multiplying
                input += 2;
                await pipe(input, next);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for add constant step - pass through as-is
/// </summary>
public class TestIfElseStepAddConstantMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, int, int>("TestIfElseStepPipeline", "AddConstant");
        var mutator = new StepMutator<Pipe<int, int>>("AddConstantMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for handler step - add 1 to the final result
/// </summary>
public class TestIfElseStepHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredHandlerStep<string, int>("TestIfElseStepPipeline", "TestHandler");
        var mutator = new StepMutator<Handler<int>>("TestHandlerMutator", 1, (handler) =>
        {
            return async (input) =>
            {
                // Add 1 to the final result before calling the handler
                input += 1;
                await handler(input);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for trim string step - pass through as-is
/// </summary>
public class TestSwitchStepTrimMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, string, string>("TestSwitchPipeline", "TrimString");
        var mutator = new StepMutator<Pipe<string, string>>("TrimStringMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for switch step selector - modify the switching logic to use >50 instead of >100
/// </summary>
public class TestSwitchStepMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredSwitchStep<string, string, int, int, int>("TestSwitchPipeline", "NumberRangeSwitch");
        var mutator = new StepMutator<SwitchSelector<string, int, int>>("NumberRangeSwitchMutator", 1, (selector) =>
        {
            return async (input, cases, defaultNext) =>
            {
                // Modified logic: change the conditions for switching
                // Now values > 50 (instead of > 100) go to GreaterThan100 case
                // Values <= 0 (instead of < 0) go to LessThanZero case
                if (int.TryParse(input, out var number))
                {
                    if (number > 50) // Changed from > 100
                    {
                        await cases["GreaterThan100"](number);
                    }
                    else if (number > 0)
                    {
                        await cases["BetweenZeroAndHundred"](number);
                    }
                    else // number <= 0 (changed from < 0)
                    {
                        await cases["LessThanZero"](number);
                    }
                }
                else
                {
                    // If not a number, use string length
                    var stringLength = input.Length;
                    await defaultNext(stringLength);
                }
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for multiply by three operation - pass through as-is
/// </summary>
public class TestSwitchStepMultiplyByThreeMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<int, int, int>("MultiplyByThree", "MultiplyOperation");
        var mutator = new StepMutator<Pipe<int, int>>("MultiplyByThreeMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for add two operation - pass through as-is
/// </summary>
public class TestSwitchStepAddTwoMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<int, int, int>("AddTwo", "AddOperation");
        var mutator = new StepMutator<Pipe<int, int>>("AddTwoMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for multiply by two operation - pass through as-is
/// </summary>
public class TestSwitchStepMultiplyByTwoMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<int, int, int>("MultiplyByTwo", "MultiplyOperation");
        var mutator = new StepMutator<Pipe<int, int>>("MultiplyByTwoMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for keep zero operation - pass through as-is
/// </summary>
public class TestSwitchStepKeepZeroMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<int, int, int>("KeepZero", "IdentityOperation");
        var mutator = new StepMutator<Pipe<int, int>>("KeepZeroMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for string length operation - pass through as-is
/// </summary>
public class TestSwitchStepStringLengthMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<int, int, int>("StringLengthPipeline", "IdentityOperation");
        var mutator = new StepMutator<Pipe<int, int>>("StringLengthMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for handler step - pass through as-is
/// </summary>
public class TestSwitchStepHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredHandlerStep<string, int>("TestSwitchPipeline", "TestHandler");
        var mutator = new StepMutator<Handler<int>>("TestHandlerMutator", 1, (handler) =>
        {
            return async (input) =>
            {
                // Pass through as-is (no mutation)
                await handler(input);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for trim string step - pass through as-is
/// </summary>
public class TestForkStepTrimMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, string, string>("TestForkPipeline", "TrimString");
        var mutator = new StepMutator<Pipe<string, string>>("TrimStringMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for fork step selector - modify the fork logic
/// </summary>
public class TestForkStepMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredForkStep<string, string, string, string>("TestForkPipeline", "DigitContentFork");
        var mutator = new StepMutator<ForkSelector<string, string, string>>("DigitContentForkMutator", 1, (selector) =>
        {
            return async (input, digitBranch, nonDigitBranch) =>
            {
                // Modified logic: strings with length > 3 go to digit branch, others to non-digit branch
                if (input.Length > 3)
                {
                    await digitBranch(input);
                }
                else
                {
                    await nonDigitBranch(input);
                }
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for remove non-digits step - pass through as-is
/// </summary>
public class TestForkStepRemoveNonDigitsMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, string, string>("DigitProcessing", "RemoveNonDigits");
        var mutator = new StepMutator<Pipe<string, string>>("RemoveNonDigitsMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for parse to int step - add 5 to the parsed value
/// </summary>
public class TestForkStepParseToIntMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, string, int>("DigitProcessing", "ParseToInt");
        var mutator = new StepMutator<Pipe<string, int>>("ParseToIntMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                // Apply original parsing logic first, then add 5 to the result
                if (int.TryParse(input, out var number))
                {
                    await next(number + 5); // Add 5 to the parsed value
                }
                else
                {
                    await next(0 + 5); // Even if parsing fails, add 5 to the default 0
                }
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for int handler step - pass through as-is
/// </summary>
public class TestForkStepIntHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredHandlerStep<string, int>("DigitProcessing", "IntHandler");
        var mutator = new StepMutator<Handler<int>>("IntHandlerMutator", 1, (handler) =>
        {
            return async (input) =>
            {
                await handler(input);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for remove digits step - pass through as-is
/// </summary>
public class TestForkStepRemoveDigitsMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredLinearStep<string, string, string>("NonDigitProcessing", "RemoveDigits");
        var mutator = new StepMutator<Pipe<string, string>>("RemoveDigitsMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for add spaces step - use asterisks instead of spaces
/// </summary>
public class TestForkStepAddSpacesMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var addSpacesStep = space.GetRequiredLinearStep<string, string, string>("NonDigitProcessing", "AddSpaces");
        var addSpacesMutator = new StepMutator<Pipe<string, string>>("AddSpacesMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                var withAsterisks = $"***{input}***"; // Use asterisks instead of spaces
                await next(withAsterisks);
            };
        });
        addSpacesStep.Mutators.AddMutator(addSpacesMutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for string handler step
/// </summary>
public class TestForkStepStringHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var step = space.GetRequiredHandlerStep<string, string>("NonDigitProcessing", "StringHandler");
        var mutator = new StepMutator<Handler<string>>("StringHandlerMutator", 1, (handler) =>
        {
            return async (input) =>
            {
                await handler(input);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }
}


// Mutators for MultiFork pipeline

/// <summary>
/// Test mutator for trim string step
/// </summary>
public class TestMultiForkStepTrimMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var trimStep = space.GetRequiredLinearStep<string, string, string>("TestMultiForkPipeline", "TrimString");
        var trimMutator = new StepMutator<Pipe<string, string>>("TrimStringMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        trimStep.Mutators.AddMutator(trimMutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for MultiFork step selector
/// </summary>
public class TestMultiForkStepMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var multiForkStep = space.GetRequiredMultiForkStep<string, string, string, char[]>("TestMultiForkPipeline", "ClassifyStringContent");
        var multiForkSelectorMutator = new StepMutator<MultiForkSelector<string, string, char[]>>("ClassifyStringContentMutator", 1, (selector) =>
        {
            return async (input, branches, defaultNext) =>
            {
                // Modified logic: always treat everything as special characters unless it's digits only
                var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);

                if (containsOnlyDigits)
                {
                    // Only digits go to digit branch
                    await branches["DigitBranch"](input);
                }
                else
                {
                    // Everything else (letters, special chars, mixed) goes to special char branch
                    await branches["SpecialCharBranch"](input);
                }
            };
        });
        multiForkStep.Mutators.AddMutator(multiForkSelectorMutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for parse to int step
/// </summary>
public class TestMultiForkStepParseToIntMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var parseStringToIntStep = space.GetRequiredLinearStep<string, string, int>("DigitProcessingPipeline", "ParseStringToInt");
        var parseStringToIntMutator = new StepMutator<Pipe<string, int>>("ParseStringToIntMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        parseStringToIntStep.Mutators.AddMutator(parseStringToIntMutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for add constant step
/// </summary>
public class TestMultiForkStepAddConstantMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var addConstantStep = space.GetRequiredLinearStep<string, int, int>("DigitProcessingPipeline", "AddConstant");
        var addConstantMutator = new StepMutator<Pipe<int, int>>("AddConstantMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                var result = input + 10 + 5; // Original +10, mutation adds +5 more
                await next(result);
            };
        });
        addConstantStep.Mutators.AddMutator(addConstantMutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for int handler step
/// </summary>
public class TestMultiForkStepIntHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var intHandlerStepDigit = space.GetRequiredHandlerStep<string, int>("DigitProcessingPipeline", "IntHandler");
        var intHandlerMutatorDigit = new StepMutator<Handler<int>>("IntHandlerMutatorDigit", 1, (handler) =>
        {
            return async (input) =>
            {
                await handler(input);
            };
        });
        intHandlerStepDigit.Mutators.AddMutator(intHandlerMutatorDigit, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for add spaces step
/// </summary>
public class TestMultiForkStepAddSpacesMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var addSpacesStep = space.GetRequiredLinearStep<string, string, string>("LetterProcessingPipeline", "AddSpaces");
        var addSpacesMutator = new StepMutator<Pipe<string, string>>("AddSpacesMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                var withAsterisks = $"***{input}***"; // Use asterisks instead of spaces
                await next(withAsterisks);
            };
        });
        addSpacesStep.Mutators.AddMutator(addSpacesMutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for string handler step
/// </summary>
public class TestMultiForkStepStringHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var stringHandlerStep = space.GetRequiredHandlerStep<string, string>("LetterProcessingPipeline", "StringHandler");
        var stringHandlerMutator = new StepMutator<Handler<string>>("StringHandlerMutator", 1, (handler) =>
        {
            return async (input) =>
            {
                await handler(input);
            };
        });
        stringHandlerStep.Mutators.AddMutator(stringHandlerMutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for remove whitespace step
/// </summary>
public class TestMultiForkStepRemoveWhitespaceMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var removeWhitespaceStep = space.GetRequiredLinearStep<string, string, string>("SpecialCharProcessingPipeline", "RemoveWhitespace");
        var removeWhitespaceMutator = new StepMutator<Pipe<string, string>>("RemoveWhitespaceMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        removeWhitespaceStep.Mutators.AddMutator(removeWhitespaceMutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for convert to char array step
/// </summary>
public class TestMultiForkStepConvertToCharArrayMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var convertToCharArrayStep = space.GetRequiredLinearStep<string, string, char[]>("SpecialCharProcessingPipeline", "ConvertToCharArray");
        var convertToCharArrayMutator = new StepMutator<Pipe<string, char[]>>("ConvertToCharArrayMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                var inputWithUnderscore = input + "_"; // Add underscore before converting
                var charArray = inputWithUnderscore.ToCharArray();
                await next(charArray);
            };
        });
        convertToCharArrayStep.Mutators.AddMutator(convertToCharArrayMutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for remove duplicates step
/// </summary>
public class TestMultiForkStepRemoveDuplicatesMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var removeDuplicatesStep = space.GetRequiredLinearStep<string, char[], char[]>("SpecialCharProcessingPipeline", "RemoveDuplicates");
        var removeDuplicatesMutator = new StepMutator<Pipe<char[], char[]>>("RemoveDuplicatesMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        removeDuplicatesStep.Mutators.AddMutator(removeDuplicatesMutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for char array handler step
/// </summary>
public class TestMultiForkStepCharArrayHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var charArrayHandlerStep = space.GetRequiredHandlerStep<string, char[]>("SpecialCharProcessingPipeline", "CharArrayHandler");
        var charArrayHandlerMutator = new StepMutator<Handler<char[]>>("CharArrayHandlerMutator", 1, (handler) =>
        {
            return async (input) =>
            {
                await handler(input);
            };
        });
        charArrayHandlerStep.Mutators.AddMutator(charArrayHandlerMutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for count digits and letters step
/// </summary>
public class TestMultiForkStepCountDigitsAndLettersMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var countStep = space.GetRequiredLinearStep<char[], char[], (int DigitCount, int LetterCount)>("DefaultProcessingPipeline", "CountDigitsAndLetters");
        var countMutator = new StepMutator<Pipe<char[], (int DigitCount, int LetterCount)>>("CountDigitsAndLettersMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                await pipe(input, next);
            };
        });
        countStep.Mutators.AddMutator(countMutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for calculate ratio step
/// </summary>
public class TestMultiForkStepCalculateRatioMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var calculateRatioStep = space.GetRequiredLinearStep<char[], (int DigitCount, int LetterCount), int>("DefaultProcessingPipeline", "CalculateRatio");
        var calculateRatioMutator = new StepMutator<Pipe<(int DigitCount, int LetterCount), int>>("CalculateRatioMutator", 1, (pipe) =>
        {
            return async (input, next) =>
            {
                // Calculate ratio of digits to letters (floor division) and add 2
                var ratio = input.LetterCount > 0 ? input.DigitCount / input.LetterCount : input.DigitCount;
                await next(ratio + 2); // Add 2 to the calculated ratio
            };
        });
        calculateRatioStep.Mutators.AddMutator(calculateRatioMutator, AddingMode.ExactPlace);
    }
}

/// <summary>
/// Test mutator for default int handler step
/// </summary>
public class TestMultiForkStepDefaultIntHandlerMutator : IMutator<Space>
{
    public void Mutate(Space space)
    {
        var intHandlerStepDefault = space.GetRequiredHandlerStep<char[], int>("DefaultProcessingPipeline", "IntHandler");
        var intHandlerMutatorDefault = new StepMutator<Handler<int>>("IntHandlerMutatorDefault", 1, (handler) =>
        {
            return async (input) =>
            {
                await handler(input);
            };
        });
        intHandlerStepDefault.Mutators.AddMutator(intHandlerMutatorDefault, AddingMode.ExactPlace);
    }
}

#endregion