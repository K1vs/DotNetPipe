using K1vs.DotNetPipe.Mutations;
using K1vs.DotNetPipe.Async;

namespace K1vs.DotNetPipe.Tests.Async;

/// <summary>
/// Test handler step implementation
/// </summary>
public class TestHandlerStep : IHandlerStep<int>
{
    private readonly Func<int, Task> _handler;

    public TestHandlerStep(Func<int, Task> handler)
    {
        _handler = handler;
    }

    public string Name => "TestHandler";

    public async Task Handle(int input)
    {
        await _handler(input);
    }
}

/// <summary>
/// Test pipeline implementation using class-based steps and mutators
/// </summary>
public class TestPipeline : IPipeline<int>
{
    private readonly Handler<int> _compiledPipeline;
    private readonly Func<int, Task> _handler;

    public TestPipeline(IEnumerable<IMutator<Space>> mutators, Func<int, Task> handler)
    {
        _handler = handler;
        var pipeline = CreateAndCompilePipeline();
        _compiledPipeline = pipeline.Compile(cfg =>
        {
            cfg.Configure(mutators);
        });
    }

    public string Name => "TestPipeline";

    private Pipeline<int> CreateAndCompilePipeline()
    {
        var handlerStep = new TestHandlerStep(_handler);

        var pipeline = Pipelines.CreateAsyncPipeline<int>("TestPipeline")
            .StartWithHandler(handlerStep)
            .BuildPipeline();

        return pipeline;
    }

    public async Task Run(int input) => await _compiledPipeline(input);
}

/// <summary>
/// Test linear step implementation
/// </summary>
public class TestLinearStep : ILinearStep<int, int>
{
    private const int ConstantToAdd = 10;

    public TestLinearStep()
    {
    }

    public string Name => "AddConstant";

    public async Task Handle(int input, Handler<int> next)
    {
        var result = input + ConstantToAdd;
        await next(result);
    }
}

/// <summary>
/// Test second linear step implementation - multiplies by a coefficient
/// </summary>
public class TestSecondLinearStep : ILinearStep<int, int>
{
    private const int MultiplierCoefficient = 2;

    public string Name => "MultiplyByCoefficient";

    public async Task Handle(int input, Handler<int> next)
    {
        var result = input * MultiplierCoefficient;
        await next(result);
    }
}

/// <summary>
/// Test three-step pipeline implementation using class-based steps and mutators
/// </summary>
public class TestThreeStepPipeline : IPipeline<int>
{
    private readonly Handler<int> _compiledPipeline;
    private readonly Func<int, Task> _handler;

    public TestThreeStepPipeline(IEnumerable<IMutator<Space>> mutators, Func<int, Task> handler)
    {
        _handler = handler;
        var pipeline = CreateAndCompilePipeline();
        _compiledPipeline = pipeline.Compile(cfg =>
        {
            cfg.Configure(mutators);
        });
    }

    public string Name => "TestThreeStepPipeline";

    private Pipeline<int> CreateAndCompilePipeline()
    {
        var firstLinearStep = new TestLinearStep(); // AddConstant step (adds 3)
        var secondLinearStep = new TestSecondLinearStep(); // MultiplyByCoefficient step (multiplies by 2)
        var handlerStep = new TestHandlerStep(_handler);

        var pipeline = Pipelines.CreateAsyncPipeline<int>("TestThreeStepPipeline")
            .StartWithLinear(firstLinearStep)
            .ThenLinear(secondLinearStep)
            .HandleWith(handlerStep)
            .BuildPipeline();

        return pipeline;
    }

    public async Task Run(int input) => await _compiledPipeline(input);
}

/// <summary>
/// Test two-step pipeline implementation using class-based steps and mutators
/// </summary>
public class TestTwoStepPipeline : IPipeline<int>
{
    private readonly Handler<int> _compiledPipeline;
    private readonly Func<int, Task> _handler;

    public TestTwoStepPipeline(IEnumerable<IMutator<Space>> mutators, Func<int, Task> handler)
    {
        _handler = handler;
        var pipeline = CreateAndCompilePipeline();
        _compiledPipeline = pipeline.Compile(cfg =>
        {
            cfg.Configure(mutators);
        });
    }

    public string Name => "TestTwoStepPipeline";

    private Pipeline<int> CreateAndCompilePipeline()
    {
        var linearStep = new TestLinearStep();
        var handlerStep = new TestHandlerStep(_handler);

        var pipeline = Pipelines.CreateAsyncPipeline<int>("TestTwoStepPipeline")
            .StartWithLinear(linearStep)
            .HandleWith(handlerStep)
            .BuildPipeline();

        return pipeline;
    }

    public async Task Run(int input) => await _compiledPipeline(input);
}

/// <summary>
/// Test string parsing linear step implementation
/// </summary>
public class TestStringParseLinearStep : ILinearStep<string, int>
{
    public string Name => "ParseString";

    public async Task Handle(string input, Handler<int> next)
    {
        if (int.TryParse(input, out var parsed))
        {
            await next(parsed);
        }
        // If doesn't parse - don't call next
    }
}

/// <summary>
/// Test add constant linear step implementation
/// </summary>
public class TestAddConstantLinearStep : ILinearStep<int, int>
{
    private const int ConstantToAdd = 3;

    public string Name => "AddConstant";

    public async Task Handle(int input, Handler<int> next)
    {
        var result = input + ConstantToAdd;
        await next(result);
    }
}

/// <summary>
/// Test string parsing pipeline implementation using class-based steps and mutators
/// </summary>
public class TestStringParsingPipeline : IPipeline<string>
{
    private readonly Handler<string> _compiledPipeline;
    private readonly Func<int, Task> _handler;

    public TestStringParsingPipeline(IEnumerable<IMutator<Space>> mutators, Func<int, Task> handler)
    {
        _handler = handler;
        var pipeline = CreateAndCompilePipeline();
        _compiledPipeline = pipeline.Compile(cfg =>
        {
            cfg.Configure(mutators);
        });
    }

    public string Name => "TestStringParsingPipeline";

    private Pipeline<string> CreateAndCompilePipeline()
    {
        var parseStringStep = new TestStringParseLinearStep();
        var addConstantStep = new TestAddConstantLinearStep();
        var handlerStep = new TestHandlerStep(_handler);

        var pipeline = Pipelines.CreateAsyncPipeline<string>("TestStringParsingPipeline")
            .StartWithLinear(parseStringStep)
            .ThenLinear(addConstantStep)
            .HandleWith(handlerStep)
            .BuildPipeline();

        return pipeline;
    }

    public async Task Run(string input) => await _compiledPipeline(input);
}

/// <summary>
/// Test trim string linear step implementation
/// </summary>
public class TestTrimStringLinearStep : ILinearStep<string, string>
{
    public string Name => "TrimString";

    public async Task Handle(string input, Handler<string> next)
    {
        var trimmed = input.Trim();
        await next(trimmed);
    }
}

/// <summary>
/// Test If step implementation that checks int vs float
/// </summary>
public class TestCheckIntOrFloatIfStep : IIfStep<string, string, int>
{
    public string Name => "CheckIntOrFloat";

    public async Task Handle(string input, Handler<string> ifNext, Handler<int> next)
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
            await ifNext(input);
        }
    }

    public OpenPipeline<string, int> BuildTruePipeline(Space space)
    {
        return space.CreatePipeline<string>("FloatProcessing")
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
            .BuildOpenPipeline();
    }
}

/// <summary>
/// Test add constant linear step for if step pipeline
/// </summary>
public class TestIfStepAddConstantLinearStep : ILinearStep<int, int>
{
    private const int ConstantToAdd = 2;

    public string Name => "AddConstant";

    public async Task Handle(int input, Handler<int> next)
    {
        var result = input + ConstantToAdd;
        await next(result);
    }
}

/// <summary>
/// Test if step pipeline implementation using class-based steps and mutators
/// </summary>
public class TestIfStepPipeline : IPipeline<string>
{
    private readonly Handler<string> _compiledPipeline;
    private readonly Func<int, Task> _handler;

    public TestIfStepPipeline(IEnumerable<IMutator<Space>> mutators, Func<int, Task> handler)
    {
        _handler = handler;
        var pipeline = CreateAndCompilePipeline();
        _compiledPipeline = pipeline.Compile(cfg =>
        {
            cfg.Configure(mutators);
        });
    }

    public string Name => "TestIfStepPipeline";

    private Pipeline<string> CreateAndCompilePipeline()
    {
        var trimStep = new TestTrimStringLinearStep();
        var ifStep = new TestCheckIntOrFloatIfStep();
        var addConstantStep = new TestIfStepAddConstantLinearStep();
        var handlerStep = new TestHandlerStep(_handler);

        var pipeline = Pipelines.CreateAsyncPipeline<string>("TestIfStepPipeline")
            .StartWithLinear(trimStep)
            .ThenIf(ifStep)
            .ThenLinear(addConstantStep)
            .HandleWith(handlerStep)
            .BuildPipeline();

        return pipeline;
    }

    public async Task Run(string input) => await _compiledPipeline(input);
}

/// <summary>
/// Test IfElse step implementation that checks int vs float
/// </summary>
public class TestCheckIntOrFloatIfElseStep : IIfElseStep<string, string, int, int>
{
    public string Name => "CheckIntOrFloat";

    public async Task Handle(string input, Handler<string> ifNext, Handler<int> elseNext)
    {
        // Try to parse as int first
        if (int.TryParse(input, out var intValue))
        {
            // If it's an int, use false branch for int processing
            await elseNext(intValue);
        }
        else
        {
            // If not an int, go to true branch (for float parsing)
            await ifNext(input);
        }
    }

    public OpenPipeline<string, int> BuildTruePipeline(Space space)
    {
        // Float processing pipeline: parse float -> round to int
        return space.CreatePipeline<string>("FloatProcessing")
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
            .BuildOpenPipeline();
    }

    public OpenPipeline<int, int> BuildFalsePipeline(Space space)
    {
        // Int processing pipeline: multiply by 2
        return space.CreatePipeline<int>("IntProcessing")
            .StartWithLinear<int>("MultiplyByTwo", async (input, next) =>
            {
                var result = input * 2;
                await next(result);
            })
            .BuildOpenPipeline();
    }
}

/// <summary>
/// Test add constant linear step for IfElse step pipeline
/// </summary>
public class TestIfElseStepAddConstantLinearStep : ILinearStep<int, int>
{
    private const int ConstantToAdd = 3;

    public string Name => "AddConstant";

    public async Task Handle(int input, Handler<int> next)
    {
        var result = input + ConstantToAdd;
        await next(result);
    }
}

/// <summary>
/// Test IfElse step pipeline implementation using class-based steps and mutators
/// </summary>
public class TestIfElseStepPipeline : IPipeline<string>
{
    private readonly Handler<string> _compiledPipeline;
    private readonly Func<int, Task> _handler;

    public TestIfElseStepPipeline(IEnumerable<IMutator<Space>> mutators, Func<int, Task> handler)
    {
        _handler = handler;
        var pipeline = CreateAndCompilePipeline();
        _compiledPipeline = pipeline.Compile(cfg =>
        {
            cfg.Configure(mutators);
        });
    }

    public string Name => "TestIfElseStepPipeline";

    private Pipeline<string> CreateAndCompilePipeline()
    {
        var trimStep = new TestTrimStringLinearStep();
        var ifElseStep = new TestCheckIntOrFloatIfElseStep();
        var addConstantStep = new TestIfElseStepAddConstantLinearStep();
        var handlerStep = new TestHandlerStep(_handler);

        var pipeline = Pipelines.CreateAsyncPipeline<string>("TestIfElseStepPipeline")
            .StartWithLinear(trimStep)
            .ThenIfElse(ifElseStep)
            .ThenLinear(addConstantStep)
            .HandleWith(handlerStep)
            .BuildPipeline();

        return pipeline;
    }

    public async Task Run(string input) => await _compiledPipeline(input);
}

/// <summary>
/// Test switch step implementation that routes by number range
/// </summary>
public class TestNumberRangeSwitchStep : ISwitchStep<string, int, int, int>
{
    public string Name => "NumberRangeSwitch";

    public async Task Handle(string input, IReadOnlyDictionary<string, Handler<int>> cases, Handler<int> defaultNext)
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
    }

    public IReadOnlyDictionary<string, OpenPipeline<int, int>> BuildCasesPipelines(Space space)
    {
        return new Dictionary<string, OpenPipeline<int, int>>
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
        }.AsReadOnly();
    }

    public OpenPipeline<int, int> BuildDefaultPipeline(Space space)
    {
        return space.CreatePipeline<int>("StringLengthPipeline")
            .StartWithLinear<int>("IdentityOperation", async (input, next) =>
            {
                await next(input); // Use string length as-is
            })
            .BuildOpenPipeline();
    }
}

/// <summary>
/// Test switch step add constant linear step
/// </summary>
public class TestSwitchStepAddConstantLinearStep : ILinearStep<int, int>
{
    private const int ConstantToAdd = 3;

    public string Name => "AddConstant";

    public async Task Handle(int input, Handler<int> next)
    {
        var result = input + ConstantToAdd;
        await next(result);
    }
}

/// <summary>
/// Test switch step pipeline implementation using class-based steps and mutators
/// </summary>
public class TestSwitchStepPipeline : IPipeline<string>
{
    private readonly Handler<string> _compiledPipeline;
    private readonly Func<int, Task> _handler;

    public TestSwitchStepPipeline(IEnumerable<IMutator<Space>> mutators, Func<int, Task> handler)
    {
        _handler = handler;
        var pipeline = CreateAndCompilePipeline();
        _compiledPipeline = pipeline.Compile(cfg =>
        {
            cfg.Configure(mutators);
        });
    }

    public string Name => "TestSwitchPipeline";

    private Pipeline<string> CreateAndCompilePipeline()
    {
        var trimStep = new TestTrimStringLinearStep();
        var switchStep = new TestNumberRangeSwitchStep();
        var addConstantStep = new TestSwitchStepAddConstantLinearStep();
        var handlerStep = new TestHandlerStep(_handler);

        var pipeline = Pipelines.CreateAsyncPipeline<string>("TestSwitchPipeline")
            .StartWithLinear(trimStep)
            .ThenSwitch(switchStep)
            .ThenLinear(addConstantStep)
            .HandleWith(handlerStep)
            .BuildPipeline();

        return pipeline;
    }

    public async Task Run(string input) => await _compiledPipeline(input);
}

/// <summary>
/// Test fork step implementation that splits by digit content
/// </summary>
public class TestDigitContentForkStep : IForkStep<string, string, string>
{
    private readonly Func<int, Task> _intHandler;
    private readonly Func<string, Task> _stringHandler;

    public TestDigitContentForkStep(Func<int, Task> intHandler, Func<string, Task> stringHandler)
    {
        _intHandler = intHandler;
        _stringHandler = stringHandler;
    }

    public string Name => "DigitContentFork";

    public async Task Handle(string input, Handler<string> branchANext, Handler<string> branchBNext)
    {
        // Check if string contains only digits (after trimming)
        var containsOnlyDigits = !string.IsNullOrEmpty(input) && input.All(char.IsDigit);

        if (containsOnlyDigits)
        {
            await branchANext(input);
        }
        else
        {
            await branchBNext(input);
        }
    }

    public Pipeline<string> BuildBranchAPipeline(Space space)
    {
        var removeNonDigitsStep = new TestForkStepRemoveNonDigitsLinearStep();
        var parseToIntStep = new TestForkStepParseToIntLinearStep();
        var intHandlerStep = new TestForkStepIntHandlerStep(_intHandler);

        var pipeline = space.CreatePipeline<string>("DigitProcessing")
            .StartWithLinear(removeNonDigitsStep)
            .ThenLinear(parseToIntStep)
            .HandleWith(intHandlerStep)
            .BuildPipeline();

        return pipeline;
    }

    public Pipeline<string> BuildBranchBPipeline(Space space)
    {
        var removeDigitsStep = new TestForkStepRemoveDigitsLinearStep();
        var addSpacesStep = new TestForkStepAddSpacesLinearStep();
        var stringHandlerStep = new TestForkStepStringHandlerStep(_stringHandler);

        var pipeline = space.CreatePipeline<string>("NonDigitProcessing")
            .StartWithLinear(removeDigitsStep)
            .ThenLinear(addSpacesStep)
            .HandleWith(stringHandlerStep)
            .BuildPipeline();

        return pipeline;
    }
}

/// <summary>
/// Test linear step for removing non-digits
/// </summary>
public class TestForkStepRemoveNonDigitsLinearStep : ILinearStep<string, string>
{
    public string Name => "RemoveNonDigits";

    public async Task Handle(string input, Handler<string> next)
    {
        var digitsOnly = new string(input.Where(char.IsDigit).ToArray());
        await next(digitsOnly);
    }
}

/// <summary>
/// Test linear step for parsing string to int
/// </summary>
public class TestForkStepParseToIntLinearStep : ILinearStep<string, int>
{
    public string Name => "ParseToInt";

    public async Task Handle(string input, Handler<int> next)
    {
        if (int.TryParse(input, out var number))
        {
            await next(number);
        }
        else
        {
            await next(0); // Default value if parsing fails
        }
    }
}

/// <summary>
/// Test handler step for int processing
/// </summary>
public class TestForkStepIntHandlerStep : IHandlerStep<int>
{
    private readonly Func<int, Task> _handler;

    public TestForkStepIntHandlerStep(Func<int, Task> handler)
    {
        _handler = handler;
    }

    public string Name => "IntHandler";

    public async Task Handle(int input)
    {
        await _handler(input);
    }
}

/// <summary>
/// Test linear step for removing digits
/// </summary>
public class TestForkStepRemoveDigitsLinearStep : ILinearStep<string, string>
{
    public string Name => "RemoveDigits";

    public async Task Handle(string input, Handler<string> next)
    {
        var nonDigitsOnly = new string(input.Where(c => !char.IsDigit(c)).ToArray());
        await next(nonDigitsOnly);
    }
}

/// <summary>
/// Test linear step for adding spaces
/// </summary>
public class TestForkStepAddSpacesLinearStep : ILinearStep<string, string>
{
    public string Name => "AddSpaces";

    public async Task Handle(string input, Handler<string> next)
    {
        var withSpaces = $"  {input}  ";
        await next(withSpaces);
    }
}

/// <summary>
/// Test handler step for string processing
/// </summary>
public class TestForkStepStringHandlerStep : IHandlerStep<string>
{
    private readonly Func<string, Task> _handler;

    public TestForkStepStringHandlerStep(Func<string, Task> handler)
    {
        _handler = handler;
    }

    public string Name => "StringHandler";

    public async Task Handle(string input)
    {
        await _handler(input);
    }
}

/// <summary>
/// Test fork step pipeline implementation using class-based steps and mutators
/// </summary>
public class TestForkStepPipeline : IPipeline<string>
{
    private readonly Handler<string> _compiledPipeline;
    private readonly Func<int, Task> _intHandler;
    private readonly Func<string, Task> _stringHandler;

    public TestForkStepPipeline(IEnumerable<IMutator<Space>> mutators, Func<int, Task> intHandler, Func<string, Task> stringHandler)
    {
        _intHandler = intHandler;
        _stringHandler = stringHandler;
        var pipeline = CreateAndCompilePipeline();
        _compiledPipeline = pipeline.Compile(cfg =>
        {
            cfg.Configure(mutators);
        });
    }

    public string Name => "TestForkPipeline";

    private Pipeline<string> CreateAndCompilePipeline()
    {
        var trimStep = new TestTrimStringLinearStep();
        var forkStep = new TestDigitContentForkStep(_intHandler, _stringHandler);

        var pipeline = Pipelines.CreateAsyncPipeline<string>("TestForkPipeline")
            .StartWithLinear(trimStep)
            .ThenFork(forkStep)
            .BuildPipeline();

        return pipeline;
    }

    public async Task Run(string input) => await _compiledPipeline(input);
}

/// <summary>
/// Test multi-fork step pipeline implementation using class-based steps and mutators
/// </summary>
public class TestMultiForkStepPipeline : IPipeline<string>
{
    private readonly Handler<string> _compiledPipeline;
    private readonly Func<int, Task> _intHandler;
    private readonly Func<string, Task> _stringHandler;
    private readonly Func<char[], Task> _charArrayHandler;

    public string Name => "TestMultiForkPipeline";

    public TestMultiForkStepPipeline(IEnumerable<IMutator<Space>> mutators, Func<int, Task> intHandler, Func<string, Task> stringHandler, Func<char[], Task> charArrayHandler)
    {
        _intHandler = intHandler;
        _stringHandler = stringHandler;
        _charArrayHandler = charArrayHandler;
        var pipeline = CreateAndCompilePipeline();
        _compiledPipeline = pipeline.Compile(cfg =>
        {
            foreach (var mutator in mutators)
            {
                cfg.Configure(space => mutator.Mutate(space));
            }
        });
    }

    private Pipeline<string> CreateAndCompilePipeline()
    {
        var trimStep = new TestMultiForkTrimStep();
        var multiForkStep = new TestMultiForkStep(_intHandler, _stringHandler, _charArrayHandler);

        var space = Pipelines.CreateAsyncSpace();

        // Create sub-pipelines
        space.CreatePipeline<string>("DigitProcessingPipeline")
            .StartWithLinear(new TestMultiForkParseToIntStep())
            .ThenLinear(new TestMultiForkAddConstantStep())
            .HandleWith(new TestMultiForkIntHandlerStep(_intHandler))
            .BuildPipeline();

        space.CreatePipeline<string>("LetterProcessingPipeline")
            .StartWithLinear(new TestMultiForkAddSpacesStep())
            .HandleWith(new TestMultiForkStringHandlerStep(_stringHandler))
            .BuildPipeline();

        space.CreatePipeline<string>("SpecialCharProcessingPipeline")
            .StartWithLinear(new TestMultiForkRemoveWhitespaceStep())
            .ThenLinear(new TestMultiForkConvertToCharArrayStep())
            .ThenLinear(new TestMultiForkRemoveDuplicatesStep())
            .HandleWith(new TestMultiForkCharArrayHandlerStep(_charArrayHandler))
            .BuildPipeline();

        space.CreatePipeline<char[]>("DefaultProcessingPipeline")
            .StartWithLinear(new TestMultiForkCountDigitsAndLettersStep())
            .ThenLinear(new TestMultiForkCalculateRatioStep())
            .HandleWith(new TestMultiForkDefaultIntHandlerStep(_intHandler))
            .BuildPipeline();

        var pipeline = space.CreatePipeline<string>("TestMultiForkPipeline")
            .StartWithLinear(trimStep)
            .ThenMultiFork(multiForkStep)
            .BuildPipeline();

        return pipeline;
    }

    public async Task Run(string input) => await _compiledPipeline(input);
}

/// <summary>
/// Test trim step for MultiFork pipeline
/// </summary>
public class TestMultiForkTrimStep : ILinearStep<string, string>
{
    public string Name => "TrimString";

    public async Task Handle(string input, Handler<string> next)
    {
        var trimmed = input.Trim();
        await next(trimmed);
    }
}

/// <summary>
/// Test multi-fork step implementation
/// </summary>
public class TestMultiForkStep : IMultiForkStep<string, string, char[]>
{
    private readonly Func<int, Task> _intHandler;
    private readonly Func<string, Task> _stringHandler;
    private readonly Func<char[], Task> _charArrayHandler;

    public string Name => "ClassifyStringContent";

    public TestMultiForkStep(Func<int, Task> intHandler, Func<string, Task> stringHandler, Func<char[], Task> charArrayHandler)
    {
        _intHandler = intHandler;
        _stringHandler = stringHandler;
        _charArrayHandler = charArrayHandler;
    }

    public async Task Handle(string input, IReadOnlyDictionary<string, Handler<string>> branches, Handler<char[]> defaultNext)
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
    }

    public IReadOnlyDictionary<string, Pipeline<string>> BuildBranchesPipelines(Space space)
    {
        return new Dictionary<string, Pipeline<string>>
        {
            ["DigitBranch"] = space.GetPipeline<string>("DigitProcessingPipeline")!,
            ["LetterBranch"] = space.GetPipeline<string>("LetterProcessingPipeline")!,
            ["SpecialCharBranch"] = space.GetPipeline<string>("SpecialCharProcessingPipeline")!
        }.AsReadOnly();
    }

    public Pipeline<char[]> BuildDefaultPipeline(Space space)
    {
        return space.GetPipeline<char[]>("DefaultProcessingPipeline")!;
    }
}

/// <summary>
/// Test parse to int step for MultiFork pipeline
/// </summary>
public class TestMultiForkParseToIntStep : ILinearStep<string, int>
{
    public string Name => "ParseStringToInt";

    public async Task Handle(string input, Handler<int> next)
    {
        if (int.TryParse(input, out var number))
        {
            await next(number);
        }
        else
        {
            await next(0); // Default value if parsing fails
        }
    }
}

/// <summary>
/// Test add constant step for MultiFork pipeline
/// </summary>
public class TestMultiForkAddConstantStep : ILinearStep<int, int>
{
    public string Name => "AddConstant";

    public async Task Handle(int input, Handler<int> next)
    {
        var result = input + 10; // Add constant 10
        await next(result);
    }
}

/// <summary>
/// Test int handler step for MultiFork pipeline
/// </summary>
public class TestMultiForkIntHandlerStep : IHandlerStep<int>
{
    private readonly Func<int, Task> _handler;

    public TestMultiForkIntHandlerStep(Func<int, Task> handler)
    {
        _handler = handler;
    }

    public string Name => "IntHandler";

    public async Task Handle(int input)
    {
        await _handler(input);
    }
}

/// <summary>
/// Test add spaces step for MultiFork pipeline
/// </summary>
public class TestMultiForkAddSpacesStep : ILinearStep<string, string>
{
    public string Name => "AddSpaces";

    public async Task Handle(string input, Handler<string> next)
    {
        var withSpaces = $"  {input}  ";
        await next(withSpaces);
    }
}

/// <summary>
/// Test string handler step for MultiFork pipeline
/// </summary>
public class TestMultiForkStringHandlerStep : IHandlerStep<string>
{
    private readonly Func<string, Task> _handler;

    public TestMultiForkStringHandlerStep(Func<string, Task> handler)
    {
        _handler = handler;
    }

    public string Name => "StringHandler";

    public async Task Handle(string input)
    {
        await _handler(input);
    }
}

/// <summary>
/// Test remove whitespace step for MultiFork pipeline
/// </summary>
public class TestMultiForkRemoveWhitespaceStep : ILinearStep<string, string>
{
    public string Name => "RemoveWhitespace";

    public async Task Handle(string input, Handler<string> next)
    {
        var noWhitespace = new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray());
        await next(noWhitespace);
    }
}

/// <summary>
/// Test convert to char array step for MultiFork pipeline
/// </summary>
public class TestMultiForkConvertToCharArrayStep : ILinearStep<string, char[]>
{
    public string Name => "ConvertToCharArray";

    public async Task Handle(string input, Handler<char[]> next)
    {
        var charArray = input.ToCharArray();
        await next(charArray);
    }
}

/// <summary>
/// Test remove duplicates step for MultiFork pipeline
/// </summary>
public class TestMultiForkRemoveDuplicatesStep : ILinearStep<char[], char[]>
{
    public string Name => "RemoveDuplicates";

    public async Task Handle(char[] input, Handler<char[]> next)
    {
        var uniqueChars = input.Distinct().ToArray();
        await next(uniqueChars);
    }
}

/// <summary>
/// Test char array handler step for MultiFork pipeline
/// </summary>
public class TestMultiForkCharArrayHandlerStep : IHandlerStep<char[]>
{
    private readonly Func<char[], Task> _handler;

    public TestMultiForkCharArrayHandlerStep(Func<char[], Task> handler)
    {
        _handler = handler;
    }

    public string Name => "CharArrayHandler";

    public async Task Handle(char[] input)
    {
        await _handler(input);
    }
}

/// <summary>
/// Test count digits and letters step for MultiFork pipeline
/// </summary>
public class TestMultiForkCountDigitsAndLettersStep : ILinearStep<char[], (int DigitCount, int LetterCount)>
{
    public string Name => "CountDigitsAndLetters";

    public async Task Handle(char[] input, Handler<(int DigitCount, int LetterCount)> next)
    {
        var digitCount = input.Count(char.IsDigit);
        var letterCount = input.Count(char.IsLetter);
        await next((digitCount, letterCount));
    }
}

/// <summary>
/// Test calculate ratio step for MultiFork pipeline
/// </summary>
public class TestMultiForkCalculateRatioStep : ILinearStep<(int DigitCount, int LetterCount), int>
{
    public string Name => "CalculateRatio";

    public async Task Handle((int DigitCount, int LetterCount) input, Handler<int> next)
    {
        // Calculate ratio of digits to letters (floor division)
        var ratio = input.LetterCount > 0 ? input.DigitCount / input.LetterCount : input.DigitCount;
        await next(ratio);
    }
}

/// <summary>
/// Test default int handler step for MultiFork pipeline
/// </summary>
public class TestMultiForkDefaultIntHandlerStep : IHandlerStep<int>
{
    private readonly Func<int, Task> _handler;

    public TestMultiForkDefaultIntHandlerStep(Func<int, Task> handler)
    {
        _handler = handler;
    }

    public string Name => "IntHandler";

    public async Task Handle(int input)
    {
        await _handler(input);
    }
}
