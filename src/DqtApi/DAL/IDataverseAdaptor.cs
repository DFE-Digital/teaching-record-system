using DqtApi.Models;

namespace DqtApi.DAL
{
    public interface IDataverseAdaptor
    {
        Contact GetTeacher(GetTeacherRequest request);
    }
}
