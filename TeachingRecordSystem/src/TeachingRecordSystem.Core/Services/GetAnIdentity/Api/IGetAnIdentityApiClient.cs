using TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;

namespace TeachingRecordSystem.Core.Services.GetAnIdentityApi;

public interface IGetAnIdentityApiClient
{
    Task<User?> GetUserById(Guid userId);

    Task<CreateTrnTokenResponse> CreateTrnToken(CreateTrnTokenRequest request);

    Task SetTeacherTrn(Guid userId, string trn);
}
