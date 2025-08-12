namespace K1vs.DotNetPipe.Tests.ReturningCancellable;

public class WithoutMutationClassPipelineTests
{
    [Theory]
    [InlineData(-4, 8)]
    [InlineData(0, 0)]
    [InlineData(2, -4)]
    public async Task BuildAndRunPipeline_WhenOneHandlerStep_ShouldReturnResult(int value, int expectedResult)
    {
        var testPipeline = new TestReturningCancellablePipeline(async (input, ct) =>
        {
            var result = input * -2;
            return await Task.FromResult(result);
        });

        var actualResult = await testPipeline.Run(value, CancellationToken.None);
        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData(-4, 5, 2)]
    [InlineData(0, 10, 20)]
    [InlineData(2, 3, 10)]
    public async Task BuildAndRunPipeline_WhenLinearStepThenHandlerStep_ShouldReturnResult(int inputValue, int constantToAdd, int expectedResult)
    {
        var testPipeline = new TestReturningCancellableTwoStepPipeline(async (input, ct) =>
        {
            var result = input * 2;
            return await Task.FromResult(result);
        });

        var actualResult = await testPipeline.Run(inputValue, constantToAdd, CancellationToken.None);
        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData(2, 3, 2, 10)]
    [InlineData(0, 5, 3, 15)]
    [InlineData(-1, 4, 2, 6)]
    [InlineData(10, -5, 4, 20)]
    public async Task BuildAndRunPipeline_WhenTwoLinearStepsThenHandlerStep_ShouldReturnResult(int inputValue, int constantToAdd, int multiplier, int expectedResult)
    {
        var testPipeline = new TestReturningCancellableThreeStepPipeline(async (input, ct) => await Task.FromResult(input));
        var actualResult = await testPipeline.Run(inputValue, constantToAdd, multiplier, CancellationToken.None);
        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData("  5  ", 3, 8)]
    [InlineData(" 10 ", -2, 8)]
    [InlineData("3.7", 5, 9)]
    [InlineData(" 2.3 ", 1, 3)]
    [InlineData("5.5", 2, 8)]
    public async Task BuildAndRunPipeline_WhenIfStepHandlesIntAndFloat_ShouldProcessCorrectly(string inputValue, int constantToAdd, int expectedResult)
    {
        var testPipeline = new TestReturningCancellableIfStepPipeline();
        var actualResult = await testPipeline.Run(inputValue, constantToAdd, CancellationToken.None);
        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData("  5  ", 3, 2, 13)]
    [InlineData(" 10 ", -2, 4, 38)]
    [InlineData("3.7", 5, 3, 9)]
    [InlineData(" 2.3 ", 1, 5, 3)]
    [InlineData("5.5", 2, 7, 8)]
    public async Task BuildAndRunPipeline_WhenIfElseStepHandlesIntFloatOrDefault_ShouldProcessCorrectly(string inputValue, int constantToAdd, int multiplier, int expectedResult)
    {
        var testPipeline = new TestReturningCancellableIfElseStepPipeline();
        var actualResult = await testPipeline.Run(inputValue, constantToAdd, multiplier, CancellationToken.None);
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
    public async Task BuildAndRunPipeline_WhenSwitchStepRoutesByNumberRange_ShouldProcessCorrectly(string inputValue, int expectedResult)
    {
        var testPipeline = new TestReturningCancellableSwitchStepPipeline();
        var actualResult = await testPipeline.Run(inputValue, CancellationToken.None);
        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData("123", 123, null)]
    [InlineData("  456  ", 456, null)]
    [InlineData("abc123def", null, "  abcdef  ")]
    [InlineData("hello", null, "  hello  ")]
    [InlineData("", null, "    ")]
    [InlineData("!@#", null, "  !@#  ")]
    public async Task BuildAndRunPipeline_WhenForkSplitsByDigitContent_ShouldProcessCorrectly(string inputValue, int? expectedIntResult, string? expectedStringResult)
    {
        var testPipeline = new TestReturningCancellableForkStepPipeline();
        var actualResult = await testPipeline.Run(inputValue, CancellationToken.None);
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
    public async Task BuildAndRunPipeline_WhenMultiForkClassifiesStringContent_ShouldProcessCorrectly(
        string inputValue,
        int? expectedIntResult,
        string? expectedStringResult,
        char[]? expectedCharArrayResult)
    {
        var testPipeline = new TestReturningCancellableMultiForkStepPipeline();
        var actualResult = await testPipeline.Run(inputValue, CancellationToken.None);

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


