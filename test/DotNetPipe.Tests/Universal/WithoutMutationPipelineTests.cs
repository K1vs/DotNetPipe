using NSubstitute;
using K1vs.DotNetPipe.Universal;
using System.Linq;

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
        var pipeline = Pipelines.CreatePipeline<int>("TestPipeline")
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

        var pipeline = Pipelines.CreatePipeline<int>("TestTwoStepPipeline")
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

        var pipeline = Pipelines.CreatePipeline<int>("TestThreeStepPipeline")
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

        var pipeline = Pipelines.CreatePipeline<string>("TestStringParsingPipeline")
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

        var pipeline = Pipelines.CreatePipeline<string>("TestIfStepPipeline")
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

        var pipeline = Pipelines.CreatePipeline<string>("TestIfElseStepPipeline")
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
        var handler = Substitute.For<Func<int, ValueTask>>();
        handler.Invoke(Arg.Is(expectedResult)).Returns(ValueTask.CompletedTask);

        var space = new Space();
        var defaultPipeline = space.CreatePipeline<int>("StringLengthPipeline")
            .StartWithLinear<int>("IdentityOperation", async (input, next) =>
            {
                await next(input); // Use string length as-is
            })
            .BuildOpenPipeline();

        var pipeline = Pipelines.CreatePipeline<string>("TestSwitchPipeline")
            .StartWithLinear<string>("TrimString", async (input, next) =>
            {
                var trimmed = input.Trim();
                await next(trimmed);
            })
            .ThenSwitch<int, int, int>("NumberRangeSwitch", async (input, cases, defaultNext) =>
            {
                // Try to parse as integer
                if (int.TryParse(input, out var number))
                {
                    if (number > 100)
                    {
                        await cases["GreaterThan100"](number);
                    }
                    else if (number > 0)
                    {
                        await cases["BetweenZeroAndHundred"](number);
                    }
                    else if (number < 0)
                    {
                        await cases["LessThanZero"](number);
                    }
                    else // number == 0
                    {
                        await cases["EqualToZero"](number);
                    }
                }
                else
                {
                    // If not a number, use string length
                    var stringLength = input.Length;
                    await defaultNext(stringLength);
                }
            },
            space => new Dictionary<string, OpenPipeline<int, int>>
            {
                ["GreaterThan100"] = space.CreatePipeline<int>("MultiplyByThree")
                    .StartWithLinear<int>("MultiplyOperation", async (input, next) =>
                    {
                        var result = input * 3;
                        await next(result);
                    })
                    .BuildOpenPipeline(),
                ["BetweenZeroAndHundred"] = space.CreatePipeline<int>("AddTwo")
                    .StartWithLinear<int>("AddOperation", async (input, next) =>
                    {
                        var result = input + 2;
                        await next(result);
                    })
                    .BuildOpenPipeline(),
                ["LessThanZero"] = space.CreatePipeline<int>("MultiplyByTwo")
                    .StartWithLinear<int>("MultiplyOperation", async (input, next) =>
                    {
                        var result = input * 2;
                        await next(result);
                    })
                    .BuildOpenPipeline(),
                ["EqualToZero"] = space.CreatePipeline<int>("KeepZero")
                    .StartWithLinear<int>("IdentityOperation", async (input, next) =>
                    {
                        await next(input); // Keep the same value (0)
                    })
                    .BuildOpenPipeline()
            }.AsReadOnly(),
            defaultPipeline)
            .HandleWith("TestHandler", async (input) => await handler(input))
            .BuildPipeline().Compile();

        // Act
        await pipeline(inputValue);

        // Assert
        await handler.Received().Invoke(Arg.Is(expectedResult));
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
        var intHandler = Substitute.For<Func<int, ValueTask>>();
        var stringHandler = Substitute.For<Func<string, ValueTask>>();
        intHandler.Invoke(Arg.Any<int>()).Returns(ValueTask.CompletedTask);
        stringHandler.Invoke(Arg.Any<string>()).Returns(ValueTask.CompletedTask);

        var pipeline = Pipelines.CreatePipeline<string>("TestForkPipeline")
            .StartWithLinear<string>("TrimString", async (input, next) =>
            {
                var trimmed = input.Trim();
                await next(trimmed);
            })
            .ThenFork<string, string>("DigitContentFork", async (input, digitBranch, nonDigitBranch) =>
            {
                // Check if string contains only digits (after trimming)
                var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);

                if (containsOnlyDigits)
                {
                    await digitBranch(input);
                }
                else
                {
                    await nonDigitBranch(input);
                }
            },
            // Digit processing branch
            space => space.CreatePipeline<string>("DigitProcessing")
                .StartWithLinear<string>("RemoveNonDigits", async (input, next) =>
                {
                    var digitsOnly = new string(input.Where(char.IsDigit).ToArray());
                    await next(digitsOnly);
                })
                .ThenLinear<int>("ParseToInt", async (input, next) =>
                {
                    if (int.TryParse(input, out var number))
                    {
                        await next(number);
                    }
                    else
                    {
                        await next(0); // Default to 0 if parsing fails
                    }
                })
                .HandleWith("IntHandler", async (input) => await intHandler(input))
                .BuildPipeline(),
            // Non-digit processing branch
            space => space.CreatePipeline<string>("NonDigitProcessing")
                .StartWithLinear<string>("RemoveDigits", async (input, next) =>
                {
                    var nonDigitsOnly = new string(input.Where(c => !char.IsDigit(c)).ToArray());
                    await next(nonDigitsOnly);
                })
                .ThenLinear<string>("AddSpaces", async (input, next) =>
                {
                    var withSpaces = $"  {input}  ";
                    await next(withSpaces);
                })
                .HandleWith("StringHandler", async (input) => await stringHandler(input))
                .BuildPipeline())
            .BuildPipeline().Compile();

        // Act
        await pipeline(inputValue);

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
        var intHandler = Substitute.For<Func<int, ValueTask>>();
        var stringHandler = Substitute.For<Func<string, ValueTask>>();
        var charArrayHandler = Substitute.For<Func<char[], ValueTask>>();

        intHandler.Invoke(Arg.Any<int>()).Returns(ValueTask.CompletedTask);
        stringHandler.Invoke(Arg.Any<string>()).Returns(ValueTask.CompletedTask);
        charArrayHandler.Invoke(Arg.Any<char[]>()).Returns(ValueTask.CompletedTask);

        var space = Pipelines.CreateSpace();

        // Create sub-pipelines before main pipeline
        space.CreatePipeline<string>("DigitProcessingPipeline")
            .StartWithLinear<int>("ParseStringToInt", async (input, next) =>
            {
                if (int.TryParse(input, out var number))
                {
                    await next(number);
                }
                else
                {
                    await next(0); // Default value if parsing fails
                }
            })
            .ThenLinear<int>("AddConstant", async (input, next) =>
            {
                var result = input + 10; // Add constant 10
                await next(result);
            })
            .HandleWith("IntHandler", async (input) => await intHandler(input))
            .BuildPipeline();

        space.CreatePipeline<string>("LetterProcessingPipeline")
            .StartWithLinear<string>("AddSpaces", async (input, next) =>
            {
                var withSpaces = $"  {input}  ";
                await next(withSpaces);
            })
            .HandleWith("StringHandler", async (input) => await stringHandler(input))
            .BuildPipeline();

        space.CreatePipeline<string>("SpecialCharProcessingPipeline")
            .StartWithLinear<string>("RemoveWhitespace", async (input, next) =>
            {
                var noWhitespace = new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray());
                await next(noWhitespace);
            })
            .ThenLinear<char[]>("ConvertToCharArray", async (input, next) =>
            {
                var charArray = input.ToCharArray();
                await next(charArray);
            })
            .ThenLinear<char[]>("RemoveDuplicates", async (input, next) =>
            {
                var uniqueChars = input.Distinct().ToArray();
                await next(uniqueChars);
            })
            .HandleWith("CharArrayHandler", async (input) => await charArrayHandler(input))
            .BuildPipeline();        var defaultPipeline = space.CreatePipeline<char[]>("DefaultProcessingPipeline")
            .StartWithLinear<(int DigitCount, int LetterCount)>("CountDigitsAndLetters", async (input, next) =>
            {
                var digitCount = input.Count(char.IsDigit);
                var letterCount = input.Count(char.IsLetter);
                await next((digitCount, letterCount));
            })
            .ThenLinear<int>("CalculateRatio", async (input, next) =>
            {
                // Calculate ratio of digits to letters (floor division)
                var ratio = input.LetterCount > 0 ? input.DigitCount / input.LetterCount : input.DigitCount;
                await next(ratio);
            })
            .HandleWith("IntHandler", async (input) => await intHandler(input))
            .BuildPipeline();

        var pipeline = space.CreatePipeline<string>("TestMultiForkPipeline")
            .StartWithLinear<string>("TrimString", async (input, next) =>
            {
                var trimmed = input.Trim();
                await next(trimmed);
            })
            .ThenMultiFork<string, char[]>("ClassifyStringContent", async (input, branches, defaultNext) =>
            {
                var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);
                var containsOnlyLetters = !string.IsNullOrEmpty(input) && input.All(char.IsLetter);
                var containsOnlySpecialChars = !string.IsNullOrEmpty(input) && input.All(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));

                if (containsOnlyDigits)
                {
                    await branches["DigitBranch"](input);
                }
                else if (containsOnlyLetters)
                {
                    await branches["LetterBranch"](input);
                }
                else if (containsOnlySpecialChars)
                {
                    await branches["SpecialCharBranch"](input);
                }
                else
                {
                    // Mixed content - go to default branch
                    var charArray = input.ToCharArray();
                    await defaultNext(charArray);
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
            await stringHandler.Received().Invoke(Arg.Is(expectedStringResult!));
            await intHandler.DidNotReceive().Invoke(Arg.Any<int>());
            await charArrayHandler.DidNotReceive().Invoke(Arg.Any<char[]>());
        }
        else if (expectedCharArrayResult.Length > 0)
        {
            // Should call char array handler (special char-only string)
            await charArrayHandler.Received().Invoke(Arg.Is<char[]>(arr => arr.SequenceEqual(expectedCharArrayResult)));
            await intHandler.DidNotReceive().Invoke(Arg.Any<int>());
            await stringHandler.DidNotReceive().Invoke(Arg.Any<string>());
        }
        else
        {
            // Should call int handler (digit-only string or mixed content)
            await intHandler.Received().Invoke(Arg.Is(expectedIntResult));
            await stringHandler.DidNotReceive().Invoke(Arg.Any<string>());
            await charArrayHandler.DidNotReceive().Invoke(Arg.Any<char[]>());
        }
    }

}
