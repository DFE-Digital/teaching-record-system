#nullable disable
using System;
using System.Threading.Tasks;

namespace QualifiedTeachersApi.Services.GetAnIdentityApi;

public interface IGetAnIdentityApiClient
{
    Task<GetAnIdentityApiUser> GetUserById(Guid userId);

    Task SetTeacherTrn(Guid userId, string trn);
}
