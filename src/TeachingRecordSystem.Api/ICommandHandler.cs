namespace TeachingRecordSystem.Api;

public interface ICommandHandler<TCommand, TResult> where TCommand : ICommand<TResult> where TResult : notnull
{
    Task<ApiResult<TResult>> ExecuteAsync(TCommand command);
}
