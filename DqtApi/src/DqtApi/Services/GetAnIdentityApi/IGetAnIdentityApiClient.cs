using System;
using System.Threading.Tasks;

namespace DqtApi.Services.GetAnIdentityApi
{
    public interface IGetAnIdentityApiClient
    {
        Task<GetAnIdentityApiUser> GetUserById(Guid userId);

        Task SetTeacherTrn(Guid userId, string trn);
    }
}
