namespace TeachingRecordSystem.SupportUi.Services.AzureActiveDirectory;

public interface IUserService
{
    Task<User?> GetUserByEmail(string email);
    Task<User?> GetUserById(string userId);
}
