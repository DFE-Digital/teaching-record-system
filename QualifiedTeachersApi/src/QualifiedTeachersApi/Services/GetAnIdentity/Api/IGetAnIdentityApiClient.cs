using System;
using System.Threading.Tasks;
using QualifiedTeachersApi.Services.GetAnIdentity.Api.Models;

namespace QualifiedTeachersApi.Services.GetAnIdentityApi;

public interface IGetAnIdentityApiClient
{
    Task<User?> GetUserById(Guid userId);

    Task SetTeacherTrn(Guid userId, string trn);
}
