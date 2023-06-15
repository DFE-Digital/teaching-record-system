using TeachingRecordSystem.Api.Services.GetAnIdentity.Api.Models;

namespace TeachingRecordSystem.Api.Services.GetAnIdentityApi;

public interface IGetAnIdentityApiClient
{
    Task<User?> GetUserById(Guid userId);

    Task<CreateTrnTokenResponse> CreateTrnToken(CreateTrnTokenRequest request);

    Task SetTeacherTrn(Guid userId, string trn);
}
