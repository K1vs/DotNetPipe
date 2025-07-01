namespace K1vs.DotNetPipe.AsyncCancellable;

/// <summary>
/// Delegate for a selector that takes an input and two handlers, one for the "if" case and one for the "next" case.
/// This is used in conditional logic within a pipeline.
/// After true handler is executed, the next handler is executed or execution is finished.
/// </summary>
/// <typeparam name="TInput">The type of the input data.</typeparam>
/// <typeparam name="TIfInput">The type of the input data for the "if" case.</typeparam>
/// <typeparam name="TNextInput">The type of the input data for the "next" case.</typeparam>
/// <param name="input">The input data to be processed.</param>
/// <param name="ifNext">The handler to execute if decide in selector.</param>
/// <param name="next">The handler to execute after the "if" handler is executed.</param>
/// <param name="ct">The cancellation token to observe while waiting for the task to complete.</param>
/// <returns>A task representing the asynchronous operation.</returns>
public delegate Task IfSelector<TInput, TIfInput, TNextInput>(TInput input,
    Handler<TIfInput> ifNext,
    Handler<TNextInput> next,
    CancellationToken ct = default);

/// <summary>
/// Delegate for a selector that takes an input and two handlers, one for the "if" case and one for the "else" case.
/// Next handler is executed after the "if" or "else" handler is executed.
/// </summary>
/// <typeparam name="TInput">The type of the input data.</typeparam>
/// <typeparam name="TIfInput">The type of the input data for the "if" case.</typeparam>
/// <typeparam name="TElseInput">The type of the input data for the "else" case.</typeparam>
/// <param name="input">The input data to be processed.</param>
/// <param name="ifNext">The handler to execute if decide in selector.</param>
/// <param name="elseNext">The handler to execute if decide in selector.</param>
/// <param name="ct">The cancellation token to observe while waiting for the task to complete.</param>
/// <returns>A task representing the asynchronous operation.</returns>
public delegate Task IfElseSelector<TInput, TIfInput, TElseInput>(TInput input,
    Handler<TIfInput> ifNext,
    Handler<TElseInput> elseNext,
    CancellationToken ct = default);

/// <summary>
/// Delegate for a selector that takes an input, a dictionary of cases, and a default handler.
/// This is used in switch-case logic within a pipeline.
/// </summary>
/// <typeparam name="TInput">The type of the input data.</typeparam>
/// <typeparam name="TCaseInput">The type of the input data for the case handlers.</typeparam>
/// <typeparam name="TDefaultInput">The type of the input data for the default handler.</typeparam>
/// <param name="input">The input data to be processed.</param>
/// <param name="cases">The dictionary of case handlers.</param>
/// <param name="defaultNext">The default handler to execute if no case matches.</param>
/// <param name="ct">The cancellation token to observe while waiting for the task to complete.</param>
/// <returns>A task representing the asynchronous operation.</returns>
public delegate Task SwitchSelector<TInput, TCaseInput, TDefaultInput>(TInput input,
    IReadOnlyDictionary<string, Handler<TCaseInput>> cases,
    Handler<TDefaultInput> defaultNext,
    CancellationToken ct = default);

/// <summary>
/// Delegate for a fork selector that takes an input and two branch handlers.
/// </summary>
/// <typeparam name="TInput">The type of the input data.</typeparam>
/// <typeparam name="TBranchAInput">The type of the input data for the first branch.</typeparam>
/// <typeparam name="TBranchBInput">The type of the input data for the second branch.</typeparam>
/// <param name="input">The input data to be processed.</param>
/// <param name="branchANext">The handler to execute for the first branch.</param>
/// <param name="branchBNext">The handler to execute for the second branch.</param>
/// <param name="ct">The cancellation token to observe while waiting for the task to complete.</param>
/// <returns>A task representing the asynchronous operation.</returns>
public delegate Task ForkSelector<TInput, TBranchAInput, TBranchBInput>(TInput input,
    Handler<TBranchAInput> branchANext,
    Handler<TBranchBInput> branchBNext,
    CancellationToken ct = default);

/// <summary>
/// Delegate for a multi-fork selector that takes an input, a dictionary of branch handlers, and a default handler.
/// </summary>
/// <typeparam name="TInput">The type of the input data.</typeparam>
/// <typeparam name="TBranchesInput">The type of the input data for the branch handlers.</typeparam>
/// <typeparam name="TDefaultInput">The type of the input data for the default handler.</typeparam>
/// <param name="input">The input data to be processed.</param>
/// <param name="branches">The dictionary of branch handlers.</param>
/// <param name="defaultNext">The default handler to execute if no branch matches.</param>
/// <param name="ct">The cancellation token to observe while waiting for the task to complete.</param>
/// <returns>A task representing the asynchronous operation.</returns>
public delegate Task MultiForkSelector<TInput, TBranchesInput, TDefaultInput>(TInput input,
    IReadOnlyDictionary<string, Handler<TBranchesInput>> branches,
    Handler<TDefaultInput> defaultNext,
    CancellationToken ct = default);

/// <summary>
/// Delegate for a pipe that takes an input and a next handler.
/// </summary>
/// <typeparam name="TInput">The type of the input data.</typeparam>
/// <typeparam name="TNextInput">The type of the input data for the next handler.</typeparam>
/// <param name="input">The input data to be processed.</param>
/// <param name="next">The handler to execute after the current handler.</param>
/// <param name="ct">The cancellation token to observe while waiting for the task to complete.</param>
/// <returns>A task representing the asynchronous operation.</returns>
public delegate Task Pipe<TInput, out TNextInput>(TInput input, Handler<TNextInput> next, CancellationToken ct = default);

/// <summary>
/// Delegate for a handler that processes an input and returns a task.
/// </summary>
/// <typeparam name="TInput">The type of the input data.</typeparam>
/// <param name="input">The input data to be processed.</param>
/// <param name="ct">The cancellation token to observe while waiting for the task to complete.</param>
/// <returns>A task representing the asynchronous operation.</returns>
public delegate Task Handler<in TInput>(TInput input, CancellationToken ct = default);