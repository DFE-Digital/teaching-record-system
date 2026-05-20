using TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;

namespace TeachingRecordSystem.Core.Services.GetAnIdentityApi;

public interface IGetAnIdentityApiClient
{
    Task<User?> GetUserByIdAsync(Guid userId);

    Task<CreateTrnTokenResponse> CreateTrnTokenAsync(CreateTrnTokenRequest request);

    Task SetTeacherTrnAsync(Guid userId, string trn);
}
