namespace TeachingRecordSystem.Core.Services;

public static class DbContextExtensions
{
    extension<T>(DbSet<T> set) where T : class
    {
        public async Task<T> FindOrThrowAsync(Guid keyValue, CancellationToken cancellationToken = default) =>
            await set.FindAsync([keyValue], cancellationToken) ?? throw new NotFoundException(keyValue, set.EntityType.Name);

        public async Task<T> FindOrThrowAsync(string keyValue, CancellationToken cancellationToken = default) =>
            await set.FindAsync([keyValue], cancellationToken) ?? throw new NotFoundException(keyValue, set.EntityType.Name);

        public async Task<T> FindOrThrowAsync(object[] keyValues, CancellationToken cancellationToken = default) =>
            await set.FindAsync(keyValues, cancellationToken) ?? throw new NotFoundException(keyValues.Length is 1 ? keyValues[0] : keyValues, set.EntityType.Name);
    }
}
