using K1vs.DotNetPipe.Cancellable;
using NSubstitute;

namespace K1vs.DotNetPipe.Tests.Cancellable;

/// <summary>
/// Tests for pipeline functionality using class-based step declaration approach.
/// These tests mirror the logic of WithoutMutationPipelineTests but use classes implementing step interfaces.
/// </summary>
public class WithoutMutationClassPipelineTests
{
    [Theory]
    [InlineData(-4, -4)]
    [InlineData(0, 0)]
    [InlineData(2, 2)]
    public async Task BuildAndRunPipeline_WhenOneHandlerStep_ShouldRun(int value, int expectedValue)
    {
        // Arrange
        var handler = Substitute.For<Func<int, CancellationToken, ValueTask>>();
        handler.Invoke(Arg.Is(expectedValue), Arg.Any<CancellationToken>()).Returns(ValueTask.CompletedTask);

        var testPipeline = new TestPipeline([], handler);

        // Act
        await testPipeline.Run(value);

        // Assert
        await handler.Received().Invoke(Arg.Is(expectedValue), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(-4, 6)]   // -4 + 10 = 6
    [InlineData(0, 10)]   // 0 + 10 = 10
    [InlineData(2, 12)]   // 2 + 10 = 12
    public async Task BuildAndRunPipeline_WhenLinearStepThenHandlerStep_ShouldRun(int inputValue, int expectedHandlerInput)
    {
        // Arrange
        var handler = Substitute.For<Func<int, CancellationToken, ValueTask>>();
        handler.Invoke(Arg.Is(expectedHandlerInput), Arg.Any<CancellationToken>()).Returns(ValueTask.CompletedTask);

        var testPipeline = new TestTwoStepPipeline([], handler);

        // Act
        await testPipeline.Run(inputValue);

        // Assert
        await handler.Received().Invoke(Arg.Is(expectedHandlerInput), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(2, 24)]   // Step1: 2 + 10 = 12, Step2: 12 * 2 = 24
    [InlineData(0, 20)]   // Step1: 0 + 10 = 10, Step2: 10 * 2 = 20
    [InlineData(-1, 18)]  // Step1: -1 + 10 = 9, Step2: 9 * 2 = 18
    public async Task BuildAndRunPipeline_WhenTwoLinearStepsThenHandlerStep_ShouldRun(int inputValue, int expectedHandlerInput)
    {
        // Arrange
        var handler = Substitute.For<Func<int, CancellationToken, ValueTask>>();
        handler.Invoke(Arg.Is(expectedHandlerInput), Arg.Any<CancellationToken>()).Returns(ValueTask.CompletedTask);

        var testPipeline = new TestThreeStepPipeline([], handler);

        // Act
        await testPipeline.Run(inputValue);

        // Assert
        await handler.Received().Invoke(Arg.Is(expectedHandlerInput), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("5", 8)]     // "5" -> 5 + 3 = 8
    [InlineData("10", 13)]   // "10" -> 10 + 3 = 13
    [InlineData("0", 3)]     // "0" -> 0 + 3 = 3
    [InlineData("abc", 0)]   // "abc" -> doesn't parse, handler not called
    [InlineData("", 0)]      // "" -> doesn't parse, handler not called
    public async Task BuildAndRunPipeline_WhenStringParseAndAddConstant_ShouldCallHandlerOnlyOnSuccessfulParse(string inputValue, int expectedValue)
    {
        // Arrange
        var handler = Substitute.For<Func<int, CancellationToken, ValueTask>>();
        handler.Invoke(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(ValueTask.CompletedTask);

        var testPipeline = new TestStringParsingPipeline([], handler);

        // Act
        await testPipeline.Run(inputValue);

        // Assert
        if (expectedValue > 0)
        {
            await handler.Received().Invoke(Arg.Is(expectedValue), Arg.Any<CancellationToken>());
        }
        else
        {
            await handler.DidNotReceive().Invoke(Arg.Any<int>(), Arg.Any<CancellationToken>());
        }
    }

    [Theory]
    [InlineData(" 10 ", 12)]   // " 10 " -> trim -> 10 -> int path -> 10 + 2 = 12
    [InlineData("3.7", 6)]     // "3.7" -> trim -> 3.7 -> float path -> 3.7 -> round to 4 -> 4 + 2 = 6
    [InlineData(" 2.3 ", 4)]   // " 2.3 " -> trim -> 2.3 -> float path -> 2.3 -> round to 2 -> 2 + 2 = 4
    [InlineData("5.5", 8)]     // "5.5" -> trim -> 5.5 -> float path -> 5.5 -> round to 6 -> 6 + 2 = 8
    public async Task BuildAndRunPipeline_WhenIfStepHandlesIntAndFloat_ShouldProcessCorrectly(string inputValue, int expectedResult)
    {
        // Arrange
        var handler = Substitute.For<Func<int, CancellationToken, ValueTask>>();
        handler.Invoke(Arg.Is(expectedResult), Arg.Any<CancellationToken>()).Returns(ValueTask.CompletedTask);

        var testPipeline = new TestIfStepPipeline([], handler);

        // Act
        await testPipeline.Run(inputValue);

        // Assert
        await handler.Received().Invoke(Arg.Is(expectedResult), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("  5  ", 13)]    // "  5  " -> trim -> 5 (int) -> false branch (int processing) -> 5 * 2 = 10 -> 10 + 3 = 13
    [InlineData(" 10 ", 23)]     // " 10 " -> trim -> 10 (int) -> false branch (int processing) -> 10 * 2 = 20 -> 20 + 3 = 23
    [InlineData("3.7", 7)]       // "3.7" -> trim -> 3.7 (float) -> true branch (float processing) -> parse 3.7 -> round to 4 -> 4 + 3 = 7
    [InlineData(" 2.3 ", 5)]     // " 2.3 " -> trim -> 2.3 (float) -> true branch (float processing) -> parse 2.3 -> round to 2 -> 2 + 3 = 5
    [InlineData("5.5", 9)]       // "5.5" -> trim -> 5.5 (float) -> true branch (float processing) -> parse 5.5 -> round to 6 -> 6 + 3 = 9
    public async Task BuildAndRunPipeline_WhenIfElseStepHandlesIntFloatOrDefault_ShouldProcessCorrectly(string inputValue, int expectedResult)
    {
        // Arrange
        var handler = Substitute.For<Func<int, CancellationToken, ValueTask>>();
        handler.Invoke(Arg.Is(expectedResult), Arg.Any<CancellationToken>()).Returns(ValueTask.CompletedTask);

        var testPipeline = new TestIfElseStepPipeline([], handler);

        // Act
        await testPipeline.Run(inputValue);

        // Assert
        await handler.Received().Invoke(Arg.Is(expectedResult), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(" 105 ", 318)]    // >100 -> GreaterThan100 -> 105 * 3 = 315 -> +3 = 318
    [InlineData(" 50 ", 55)]      // 0<x<=100 -> BetweenZeroAndHundred -> 50 + 2 = 52 -> +3 = 55
    [InlineData(" -5 ", -7)]      // <0 -> LessThanZero -> -5 * 2 = -10 -> +3 = -7
    [InlineData(" 0 ", 3)]        // ==0 -> EqualToZero -> 0 (identity) -> +3 = 3
    [InlineData("abc", 6)]        // not a number -> string length = 3 -> +3 = 6
    [InlineData("hello", 8)]      // not a number -> string length = 5 -> +3 = 8
    [InlineData("", 3)]           // empty string -> length = 0 -> +3 = 3
    public async Task BuildAndRunPipeline_WhenSwitchStepRoutesByNumberRange_ShouldProcessCorrectly(string inputValue, int expectedResult)
    {
        // Arrange
        var handler = Substitute.For<Func<int, CancellationToken, ValueTask>>();
        handler.Invoke(Arg.Is(expectedResult), Arg.Any<CancellationToken>()).Returns(ValueTask.CompletedTask);

        var testPipeline = new TestSwitchStepPipeline([], handler);

        // Act
        await testPipeline.Run(inputValue);

        // Assert
        await handler.Received().Invoke(Arg.Is(expectedResult), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("123", 123, "")]
    [InlineData("  456  ", 456, "")]
    [InlineData("abc123def", 0, "  abcdef  ")]
    [InlineData("hello", 0, "  hello  ")]
    [InlineData("", 0, "    ")]
    [InlineData("!@#", 0, "  !@#  ")]
    public async Task BuildAndRunPipeline_WhenForkSplitsByDigitContent_ShouldProcessCorrectly(string inputValue, int expectedIntResult, string? expectedStringResult)
    {
        // Arrange
        var intHandler = Substitute.For<Func<int, CancellationToken, ValueTask>>();
        var stringHandler = Substitute.For<Func<string, CancellationToken, ValueTask>>();
        intHandler.Invoke(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(ValueTask.CompletedTask);
        stringHandler.Invoke(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(ValueTask.CompletedTask);

        var pipeline = new TestForkStepPipeline([], intHandler, stringHandler);

        // Act
        await pipeline.Run(inputValue);

        // Assert
        if (expectedStringResult == "")
        {
            // Should call int handler (digit-only string)
            await intHandler.Received().Invoke(Arg.Is(expectedIntResult), Arg.Any<CancellationToken>());
            await stringHandler.DidNotReceive().Invoke(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }
        else
        {
            // Should call string handler (non-digit string)
            await stringHandler.Received().Invoke(Arg.Is(expectedStringResult!), Arg.Any<CancellationToken>());
            await intHandler.DidNotReceive().Invoke(Arg.Any<int>(), Arg.Any<CancellationToken>());
        }
    }

    [Theory]
    [InlineData("123", 133, "", new char[0])]    // digits only -> DigitBranch -> parse to 123 -> +10 = 133
    [InlineData("  456  ", 466, "", new char[0])]    // digits only (trimmed) -> DigitBranch -> parse to 456 -> +10 = 466
    [InlineData("abc", 0, "  abc  ", new char[0])]    // letters only -> LetterBranch -> add spaces -> StringHandler
    [InlineData("xyz", 0, "  xyz  ", new char[0])]    // letters only -> LetterBranch -> add spaces -> StringHandler
    [InlineData("!@#", 0, "", new char[] { '!', '@', '#' })]    // special chars only -> SpecialCharBranch -> remove whitespace -> convert to char array -> remove duplicates -> CharArrayHandler
    [InlineData("@@@", 0, "", new char[] { '@' })]    // repeated special chars -> SpecialCharBranch -> unique char array
    [InlineData("a1b2", 1, "", new char[0])]    // mixed content -> DefaultBranch -> convert to char array ['a','1','b','2'] -> count: 2 digits, 2 letters -> ratio: 2/2 = 1 -> IntHandler
    [InlineData("hello123", 0, "", new char[0])]    // mixed content -> DefaultBranch -> convert to char array ['h','e','l','l','o','1','2','3'] -> count: 3 digits, 5 letters -> ratio: 3/5 = 0 -> IntHandler
    [InlineData("12345abc", 1, "", new char[0])]    // mixed content -> DefaultBranch -> convert to char array ['1','2','3','4','5','a','b','c'] -> count: 5 digits, 3 letters -> ratio: 5/3 = 1 -> IntHandler
    public async Task BuildAndRunPipeline_WhenMultiForkClassifiesStringContent_ShouldProcessCorrectly(
        string inputValue,
        int expectedIntResult,
        string expectedStringResult,
        char[] expectedCharArrayResult)
    {
        // Arrange
        var intHandler = Substitute.For<Func<int, CancellationToken, ValueTask>>();
        var stringHandler = Substitute.For<Func<string, CancellationToken, ValueTask>>();
        var charArrayHandler = Substitute.For<Func<char[], CancellationToken, ValueTask>>();
        intHandler.Invoke(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(ValueTask.CompletedTask);
        stringHandler.Invoke(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(ValueTask.CompletedTask);
        charArrayHandler.Invoke(Arg.Any<char[]>(), Arg.Any<CancellationToken>()).Returns(ValueTask.CompletedTask);

        var pipeline = new TestMultiForkStepPipeline([], intHandler, stringHandler, charArrayHandler);

        // Act
        await pipeline.Run(inputValue);

        // Assert
        if (expectedStringResult != "")
        {
            // Should call string handler (letter-only string) - but this won't happen anymore with new mutator
            await stringHandler.Received().Invoke(Arg.Is(expectedStringResult!), Arg.Any<CancellationToken>());
            await intHandler.DidNotReceive().Invoke(Arg.Any<int>(), Arg.Any<CancellationToken>());
            await charArrayHandler.DidNotReceive().Invoke(Arg.Any<char[]>(), Arg.Any<CancellationToken>());
        }
        else if (expectedCharArrayResult.Length > 0)
        {
            // Should call char array handler (everything except digits-only strings)
            await charArrayHandler.Received().Invoke(Arg.Is<char[]>(arr => arr.SequenceEqual(expectedCharArrayResult)), Arg.Any<CancellationToken>());
            await intHandler.DidNotReceive().Invoke(Arg.Any<int>(), Arg.Any<CancellationToken>());
            await stringHandler.DidNotReceive().Invoke(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }
        else
        {
            // Should call int handler (digit-only strings)
            await intHandler.Received().Invoke(Arg.Is(expectedIntResult), Arg.Any<CancellationToken>());
            await stringHandler.DidNotReceive().Invoke(Arg.Any<string>(), Arg.Any<CancellationToken>());
            await charArrayHandler.DidNotReceive().Invoke(Arg.Any<char[]>(), Arg.Any<CancellationToken>());
        }
    }
}
