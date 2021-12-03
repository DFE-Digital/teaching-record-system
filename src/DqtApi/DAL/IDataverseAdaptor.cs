using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DqtApi.Models;

namespace DqtApi.DAL
{
    public interface IDataverseAdaptor
    {
        Task<IEnumerable<Contact>> GetMatchingTeachersAsync(GetTeacherRequest request);

        Task<IEnumerable<dfeta_qualification>> GetQualificationsAsync(Guid teacherId);
    }
}
