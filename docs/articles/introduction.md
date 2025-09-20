# Introduction

DotNetPipe is a fluent pipeline builder with minimal overhead and a flexible mutation API. It targets two main use cases:

- Library/framework authors define default pipelines with linear and conditional steps
- End users (library consumers) adjust behavior by attaching mutators to named steps without changing the original code

This page describes step types, pipeline kinds, and provides usage + mutator examples for each.

## Step types

- Handler: terminal step that handles the current input
- Linear: transforms input and forwards it to the next step
- If: routes to a true sub-pipeline or continues the main flow
- IfElse: routes to a true or false sub-pipeline
- Switch: selects a case sub-pipeline by name (with default)
- Fork: splits into two branches, each with its own sub-pipeline
- MultiFork: splits into N named branches with a default

Below each step we show a minimal Universal (ValueTask) snippet and a matching mutator.

### Handler
```csharp
using K1vs.DotNetPipe;
using K1vs.DotNetPipe.Universal;
using K1vs.DotNetPipe.Mutations;

var pipeline = Pipelines.CreatePipeline<int>("P")
    .StartWithHandler("H", async v => { Console.WriteLine(v); await ValueTask.CompletedTask; })
    .BuildPipeline()
    .Compile();

await pipeline(1);
```

```csharp
// Mutator: add 1 before handler
cfg.Configure(space =>
{
    var step = space.GetRequiredHandlerStep<int, int>("P", "H");
    step.Mutators.AddMutator(
        new StepMutator<Handler<int>>("H+1", 1, handler => async input => await handler(input + 1)),
        AddingMode.ExactPlace);
});
```

### Linear
```csharp
var pipeline = Pipelines.CreatePipeline<int>("P")
    .StartWithLinear<int>("L", async (input, next) => await next(input + 10))
    .HandleWith("H", async v => await ValueTask.CompletedTask)
    .BuildPipeline()
    .Compile();
```

```csharp
// Mutator: multiply input by 2 before original linear step
cfg.Configure(space =>
{
    var step = space.GetRequiredLinearStep<int, int, int>("P", "L");
    step.Mutators.AddMutator(
        new StepMutator<Pipe<int, int>>("L*2", 1, pipe => async (i, n) => await pipe(i * 2, n)),
        AddingMode.ExactPlace);
});
```

### If
```csharp
var pipeline = Pipelines.CreatePipeline<string>("P")
    .StartWithLinear<string>("Trim", async (s, next) => await next(s.Trim()))
    .ThenIf<string, int>("CheckInt", async (s, ifNext, next) =>
    {
        if (int.TryParse(s, out var iv)) await next(iv);
        else await ifNext(s);
    }, space => space.CreatePipeline<string>("FloatPath")
        .StartWithLinear<double>("ParseFloat", async (s, n) => { if (double.TryParse(s, out var f)) await n(f); })
        .ThenLinear<int>("Round", async (d, n) => await n((int)Math.Round(d)))
        .BuildOpenPipeline())
    .HandleWith("H", async v => await ValueTask.CompletedTask)
    .BuildPipeline()
    .Compile();
```

```csharp
// Mutator: always prefer FloatPath
cfg.Configure(space =>
{
    var step = space.GetRequiredIfStep<string, string, string, int>("P", "CheckInt");
    step.Mutators.AddMutator(
        new StepMutator<IfSelector<string, string, int>>("AlwaysFloat", 1, sel => async (s, ifNext, next) => await ifNext(s)),
        AddingMode.ExactPlace);
});
```

### IfElse
```csharp
var pipeline = Pipelines.CreatePipeline<string>("P")
    .StartWithLinear<string>("Trim", async (s, n) => await n(s.Trim()))
    .ThenIfElse<string, int, int>("CheckIntOrFloat", async (s, tNext, fNext) =>
    {
        if (int.TryParse(s, out var iv)) await fNext(iv); else await tNext(s);
    },
    space => space.CreatePipeline<string>("FloatPath").StartWithLinear<double>("ParseFloat", async (s, n) => { if (double.TryParse(s, out var f)) await n(f); }).ThenLinear<int>("Round", async (d, n) => await n((int)Math.Round(d))).BuildOpenPipeline(),
    space => space.CreatePipeline<int>("IntPath").StartWithLinear<int>("Mul2", async (i, n) => await n(i * 2)).BuildOpenPipeline())
    .HandleWith("H", async v => await ValueTask.CompletedTask)
    .BuildPipeline()
    .Compile();
```

```csharp
// Mutator: swap branches
cfg.Configure(space =>
{
    var step = space.GetRequiredIfElseStep<string, string, string, int, int>("P", "CheckIntOrFloat");
    step.Mutators.AddMutator(
        new StepMutator<IfElseSelector<string, string, int>>("Swap", 1, sel => async (s, t, f) => await sel(s, f, t)),
        AddingMode.ExactPlace);
});
```

### Switch
```csharp
var space = Pipelines.CreateSpace();
var def = space.CreatePipeline<int>("Default").StartWithLinear<int>("Id", async (i, n) => await n(i)).BuildOpenPipeline();
var pipeline = space.CreatePipeline<string>("P")
    .StartWithLinear<string>("Trim", async (s, n) => await n(s.Trim()))
    .ThenSwitch<int, int, int>("Range", async (s, cases, d) =>
    {
        if (int.TryParse(s, out var n))
        {
            if (n > 100) await cases["Gt100"](n);
            else if (n > 0) await cases["Between"](n);
            else await cases["LeZero"](n);
        }
        else await d(s.Length);
    },
    sp => new Dictionary<string, OpenPipeline<int, int>>
    {
        ["Gt100"] = sp.CreatePipeline<int>("Mul3").StartWithLinear<int>("Mul", async (i, n) => await n(i * 3)).BuildOpenPipeline(),
        ["Between"] = sp.CreatePipeline<int>("Add2").StartWithLinear<int>("Add", async (i, n) => await n(i + 2)).BuildOpenPipeline(),
        ["LeZero"] = sp.CreatePipeline<int>("Mul2").StartWithLinear<int>("Mul", async (i, n) => await n(i * 2)).BuildOpenPipeline(),
    }.AsReadOnly(),
    def)
    .HandleWith("H", async v => await ValueTask.CompletedTask)
    .BuildPipeline()
    .Compile();
```

```csharp
// Mutator: change threshold to > 50
cfg.Configure(space =>
{
    var step = space.GetRequiredSwitchStep<string, string, int, int, int>("P", "Range");
    step.Mutators.AddMutator(
        new StepMutator<SwitchSelector<string, int, int>>(">50", 1, sel => async (s, cases, d) =>
        {
            if (int.TryParse(s, out var n)) { if (n > 50) await cases["Gt100"](n); else if (n > 0) await cases["Between"](n); else await cases["LeZero"](n); }
            else await d(s.Length);
        }),
        AddingMode.ExactPlace);
});
```

### Fork
```csharp
var pipeline = Pipelines.CreatePipeline<string>("P")
    .StartWithLinear<string>("Trim", async (s, n) => await n(s.Trim()))
    .ThenFork<string, string>("DigitsOrOther", async (s, a, b) => { if (s.All(char.IsDigit)) await a(s); else await b(s); },
        sp => sp.CreatePipeline<string>("Digits").StartWithHandler("IntH", async s => await ValueTask.CompletedTask).BuildPipeline(),
        sp => sp.CreatePipeline<string>("Other").StartWithHandler("StrH", async s => await ValueTask.CompletedTask).BuildPipeline())
    .BuildPipeline()
    .Compile();
```

```csharp
// Mutator: send strings with length > 3 to digits branch
cfg.Configure(space =>
{
    var step = space.GetRequiredForkStep<string, string, string, string>("P", "DigitsOrOther");
    step.Mutators.AddMutator(
        new StepMutator<ForkSelector<string, string, string>>("Len>3", 1, sel => async (s, a, b) => { if (s.Length > 3) await a(s); else await b(s); }),
        AddingMode.ExactPlace);
});
```

### MultiFork
```csharp
var space = Pipelines.CreateSpace();
space.CreatePipeline<string>("Digits").StartWithHandler("IntH", async s => await ValueTask.CompletedTask).BuildPipeline();
space.CreatePipeline<string>("Letters").StartWithHandler("StrH", async s => await ValueTask.CompletedTask).BuildPipeline();
space.CreatePipeline<string>("Special").StartWithHandler("CharH", async s => await ValueTask.CompletedTask).BuildPipeline();
var def = space.CreatePipeline<char[]>("Default").StartWithHandler("DefH", async arr => await ValueTask.CompletedTask).BuildPipeline();

var pipeline = space.CreatePipeline<string>("P")
    .StartWithLinear<string>("Trim", async (s, n) => await n(s.Trim()))
    .ThenMultiFork<string, char[]>("Classify", async (s, branches, d) =>
    {
        if (s.All(char.IsDigit)) await branches["Digits"](s);
        else if (s.All(char.IsLetter)) await branches["Letters"](s);
        else if (s.All(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c))) await branches["Special"](s);
        else await d(s.ToCharArray());
    },
    sp => new Dictionary<string, Pipeline<string>>
    {
        ["Digits"] = sp.GetPipeline<string>("Digits")!,
        ["Letters"] = sp.GetPipeline<string>("Letters")!,
        ["Special"] = sp.GetPipeline<string>("Special")!
    }.AsReadOnly(),
    sp => sp.GetPipeline<char[]>("Default")!)
    .BuildPipeline()
    .Compile();
```

```csharp
// Mutator: treat everything that is not digits-only as Special
cfg.Configure(space =>
{
    var step = space.GetRequiredMultiForkStep<string, string, string, char[]>("P", "Classify");
    step.Mutators.AddMutator(
        new StepMutator<MultiForkSelector<string, string, char[]>>("PreferSpecial", 1, sel => async (s, branches, d) =>
        {
            if (s.All(char.IsDigit)) await branches["Digits"](s); else await branches["Special"](s);
        }),
        AddingMode.ExactPlace);
});
```

## Pipeline kinds

- Universal: `ValueTask`-based (default in examples above)
- Async: `Task`-based
- Sync: synchronous (void)

Each has cancellable variants (accepting `CancellationToken`) and returning variants (pipelines that return a result instead of `void`/`ValueTask`).

### Returning example (Universal)
```csharp
using K1vs.DotNetPipe.Returning;

var space = Pipelines.CreateReturningSpace();
var compiled = space.CreatePipeline<int, int>("P")
    .StartWithLinear<int, int>("AddConst", async (v, next) => await next(v + 10))
    .HandleWith("H", async v => await Task.FromResult(v))
    .BuildPipeline().Compile();

var result = await compiled(5); // 15
```

```csharp
// Mutator: add +5 before linear step
cfg.Configure(s =>
{
    var step = s.GetRequiredLinearStep<int, int, int>("P", "AddConst");
    step.Mutators.AddMutator(new StepMutator<K1vs.DotNetPipe.Returning.Pipe<int, int>>("Add+5", 1, pipe => async (i, n) => await pipe(i + 5, n)), AddingMode.ExactPlace);
});
```

### Async and Sync variants
The same patterns apply to Async (`K1vs.DotNetPipe.Async`) and Sync (`K1vs.DotNetPipe.Sync`) namespaces. Replace delegate return types accordingly (`Task` or `void`), and use cancellable variants when you need a `CancellationToken`.