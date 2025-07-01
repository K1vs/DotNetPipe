using K1vs.DotNetPipe.Mutations;
using K1vs.DotNetPipe.AsyncCancellable;
using NSubstitute;

namespace K1vs.DotNetPipe.Tests.AsyncCancellable;

public class WithMutationPipelineTests
{
    [Theory]
    [InlineData(-4, -3)]
    [InlineData(0, 1)]
    [InlineData(2, 3)]
    public async Task BuildAndRunPipeline_WhenOneHandlerStep_ShouldRun(int value, int expectedValue)
    {
        // Arrange
        var handler = Substitute.For<Func<int, CancellationToken, Task>>();
        handler.Invoke(Arg.Is(value), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        var pipeline = Pipelines.CreateAsyncCancellablePipeline<int>("TestPipeline")
            .StartWithHandler("TestHandler", async (input, ct) => await handler(input, ct))
            .BuildPipeline()
            .Compile(cfg =>
            {
                cfg.Configure(ConfigureTestPipelineMutators);
            });
        // Act
        await pipeline(value, CancellationToken.None);
        // Assert
        await handler.Received().Invoke(Arg.Is(expectedValue), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(-4)]
    [InlineData(0)]
    [InlineData(2)]
    public async Task BuildAndRunPipeline_WhenOneHandlerStepCancellationRequested_ShouldRunAndCancel(int value)
    {
        // Arrange
        var handler = Substitute.For<Func<int, CancellationToken, Task>>();
        Func<int, CancellationToken, Task> cancellableHandler = async (input, ct) =>
        {
            await Task.Delay(500, ct); // Simulate some work
            await handler.Invoke(input, ct);
        };
        handler.Invoke(Arg.Is(value), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        var pipeline = Pipelines.CreateAsyncCancellablePipeline<int>("TestPipeline")
            .StartWithHandler("TestHandler", async (input, ct) => await cancellableHandler(input, ct))
            .BuildPipeline()
            .Compile(cfg =>
            {
                cfg.Configure(ConfigureTestPipelineMutators);
            });
        var cts = new CancellationTokenSource();
        // Act
        cts.CancelAfter(50);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await pipeline(value, cts.Token);
        });
        // Assert
        await handler.DidNotReceive().Invoke(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    private void ConfigureTestPipelineMutators(Space space)
    {
        var step = space.GetRequiredHandlerStep<int, int>("TestPipeline", "TestHandler");
        var mutator = new StepMutator<Handler<int>>("TestHandlerMutator", 1, (handler) =>
        {
            return async (input, ct) =>
            {
                input += 1;
                await handler(input, ct);
            };
        });
        step.Mutators.AddMutator(mutator, AddingMode.ExactPlace);
    }

    [Theory]
    [InlineData(-4, 5, -2)]   // ((-4 * 2) + 5) + 1 = (-8 + 5) + 1 = -2
    [InlineData(0, 10, 11)]  // ((0 * 2) + 10) + 1 = (0 + 10) + 1 = 11
    [InlineData(2, 3, 8)]    // ((2 * 2) + 3) + 1 = (4 + 3) + 1 = 8
    public async Task BuildAndRunPipeline_WhenLinearStepThenHandlerStep_ShouldRun(int inputValue, int constantToAdd, int expectedHandlerInput)
    {
        // Arrange
        var handler = Substitute.For<Func<int, CancellationToken, Task>>();
        handler.Invoke(Arg.Is(expectedHandlerInput), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var pipeline = Pipelines.CreateAsyncCancellablePipeline<int>("TestTwoStepPipeline")
            .StartWithLinear<int>("AddConstant", async (input, next, ct) =>
            {
                var result = input + constantToAdd;
                await next(result, ct);
            })
            .HandleWith("TestHandler", async (input, ct) => await handler(input, ct))
            .BuildPipeline()
            .Compile(cfg =>
            {
                cfg.Configure(ConfigureTwoStepPipelineMutators);
            });

        // Act
        await pipeline(inputValue);

        // Assert
        await handler.Received().Invoke(Arg.Is(expectedHandlerInput), Arg.Any<CancellationToken>());
    }

    private void ConfigureTwoStepPipelineMutators(Space space)
    {
        // Mutator for linear step - multiply input by 2 before processing
        var linearStep = space.GetRequiredLinearStep<int, int, int>("TestTwoStepPipeline", "AddConstant");
        var linearMutator = new StepMutator<Pipe<int, int>>("AddConstantMutator", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                // Apply mutation to input first - multiply by 2
                input *= 2;
                await pipe(input, next, ct);
            };
        });
        linearStep.Mutators.AddMutator(linearMutator, AddingMode.ExactPlace);

        // Mutator for handler step - add 1
        var handlerStep = space.GetRequiredHandlerStep<int, int>("TestTwoStepPipeline", "TestHandler");
        var handlerMutator = new StepMutator<Handler<int>>("TestHandlerMutator", 1, (handler) =>
        {
            return async (input, ct) =>
            {
                input += 1;
                await handler(input, ct);
            };
        });
        handlerStep.Mutators.AddMutator(handlerMutator, AddingMode.ExactPlace);
    }

    [Theory]
    [InlineData(2, 3, 2, 31)]   // Step1: (2 + 5) + 3 = 10, Step2: (10 + 5) * 2 = 30, Handler: 30 + 1 = 31
    [InlineData(0, 5, 3, 46)]   // Step1: (0 + 5) + 5 = 10, Step2: (10 + 5) * 3 = 45, Handler: 45 + 1 = 46
    [InlineData(-1, 4, 2, 27)]  // Step1: (-1 + 5) + 4 = 8, Step2: (8 + 5) * 2 = 26, Handler: 26 + 1 = 27
    [InlineData(10, -5, 4, 61)] // Step1: (10 + 5) + (-5) = 10, Step2: (10 + 5) * 4 = 60, Handler: 60 + 1 = 61
    public async Task BuildAndRunPipeline_WhenTwoLinearStepsThenHandlerStep_ShouldRun(int inputValue, int constantToAdd, int multiplier, int expectedHandlerInput)
    {
        // Arrange
        var handler = Substitute.For<Func<int, CancellationToken, Task>>();
        handler.Invoke(Arg.Is(expectedHandlerInput), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var pipeline = Pipelines.CreateAsyncCancellablePipeline<int>("TestThreeStepPipeline")
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
            .BuildPipeline()
            .Compile(cfg =>
            {
                cfg.Configure(ConfigureThreeStepPipelineMutators);
            });

        // Act
        await pipeline(inputValue);

        // Assert
        await handler.Received().Invoke(Arg.Is(expectedHandlerInput), Arg.Any<CancellationToken>());
    }

    private void ConfigureThreeStepPipelineMutators(Space space)
    {
        // Mutator for first linear step (AddConstant) - add 5 to input
        var firstLinearStep = space.GetRequiredLinearStep<int, int, int>("TestThreeStepPipeline", "AddConstant");
        var firstLinearMutator = new StepMutator<Pipe<int, int>>("AddConstantMutator", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                // Apply mutation to input first - add 5
                input += 5;
                await pipe(input, next, ct);
            };
        });
        firstLinearStep.Mutators.AddMutator(firstLinearMutator, AddingMode.ExactPlace);

        // Mutator for second linear step (MultiplyByCoefficient) - add 5 to input before multiplying
        var secondLinearStep = space.GetRequiredLinearStep<int, int, int>("TestThreeStepPipeline", "MultiplyByCoefficient");
        var secondLinearMutator = new StepMutator<Pipe<int, int>>("MultiplyByCoefficient", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                // Apply mutation to input first - add 5 before multiplying
                input += 5;
                await pipe(input, next, ct);
            };
        });
        secondLinearStep.Mutators.AddMutator(secondLinearMutator, AddingMode.ExactPlace);

        // Mutator for handler step - add 1
        var handlerStep = space.GetRequiredHandlerStep<int, int>("TestThreeStepPipeline", "TestHandler");
        var handlerMutator = new StepMutator<Handler<int>>("TestHandlerMutator", 1, (handler) =>
        {
            return async (input, ct) =>
            {
                input += 1;
                await handler(input, ct);
            };
        });
        handlerStep.Mutators.AddMutator(handlerMutator, AddingMode.ExactPlace);
    }

    [Theory]
    [InlineData("5", 3, 11)]    // "5" -> 5 + 2 = 7, 7 + 3 = 10, handler gets 10 + 1 = 11
    [InlineData("10", -2, 11)]  // "10" -> 10 + 2 = 12, 12 + (-2) = 10, handler gets 10 + 1 = 11
    [InlineData("0", 5, 8)]     // "0" -> 0 + 2 = 2, 2 + 5 = 7, handler gets 7 + 1 = 8
    [InlineData("abc", 3, 0)]   // "abc" -> doesn't parse, handler not called
    [InlineData("", 10, 0)]     // "" -> doesn't parse, handler not called
    public async Task BuildAndRunPipeline_WhenStringParseAndAddConstant_ShouldCallHandlerOnlyOnSuccessfulParse(string inputValue, int constantToAdd, int expectedCallCount)
    {
        // Arrange
        var handler = Substitute.For<Func<int, CancellationToken, Task>>();
        handler.Invoke(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var pipeline = Pipelines.CreateAsyncCancellablePipeline<string>("TestStringParsingPipeline")
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
            .BuildPipeline()
            .Compile(cfg =>
            {
                cfg.Configure(ConfigureStringParsingPipelineMutators);
            });

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

    private void ConfigureStringParsingPipelineMutators(Space space)
    {
        // Mutator for ParseString step - add 2 to input after parsing (only if parsing succeeds)
        var parseStep = space.GetRequiredLinearStep<string, string, int>("TestStringParsingPipeline", "ParseString");
        var parseMutator = new StepMutator<Pipe<string, int>>("ParseStringMutator", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                // Custom parsing logic with mutation
                if (int.TryParse(input, out var parsed))
                {
                    parsed += 2; // Apply mutation - add 2 to successfully parsed value
                    await next(parsed, ct);
                }
                // If doesn't parse - don't call next
            };
        });
        parseStep.Mutators.AddMutator(parseMutator, AddingMode.ExactPlace);

        // Mutator for AddConstant step - pass through as-is
        var addConstantStep = space.GetRequiredLinearStep<string, int, int>("TestStringParsingPipeline", "AddConstant");
        var addConstantMutator = new StepMutator<Pipe<int, int>>("AddConstantMutator", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                // Apply original add constant logic as-is
                await pipe(input, next, ct);
            };
        });
        addConstantStep.Mutators.AddMutator(addConstantMutator, AddingMode.ExactPlace);

        // Mutator for handler step - add 1
        var handlerStep = space.GetRequiredHandlerStep<string, int>("TestStringParsingPipeline", "TestHandler");
        var handlerMutator = new StepMutator<Handler<int>>("TestHandlerMutator", 1, (handler) =>
        {
            return async (input, ct) =>
            {
                input += 1;
                await handler(input, ct);
            };
        });
        handlerStep.Mutators.AddMutator(handlerMutator, AddingMode.ExactPlace);
    }

    [Theory]
    [InlineData(" 10 ", -2, 9)]     // " 10 " -> trim -> 10 -> float pipeline -> 10+1=11 -> round to 11 -> 11 + (-2) = 9
    [InlineData("3.7", 5, 10)]      // "3.7" -> trim -> 3.7 -> float pipeline -> 3.7+1=4.7 -> round to 5 -> 5 + 5 = 10
    [InlineData(" 2.3 ", 1, 4)]     // " 2.3 " -> trim -> 2.3 -> float pipeline -> 2.3+1=3.3 -> round to 3 -> 3 + 1 = 4
    [InlineData("5.5", 2, 8)]       // "5.5" -> trim -> 5.5 -> float pipeline -> 5.5+1=6.5 -> round to 6 -> 6 + 2 = 8
    public async Task BuildAndRunPipeline_WhenIfStepHandlesIntAndFloat_ShouldProcessCorrectly(string inputValue, int constantToAdd, int expectedResult)
    {
        // Arrange
        var handler = Substitute.For<Func<int, CancellationToken, Task>>();
        handler.Invoke(Arg.Is(expectedResult), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var pipeline = Pipelines.CreateAsyncCancellablePipeline<string>("TestIfStepPipeline")
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
            .BuildPipeline()
            .Compile(cfg =>
            {
                cfg.Configure(ConfigureIfStepPipelineMutators);
            });

        // Act
        await pipeline(inputValue);

        // Assert
        await handler.Received().Invoke(Arg.Is(expectedResult), Arg.Any<CancellationToken>());
    }

    private void ConfigureIfStepPipelineMutators(Space space)
    {
        // Mutator for TrimString step - pass through as-is
        var trimStep = space.GetRequiredLinearStep<string, string, string>("TestIfStepPipeline", "TrimString");
        var trimMutator = new StepMutator<Pipe<string, string>>("TrimStringMutator", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                await pipe(input, next, ct);
            };
        });
        trimStep.Mutators.AddMutator(trimMutator, AddingMode.ExactPlace);

        // Mutator for the If step selector - modify the logic to treat all strings as potential floats
        var ifStep = space.GetRequiredIfStep<string, string, string, int>("TestIfStepPipeline", "CheckIntOrFloat");
        var ifSelectorMutator = new StepMutator<IfSelector<string, string, int>>("CheckIntOrFloatMutator", 1, (selector) =>
        {
            return async (input, conditionalNext, next, ct) =>
            {
                // Modified logic: always try to go to conditional pipeline first (for float parsing)
                // This will change the behavior - all inputs will be treated as potential floats
                await conditionalNext(input, ct);
            };
        });
        ifStep.Mutators.AddMutator(ifSelectorMutator, AddingMode.ExactPlace);

        // Mutator for ParseFloat step in FloatProcessing - pass through as-is
        var parseFloatStep = space.GetRequiredLinearStep<string, string, double>("FloatProcessing", "ParseFloat");
        var parseFloatMutator = new StepMutator<Pipe<string, double>>("ParseFloatMutator", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                await pipe(input, next, ct);
            };
        });
        parseFloatStep.Mutators.AddMutator(parseFloatMutator, AddingMode.ExactPlace);

        // Mutator for the RoundToInt step in the conditional pipeline - add 1 to input before rounding
        var roundStep = space.GetRequiredLinearStep<string, double, int>("FloatProcessing", "RoundToInt");
        var roundMutator = new StepMutator<Pipe<double, int>>("RoundToIntMutator", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                // Add 1 to the input before rounding
                input += 1;
                var rounded = (int)Math.Round(input);
                await next(rounded, ct);
            };
        });
        roundStep.Mutators.AddMutator(roundMutator, AddingMode.ExactPlace);

        // Mutator for AddConstant step - pass through as-is
        var addConstantStep = space.GetRequiredLinearStep<string, int, int>("TestIfStepPipeline", "AddConstant");
        var addConstantMutator = new StepMutator<Pipe<int, int>>("AddConstantMutator", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                await pipe(input, next, ct);
            };
        });
        addConstantStep.Mutators.AddMutator(addConstantMutator, AddingMode.ExactPlace);

        // Mutator for handler step - pass through as-is
        var handlerStep = space.GetRequiredHandlerStep<string, int>("TestIfStepPipeline", "TestHandler");
        var handlerMutator = new StepMutator<Handler<int>>("TestHandlerMutator", 1, (handler) =>
        {
            return async (input, ct) =>
            {
                await handler(input, ct);
            };
        });
        handlerStep.Mutators.AddMutator(handlerMutator, AddingMode.ExactPlace);
    }

    [Theory]
    [InlineData("  5  ", 3, 2, 10)]    // "  5  " -> trim -> 5 (int) -> true branch (float processing) -> parse 5.0 -> +1 = 6.0 -> round to 6 -> 6 + 3 = 9 -> +1 = 10
    [InlineData(" 10 ", -2, 4, 10)]   // " 10 " -> trim -> 10 (int) -> true branch (float processing) -> parse 10.0 -> +1 = 11.0 -> round to 11 -> 11 + (-2) = 9 -> +1 = 10
    [InlineData("3.7", 5, 3, 12)]     // "3.7" -> trim -> 3.7 (float) -> false branch (int processing) -> 0 + 2 = 2 -> 2 * 3 = 6 -> 6 + 5 = 11 -> +1 = 12
    [InlineData(" 2.3 ", 1, 5, 12)]   // " 2.3 " -> trim -> 2.3 (float) -> false branch (int processing) -> 0 + 2 = 2 -> 2 * 5 = 10 -> 10 + 1 = 11 -> +1 = 12
    [InlineData("5.5", 2, 7, 17)]     // "5.5" -> trim -> 5.5 (float) -> false branch (int processing) -> 0 + 2 = 2 -> 2 * 7 = 14 -> 14 + 2 = 16 -> +1 = 17
    public async Task BuildAndRunPipeline_WhenIfElseStepHandlesIntFloatOrDefault_ShouldProcessCorrectly(string inputValue, int constantToAdd, int multiplier, int expectedResult)
    {
        // Arrange
        var handler = Substitute.For<Func<int, CancellationToken, Task>>();
        handler.Invoke(Arg.Is(expectedResult), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var pipeline = Pipelines.CreateAsyncCancellablePipeline<string>("TestIfElseStepPipeline")
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
            .BuildPipeline()
            .Compile(cfg =>
            {
                cfg.Configure(ConfigureIfElseStepPipelineMutators);
            });

        // Act
        await pipeline(inputValue);

        // Assert
        await handler.Received().Invoke(Arg.Is(expectedResult), Arg.Any<CancellationToken>());
    }

    private void ConfigureIfElseStepPipelineMutators(Space space)
    {
        // Mutator for TrimString step - pass through as-is
        var trimStep = space.GetRequiredLinearStep<string, string, string>("TestIfElseStepPipeline", "TrimString");
        var trimMutator = new StepMutator<Pipe<string, string>>("TrimStringMutator", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                await pipe(input, next, ct);
            };
        });
        trimStep.Mutators.AddMutator(trimMutator, AddingMode.ExactPlace);

        // Mutator for the IfElse step selector - modify the logic to swap the branches
        var ifElseStep = space.GetRequiredIfElseStep<string, string, string, int, int>("TestIfElseStepPipeline", "CheckIntOrFloat");
        var ifElseSelectorMutator = new StepMutator<IfElseSelector<string, string, int>>("CheckIntOrFloatMutator", 1, (selector) =>
        {
            return async (input, trueNext, falseNext, ct) =>
            {
                // Modified logic: swap the branches - what was true becomes false and vice versa
                if (int.TryParse(input, out var intValue))
                {
                    // If it's an int, go to true branch (was false branch before)
                    // Now int values go to float processing pipeline
                    await trueNext(input, ct);
                }
                else
                {
                    // If not an int, go to false branch (was true branch before)
                    // This will cause an error since false branch expects int, but we have string
                    // So we need to provide a default int value
                    await falseNext(0, ct); // Use 0 as default for non-parseable strings
                }
            };
        });
        ifElseStep.Mutators.AddMutator(ifElseSelectorMutator, AddingMode.ExactPlace);

        // Mutator for ParseFloat step in FloatProcessing - pass through as-is
        var parseFloatStep = space.GetRequiredLinearStep<string, string, double>("FloatProcessing", "ParseFloat");
        var parseFloatMutator = new StepMutator<Pipe<string, double>>("ParseFloatMutator", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                await pipe(input, next, ct);
            };
        });
        parseFloatStep.Mutators.AddMutator(parseFloatMutator, AddingMode.ExactPlace);

        // Mutator for the RoundToInt step in the FloatProcessing pipeline - add 1 to input before rounding
        var roundStep = space.GetRequiredLinearStep<string, double, int>("FloatProcessing", "RoundToInt");
        var roundMutator = new StepMutator<Pipe<double, int>>("RoundToIntMutator", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                // Add 1 to the input before rounding
                input += 1;
                var rounded = (int)Math.Round(input);
                await next(rounded, ct);
            };
        });
        roundStep.Mutators.AddMutator(roundMutator, AddingMode.ExactPlace);

        // Mutator for ParseIntOrDefault step in the IntOrDefaultProcessing pipeline - multiply by multiplier + 2
        var parseIntStep = space.GetRequiredLinearStep<int, int, int>("IntOrDefaultProcessing", "ParseIntOrDefault");
        var parseIntMutator = new StepMutator<Pipe<int, int>>("ParseIntOrDefaultMutator", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                // Apply original logic but use (multiplier + 2) instead of multiplier
                // Since we can't access multiplier directly, we need to find another way
                // We'll add 2 to the input before multiplying
                input += 2; // This effectively changes the multiplier effect
                await pipe(input, next, ct);
            };
        });
        parseIntStep.Mutators.AddMutator(parseIntMutator, AddingMode.ExactPlace);

        // Mutator for AddConstant step - pass through as-is
        var addConstantStep = space.GetRequiredLinearStep<string, int, int>("TestIfElseStepPipeline", "AddConstant");
        var addConstantMutator = new StepMutator<Pipe<int, int>>("AddConstantMutator", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                await pipe(input, next, ct);
            };
        });
        addConstantStep.Mutators.AddMutator(addConstantMutator, AddingMode.ExactPlace);

        // Mutator for handler step - add 1 to the final result
        var handlerStep = space.GetRequiredHandlerStep<string, int>("TestIfElseStepPipeline", "TestHandler");
        var handlerMutator = new StepMutator<Handler<int>>("TestHandlerMutator", 1, (handler) =>
        {
            return async (input, ct) =>
            {
                // Add 1 to the final result before calling the handler
                input += 1;
                await handler(input, ct);
            };
        });
        handlerStep.Mutators.AddMutator(handlerMutator, AddingMode.ExactPlace);
    }

    [Theory]
    [InlineData(" 105 ", 318)]    // >50 -> GreaterThan100 -> *3 = 315 -> +3 = 318
    [InlineData(" 50 ", 55)]      // 0<x<=50 -> BetweenZeroAndHundred -> +2 = 52 -> +3 = 55
    [InlineData(" -5 ", -7)]      // <=0 -> LessThanZero -> *2 = -10 -> +3 = -7
    [InlineData(" 0 ", 3)]        // <=0 -> LessThanZero -> *2 = 0 -> +3 = 3
    [InlineData("abc", 6)]      // not a number -> string length = 3 -> +3 = 6
    [InlineData("hello", 8)]    // not a number -> string length = 5 -> +3 = 8
    [InlineData("", 3)]         // empty string -> length = 0 -> +3 = 3
    public async Task BuildAndRunPipeline_WhenSwitchStepRoutesByNumberRange_ShouldProcessCorrectly(string inputValue, int expectedResult)
    {
        // Arrange
        var handler = Substitute.For<Func<int, CancellationToken, Task>>();
        handler.Invoke(Arg.Is(expectedResult), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var space = Pipelines.CreateAsyncCancellableSpace();
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
            .BuildPipeline()
            .Compile(cfg =>
            {
                cfg.Configure(ConfigureSwitchStepPipelineMutators);
            });

        // Act
        await pipeline(inputValue);

        // Assert
        await handler.Received().Invoke(Arg.Is(expectedResult), Arg.Any<CancellationToken>());
    }

    private void ConfigureSwitchStepPipelineMutators(Space space)
    {
        // Mutator for TrimString step - pass through as-is
        var trimStep = space.GetRequiredLinearStep<string, string, string>("TestSwitchPipeline", "TrimString");
        var trimMutator = new StepMutator<Pipe<string, string>>("TrimStringMutator", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                await pipe(input, next, ct);
            };
        });
        trimStep.Mutators.AddMutator(trimMutator, AddingMode.ExactPlace);

        // Mutator for NumberRangeSwitch selector - modify the switching logic
        var switchStep = space.GetRequiredSwitchStep<string, string, int, int, int>("TestSwitchPipeline", "NumberRangeSwitch");
        var switchSelectorMutator = new StepMutator<SwitchSelector<string, int, int>>("NumberRangeSwitchMutator", 1, (selector) =>
        {
            return async (input, cases, defaultNext, ct) =>
            {
                // Modified logic: change the conditions for switching
                // Now values > 50 (instead of > 100) go to GreaterThan100 case
                // Values <= 0 (instead of < 0) go to LessThanZero case
                if (int.TryParse(input, out var number))
                {
                    if (number > 50) // Changed from > 100
                    {
                        await cases["GreaterThan100"](number, ct);
                    }
                    else if (number > 0)
                    {
                        await cases["BetweenZeroAndHundred"](number, ct);
                    }
                    else // number <= 0 (changed from < 0)
                    {
                        await cases["LessThanZero"](number, ct);
                    }
                }
                else
                {
                    // If not a number, use string length
                    var stringLength = input.Length;
                    await defaultNext(stringLength, ct);
                }
            };
        });
        switchStep.Mutators.AddMutator(switchSelectorMutator, AddingMode.ExactPlace);

        // Mutator for MultiplyOperation in MultiplyByThree pipeline - pass through as-is
        var multiplyByThreeStep = space.GetRequiredLinearStep<int, int, int>("MultiplyByThree", "MultiplyOperation");
        var multiplyByThreeMutator = new StepMutator<Pipe<int, int>>("MultiplyByThreeMutator", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                await pipe(input, next, ct);
            };
        });
        multiplyByThreeStep.Mutators.AddMutator(multiplyByThreeMutator, AddingMode.ExactPlace);

        // Mutator for AddOperation in AddTwo pipeline - pass through as-is
        var addTwoStep = space.GetRequiredLinearStep<int, int, int>("AddTwo", "AddOperation");
        var addTwoMutator = new StepMutator<Pipe<int, int>>("AddTwoMutator", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                await pipe(input, next, ct);
            };
        });
        addTwoStep.Mutators.AddMutator(addTwoMutator, AddingMode.ExactPlace);

        // Mutator for MultiplyOperation in MultiplyByTwo pipeline - pass through as-is
        var multiplyByTwoStep = space.GetRequiredLinearStep<int, int, int>("MultiplyByTwo", "MultiplyOperation");
        var multiplyByTwoMutator = new StepMutator<Pipe<int, int>>("MultiplyByTwoMutator", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                await pipe(input, next, ct);
            };
        });
        multiplyByTwoStep.Mutators.AddMutator(multiplyByTwoMutator, AddingMode.ExactPlace);

        // Mutator for IdentityOperation in KeepZero pipeline - pass through as-is
        var keepZeroStep = space.GetRequiredLinearStep<int, int, int>("KeepZero", "IdentityOperation");
        var keepZeroMutator = new StepMutator<Pipe<int, int>>("KeepZeroMutator", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                await pipe(input, next, ct);
            };
        });
        keepZeroStep.Mutators.AddMutator(keepZeroMutator, AddingMode.ExactPlace);

        // Mutator for IdentityOperation in StringLengthPipeline - pass through as-is
        var stringLengthStep = space.GetRequiredLinearStep<int, int, int>("StringLengthPipeline", "IdentityOperation");
        var stringLengthMutator = new StepMutator<Pipe<int, int>>("StringLengthMutator", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                await pipe(input, next, ct);
            };
        });
        stringLengthStep.Mutators.AddMutator(stringLengthMutator, AddingMode.ExactPlace);

        // Mutator for handler step - add 3 to all results
        var handlerStep = space.GetRequiredHandlerStep<string, int>("TestSwitchPipeline", "TestHandler");
        var handlerMutator = new StepMutator<Handler<int>>("TestHandlerMutator", 1, (handler) =>
        {
            return async (input, ct) =>
            {
                input += 3;
                await handler(input, ct);
            };
        });
        handlerStep.Mutators.AddMutator(handlerMutator, AddingMode.ExactPlace);
    }

    [Theory]
    [InlineData("123", 0, "******")]         // Length 3 -> non-digit branch -> remove digits -> "" -> add *** -> "******"
    [InlineData("  456  ", 0, "******")]     // Trimmed to "456" (length 3) -> non-digit branch -> remove digits -> "" -> add *** -> "******"
    [InlineData("abc123def", 128, "")]        // Length 9 -> digit branch -> remove non-digits -> "123" -> parse to 123 + 5 = 128
    [InlineData("hello", 5, "")]             // Length 5 -> digit branch -> remove non-digits -> "" -> parse fails -> 0 + 5 = 5
    [InlineData("", 0, "******")]            // Length 0 -> non-digit branch -> remove digits -> "" -> add *** -> "******"
    [InlineData("!@#", 0, "***!@#***")]      // Length 3 -> non-digit branch -> remove digits -> "!@#" -> add *** -> "***!@#***"
    public async Task BuildAndRunPipeline_WhenForkSplitsByDigitContent_ShouldProcessCorrectly(string inputValue, int expectedIntResult, string? expectedStringResult)
    {
        // Arrange
        var intHandler = Substitute.For<Func<int, CancellationToken, Task>>();
        var stringHandler = Substitute.For<Func<string, CancellationToken, Task>>();
        intHandler.Invoke(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        stringHandler.Invoke(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var pipeline = Pipelines.CreateAsyncCancellablePipeline<string>("TestForkPipeline")
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
            .BuildPipeline()
            .Compile(cfg =>
            {
                cfg.Configure(ConfigureForkPipelineMutators);
            });

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

    private void ConfigureForkPipelineMutators(Space space)
    {
        // Mutator for TrimString step - pass through as-is
        var trimStep = space.GetRequiredLinearStep<string, string, string>("TestForkPipeline", "TrimString");
        var trimMutator = new StepMutator<Pipe<string, string>>("TrimStringMutator", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                await pipe(input, next, ct);
            };
        });
        trimStep.Mutators.AddMutator(trimMutator, AddingMode.ExactPlace);

        // Mutator for DigitContentFork selector - modify the fork logic
        var forkStep = space.GetRequiredForkStep<string, string, string, string>("TestForkPipeline", "DigitContentFork");
        var forkSelectorMutator = new StepMutator<ForkSelector<string, string, string>>("DigitContentForkMutator", 1, (selector) =>
        {
            return async (input, digitBranch, nonDigitBranch, ct) =>
            {
                // Modified logic: strings with length > 3 go to digit branch, others to non-digit branch
                if (input.Length > 3)
                {
                    await digitBranch(input, ct);
                }
                else
                {
                    await nonDigitBranch(input, ct);
                }
            };
        });
        forkStep.Mutators.AddMutator(forkSelectorMutator, AddingMode.ExactPlace);

        // Mutator for RemoveNonDigits step in DigitProcessing - pass through as-is
        var removeNonDigitsStep = space.GetRequiredLinearStep<string, string, string>("DigitProcessing", "RemoveNonDigits");
        var removeNonDigitsMutator = new StepMutator<Pipe<string, string>>("RemoveNonDigitsMutator", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                await pipe(input, next, ct);
            };
        });
        removeNonDigitsStep.Mutators.AddMutator(removeNonDigitsMutator, AddingMode.ExactPlace);

        // Mutator for ParseToInt step in the DigitProcessing pipeline - add 5 to the parsed value
        var parseToIntStep = space.GetRequiredLinearStep<string, string, int>("DigitProcessing", "ParseToInt");
        var parseToIntMutator = new StepMutator<Pipe<string, int>>("ParseToIntMutator", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                // Apply original parsing logic first, then add 5 to the result
                if (int.TryParse(input, out var number))
                {
                    await next(number + 5, ct); // Add 5 to the parsed value
                }
                else
                {
                    await next(0 + 5, ct); // Even if parsing fails, add 5 to the default 0
                }
            };
        });
        parseToIntStep.Mutators.AddMutator(parseToIntMutator, AddingMode.ExactPlace);

        // Mutator for RemoveDigits step in NonDigitProcessing - pass through as-is
        var removeDigitsStep = space.GetRequiredLinearStep<string, string, string>("NonDigitProcessing", "RemoveDigits");
        var removeDigitsMutator = new StepMutator<Pipe<string, string>>("RemoveDigitsMutator", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                await pipe(input, next, ct);
            };
        });
        removeDigitsStep.Mutators.AddMutator(removeDigitsMutator, AddingMode.ExactPlace);

        // Mutator for AddSpaces step in the NonDigitProcessing pipeline - use asterisks instead of spaces
        var addSpacesStep = space.GetRequiredLinearStep<string, string, string>("NonDigitProcessing", "AddSpaces");
        var addSpacesMutator = new StepMutator<Pipe<string, string>>("AddSpacesMutator", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                var withAsterisks = $"***{input}***"; // Use asterisks instead of spaces
                await next(withAsterisks, ct);
            };
        });
        addSpacesStep.Mutators.AddMutator(addSpacesMutator, AddingMode.ExactPlace);

        // Mutator for IntHandler - pass through as-is
        var intHandlerStep = space.GetRequiredHandlerStep<string, int>("DigitProcessing", "IntHandler");
        var intHandlerMutator = new StepMutator<Handler<int>>("IntHandlerMutator", 1, (handler) =>
        {
            return async (input, ct) =>
            {
                await handler(input, ct);
            };
        });
        intHandlerStep.Mutators.AddMutator(intHandlerMutator, AddingMode.ExactPlace);

        // Mutator for StringHandler - pass through as-is
        var stringHandlerStep = space.GetRequiredHandlerStep<string, string>("NonDigitProcessing", "StringHandler");
        var stringHandlerMutator = new StepMutator<Handler<string>>("StringHandlerMutator", 1, (handler) =>
        {
            return async (input, ct) =>
            {
                await handler(input, ct);
            };
        });
        stringHandlerStep.Mutators.AddMutator(stringHandlerMutator, AddingMode.ExactPlace);
    }

    [Theory]
    [InlineData("123", 138, "", new char[0])]             // Only digits -> digit branch -> parse to int (123) + 10 + 5 = 138
    [InlineData("  456  ", 471, "", new char[0])]         // Only digits (with spaces) -> digit branch -> 456 + 10 + 5 = 471
    [InlineData("abc", 0, "", new char[] { 'a', 'b', 'c', '_' })]      // Only letters -> special char branch -> add underscore, remove whitespace, convert to array, remove duplicates
    [InlineData("xyz", 0, "", new char[] { 'x', 'y', 'z', '_' })]      // Only letters -> special char branch -> add underscore, remove whitespace, convert to array, remove duplicates
    [InlineData("!@#", 0, "", new char[] { '!', '@', '#', '_' })] // Only special chars -> special char branch -> add underscore, remove whitespace, convert to array, remove duplicates
    [InlineData("@@@", 0, "", new char[] { '@', '_' })]   // Special chars with duplicates -> special char branch -> unique chars + underscore
    [InlineData("a1b2", 0, "", new char[] { 'a', '1', 'b', '2', '_' })]              // Mixed -> special char branch -> add underscore, remove whitespace, convert to array, remove duplicates
    [InlineData("hello123", 0, "", new char[] { 'h', 'e', 'l', 'o', '1', '2', '3', '_' })]          // Mixed -> special char branch -> add underscore, remove whitespace, convert to array, remove duplicates
    [InlineData("12345abc", 0, "", new char[] { '1', '2', '3', '4', '5', 'a', 'b', 'c', '_' })]          // Mixed -> special char branch -> add underscore, remove whitespace, convert to array, remove duplicates
    public async Task BuildAndRunPipeline_WhenMultiForkClassifiesStringContent_ShouldProcessCorrectly(
        string inputValue,
        int expectedIntResult,
        string? expectedStringResult,
        char[] expectedCharArrayResult)
    {
        // Arrange
        var intHandler = Substitute.For<Func<int, CancellationToken, Task>>();
        var stringHandler = Substitute.For<Func<string, CancellationToken, Task>>();
        var charArrayHandler = Substitute.For<Func<char[], CancellationToken, Task>>();

        intHandler.Invoke(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        stringHandler.Invoke(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        charArrayHandler.Invoke(Arg.Any<char[]>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var space = Pipelines.CreateAsyncCancellableSpace();

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
            .BuildPipeline();

        var defaultPipeline = space.CreatePipeline<char[]>("DefaultProcessingPipeline")
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
            .BuildPipeline()
            .Compile(cfg =>
            {
                cfg.Configure(ConfigureMultiForkPipelineMutators);
            });

        // Act
        await pipeline(inputValue);

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

    private void ConfigureMultiForkPipelineMutators(Space space)
    {
        // Mutator for TrimString step - pass through as-is
        var trimStep = space.GetRequiredLinearStep<string, string, string>("TestMultiForkPipeline", "TrimString");
        var trimMutator = new StepMutator<Pipe<string, string>>("TrimStringMutator", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                await pipe(input, next, ct);
            };
        });
        trimStep.Mutators.AddMutator(trimMutator, AddingMode.ExactPlace);

        // Mutator for ClassifyStringContent selector - modify the classification logic
        var multiForkStep = space.GetRequiredMultiForkStep<string, string, string, char[]>("TestMultiForkPipeline", "ClassifyStringContent");
        var multiForkSelectorMutator = new StepMutator<MultiForkSelector<string, string, char[]>>("ClassifyStringContentMutator", 1, (selector) =>
        {
            return async (input, branches, defaultNext, ct) =>
            {
                // Modified logic: always treat everything as special characters unless it's digits only
                var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);

                if (containsOnlyDigits)
                {
                    // Only digits go to digit branch
                    await branches["DigitBranch"](input, ct);
                }
                else
                {
                    // Everything else (letters, special chars, mixed) goes to special char branch
                    await branches["SpecialCharBranch"](input, ct);
                }
            };
        });
        multiForkStep.Mutators.AddMutator(multiForkSelectorMutator, AddingMode.ExactPlace);

        // Mutator for ParseStringToInt step in DigitProcessingPipeline - pass through as-is
        var parseStringToIntStep = space.GetRequiredLinearStep<string, string, int>("DigitProcessingPipeline", "ParseStringToInt");
        var parseStringToIntMutator = new StepMutator<Pipe<string, int>>("ParseStringToIntMutator", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                await pipe(input, next, ct);
            };
        });
        parseStringToIntStep.Mutators.AddMutator(parseStringToIntMutator, AddingMode.ExactPlace);

        // Mutator for AddConstant step in DigitProcessingPipeline - add 5 more to the result
        var addConstantStep = space.GetRequiredLinearStep<string, int, int>("DigitProcessingPipeline", "AddConstant");
        var addConstantMutator = new StepMutator<Pipe<int, int>>("AddConstantMutator", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                var result = input + 10 + 5; // Original +10, mutation adds +5 more
                await next(result, ct);
            };
        });
        addConstantStep.Mutators.AddMutator(addConstantMutator, AddingMode.ExactPlace);

        // Mutator for AddSpaces step in LetterProcessingPipeline - use asterisks instead of spaces
        var addSpacesStep = space.GetRequiredLinearStep<string, string, string>("LetterProcessingPipeline", "AddSpaces");
        var addSpacesMutator = new StepMutator<Pipe<string, string>>("AddSpacesMutator", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                var withAsterisks = $"***{input}***"; // Use asterisks instead of spaces
                await next(withAsterisks, ct);
            };
        });
        addSpacesStep.Mutators.AddMutator(addSpacesMutator, AddingMode.ExactPlace);

        // Mutator for RemoveWhitespace step in SpecialCharProcessingPipeline - pass through as-is
        var removeWhitespaceStep = space.GetRequiredLinearStep<string, string, string>("SpecialCharProcessingPipeline", "RemoveWhitespace");
        var removeWhitespaceMutator = new StepMutator<Pipe<string, string>>("RemoveWhitespaceMutator", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                await pipe(input, next, ct);
            };
        });
        removeWhitespaceStep.Mutators.AddMutator(removeWhitespaceMutator, AddingMode.ExactPlace);

        // Mutator for ConvertToCharArray step in SpecialCharProcessingPipeline - add underscore
        var convertToCharArrayStep = space.GetRequiredLinearStep<string, string, char[]>("SpecialCharProcessingPipeline", "ConvertToCharArray");
        var convertToCharArrayMutator = new StepMutator<Pipe<string, char[]>>("ConvertToCharArrayMutator", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                var inputWithUnderscore = input + "_"; // Add underscore before converting
                var charArray = inputWithUnderscore.ToCharArray();
                await next(charArray, ct);
            };
        });
        convertToCharArrayStep.Mutators.AddMutator(convertToCharArrayMutator, AddingMode.ExactPlace);

        // Mutator for RemoveDuplicates step in SpecialCharProcessingPipeline - pass through as-is
        var removeDuplicatesStep = space.GetRequiredLinearStep<string, char[], char[]>("SpecialCharProcessingPipeline", "RemoveDuplicates");
        var removeDuplicatesMutator = new StepMutator<Pipe<char[], char[]>>("RemoveDuplicatesMutator", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                await pipe(input, next, ct);
            };
        });
        removeDuplicatesStep.Mutators.AddMutator(removeDuplicatesMutator, AddingMode.ExactPlace);

        // Mutator for CountDigitsAndLetters step in DefaultProcessingPipeline - pass through as-is
        var countStep = space.GetRequiredLinearStep<char[], char[], (int DigitCount, int LetterCount)>("DefaultProcessingPipeline", "CountDigitsAndLetters");
        var countMutator = new StepMutator<Pipe<char[], (int DigitCount, int LetterCount)>>("CountDigitsAndLettersMutator", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                await pipe(input, next, ct);
            };
        });
        countStep.Mutators.AddMutator(countMutator, AddingMode.ExactPlace);

        // Mutator for CalculateRatio step in DefaultProcessingPipeline - add 2 to the ratio
        var calculateRatioStep = space.GetRequiredLinearStep<char[], (int DigitCount, int LetterCount), int>("DefaultProcessingPipeline", "CalculateRatio");
        var calculateRatioMutator = new StepMutator<Pipe<(int DigitCount, int LetterCount), int>>("CalculateRatioMutator", 1, (pipe) =>
        {
            return async (input, next, ct) =>
            {
                // Calculate ratio of digits to letters (floor division) and add 2
                var ratio = input.LetterCount > 0 ? input.DigitCount / input.LetterCount : input.DigitCount;
                await next(ratio + 2, ct); // Add 2 to the calculated ratio
            };
        });
        calculateRatioStep.Mutators.AddMutator(calculateRatioMutator, AddingMode.ExactPlace);

        // Mutators for handlers - pass through as-is
        var intHandlerStepDigit = space.GetRequiredHandlerStep<string, int>("DigitProcessingPipeline", "IntHandler");
        var intHandlerMutatorDigit = new StepMutator<Handler<int>>("IntHandlerMutatorDigit", 1, (handler) =>
        {
            return async (input, ct) =>
            {
                await handler(input, ct);
            };
        });
        intHandlerStepDigit.Mutators.AddMutator(intHandlerMutatorDigit, AddingMode.ExactPlace);

        var stringHandlerStep = space.GetRequiredHandlerStep<string, string>("LetterProcessingPipeline", "StringHandler");
        var stringHandlerMutator = new StepMutator<Handler<string>>("StringHandlerMutator", 1, (handler) =>
        {
            return async (input, ct) =>
            {
                await handler(input, ct);
            };
        });
        stringHandlerStep.Mutators.AddMutator(stringHandlerMutator, AddingMode.ExactPlace);

        var charArrayHandlerStep = space.GetRequiredHandlerStep<string, char[]>("SpecialCharProcessingPipeline", "CharArrayHandler");
        var charArrayHandlerMutator = new StepMutator<Handler<char[]>>("CharArrayHandlerMutator", 1, (handler) =>
        {
            return async (input, ct) =>
            {
                await handler(input, ct);
            };
        });
        charArrayHandlerStep.Mutators.AddMutator(charArrayHandlerMutator, AddingMode.ExactPlace);

        var intHandlerStepDefault = space.GetRequiredHandlerStep<char[], int>("DefaultProcessingPipeline", "IntHandler");
        var intHandlerMutatorDefault = new StepMutator<Handler<int>>("IntHandlerMutatorDefault", 1, (handler) =>
        {
            return async (input, ct) =>
            {
                await handler(input, ct);
            };
        });
        intHandlerStepDefault.Mutators.AddMutator(intHandlerMutatorDefault, AddingMode.ExactPlace);
    }
}