using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DqtApi.DataStore.Crm.Models;

namespace DqtApi.DataStore.Crm
{
    public interface IDataverseAdapter
    {
        Task<CreateTeacherResult> CreateTeacher(CreateTeacherCommand command);

        Task<IEnumerable<Account>> GetIttProviders();

        Task<IEnumerable<Contact>> GetMatchingTeachersAsync(GetTeacherRequest request);

        Task<IEnumerable<dfeta_qualification>> GetQualificationsAsync(Guid teacherId);

        Task<Contact> GetTeacherAsync(Guid teacherId, bool resolveMerges = true, params string[] columnNames);

        Task<bool> UnlockTeacherRecordAsync(Guid teacherId);
    }
}
