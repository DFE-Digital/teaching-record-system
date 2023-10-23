namespace TeachingRecordSystem.SupportUi.Services.AzureActiveDirectory;

public interface IAadUserService
{
    Task<User?> GetUserByEmail(string email);
    Task<User?> GetUserById(string userId);
}
