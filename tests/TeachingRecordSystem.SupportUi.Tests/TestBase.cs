using GovUk.Questions.AspNetCore.Testing;
using Microsoft.Extensions.Time.Testing;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Alerts;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserMatching;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.SupportUi.Services.AzureActiveDirectory;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;
using TeachingRecordSystem.TestCommon.Database;
using TeachingRecordSystem.TestCommon.Infrastructure;
using User = TeachingRecordSystem.Core.DataStore.Postgres.Models.User;

namespace TeachingRecordSystem.SupportUi.Tests;

public abstract class TestBase : IDisposable, IAsyncLifetime
{
    private readonly List<IDisposable> _disposables = new();
    private TestDatabaseLease? _databaseLease;
    private IDisposable? _serviceScope;

    protected TestBase(HostFixture hostFixture)
    {
        HostFixture = hostFixture;

        var testScopedServices = TestScopedServices.Reset(HostFixture.Services);
        testScopedServices.EventObserver.Clear();

        HttpClient = hostFixture.CreateClient(new() { AllowAutoRedirect = false });

        SetCurrentUser(HostFixture.AdminUser);
    }

    protected HostFixture HostFixture { get; }

    protected string DatabaseName =>
        _databaseLease?.DatabaseName ?? throw new InvalidOperationException("No database has been leased.");

    public virtual async ValueTask InitializeAsync()
    {
        _databaseLease = await TestDatabases.AcquireAsync(TestContext.Current.CancellationToken);
        _serviceScope = TestServiceScope.Push(HostFixture.RootServices);
    }

    public virtual async ValueTask DisposeAsync()
    {
        _serviceScope?.Dispose();
        _serviceScope = null;

        if (_databaseLease is null)
        {
            return;
        }

        // Keep a failing test's data so it can be inspected: psql -d <name>
        if (TestContext.Current.TestState?.Result == TestResult.Failed)
        {
            TestContext.Current.SendDiagnosticMessage($"Retained test database '{_databaseLease.DatabaseName}'.");
            _databaseLease.Retain();
        }

        await _databaseLease.DisposeAsync();
        _databaseLease = null;
    }

    protected IDbContextFactory<TrsDbContext> DbContextFactory => HostFixture.Services.GetRequiredService<IDbContextFactory<TrsDbContext>>();

    protected EventCapture Events => TestScopedServices.GetCurrent().Events;

    protected CaptureEventObserver EventObserver => TestScopedServices.GetCurrent().EventObserver;

    protected FakeTimeProvider TimeProvider => TestScopedServices.GetCurrent().TimeProvider;

    protected OneLoginUserMatchingSupportTaskService OneLoginSupportTaskService => HostFixture.Services.GetRequiredService<OneLoginUserMatchingSupportTaskService>();

    protected OneLoginService OneLoginService => HostFixture.Services.GetRequiredService<OneLoginService>();

    protected AlertService AlertService => HostFixture.Services.GetRequiredService<AlertService>();

    protected Mock<IAadUserService> AzureActiveDirectoryUserServiceMock =>
        TestScopedServices.GetCurrent().AzureActiveDirectoryUserServiceMock;

    protected HttpClient HttpClient { get; }

    protected TestData TestData => HostFixture.Services.GetRequiredService<TestData>();

    protected JourneyHelper JourneyHelper => HostFixture.Services.GetRequiredService<JourneyHelper>();

    protected TestableFeatureProvider FeatureProvider => TestScopedServices.GetCurrent().FeatureProvider;

    protected ReferenceDataCache ReferenceDataCache => HostFixture.Services.GetRequiredService<ReferenceDataCache>();

    protected Mock<IFileService> FileServiceMock => TestScopedServices.GetCurrent().BlobStorageFileServiceMock;

    protected TrnRequestOptions TrnRequestOptions => TestScopedServices.GetCurrent().TrnRequestOptions;

    protected SupportTaskAssignmentOptions SupportTaskAssignmentOptions => TestScopedServices.GetCurrent().SupportTaskAssignmentOptions;

    public virtual void Dispose()
    {
        _disposables.ForEach(x => x.Dispose());
        _disposables.Clear();
    }

    protected T CreateJourneyCoordinator<T>()
    {
        var scope = HostFixture.Services.CreateScope();
        _disposables.Add(scope);
        return ActivatorUtilities.CreateInstance<T>(scope.ServiceProvider);
    }

    protected Guid GetCurrentUserId() =>
        TestScopedServices.GetCurrent().CurrentUserProvider.CurrentUser?.UserId ?? throw new InvalidOperationException("No current user set.");

    protected void SetCurrentUser(User user) =>
        TestScopedServices.GetCurrent().CurrentUserProvider.CurrentUser = user;

    // The template seeds an admin user so every test has a current user to act as. Tests that assert over
    // the whole users table need to remove it first, which is what [ClearDbBeforeTest] used to do for them.
    protected Task DeleteSeededUsersAsync() => WithDbContextAsync(async dbContext =>
    {
        await dbContext.Users
            .Where(u => u.UserId == HostFixture.AdminUser.UserId)
            .ExecuteDeleteAsync();
    });

    protected Task<T> WithDbContextAsync<T>(Func<TrsDbContext, Task<T>> action) =>
        DbContextFactory.WithDbContextAsync(action);

    protected Task WithDbContextAsync(Func<TrsDbContext, Task> action) =>
        DbContextFactory.WithDbContextAsync(action);

    protected Task<(Person, Alert)> CreatePersonWithOpenAlert(bool populateOptional = true, EventModels.RaisedByUserInfo? createdByUser = null)
    {
        return CreatePersonWithAlert(isOpenAlert: true, populateOptional: populateOptional, createdByUser: createdByUser);
    }

    protected Task<(Person, Alert)> CreatePersonWithClosedAlert(bool populateOptional = true, EventModels.RaisedByUserInfo? createdByUser = null)
    {
        return CreatePersonWithAlert(isOpenAlert: false, populateOptional: populateOptional, createdByUser: createdByUser);
    }

    protected async Task<(Person, Alert)> CreatePersonWithAlert(bool isOpenAlert, bool populateOptional = true, EventModels.RaisedByUserInfo? createdByUser = null)
    {
        var person = await TestData.CreatePersonAsync(p => p
            .WithAlert(a =>
            {
                a.WithStartDate(TimeProvider.Today.AddDays(-30));
                a.WithEndDate(isOpenAlert ? null : TimeProvider.Today.AddDays(-1));
                a.WithExternalLink(populateOptional ? TestData.GenerateUrl() : null);
            }));

        return (person, person.Alerts!.Single());
    }

    protected static HttpContent CreateEvidenceFileBinaryContent(byte[]? content = null)
    {
        var byteArrayContent = new ByteArrayContent(content ?? []);
        byteArrayContent.Headers.Add("Content-Type", "application/octet-stream");
        return byteArrayContent;
    }
}
