using FakeXrmEasy;
using FakeXrmEasy.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;

namespace TeachingRecordSystem.Api.UnitTests;

public abstract class OperationTestBase
{
    private readonly TestScopedServices _testServices;

    protected OperationTestBase(OperationTestFixture operationTestFixture)
    {
        OperationTestFixture = operationTestFixture;

        _testServices = TestScopedServices.Reset();
    }

    public OperationTestFixture OperationTestFixture { get; }

    public DbFixture DbFixture => OperationTestFixture.DbFixture;

    public TestableClock Clock => _testServices.Clock;

    public ICurrentUserProvider CurrentUserProvider => OperationTestFixture.Services.GetRequiredService<ICurrentUserProvider>();

    public TestData TestData => OperationTestFixture.TestData;

    public CrmQueryDispatcherSpy CrmQueryDispatcherSpy => _testServices.CrmQueryDispatcherSpy;

    public IXrmFakedContext XrmFakedContext => OperationTestFixture.Services.GetRequiredService<IXrmFakedContext>();

    public Mock<IGetAnIdentityApiClient> GetAnIdentityApiClientMock => Mock.Get(OperationTestFixture.Services.GetRequiredService<IGetAnIdentityApiClient>());

    public T AssertSuccess<T>(ApiResult<T> result) where T : notnull
    {
        Assert.False(result.IsError, "Result is not in a Success state.");
        return result.GetSuccess();
    }

    public ApiError AssertError<T>(ApiResult<T> result, int expectedErrorCode) where T : notnull
    {
        Assert.True(result.IsError, "Result is not in a Error state.");
        var error = result.GetError();
        Assert.Equal(expectedErrorCode, error.ErrorCode);
        return error;
    }

    public async Task WithHandler<THandler>(Func<THandler, Task> action, params object[] parameters) where THandler : notnull
    {
        var serviceScopeFactory = OperationTestFixture.Services.GetRequiredService<IServiceScopeFactory>();
        using var scope = serviceScopeFactory.CreateScope();
        var handler = ActivatorUtilities.CreateInstance<THandler>(scope.ServiceProvider, parameters);
        await action(handler);
    }
}
