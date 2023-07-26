namespace TeachingRecordSystem.Core;

public static class SentryErrors
{
    private static readonly AsyncLocal<List<Func<Exception, bool>>> _exceptionsToSkip = new();

    public static bool ShouldReport(Exception ex) => _exceptionsToSkip.Value is null ||
        !_exceptionsToSkip.Value.Any(f => f(ex));

    public static ISuppressScope Suppress<TException>()
        where TException : Exception
    {
        return Suppress<TException>(_ => true);
    }

    public static ISuppressScope Suppress<TException>(Func<TException, bool> filter)
        where TException : Exception
    {
        return Suppress(ex => ex is TException typedException && filter(typedException));
    }

    public static ISuppressScope Suppress(Func<Exception, bool> filter)
    {
        _exceptionsToSkip.Value ??= new();
        _exceptionsToSkip.Value.Add(filter);
        return new SuppressScope(filter, () => _exceptionsToSkip.Value.Remove(filter));
    }

    private class SuppressScope : ISuppressScope
    {
        private readonly Func<Exception, bool> _filter;
        private readonly Action _onDispose;

        public SuppressScope(Func<Exception, bool> filter, Action onDispose)
        {
            _filter = filter;
            _onDispose = onDispose;
        }

        public Func<Exception, bool> ExceptionPredicate => _filter;

        public void Dispose()
        {
            _onDispose();
        }

        public bool IsExceptionSuppressed(Exception ex) => _filter(ex);
    }

    public interface ISuppressScope : IDisposable
    {
        bool IsExceptionSuppressed(Exception ex);
    }
}
