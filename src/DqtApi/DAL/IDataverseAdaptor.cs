using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DqtApi.Models;

namespace DqtApi.DAL
{
    public interface IDataverseAdaptor
    {
        Task<IEnumerable<Account>> GetIttProviders();

        Task<IEnumerable<Contact>> GetMatchingTeachersAsync(GetTeacherRequest request);

        Task<IEnumerable<dfeta_qualification>> GetQualificationsAsync(Guid teacherId);

        Task<Contact> GetTeacherAsync(Guid teacherId);

        Task<bool> UnlockTeacherRecordAsync(Guid teacherId);
    }
}
