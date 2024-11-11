namespace TeachingRecordSystem.Core.Dqt;

public interface ICrmTransactionalQueryHandler<TQuery, TResult>
    where TQuery : ICrmTransactionalQuery<TResult>
{
    Func<TResult> AppendQuery(TQuery query, RequestBuilder requestBuilder);
}
