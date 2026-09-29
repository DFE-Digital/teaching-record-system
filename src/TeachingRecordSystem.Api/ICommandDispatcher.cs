namespace TeachingRecordSystem.Api;

public interface ICommandDispatcher
{
    Task<ApiResult<TResult>> DispatchAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken)
        where TResult : notnull;
}

public class CommandDispatcher(IServiceProvider serviceProvider) : ICommandDispatcher
{
    public async Task<ApiResult<TResult>> DispatchAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken)
        where TResult : notnull
    {
        var handler = serviceProvider.GetRequiredService(typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResult)));

        var wrapperHandlerType = typeof(CommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResult));
        var wrappedHandler = (CommandHandler<TResult>)Activator.CreateInstance(wrapperHandlerType, handler)!;

        return await wrappedHandler.ExecuteAsync(command, cancellationToken);
    }

    private abstract class CommandHandler<T> where T : notnull
    {
        public abstract Task<ApiResult<T>> ExecuteAsync(ICommand<T> query, CancellationToken cancellationToken);
    }

    private class CommandHandler<TCommand, TResult>(ICommandHandler<TCommand, TResult> innerHandler) : CommandHandler<TResult>
        where TCommand : ICommand<TResult>
        where TResult : notnull
    {
        public override Task<ApiResult<TResult>> ExecuteAsync(
            ICommand<TResult> command,
            CancellationToken cancellationToken)
        {
            return innerHandler.ExecuteAsync((TCommand)command, cancellationToken);
        }
    }
}
