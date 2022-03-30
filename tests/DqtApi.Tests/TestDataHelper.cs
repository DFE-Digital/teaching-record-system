using DqtApi.DataStore.Crm;
using DqtApi.Tests.DataverseIntegration;
using Microsoft.Extensions.Caching.Memory;

namespace DqtApi.Tests
{
    public partial class TestDataHelper
    {
        private readonly DataverseAdapter _dataverseAdapter;
        private readonly IMemoryCache _globalCache;

        public TestDataHelper(CrmClientFixture.TestDataScope dataScope, IMemoryCache memoryCache)
        {
            _dataverseAdapter = dataScope.CreateDataverseAdapter();
            _globalCache = memoryCache;
        }
    }
}
