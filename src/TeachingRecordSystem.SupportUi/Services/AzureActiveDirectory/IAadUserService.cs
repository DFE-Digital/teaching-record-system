namespace TeachingRecordSystem.SupportUi.Services.AzureActiveDirectory;

public interface IAadUserService
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(string userId);
}
