namespace QualifiedTeachersApi.Infrastructure.Logging;

public static class LogSuppressions
{
    private static readonly AsyncLocal<List<Predicate<Exception>>> _exceptionsToIgnore = new();

    public static bool ShouldIgnoreException(Exception exception) =>
        _exceptionsToIgnore.Value is not null && _exceptionsToIgnore.Value.Any(f => f(exception));

    public static IDisposable SuppressException<TException>()
        where TException : Exception
    {
        return SuppressException<TException>(_ => true);
    }

    public static IDisposable SuppressException<TException>(Func<TException, bool> filter)
        where TException : Exception
    {
        return SuppressException(ex => ex is TException typedException && filter(typedException));
    }

    public static IDisposable SuppressException(Predicate<Exception> filter)
    {
        _exceptionsToIgnore.Value ??= new();
        _exceptionsToIgnore.Value.Add(filter);
        return new SuppressExceptionScope(() => _exceptionsToIgnore.Value.Remove(filter));
    }

    private class SuppressExceptionScope : IDisposable
    {
        private readonly Action _onDispose;

        public SuppressExceptionScope(Action onDispose)
        {
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            _onDispose();
        }
    }
}
