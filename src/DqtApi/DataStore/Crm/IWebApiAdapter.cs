using System.Threading.Tasks;

namespace DqtApi.DataStore.Crm
{
    public interface IWebApiAdapter
    {
        Task<(int NumberOfRequests, double RemainingExecutionTime)> GetRemainingApiLimits();
    }
}
