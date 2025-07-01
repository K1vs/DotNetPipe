using NSubstitute;
using K1vs.DotNetPipe.Cancellable;
using System.Linq;

namespace K1vs.DotNetPipe.Tests.Cancellable;

public class WithoutMutationPipelineTests
{
    [Theory]
    [InlineData(-4)]
    [InlineData(0)]
    [InlineData(2)]
    public async Task BuildAndRunPipeline_WhenOneHandlerStep_ShouldRun(int value)
    {
        // Arrange
        var handler = Substitute.For<Func<int, CancellationToken, ValueTask>>();
        handler.Invoke(Arg.Is(value), Arg.Any<CancellationToken>()).Returns(ValueTask.CompletedTask);
        var pipeline = Pipelines.CreateCancellablePipeline<int>("TestPipeline")
            .StartWithHandler("TestHandler", async (input, ct) => await handler(input, ct))
            .BuildPipeline().Compile();
        // Act
        await pipeline(value);
        // Assert
        await handler.Received().Invoke(Arg.Is(value), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(-4, 5, 1)]
    [InlineData(0, 10, 10)]
    [InlineData(2, 3, 5)]
    public async Task BuildAndRunPipeline_WhenLinearStepThenHandlerStep_ShouldRun(int inputValue, int constantToAdd, int expectedHandlerInput)
    {
        // Arrange
        var handler = Substitute.For<Func<int, CancellationToken, ValueTask>>();
        handler.Invoke(Arg.Is(expectedHandlerInput), Arg.Any<CancellationToken>()).Returns(ValueTask.CompletedTask);

        var pipeline = Pipelines.CreateCancellablePipeline<int>("TestTwoStepPipeline")
            .StartWithLinear<int>("AddConstant", async (input, next, ct) =>
            {
                var result = input + constantToAdd;
                await next(result, ct);
            })
            .HandleWith("TestHandler", async (input, ct) => await handler(input, ct))
            .BuildPipeline().Compile();

        // Act
        await pipeline(inputValue);

        // Assert
        await handler.Received().Invoke(Arg.Is(expectedHandlerInput), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(2, 3, 2, 10)]  // (2 + 3) * 2 = 10
    [InlineData(0, 5, 3, 15)]  // (0 + 5) * 3 = 15
    [InlineData(-1, 4, 2, 6)]  // (-1 + 4) * 2 = 6
    [InlineData(10, -5, 4, 20)] // (10 + (-5)) * 4 = 20
    public async Task BuildAndRunPipeline_WhenTwoLinearStepsThenHandlerStep_ShouldRun(int inputValue, int constantToAdd, int multiplier, int expectedHandlerInput)
    {
        // Arrange
        var handler = Substitute.For<Func<int, CancellationToken, ValueTask>>();
        handler.Invoke(Arg.Is(expectedHandlerInput), Arg.Any<CancellationToken>()).Returns(ValueTask.CompletedTask);

        var pipeline = Pipelines.CreateCancellablePipeline<int>("TestThreeStepPipeline")
            .StartWithLinear<int>("AddConstant", async (input, next, ct) =>
            {
                var result = input + constantToAdd;
                await next(result, ct);
            })
            .ThenLinear<int>("MultiplyByCoefficient", async (input, next, ct) =>
            {
                var result = input * multiplier;
                await next(result, ct);
            })
            .HandleWith("TestHandler", async (input, ct) => await handler(input, ct))
            .BuildPipeline().Compile();

        // Act
        await pipeline(inputValue);

        // Assert
        await handler.Received().Invoke(Arg.Is(expectedHandlerInput), Arg.Any<CancellationToken>());
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
        var handler = Substitute.For<Func<int, CancellationToken, ValueTask>>();
        handler.Invoke(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(ValueTask.CompletedTask);

        var pipeline = Pipelines.CreateCancellablePipeline<string>("TestStringParsingPipeline")
            .StartWithLinear<int>("ParseString", async (input, next, ct) =>
            {
                if (int.TryParse(input, out var parsed))
                {
                    await next(parsed, ct);
                }
                // If doesn't parse - don't call next
            })
            .ThenLinear<int>("AddConstant", async (input, next, ct) =>
            {
                var result = input + constantToAdd;
                await next(result, ct);
            })
            .HandleWith("TestHandler", async (input, ct) => await handler(input, ct))
            .BuildPipeline().Compile();

        // Act
        await pipeline(inputValue);

        // Assert
        if (expectedCallCount > 0)
        {
            await handler.Received().Invoke(Arg.Is(expectedCallCount), Arg.Any<CancellationToken>());
        }
        else
        {
            await handler.DidNotReceive().Invoke(Arg.Any<int>(), Arg.Any<CancellationToken>());
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
        var handler = Substitute.For<Func<int, CancellationToken, ValueTask>>();
        handler.Invoke(Arg.Is(expectedResult), Arg.Any<CancellationToken>()).Returns(ValueTask.CompletedTask);

        var pipeline = Pipelines.CreateCancellablePipeline<string>("TestIfStepPipeline")
            .StartWithLinear<string>("TrimString", async (input, next, ct) =>
            {
                var trimmed = input.Trim();
                await next(trimmed, ct);
            })
            .ThenIf<string, int>("CheckIntOrFloat", async (input, conditionalNext, next, ct) =>
            {
                // Try to parse as int first
                if (int.TryParse(input, out var intValue))
                {
                    // If it's an int, continue with main pipeline
                    await next(intValue, ct);
                }
                else
                {
                    // If not an int, go to conditional pipeline (for float parsing)
                    await conditionalNext(input, ct);
                }
            }, space => space.CreatePipeline<string>("FloatProcessing")
                .StartWithLinear<double>("ParseFloat", async (input, next, ct) =>
                {
                    if (double.TryParse(input, out var floatValue))
                    {
                        await next(floatValue, ct);
                    }
                })
                .ThenLinear<int>("RoundToInt", async (input, next, ct) =>
                {
                    var rounded = (int)Math.Round(input);
                    await next(rounded, ct);
                })
                .BuildOpenPipeline())
            .ThenLinear<int>("AddConstant", async (input, next, ct) =>
            {
                var result = input + constantToAdd;
                await next(result, ct);
            })
            .HandleWith("TestHandler", async (input, ct) => await handler(input, ct))
            .BuildPipeline().Compile();

        // Act
        await pipeline(inputValue);

        // Assert
        await handler.Received().Invoke(Arg.Is(expectedResult), Arg.Any<CancellationToken>());
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
        var handler = Substitute.For<Func<int, CancellationToken, ValueTask>>();
        handler.Invoke(Arg.Is(expectedResult), Arg.Any<CancellationToken>()).Returns(ValueTask.CompletedTask);

        var pipeline = Pipelines.CreateCancellablePipeline<string>("TestIfElseStepPipeline")
            .StartWithLinear<string>("TrimString", async (input, next, ct) =>
            {
                var trimmed = input.Trim();
                await next(trimmed, ct);
            })
            .ThenIfElse<string, int, int>("CheckIntOrFloat", async (input, trueNext, falseNext, ct) =>
            {
                // Try to parse as int first
                if (int.TryParse(input, out var intValue))
                {
                    // If it's an int, we actually want to bypass both branches and continue directly
                    // But since IfElse requires going through one of the branches, we'll use false branch for int
                    await falseNext(intValue, ct);
                }
                else
                {
                    // If not an int, go to true branch (for float parsing)
                    await trueNext(input, ct);
                }
            },
            // True branch - float processing
            space => space.CreatePipeline<string>("FloatProcessing")
                .StartWithLinear<double>("ParseFloat", async (input, next, ct) =>
                {
                    if (double.TryParse(input, out var floatValue))
                    {
                        await next(floatValue, ct);
                    }
                })
                .ThenLinear<int>("RoundToInt", async (input, next, ct) =>
                {
                    var rounded = (int)Math.Round(input);
                    await next(rounded, ct);
                })
                .BuildOpenPipeline(),
            // False branch - multiply by multiplier
            space => space.CreatePipeline<int>("IntOrDefaultProcessing")
                .StartWithLinear<int>("ParseIntOrDefault", async (input, next, ct) =>
                {
                    // If we got here, it means we parsed as int in the false branch
                    // So we just pass it through
                    await next(input * multiplier, ct);
                })
                .BuildOpenPipeline())
            .ThenLinear<int>("AddConstant", async (input, next, ct) =>
            {
                var result = input + constantToAdd;
                await next(result, ct);
            })
            .HandleWith("TestHandler", async (input, ct) => await handler(input, ct))
            .BuildPipeline().Compile();

        // Act
        await pipeline(inputValue);

        // Assert
        await handler.Received().Invoke(Arg.Is(expectedResult), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("105", 315)]    // >100 -> *3 = 315
    [InlineData("50", 52)]      // 0<x<100 -> +2 = 52
    [InlineData("-5", -10)]     // <0 -> *2 = -10
    [InlineData("0", 0)]        // =0 -> stay 0
    [InlineData("abc", 3)]      // not a number -> string length = 3
    [InlineData("hello", 5)]    // not a number -> string length = 5
    [InlineData("", 0)]         // empty string -> length = 0
    public async Task BuildAndRunPipeline_WhenSwitchStepRoutesByNumberRange_ShouldProcessCorrectly(string inputValue, int expectedResult)
    {
        // Arrange
        var handler = Substitute.For<Func<int, CancellationToken, ValueTask>>();
        handler.Invoke(Arg.Is(expectedResult), Arg.Any<CancellationToken>()).Returns(ValueTask.CompletedTask);

        var space = Pipelines.CreateCancellableSpace();
        var defaultPipeline = space.CreatePipeline<int>("StringLengthPipeline")
            .StartWithLinear<int>("IdentityOperation", async (input, next, ct) =>
            {
                await next(input, ct); // Use string length as-is
            })
            .BuildOpenPipeline();

        var pipeline = space.CreatePipeline<string>("TestSwitchPipeline")
            .StartWithLinear<string>("TrimString", async (input, next, ct) =>
            {
                var trimmed = input.Trim();
                await next(trimmed, ct);
            })
            .ThenSwitch<int, int, int>("NumberRangeSwitch", async (input, cases, defaultNext, ct) =>
            {
                // Try to parse as integer
                if (int.TryParse(input, out var number))
                {
                    if (number > 100)
                    {
                        await cases["GreaterThan100"](number, ct);
                    }
                    else if (number > 0)
                    {
                        await cases["BetweenZeroAndHundred"](number, ct);
                    }
                    else if (number < 0)
                    {
                        await cases["LessThanZero"](number, ct);
                    }
                    else // number == 0
                    {
                        await cases["EqualToZero"](number, ct);
                    }
                }
                else
                {
                    // If not a number, use string length
                    var stringLength = input.Length;
                    await defaultNext(stringLength, ct);
                }
            },
            space => new Dictionary<string, OpenPipeline<int, int>>
            {
                ["GreaterThan100"] = space.CreatePipeline<int>("MultiplyByThree")
                    .StartWithLinear<int>("MultiplyOperation", async (input, next, ct) =>
                    {
                        var result = input * 3;
                        await next(result, ct);
                    })
                    .BuildOpenPipeline(),
                ["BetweenZeroAndHundred"] = space.CreatePipeline<int>("AddTwo")
                    .StartWithLinear<int>("AddOperation", async (input, next, ct) =>
                    {
                        var result = input + 2;
                        await next(result, ct);
                    })
                    .BuildOpenPipeline(),
                ["LessThanZero"] = space.CreatePipeline<int>("MultiplyByTwo")
                    .StartWithLinear<int>("MultiplyOperation", async (input, next, ct) =>
                    {
                        var result = input * 2;
                        await next(result, ct);
                    })
                    .BuildOpenPipeline(),
                ["EqualToZero"] = space.CreatePipeline<int>("KeepZero")
                    .StartWithLinear<int>("IdentityOperation", async (input, next, ct) =>
                    {
                        await next(input, ct); // Keep the same value (0)
                    })
                    .BuildOpenPipeline()
            }.AsReadOnly(),
            defaultPipeline)
            .HandleWith("TestHandler", async (input, ct) => await handler(input, ct))
            .BuildPipeline().Compile();

        // Act
        await pipeline(inputValue);

        // Assert
        await handler.Received().Invoke(Arg.Is(expectedResult), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("123", 123, "")]             // Only digits -> first branch -> parse to int -> 123
    [InlineData("  456  ", 456, "")]         // Only digits (with spaces) -> first branch -> 456
    [InlineData("abc123def", 0, "  abcdef  ")] // Mixed -> second branch -> remove digits -> add spaces
    [InlineData("hello", 0, "  hello  ")]    // No digits -> second branch -> add spaces
    [InlineData("", 0, "    ")]              // Empty -> second branch -> add spaces
    [InlineData("!@#", 0, "  !@#  ")]        // Special chars -> second branch -> add spaces
    public async Task BuildAndRunPipeline_WhenForkSplitsByDigitContent_ShouldProcessCorrectly(string inputValue, int expectedIntResult, string? expectedStringResult)
    {
        // Arrange
        var intHandler = Substitute.For<Func<int, CancellationToken, ValueTask>>();
        var stringHandler = Substitute.For<Func<string, CancellationToken, ValueTask>>();
        intHandler.Invoke(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(ValueTask.CompletedTask);
        stringHandler.Invoke(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(ValueTask.CompletedTask);

        var pipeline = Pipelines.CreateCancellablePipeline<string>("TestForkPipeline")
            .StartWithLinear<string>("TrimString", async (input, next, ct) =>
            {
                var trimmed = input.Trim();
                await next(trimmed, ct);
            })
            .ThenFork<string, string>("DigitContentFork", async (input, digitBranch, nonDigitBranch, ct) =>
            {
                // Check if string contains only digits (after trimming)
                var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);

                if (containsOnlyDigits)
                {
                    await digitBranch(input, ct);
                }
                else
                {
                    await nonDigitBranch(input, ct);
                }
            },
            // Digit processing branch
            space => space.CreatePipeline<string>("DigitProcessing")
                .StartWithLinear<string>("RemoveNonDigits", async (input, next, ct) =>
                {
                    var digitsOnly = new string(input.Where(char.IsDigit).ToArray());
                    await next(digitsOnly, ct);
                })
                .ThenLinear<int>("ParseToInt", async (input, next, ct) =>
                {
                    if (int.TryParse(input, out var number))
                    {
                        await next(number, ct);
                    }
                    else
                    {
                        await next(0, ct); // Default to 0 if parsing fails
                    }
                })
                .HandleWith("IntHandler", async (input, ct) => await intHandler(input, ct))
                .BuildPipeline(),
            // Non-digit processing branch
            space => space.CreatePipeline<string>("NonDigitProcessing")
                .StartWithLinear<string>("RemoveDigits", async (input, next, ct) =>
                {
                    var nonDigitsOnly = new string(input.Where(c => !char.IsDigit(c)).ToArray());
                    await next(nonDigitsOnly, ct);
                })
                .ThenLinear<string>("AddSpaces", async (input, next, ct) =>
                {
                    var withSpaces = $"  {input}  ";
                    await next(withSpaces, ct);
                })
                .HandleWith("StringHandler", async (input, ct) => await stringHandler(input, ct))
                .BuildPipeline())
            .BuildPipeline().Compile();

        // Act
        await pipeline(inputValue);

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
    [InlineData("123", 133, "", new char[0])]             // Only digits -> first branch -> parse to int (123) + 10 = 133
    [InlineData("  456  ", 466, "", new char[0])]         // Only digits (with spaces) -> first branch -> 466
    [InlineData("abc", 0, "  abc  ", new char[0])]        // Only letters -> second branch -> add spaces
    [InlineData("xyz", 0, "  xyz  ", new char[0])]        // Only letters -> second branch -> add spaces
    [InlineData("!@#", 0, "", new char[] { '!', '@', '#' })] // Only special chars -> third branch -> remove whitespace, convert to array, remove duplicates
    [InlineData("@@@", 0, "", new char[] { '@' })]        // Special chars with duplicates -> third branch -> unique chars only    [InlineData("a1b2", 1, "", new char[0])]              // Mixed -> default branch -> 2 digits, 2 letters -> ratio = 1
    [InlineData("hello123", 0, "", new char[0])]          // Mixed -> default branch -> 3 digits, 5 letters -> ratio = 0 (floor)
    [InlineData("12345abc", 1, "", new char[0])]          // Mixed -> default branch -> 5 digits, 3 letters -> ratio = 1 (floor)
    public async Task BuildAndRunPipeline_WhenMultiForkClassifiesStringContent_ShouldProcessCorrectly(
        string inputValue,
        int expectedIntResult,
        string? expectedStringResult,
        char[] expectedCharArrayResult)
    {
        // Arrange
        var intHandler = Substitute.For<Func<int, CancellationToken, ValueTask>>();
        var stringHandler = Substitute.For<Func<string, CancellationToken, ValueTask>>();
        var charArrayHandler = Substitute.For<Func<char[], CancellationToken, ValueTask>>();

        intHandler.Invoke(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(ValueTask.CompletedTask);
        stringHandler.Invoke(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(ValueTask.CompletedTask);
        charArrayHandler.Invoke(Arg.Any<char[]>(), Arg.Any<CancellationToken>()).Returns(ValueTask.CompletedTask);

        var space = Pipelines.CreateCancellableSpace();

        // Create sub-pipelines before main pipeline
        space.CreatePipeline<string>("DigitProcessingPipeline")
            .StartWithLinear<int>("ParseStringToInt", async (input, next, ct) =>
            {
                if (int.TryParse(input, out var number))
                {
                    await next(number, ct);
                }
                else
                {
                    await next(0, ct); // Default value if parsing fails
                }
            })
            .ThenLinear<int>("AddConstant", async (input, next, ct) =>
            {
                var result = input + 10; // Add constant 10
                await next(result, ct);
            })
            .HandleWith("IntHandler", async (input, ct) => await intHandler(input, ct))
            .BuildPipeline();

        space.CreatePipeline<string>("LetterProcessingPipeline")
            .StartWithLinear<string>("AddSpaces", async (input, next, ct) =>
            {
                var withSpaces = $"  {input}  ";
                await next(withSpaces, ct);
            })
            .HandleWith("StringHandler", async (input, ct) => await stringHandler(input, ct))
            .BuildPipeline();

        space.CreatePipeline<string>("SpecialCharProcessingPipeline")
            .StartWithLinear<string>("RemoveWhitespace", async (input, next, ct) =>
            {
                var noWhitespace = new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray());
                await next(noWhitespace, ct);
            })
            .ThenLinear<char[]>("ConvertToCharArray", async (input, next, ct) =>
            {
                var charArray = input.ToCharArray();
                await next(charArray, ct);
            })
            .ThenLinear<char[]>("RemoveDuplicates", async (input, next, ct) =>
            {
                var uniqueChars = input.Distinct().ToArray();
                await next(uniqueChars, ct);
            })
            .HandleWith("CharArrayHandler", async (input, ct) => await charArrayHandler(input, ct))
            .BuildPipeline();        var defaultPipeline = space.CreatePipeline<char[]>("DefaultProcessingPipeline")
            .StartWithLinear<(int DigitCount, int LetterCount)>("CountDigitsAndLetters", async (input, next, ct) =>
            {
                var digitCount = input.Count(char.IsDigit);
                var letterCount = input.Count(char.IsLetter);
                await next((digitCount, letterCount), ct);
            })
            .ThenLinear<int>("CalculateRatio", async (input, next, ct) =>
            {
                // Calculate ratio of digits to letters (floor division)
                var ratio = input.LetterCount > 0 ? input.DigitCount / input.LetterCount : input.DigitCount;
                await next(ratio, ct);
            })
            .HandleWith("IntHandler", async (input, ct) => await intHandler(input, ct))
            .BuildPipeline();

        var pipeline = space.CreatePipeline<string>("TestMultiForkPipeline")
            .StartWithLinear<string>("TrimString", async (input, next, ct) =>
            {
                var trimmed = input.Trim();
                await next(trimmed, ct);
            })
            .ThenMultiFork<string, char[]>("ClassifyStringContent", async (input, branches, defaultNext, ct) =>
            {
                var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);
                var containsOnlyLetters = !string.IsNullOrEmpty(input) && input.All(char.IsLetter);
                var containsOnlySpecialChars = !string.IsNullOrEmpty(input) && input.All(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));

                if (containsOnlyDigits)
                {
                    await branches["DigitBranch"](input, ct);
                }
                else if (containsOnlyLetters)
                {
                    await branches["LetterBranch"](input, ct);
                }
                else if (containsOnlySpecialChars)
                {
                    await branches["SpecialCharBranch"](input, ct);
                }
                else
                {
                    // Mixed content - go to default branch
                    var charArray = input.ToCharArray();
                    await defaultNext(charArray, ct);
                }
            },
            space => new Dictionary<string, Pipeline<string>>
            {
                ["DigitBranch"] = space.GetPipeline<string>("DigitProcessingPipeline")!,
                ["LetterBranch"] = space.GetPipeline<string>("LetterProcessingPipeline")!,
                ["SpecialCharBranch"] = space.GetPipeline<string>("SpecialCharProcessingPipeline")!
            }.AsReadOnly(),
            space => space.GetPipeline<char[]>("DefaultProcessingPipeline")!)
            .BuildPipeline().Compile();

        // Act
        await pipeline(inputValue);

        // Assert
        if (expectedStringResult != "")
        {
            // Should call string handler (letter-only string)
            await stringHandler.Received().Invoke(Arg.Is(expectedStringResult!), Arg.Any<CancellationToken>());
            await intHandler.DidNotReceive().Invoke(Arg.Any<int>(), Arg.Any<CancellationToken>());
            await charArrayHandler.DidNotReceive().Invoke(Arg.Any<char[]>(), Arg.Any<CancellationToken>());
        }
        else if (expectedCharArrayResult.Length > 0)
        {
            // Should call char array handler (special char-only string)
            await charArrayHandler.Received().Invoke(Arg.Is<char[]>(arr => arr.SequenceEqual(expectedCharArrayResult)), Arg.Any<CancellationToken>());
            await intHandler.DidNotReceive().Invoke(Arg.Any<int>(), Arg.Any<CancellationToken>());
            await stringHandler.DidNotReceive().Invoke(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }
        else
        {
            // Should call int handler (digit-only string or mixed content)
            await intHandler.Received().Invoke(Arg.Is(expectedIntResult), Arg.Any<CancellationToken>());
            await stringHandler.DidNotReceive().Invoke(Arg.Any<string>(), Arg.Any<CancellationToken>());
            await charArrayHandler.DidNotReceive().Invoke(Arg.Any<char[]>(), Arg.Any<CancellationToken>());
        }
    }

}
