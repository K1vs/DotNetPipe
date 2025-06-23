namespace K1vs.DotNetPipe.Universal;

public delegate ValueTask IfSelector<TInput, TIfInput, TNextInput>(TInput input,
    Handler<TIfInput> ifNext,
    Handler<TNextInput> next);

public delegate ValueTask IfElseSelector<TInput, TIfInput, TElseInput>(TInput input,
    Handler<TIfInput> ifNext,
    Handler<TElseInput> elseNext);

public delegate ValueTask SwitchSelector<TInput, TCaseInput, TDefaultInput>(TInput input,
    IReadOnlyDictionary<string, Handler<TCaseInput>> cases,
    Handler<TDefaultInput> defaultNext);

public delegate ValueTask ForkSelector<TInput, TBranchAInput, TBranchBInput>(TInput input,
    Handler<TBranchAInput> branchANext,
    Handler<TBranchBInput> branchBNext);

public delegate ValueTask MultiForkSelector<TInput, TBranchesInput, TDefaultInput>(TInput input,
    IReadOnlyDictionary<string, Handler<TBranchesInput>> branches,
    Handler<TDefaultInput> defaultNext);

public delegate ValueTask Pipe<TInput, out TNextInput>(TInput input, Handler<TNextInput> next);

public delegate ValueTask Handler<in TInput>(TInput input);