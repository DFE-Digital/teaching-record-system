namespace TeachingRecordSystem.Core.Services;

public static class DbContextExtensions
{
    extension<T>(DbSet<T> set) where T : class
    {
        public async Task<T> FindOrThrowAsync(Guid keyValue) =>
            await set.FindAsync(keyValue) ?? throw new NotFoundException(keyValue, set.EntityType.Name);

        public async Task<T> FindOrThrowAsync(string keyValue) =>
            await set.FindAsync(keyValue) ?? throw new NotFoundException(keyValue, set.EntityType.Name);

        public async Task<T> FindOrThrowAsync(object[] keyValues) =>
            await set.FindAsync(keyValues) ?? throw new NotFoundException(keyValues.Length is 1 ? keyValues[0] : keyValues, set.EntityType.Name);
    }
}
