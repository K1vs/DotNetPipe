using K1vs.DotNetPipe.ReturningSync;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace K1vs.DotNetPipe.Tests.ReturningSync;

public class WithoutMutationClassPipelineTests
{
    [Theory]
    [InlineData(-4, 8)]
    [InlineData(0, 0)]
    [InlineData(2, -4)]
    public void BuildAndRunPipeline_WhenOneHandlerStep_ShouldReturnResult(int value, int expectedResult)
    {
        // Arrange
        var testPipeline = new TestReturningPipeline((input) =>
        {
            var result = input * -2;
            return result;
        });

        // Act
        var actualResult = testPipeline.Run(value);

        // Assert
        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData(-4, 5, 2)]
    [InlineData(0, 10, 20)]
    [InlineData(2, 3, 10)]
    public void BuildAndRunPipeline_WhenLinearStepThenHandlerStep_ShouldReturnResult(int inputValue, int constantToAdd, int expectedResult)
    {
        // Arrange
        var testPipeline = new TestReturningTwoStepPipeline((input) =>
        {
            var result = input * 2;
            return result;
        });

        // Act
        var actualResult = testPipeline.Run(inputValue, constantToAdd);

        // Assert
        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData(2, 3, 2, 10)]
    [InlineData(0, 5, 3, 15)]
    [InlineData(-1, 4, 2, 6)]
    [InlineData(10, -5, 4, 20)]
    public void BuildAndRunPipeline_WhenTwoLinearStepsThenHandlerStep_ShouldReturnResult(int inputValue, int constantToAdd, int multiplier, int expectedResult)
    {
        // Arrange
        var testPipeline = new TestReturningThreeStepPipeline((input) =>
        {
            return input;
        });

        // Act
        var actualResult = testPipeline.Run(inputValue, constantToAdd, multiplier);

        // Assert
        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData("  5  ", 3, 8)]
    [InlineData(" 10 ", -2, 8)]
    [InlineData("3.7", 5, 9)]
    [InlineData(" 2.3 ", 1, 3)]
    [InlineData("5.5", 2, 8)]
    public void BuildAndRunPipeline_WhenIfStepHandlesIntAndFloat_ShouldProcessCorrectly(string inputValue, int constantToAdd, int expectedResult)
    {
        // Arrange
        var testPipeline = new TestReturningIfStepPipeline();

        // Act
        var actualResult = testPipeline.Run(inputValue, constantToAdd);

        // Assert
        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData("  5  ", 3, 2, 13)]
    [InlineData(" 10 ", -2, 4, 38)]
    [InlineData("3.7", 5, 3, 9)]
    [InlineData(" 2.3 ", 1, 5, 3)]
    [InlineData("5.5", 2, 7, 8)]
    public void BuildAndRunPipeline_WhenIfElseStepHandlesIntFloatOrDefault_ShouldProcessCorrectly(string inputValue, int constantToAdd, int multiplier, int expectedResult)
    {
        // Arrange
        var testPipeline = new TestReturningIfElseStepPipeline();

        // Act
        var actualResult = testPipeline.Run(inputValue, constantToAdd, multiplier);

        // Assert
        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData("105", 315)]
    [InlineData("50", 52)]
    [InlineData("-5", -10)]
    [InlineData("0", 0)]
    [InlineData("abc", 3)]
    [InlineData("hello", 5)]
    [InlineData("", 0)]
    public void BuildAndRunPipeline_WhenSwitchStepRoutesByNumberRange_ShouldProcessCorrectly(string inputValue, int expectedResult)
    {
        // Arrange
        var testPipeline = new TestReturningSwitchStepPipeline();

        // Act
        var actualResult = testPipeline.Run(inputValue);

        // Assert
        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData("123", 123, null)]
    [InlineData("  456  ", 456, null)]
    [InlineData("abc123def", null, "  abcdef  ")]
    [InlineData("hello", null, "  hello  ")]
    [InlineData("", null, "    ")]
    [InlineData("!@#", null, "  !@#  ")]
    public void BuildAndRunPipeline_WhenForkSplitsByDigitContent_ShouldProcessCorrectly(string inputValue, int? expectedIntResult, string? expectedStringResult)
    {
        // Arrange
        var testPipeline = new TestReturningForkStepPipeline();

        // Act
        var actualResult = testPipeline.Run(inputValue);

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
    [InlineData("123", 133, null, null)]
    [InlineData("  456  ", 466, null, null)]
    [InlineData("abc", null, "  abc  ", null)]
    [InlineData("xyz", null, "  xyz  ", null)]
    [InlineData("!@#", null, null, new char[] { '!', '@', '#' })]
    [InlineData("@@@", null, null, new char[] { '@' })]
    [InlineData("hello123", null, null, new char[] { 'h', 'e', 'l', 'o', '1', '2', '3' })]
    public void BuildAndRunPipeline_WhenMultiForkClassifiesStringContent_ShouldProcessCorrectly(
        string inputValue,
        int? expectedIntResult,
        string? expectedStringResult,
        char[]? expectedCharArrayResult)
    {
        // Arrange
        var testPipeline = new TestReturningMultiForkStepPipeline();

        // Act
        var actualResult = testPipeline.Run(inputValue);

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

