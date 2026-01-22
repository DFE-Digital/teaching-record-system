using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.Tests.Services;

namespace TeachingRecordSystem.Core.Tests.ApiSchema;

public abstract class EventMapperTestBase(ServiceFixture fixture) : ServiceTestBase(fixture)
{
    protected ReferenceDataCache ReferenceDataCache => Services.GetRequiredService<ReferenceDataCache>();

    protected async Task WithEventMapper<TMapper>(Func<TMapper, Task> action)
    {
        await WithServiceAsync(action);
    }
}
