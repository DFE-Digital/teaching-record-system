using System.Threading.Tasks;

namespace QualifiedTeachersApi.DataStore.Crm;

public interface IWebApiAdapter
{
    Task<(int NumberOfRequests, double RemainingExecutionTime)> GetRemainingApiLimits();
}
