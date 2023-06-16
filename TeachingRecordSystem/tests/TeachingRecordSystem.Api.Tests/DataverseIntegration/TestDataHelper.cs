#nullable disable
using Microsoft.Extensions.Caching.Memory;
using TeachingRecordSystem.Api.DataStore.Crm;
using TeachingRecordSystem.Api.Tests.DataverseIntegration;

namespace TeachingRecordSystem.Api.Tests;

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
