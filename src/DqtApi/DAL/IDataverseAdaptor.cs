using System.Threading.Tasks;
using DqtApi.Models;

namespace DqtApi.DAL
{
    public interface IDataverseAdaptor
    {
        Task<Teacher> GetTeacherByTRN(string trn);
    }
}