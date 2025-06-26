using NSubstitute;
using K1vs.DotNetPipe.Sync;
using System.Linq;

namespace K1vs.DotNetPipe.Tests.Sync;

public class WithoutMutationPipelineTests
{
    [Theory]
    [InlineData(-4)]
    [InlineData(0)]
    [InlineData(2)]
    public void BuildAndRunPipeline_WhenOneHandlerStep_ShouldRun(int value)
    {
        // Arrange
        var handler = Substitute.For<Func<int, ValueTask>>();
        handler.Invoke(Arg.Is(value)).Returns(ValueTask.CompletedTask);
        var pipeline = Pipelines.CreateSyncPipeline<int>("TestPipeline")
            .StartWithHandler("TestHandler", async (input) => await handler(input))
            .BuildPipeline().Compile();
        // Act
        pipeline(value);
        // Assert
        handler.Received().Invoke(Arg.Is(value));
    }

    [Theory]
    [InlineData(-4, 5, 1)]
    [InlineData(0, 10, 10)]
    [InlineData(2, 3, 5)]
    public void BuildAndRunPipeline_WhenLinearStepThenHandlerStep_ShouldRun(int inputValue, int constantToAdd, int expectedHandlerInput)
    {
        // Arrange
        var handler = Substitute.For<Action<int>>();
        handler.Invoke(Arg.Is(expectedHandlerInput));

        var pipeline = Pipelines.CreateSyncPipeline<int>("TestTwoStepPipeline")
            .StartWithLinear<int>("AddConstant", (input, next) =>
            {
                var result = input + constantToAdd;
                next(result);
            })
            .HandleWith("TestHandler", (input) => handler(input))
            .BuildPipeline().Compile();

        // Act
        pipeline(inputValue);

        // Assert
        handler.Received().Invoke(Arg.Is(expectedHandlerInput));
    }

    [Theory]
    [InlineData(2, 3, 2, 10)]  // (2 + 3) * 2 = 10
    [InlineData(0, 5, 3, 15)]  // (0 + 5) * 3 = 15
    [InlineData(-1, 4, 2, 6)]  // (-1 + 4) * 2 = 6
    [InlineData(10, -5, 4, 20)] // (10 + (-5)) * 4 = 20
    public void BuildAndRunPipeline_WhenTwoLinearStepsThenHandlerStep_ShouldRun(int inputValue, int constantToAdd, int multiplier, int expectedHandlerInput)
    {
        // Arrange
        var handler = Substitute.For<Action<int>>();
        handler.Invoke(Arg.Is(expectedHandlerInput));

        var pipeline = Pipelines.CreateSyncPipeline<int>("TestThreeStepPipeline")
            .StartWithLinear<int>("AddConstant", (input, next) =>
            {
                var result = input + constantToAdd;
                next(result);
            })
            .ThenLinear<int>("MultiplyByCoefficient", (input, next) =>
            {
                var result = input * multiplier;
                next(result);
            })
            .HandleWith("TestHandler", (input) => handler(input))
            .BuildPipeline().Compile();

        // Act
        pipeline(inputValue);

        // Assert
        handler.Received().Invoke(Arg.Is(expectedHandlerInput));
    }

    [Theory]
    [InlineData("5", 3, 8)]    // "5" -> 5, 5 + 3 = 8
    [InlineData("10", -2, 8)]  // "10" -> 10, 10 + (-2) = 8
    [InlineData("0", 5, 5)]    // "0" -> 0, 0 + 5 = 5
    [InlineData("abc", 3, 0)]  // "abc" -> doesn't parse, handler not called
    [InlineData("", 10, 0)]    // "" -> doesn't parse, handler not called
    public void BuildAndRunPipeline_WhenStringParseAndAddConstant_ShouldCallHandlerOnlyOnSuccessfulParse(string inputValue, int constantToAdd, int expectedCallCount)
    {
        // Arrange
        var handler = Substitute.For<Action<int>>();
        handler.Invoke(Arg.Any<int>());

        var pipeline = Pipelines.CreateSyncPipeline<string>("TestStringParsingPipeline")
            .StartWithLinear<int>("ParseString", (input, next) =>
            {
                if (int.TryParse(input, out var parsed))
                {
                    next(parsed);
                }
                // If doesn't parse - don't call next
            })
            .ThenLinear<int>("AddConstant", (input, next) =>
            {
                var result = input + constantToAdd;
                next(result);
            })
            .HandleWith("TestHandler", (input) => handler(input))
            .BuildPipeline().Compile();

        // Act
        pipeline(inputValue);

        // Assert
        if (expectedCallCount > 0)
        {
            handler.Received().Invoke(Arg.Is(expectedCallCount));
        }
        else
        {
            handler.DidNotReceive().Invoke(Arg.Any<int>());
        }
    }

    [Theory]
    [InlineData("  5  ", 3, 8)]     // "  5  " -> trim -> 5 (int) -> 5 + 3 = 8
    [InlineData(" 10 ", -2, 8)]     // " 10 " -> trim -> 10 (int) -> 10 + (-2) = 8
    [InlineData("3.7", 5, 9)]       // "3.7" -> trim -> 3.7 (float) -> round to 4 -> 4 + 5 = 9
    [InlineData(" 2.3 ", 1, 3)]     // " 2.3 " -> trim -> 2.3 (float) -> round to 2 -> 2 + 1 = 3
    [InlineData("5.5", 2, 8)]       // "5.5" -> trim -> 5.5 (float) -> round to 6 -> 6 + 2 = 8
    public void BuildAndRunPipeline_WhenIfStepHandlesIntAndFloat_ShouldProcessCorrectly(string inputValue, int constantToAdd, int expectedResult)
    {
        // Arrange
        var handler = Substitute.For<Action<int>>();
        handler.Invoke(Arg.Is(expectedResult));

        var pipeline = Pipelines.CreateSyncPipeline<string>("TestIfStepPipeline")
            .StartWithLinear<string>("TrimString", (input, next) =>
            {
                var trimmed = input.Trim();
                next(trimmed);
            })
            .ThenIf<string, int>("CheckIntOrFloat", (input, conditionalNext, next) =>
            {
                // Try to parse as int first
                if (int.TryParse(input, out var intValue))
                {
                    // If it's an int, continue with main pipeline
                    next(intValue);
                }
                else
                {
                    // If not an int, go to conditional pipeline (for float parsing)
                    conditionalNext(input);
                }
            }, space => space.CreatePipeline<string>("FloatProcessing")
                .StartWithLinear<double>("ParseFloat", (input, next) =>
                {
                    if (double.TryParse(input, out var floatValue))
                    {
                        next(floatValue);
                    }
                })
                .ThenLinear<int>("RoundToInt", (input, next) =>
                {
                    var rounded = (int)Math.Round(input);
                    next(rounded);
                })
                .BuildOpenPipeline())
            .ThenLinear<int>("AddConstant", (input, next) =>
            {
                var result = input + constantToAdd;
                next(result);
            })
            .HandleWith("TestHandler", (input) => handler(input))
            .BuildPipeline().Compile();

        // Act
        pipeline(inputValue);

        // Assert
        handler.Received().Invoke(Arg.Is(expectedResult));
    }

    [Theory]
    [InlineData("  5  ", 3, 2, 13)]    // "  5  " -> trim -> 5 (int) -> false branch -> 5 * 2 = 10 -> 10 + 3 = 13
    [InlineData(" 10 ", -2, 4, 38)]    // " 10 " -> trim -> 10 (int) -> false branch -> 10 * 4 = 40 -> 40 + (-2) = 38
    [InlineData("3.7", 5, 3, 9)]       // "3.7" -> trim -> 3.7 (float) -> true branch -> round to 4 -> 4 + 5 = 9
    [InlineData(" 2.3 ", 1, 5, 3)]     // " 2.3 " -> trim -> 2.3 (float) -> true branch -> round to 2 -> 2 + 1 = 3
    [InlineData("5.5", 2, 7, 8)]       // "5.5" -> trim -> 5.5 (float) -> true branch -> round to 6 -> 6 + 2 = 8
    public void BuildAndRunPipeline_WhenIfElseStepHandlesIntFloatOrDefault_ShouldProcessCorrectly(string inputValue, int constantToAdd, int multiplier, int expectedResult)
    {
        // Arrange
        var handler = Substitute.For<Action<int>>();
        handler.Invoke(Arg.Is(expectedResult));

        var pipeline = Pipelines.CreateSyncPipeline<string>("TestIfElseStepPipeline")
            .StartWithLinear<string>("TrimString", (input, next) =>
            {
                var trimmed = input.Trim();
                next(trimmed);
            })
            .ThenIfElse<string, int, int>("CheckIntOrFloat", (input, trueNext, falseNext) =>
            {
                // Try to parse as int first
                if (int.TryParse(input, out var intValue))
                {
                    // If it's an int, we actually want to bypass both branches and continue directly
                    // But since IfElse requires going through one of the branches, we'll use false branch for int
                    falseNext(intValue);
                }
                else
                {
                    // If not an int, go to true branch (for float parsing)
                    trueNext(input);
                }
            },
            // True branch - float processing
            space => space.CreatePipeline<string>("FloatProcessing")
                .StartWithLinear<double>("ParseFloat", (input, next) =>
                {
                    if (double.TryParse(input, out var floatValue))
                    {
                        next(floatValue);
                    }
                })
                .ThenLinear<int>("RoundToInt", (input, next) =>
                {
                    var rounded = (int)Math.Round(input);
                    next(rounded);
                })
                .BuildOpenPipeline(),
            // False branch - multiply by multiplier
            space => space.CreatePipeline<int>("IntOrDefaultProcessing")
                .StartWithLinear<int>("ParseIntOrDefault", (input, next) =>
                {
                    // If we got here, it means we parsed as int in the false branch
                    // So we just pass it through
                    next(input * multiplier);
                })
                .BuildOpenPipeline())
            .ThenLinear<int>("AddConstant", (input, next) =>
            {
                var result = input + constantToAdd;
                next(result);
            })
            .HandleWith("TestHandler", (input) => handler(input))
            .BuildPipeline().Compile();

        // Act
        pipeline(inputValue);

        // Assert
        handler.Received().Invoke(Arg.Is(expectedResult));
    }

    [Theory]
    [InlineData("105", 315)]    // >100 -> *3 = 315
    [InlineData("50", 52)]      // 0<x<100 -> +2 = 52
    [InlineData("-5", -10)]     // <0 -> *2 = -10
    [InlineData("0", 0)]        // =0 -> stay 0
    [InlineData("abc", 3)]      // not a number -> string length = 3
    [InlineData("hello", 5)]    // not a number -> string length = 5
    [InlineData("", 0)]         // empty string -> length = 0
    public void BuildAndRunPipeline_WhenSwitchStepRoutesByNumberRange_ShouldProcessCorrectly(string inputValue, int expectedResult)
    {
        // Arrange
        var handler = Substitute.For<Action<int>>();
        handler.Invoke(Arg.Is(expectedResult));

        var space = Pipelines.CreateSyncSpace();
        var defaultPipeline = space.CreatePipeline<int>("StringLengthPipeline")
            .StartWithLinear<int>("IdentityOperation", (input, next) =>
            {
                next(input); // Use string length as-is
            })
            .BuildOpenPipeline();

        var pipeline = space.CreatePipeline<string>("TestSwitchPipeline")
            .StartWithLinear<string>("TrimString", (input, next) =>
            {
                var trimmed = input.Trim();
                next(trimmed);
            })
            .ThenSwitch<int, int, int>("NumberRangeSwitch", (input, cases, defaultNext) =>
            {
                // Try to parse as integer
                if (int.TryParse(input, out var number))
                {
                    if (number > 100)
                    {
                        cases["GreaterThan100"](number);
                    }
                    else if (number > 0)
                    {
                        cases["BetweenZeroAndHundred"](number);
                    }
                    else if (number < 0)
                    {
                        cases["LessThanZero"](number);
                    }
                    else // number == 0
                    {
                        cases["EqualToZero"](number);
                    }
                }
                else
                {
                    // If not a number, use string length
                    var stringLength = input.Length;
                    defaultNext(stringLength);
                }
            },
            space => new Dictionary<string, OpenPipeline<int, int>>
            {
                ["GreaterThan100"] = space.CreatePipeline<int>("MultiplyByThree")
                    .StartWithLinear<int>("MultiplyOperation", (input, next) =>
                    {
                        var result = input * 3;
                        next(result);
                    })
                    .BuildOpenPipeline(),
                ["BetweenZeroAndHundred"] = space.CreatePipeline<int>("AddTwo")
                    .StartWithLinear<int>("AddOperation", (input, next) =>
                    {
                        var result = input + 2;
                        next(result);
                    })
                    .BuildOpenPipeline(),
                ["LessThanZero"] = space.CreatePipeline<int>("MultiplyByTwo")
                    .StartWithLinear<int>("MultiplyOperation", (input, next) =>
                    {
                        var result = input * 2;
                        next(result);
                    })
                    .BuildOpenPipeline(),
                ["EqualToZero"] = space.CreatePipeline<int>("KeepZero")
                    .StartWithLinear<int>("IdentityOperation", (input, next) =>
                    {
                        next(input); // Keep the same value (0)
                    })
                    .BuildOpenPipeline()
            }.AsReadOnly(),
            defaultPipeline)
            .HandleWith("TestHandler", (input) => handler(input))
            .BuildPipeline().Compile();

        // Act
        pipeline(inputValue);

        // Assert
        handler.Received().Invoke(Arg.Is(expectedResult));
    }

    [Theory]
    [InlineData("123", 123, "")]             // Only digits -> first branch -> parse to int -> 123
    [InlineData("  456  ", 456, "")]         // Only digits (with spaces) -> first branch -> 456
    [InlineData("abc123def", 0, "  abcdef  ")] // Mixed -> second branch -> remove digits -> add spaces
    [InlineData("hello", 0, "  hello  ")]    // No digits -> second branch -> add spaces
    [InlineData("", 0, "    ")]              // Empty -> second branch -> add spaces
    [InlineData("!@#", 0, "  !@#  ")]        // Special chars -> second branch -> add spaces
    public void BuildAndRunPipeline_WhenForkSplitsByDigitContent_ShouldProcessCorrectly(string inputValue, int expectedIntResult, string? expectedStringResult)
    {
        // Arrange
        var intHandler = Substitute.For<Action<int>>();
        var stringHandler = Substitute.For<Action<string>>();
        intHandler.Invoke(Arg.Any<int>());
        stringHandler.Invoke(Arg.Any<string>());

        var pipeline = Pipelines.CreateSyncPipeline<string>("TestForkPipeline")
            .StartWithLinear<string>("TrimString", (input, next) =>
            {
                var trimmed = input.Trim();
                next(trimmed);
            })
            .ThenFork<string, string>("DigitContentFork", (input, digitBranch, nonDigitBranch) =>
            {
                // Check if string contains only digits (after trimming)
                var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);

                if (containsOnlyDigits)
                {
                    digitBranch(input);
                }
                else
                {
                    nonDigitBranch(input);
                }
            },
            // Digit processing branch
            space => space.CreatePipeline<string>("DigitProcessing")
                .StartWithLinear<string>("RemoveNonDigits", (input, next) =>
                {
                    var digitsOnly = new string(input.Where(char.IsDigit).ToArray());
                    next(digitsOnly);
                })
                .ThenLinear<int>("ParseToInt", (input, next) =>
                {
                    if (int.TryParse(input, out var number))
                    {
                        next(number);
                    }
                    else
                    {
                        next(0); // Default to 0 if parsing fails
                    }
                })
                .HandleWith("IntHandler", (input) => intHandler(input))
                .BuildPipeline(),
            // Non-digit processing branch
            space => space.CreatePipeline<string>("NonDigitProcessing")
                .StartWithLinear<string>("RemoveDigits", (input, next) =>
                {
                    var nonDigitsOnly = new string(input.Where(c => !char.IsDigit(c)).ToArray());
                    next(nonDigitsOnly);
                })
                .ThenLinear<string>("AddSpaces", (input, next) =>
                {
                    var withSpaces = $"  {input}  ";
                    next(withSpaces);
                })
                .HandleWith("StringHandler", (input) => stringHandler(input))
                .BuildPipeline())
            .BuildPipeline().Compile();

        // Act
        pipeline(inputValue);

        // Assert
        if (expectedStringResult == "")
        {
            // Should call int handler (digit-only string)
            intHandler.Received().Invoke(Arg.Is(expectedIntResult));
            stringHandler.DidNotReceive().Invoke(Arg.Any<string>());
        }
        else
        {
            // Should call string handler (non-digit string)
            stringHandler.Received().Invoke(Arg.Is(expectedStringResult!));
            intHandler.DidNotReceive().Invoke(Arg.Any<int>());
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
    public void BuildAndRunPipeline_WhenMultiForkClassifiesStringContent_ShouldProcessCorrectly(
        string inputValue,
        int expectedIntResult,
        string? expectedStringResult,
        char[] expectedCharArrayResult)
    {
        // Arrange
        var intHandler = Substitute.For<Action<int>>();
        var stringHandler = Substitute.For<Action<string>>();
        var charArrayHandler = Substitute.For<Action<char[]>>();

        intHandler.Invoke(Arg.Any<int>());
        stringHandler.Invoke(Arg.Any<string>());
        charArrayHandler.Invoke(Arg.Any<char[]>());

        var space = Pipelines.CreateSyncSpace();

        // Create sub-pipelines before main pipeline
        space.CreatePipeline<string>("DigitProcessingPipeline")
            .StartWithLinear<int>("ParseStringToInt", (input, next) =>
            {
                if (int.TryParse(input, out var number))
                {
                    next(number);
                }
                else
                {
                    next(0); // Default value if parsing fails
                }
            })
            .ThenLinear<int>("AddConstant", (input, next) =>
            {
                var result = input + 10; // Add constant 10
                next(result);
            })
            .HandleWith("IntHandler", (input) => intHandler(input))
            .BuildPipeline();

        space.CreatePipeline<string>("LetterProcessingPipeline")
            .StartWithLinear<string>("AddSpaces", (input, next) =>
            {
                var withSpaces = $"  {input}  ";
                next(withSpaces);
            })
            .HandleWith("StringHandler", (input) => stringHandler(input))
            .BuildPipeline();

        space.CreatePipeline<string>("SpecialCharProcessingPipeline")
            .StartWithLinear<string>("RemoveWhitespace", (input, next) =>
            {
                var noWhitespace = new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray());
                next(noWhitespace);
            })
            .ThenLinear<char[]>("ConvertToCharArray", (input, next) =>
            {
                var charArray = input.ToCharArray();
                next(charArray);
            })
            .ThenLinear<char[]>("RemoveDuplicates", (input, next) =>
            {
                var uniqueChars = input.Distinct().ToArray();
                next(uniqueChars);
            })
            .HandleWith("CharArrayHandler", (input) => charArrayHandler(input))
            .BuildPipeline();
        var defaultPipeline = space.CreatePipeline<char[]>("DefaultProcessingPipeline")
            .StartWithLinear<(int DigitCount, int LetterCount)>("CountDigitsAndLetters", (input, next) =>
            {
                var digitCount = input.Count(char.IsDigit);
                var letterCount = input.Count(char.IsLetter);
                next((digitCount, letterCount));
            })
            .ThenLinear<int>("CalculateRatio", (input, next) =>
            {
                // Calculate ratio of digits to letters (floor division)
                var ratio = input.LetterCount > 0 ? input.DigitCount / input.LetterCount : input.DigitCount;
                next(ratio);
            })
            .HandleWith("IntHandler", (input) => intHandler(input))
            .BuildPipeline();

        var pipeline = space.CreatePipeline<string>("TestMultiForkPipeline")
            .StartWithLinear<string>("TrimString", (input, next) =>
            {
                var trimmed = input.Trim();
                next(trimmed);
            })
            .ThenMultiFork<string, char[]>("ClassifyStringContent", (input, branches, defaultNext) =>
            {
                var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);
                var containsOnlyLetters = !string.IsNullOrEmpty(input) && input.All(char.IsLetter);
                var containsOnlySpecialChars = !string.IsNullOrEmpty(input) && input.All(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));

                if (containsOnlyDigits)
                {
                    branches["DigitBranch"](input);
                }
                else if (containsOnlyLetters)
                {
                    branches["LetterBranch"](input);
                }
                else if (containsOnlySpecialChars)
                {
                    branches["SpecialCharBranch"](input);
                }
                else
                {
                    // Mixed content - go to default branch
                    var charArray = input.ToCharArray();
                    defaultNext(charArray);
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
        pipeline(inputValue);

        // Assert
        if (expectedStringResult != "")
        {
            // Should call string handler (letter-only string)
            stringHandler.Received().Invoke(Arg.Is(expectedStringResult!));
            intHandler.DidNotReceive().Invoke(Arg.Any<int>());
            charArrayHandler.DidNotReceive().Invoke(Arg.Any<char[]>());
        }
        else if (expectedCharArrayResult.Length > 0)
        {
            // Should call char array handler (special char-only string)
            charArrayHandler.Received().Invoke(Arg.Is<char[]>(arr => arr.SequenceEqual(expectedCharArrayResult)));
            intHandler.DidNotReceive().Invoke(Arg.Any<int>());
            stringHandler.DidNotReceive().Invoke(Arg.Any<string>());
        }
        else
        {
            // Should call int handler (digit-only string or mixed content)
            intHandler.Received().Invoke(Arg.Is(expectedIntResult));
            stringHandler.DidNotReceive().Invoke(Arg.Any<string>());
            charArrayHandler.DidNotReceive().Invoke(Arg.Any<char[]>());
        }
    }

}
