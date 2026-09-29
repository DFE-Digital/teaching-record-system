namespace TeachingRecordSystem.SupportUi.Services.AzureActiveDirectory;

public interface IAadUserService
{
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);
}
