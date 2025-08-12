using K1vs.DotNetPipe.ReturningAsync;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;
using System;
using System.Linq;

namespace K1vs.DotNetPipe.Tests.ReturningAsync;

public class WithoutMutationPipelineTests
{
    [Theory]
    [InlineData(-4, 8)]
    [InlineData(0, 0)]
    [InlineData(2, -4)]
    public async Task BuildAndRunPipeline_WhenOneFuncStep_ShouldReturnResult(int value, int expectedResult)
    {
        // Arrange
        var pipeline = Pipelines.CreateReturningAsyncPipeline<int, int>("TestPipeline")
            .StartWithHandler("TestHandler", async (input) =>
            {
                var result = input * -2;
                return await Task.FromResult(result);
            })
            .BuildPipeline().Compile();

        // Act
        var actualResult = await pipeline(value);

        // Assert
        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData(-4, 5, 2)]
    [InlineData(0, 10, 20)]
    [InlineData(2, 3, 10)]
    public async Task BuildAndRunPipeline_WhenLinearStepThenFuncStep_ShouldReturnResult(int inputValue, int constantToAdd, int expectedResult)
    {
        // Arrange
        var pipeline = Pipelines.CreateReturningAsyncPipeline<int, int>("TestTwoStepPipeline")
            .StartWithLinear<int, int>("AddConstant", async (input, next) =>
            {
                var result = input + constantToAdd;
                return await next(result);
            })
            .HandleWith("TestHandler", async (input) =>
            {
                var result = input * 2;
                return await Task.FromResult(result);
            })
            .BuildPipeline().Compile();

        // Act
        var actualResult = await pipeline(inputValue);

        // Assert
        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData(2, 3, 2, 10)]  // (2 + 3) * 2 = 10
    [InlineData(0, 5, 3, 15)]  // (0 + 5) * 3 = 15
    [InlineData(-1, 4, 2, 6)]  // (-1 + 4) * 2 = 6
    [InlineData(10, -5, 4, 20)] // (10 + (-5)) * 4 = 20
    public async Task BuildAndRunPipeline_WhenTwoLinearStepsThenFuncStep_ShouldReturnResult(int inputValue, int constantToAdd, int multiplier, int expectedResult)
    {
        // Arrange
        var pipeline = Pipelines.CreateReturningAsyncPipeline<int, int>("TestThreeStepPipeline")
            .StartWithLinear<int, int>("AddConstant", async (input, next) =>
            {
                var result = input + constantToAdd;
                return await next(result);
            })
            .ThenLinear<int, int>("MultiplyByCoefficient", async (input, next) =>
            {
                var result = input * multiplier;
                return await next(result);
            })
            .HandleWith("TestHandler", async (input) =>
            {
                return await Task.FromResult(input);
            })
            .BuildPipeline().Compile();

        // Act
        var actualResult = await pipeline(inputValue);

        // Assert
        Assert.Equal(expectedResult, actualResult);
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
        var pipeline = Pipelines.CreateReturningAsyncPipeline<string, int>("TestIfStepPipeline")
            .StartWithLinear<string, int>("TrimString", async (input, next) =>
            {
                var trimmed = input.Trim();
                return await next(trimmed);
            })
            .ThenIf<string, int, int, int>("CheckIntOrFloat", async (input, conditionalNext, next) =>
            {
                // Try to parse as int first
                if (int.TryParse(input, out var intValue))
                {
                    // If it's an int, continue with main pipeline
                    return await next(intValue);
                }
                else
                {
                    // If not an int, go to conditional pipeline (for float parsing)
                    return await conditionalNext(input);
                }
            }, space => space.CreatePipeline<string, int>("FloatProcessing")
                .StartWithLinear<double, int>("ParseFloat", async (input, next) =>
                {
                    if (double.TryParse(input, out var floatValue))
                    {
                        return await next(floatValue);
                    }
                    return 0; // Should not happen in this test
                })
                .ThenLinear<int, int>("RoundToInt", async (input, next) =>
                {
                    var rounded = (int)Math.Round(input);
                    return await next(rounded);
                })
                .BuildOpenPipeline())
            .ThenLinear<int, int>("AddConstant", async (input, next) =>
            {
                var result = input + constantToAdd;
                return await next(result);
            })
            .HandleWith("TestHandler", async (input) => await Task.FromResult(input))
            .BuildPipeline().Compile();

        // Act
        var actualResult = await pipeline(inputValue);

        // Assert
        Assert.Equal(expectedResult, actualResult);
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
        var pipeline = Pipelines.CreateReturningAsyncPipeline<string, int>("TestIfElseStepPipeline")
            .StartWithLinear<string, int>("TrimString", async (input, next) =>
            {
                var trimmed = input.Trim();
                return await next(trimmed);
            })
            .ThenIfElse<string, int, int, int, int, int>("CheckIntOrFloat", async (input, trueNext, falseNext) =>
            {
                // Try to parse as int first
                if (int.TryParse(input, out var intValue))
                {
                    // If it's an int, go to false branch
                    return await falseNext(intValue);
                }
                else
                {
                    // If not an int, go to true branch (for float parsing)
                    return await trueNext(input);
                }
            },
            // True branch - float processing
            space => space.CreatePipeline<string, int>("FloatProcessing")
                .StartWithLinear<double, int>("ParseFloat", async (input, next) =>
                {
                    if (double.TryParse(input, out var floatValue))
                    {
                        return await next(floatValue);
                    }
                    return 0; // Should not happen
                })
                .ThenLinear<int, int>("RoundToInt", async (input, next) =>
                {
                    var rounded = (int)Math.Round(input);
                    return await next(rounded);
                })
                .BuildOpenPipeline(),
            // False branch - multiply by multiplier
            space => space.CreatePipeline<int, int>("IntOrDefaultProcessing")
                .StartWithLinear<int, int>("ParseIntOrDefault", async (input, next) =>
                {
                    return await next(input * multiplier);
                })
                .BuildOpenPipeline())
            .ThenLinear<int, int>("AddConstant", async (input, next) =>
            {
                var result = input + constantToAdd;
                return await next(result);
            })
            .HandleWith("TestHandler", async (input) => await Task.FromResult(input))
            .BuildPipeline().Compile();

        // Act
        var actualResult = await pipeline(inputValue);

        // Assert
        Assert.Equal(expectedResult, actualResult);
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
        var space = Pipelines.CreateReturningAsyncSpace();
        var defaultPipeline = space.CreatePipeline<int, int>("StringLengthPipeline")
            .StartWithLinear<int, int>("IdentityOperation", async (input, next) =>
            {
                return await next(input); // Use string length as-is
            })
            .BuildOpenPipeline();

        var pipeline = space.CreatePipeline<string, int>("TestSwitchPipeline")
            .StartWithLinear<string, int>("TrimString", async (input, next) =>
            {
                var trimmed = input.Trim();
                return await next(trimmed);
            })
            .ThenSwitch<int, int, int, int, int, int>("NumberRangeSwitch", async (input, cases, defaultNext) =>
            {
                // Try to parse as integer
                if (int.TryParse(input, out var number))
                {
                    if (number > 100)
                    {
                        return await cases["GreaterThan100"](number);
                    }
                    else if (number > 0)
                    {
                        return await cases["BetweenZeroAndHundred"](number);
                    }
                    else if (number < 0)
                    {
                        return await cases["LessThanZero"](number);
                    }
                    else // number == 0
                    {
                        return await cases["EqualToZero"](number);
                    }
                }
                else
                {
                    // If not a number, use string length
                    var stringLength = input.Length;
                    return await defaultNext(stringLength);
                }
            },
            space => new Dictionary<string, OpenPipeline<int, int, int, int>>
            {
                ["GreaterThan100"] = space.CreatePipeline<int, int>("MultiplyByThree")
                    .StartWithLinear<int, int>("MultiplyOperation", async (input, next) =>
                    {
                        var result = input * 3;
                        return await next(result);
                    })
                    .BuildOpenPipeline(),
                ["BetweenZeroAndHundred"] = space.CreatePipeline<int, int>("AddTwo")
                    .StartWithLinear<int, int>("AddOperation", async (input, next) =>
                    {
                        var result = input + 2;
                        return await next(result);
                    })
                    .BuildOpenPipeline(),
                ["LessThanZero"] = space.CreatePipeline<int, int>("MultiplyByTwo")
                    .StartWithLinear<int, int>("MultiplyOperation", async (input, next) =>
                    {
                        var result = input * 2;
                        return await next(result);
                    })
                    .BuildOpenPipeline(),
                ["EqualToZero"] = space.CreatePipeline<int, int>("KeepZero")
                    .StartWithLinear<int, int>("IdentityOperation", async (input, next) =>
                    {
                        return await next(input); // Keep the same value (0)
                    })
                    .BuildOpenPipeline()
            }.AsReadOnly(),
            defaultPipeline)
            .HandleWith("TestHandler", async (input) => await Task.FromResult(input))
            .BuildPipeline().Compile();

        // Act
        var actualResult = await pipeline(inputValue);

        // Assert
        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData("123", 123, null)]             // Only digits -> first branch -> parse to int -> 123
    [InlineData("  456  ", 456, null)]         // Only digits (with spaces) -> first branch -> 456
    [InlineData("abc123def", null, "  abcdef  ")] // Mixed -> second branch -> remove digits -> add spaces
    [InlineData("hello", null, "  hello  ")]    // No digits -> second branch -> add spaces
    [InlineData("", null, "    ")]              // Empty -> second branch -> add spaces
    [InlineData("!@#", null, "  !@#  ")]        // Special chars -> second branch -> add spaces
    public async Task BuildAndRunPipeline_WhenForkSplitsByDigitContent_ShouldProcessCorrectly(string inputValue, int? expectedIntResult, string? expectedStringResult)
    {
        // Arrange
        var pipeline = Pipelines.CreateReturningAsyncPipeline<string, (int?, string?)>("TestForkPipeline")
            .StartWithLinear<string, (int?, string?)>("TrimString", async (input, next) =>
            {
                var trimmed = input.Trim();
                return await next(trimmed);
            })
            .ThenFork<string, (int?, string?), string, (int?, string?)>("DigitContentFork", async (input, digitBranch, nonDigitBranch) =>
            {
                // Check if string contains only digits (after trimming)
                var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);

                if (containsOnlyDigits)
                {
                    return await digitBranch(input);
                }
                else
                {
                    return await nonDigitBranch(input);
                }
            },
            // Digit processing branch
            space => space.CreatePipeline<string, (int?, string?)>("DigitProcessing")
                .StartWithHandler("IntHandler", async (input) => await Task.FromResult<(int?, string?)>((int.Parse(input), null)))
                .BuildPipeline(),
            // Non-digit processing branch
            space => space.CreatePipeline<string, (int?, string?)>("NonDigitProcessing")
                .StartWithLinear<string, (int?, string?)>("RemoveDigits", async (input, next) =>
                {
                    var nonDigitsOnly = new string(input.Where(c => !char.IsDigit(c)).ToArray());
                    return await next(nonDigitsOnly);
                })
                .ThenLinear<string, (int?, string?)>("AddSpaces", async (input, next) =>
                {
                    var withSpaces = $"  {input}  ";
                    return await next(withSpaces);
                })
                .HandleWith("StringHandler", async (input) => await Task.FromResult<(int?, string?)>((null, input)))
                .BuildPipeline())
            .BuildPipeline().Compile();

        // Act
        var actualResult = await pipeline(inputValue);

        // Assert
        if (expectedIntResult.HasValue)
        {
            Assert.Equal(expectedIntResult, actualResult.Item1);
            Assert.Null(actualResult.Item2);
        }
        else
        {
            Assert.Null(actualResult.Item1);
            Assert.Equal(expectedStringResult, actualResult.Item2);
        }
    }

    [Theory]
    [InlineData("123", 133, null, null)]             // Only digits -> first branch -> parse to int (123) + 10 = 133
    [InlineData("  456  ", 466, null, null)]         // Only digits (with spaces) -> first branch -> 466
    [InlineData("abc", null, "  abc  ", null)]        // Only letters -> second branch -> add spaces
    [InlineData("xyz", null, "  xyz  ", null)]        // Only letters -> second branch -> add spaces
    [InlineData("!@#", null, null, new char[] { '!', '@', '#' })] // Only special chars -> third branch -> remove whitespace, convert to array, remove duplicates
    [InlineData("@@@", null, null, new char[] { '@' })]        // Special chars with duplicates -> third branch -> unique chars only    [InlineData("a1b2", 1, "", new char[0])]              // Mixed -> default branch -> 2 digits, 2 letters -> ratio = 1
    [InlineData("hello123", null, null, new char[] { 'h', 'e', 'l', 'o', '1', '2', '3' })]          // Mixed -> default branch -> 3 digits, 5 letters -> ratio = 0 (floor)
    [InlineData("12345abc", null, null, new char[] { '1', '2', '3', '4', '5', 'a', 'b', 'c' })]          // Mixed -> default branch -> 5 digits, 3 letters -> ratio = 1 (floor)
    public async Task BuildAndRunPipeline_WhenMultiForkClassifiesStringContent_ShouldProcessCorrectly(
        string inputValue,
        int? expectedIntResult,
        string? expectedStringResult,
        char[]? expectedCharArrayResult)
    {
        // Arrange
        var space = Pipelines.CreateReturningAsyncSpace();

        // Create sub-pipelines before main pipeline
        space.CreatePipeline<string, (int?, string?, char[]?)>("DigitProcessingPipeline")
            .StartWithLinear<int?, (int?, string?, char[]?)>("ParseStringToInt", async (input, next) =>
            {
                if (int.TryParse(input, out var number))
                {
                    return await next(number);
                }
                else
                {
                    return await next(0); // Default value if parsing fails
                }
            })
            .ThenLinear<int?, (int?, string?, char[]?)>("AddConstant", async (input, next) =>
            {
                var result = input + 10; // Add constant 10
                return await next(result);
            })
            .HandleWith("IntHandler", async (input) => await Task.FromResult<(int?, string?, char[]?)>((input, null, null)))
            .BuildPipeline();

        space.CreatePipeline<string, (int?, string?, char[]?)>("LetterProcessingPipeline")
            .StartWithLinear<string, (int?, string?, char[]?)>("AddSpaces", async (input, next) =>
            {
                var withSpaces = $"  {input}  ";
                return await next(withSpaces);
            })
            .HandleWith("StringHandler", async (input) => await Task.FromResult<(int?, string?, char[]?)>((null, input, null)))
            .BuildPipeline();

        space.CreatePipeline<string, (int?, string?, char[]?)>("SpecialCharProcessingPipeline")
            .StartWithLinear<string, (int?, string?, char[]?)>("RemoveWhitespace", async (input, next) =>
            {
                var noWhitespace = new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray());
                return await next(noWhitespace);
            })
            .ThenLinear<char[], (int?, string?, char[]?)>("ConvertToCharArray", async (input, next) =>
            {
                var charArray = input.ToCharArray();
                return await next(charArray);
            })
            .ThenLinear<char[], (int?, string?, char[]?)>("RemoveDuplicates", async (input, next) =>
            {
                var uniqueChars = input.Distinct().ToArray();
                return await next(uniqueChars);
            })
            .HandleWith("CharArrayHandler", async (input) => await Task.FromResult<(int?, string?, char[]?)>((null, null, input)))
            .BuildPipeline();
        
        var defaultPipeline = space.CreatePipeline<char[], (int?, string?, char[]?)>("DefaultProcessingPipeline")
            .StartWithHandler("DefaultHandler", async (input) => await Task.FromResult<(int?, string?, char[]?)>((null, null, input.Distinct().ToArray())))
            .BuildPipeline();

        var pipeline = space.CreatePipeline<string, (int?, string?, char[]?)>("TestMultiForkPipeline")
            .StartWithLinear<string, (int?, string?, char[]?)>("TrimString", async (input, next) =>
            {
                var trimmed = input.Trim();
                return await next(trimmed);
            })
            .ThenMultiFork<string, (int?, string?, char[]?), char[], (int?, string?, char[]?)>("ClassifyStringContent", async (input, branches, defaultNext) =>
            {
                var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);
                var containsOnlyLetters = !string.IsNullOrEmpty(input) && input.All(char.IsLetter);
                var containsOnlySpecialChars = !string.IsNullOrEmpty(input) && input.All(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));

                if (containsOnlyDigits)
                {
                    return await branches["DigitBranch"](input);
                }
                else if (containsOnlyLetters)
                {
                    return await branches["LetterBranch"](input);
                }
                else if (containsOnlySpecialChars)
                {
                    return await branches["SpecialCharBranch"](input);
                }
                else
                {
                    // Mixed content - go to default branch
                    var charArray = input.ToCharArray();
                    return await defaultNext(charArray);
                }
            },
            space => new Dictionary<string, Pipeline<string, (int?, string?, char[]?)>>
            {
                ["DigitBranch"] = space.GetPipeline<string, (int?, string?, char[]?)>("DigitProcessingPipeline")!,
                ["LetterBranch"] = space.GetPipeline<string, (int?, string?, char[]?)>("LetterProcessingPipeline")!,
                ["SpecialCharBranch"] = space.GetPipeline<string, (int?, string?, char[]?)>("SpecialCharProcessingPipeline")!
            }.AsReadOnly(),
            space => space.GetPipeline<char[], (int?, string?, char[]?)>("DefaultProcessingPipeline")!)
            .BuildPipeline().Compile();

        // Act
        var actualResult = await pipeline(inputValue);

        // Assert
        if (expectedIntResult.HasValue)
        {
            Assert.Equal(expectedIntResult, actualResult.Item1);
            Assert.Null(actualResult.Item2);
            Assert.Null(actualResult.Item3);
        }
        else if (expectedStringResult != null)
        {
            Assert.Null(actualResult.Item1);
            Assert.Equal(expectedStringResult, actualResult.Item2);
            Assert.Null(actualResult.Item3);
        }
        else
        {
            Assert.Null(actualResult.Item1);
            Assert.Null(actualResult.Item2);
            Assert.Equal(expectedCharArrayResult, actualResult.Item3);
        }
    }
}

