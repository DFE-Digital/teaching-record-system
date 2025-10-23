using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Models.SupportTasks;
using Xunit.Abstractions;
using static TeachingRecordSystem.TestCommon.TestData;

namespace TeachingRecordSystem.Core.Tests.Jobs;

[Collection(nameof(DisableParallelization))]
public class DeletePersonAndChildRecordsWithoutATrnJobTests(
        DeletePersonAndChildRecordsWithoutATrnJobFixture fixture,
        ITestOutputHelper outputHelper)
    : IClassFixture<DeletePersonAndChildRecordsWithoutATrnJobFixture>, IAsyncLifetime
{
    public TrsDbContext DbContext => fixture.DbContext;
    public DbFixture DbFixture => fixture.DbFixture;
    public TestData TestData => fixture.TestData;
    public TestableClock Clock => fixture.Clock;
    public ILoggerFactory LoggerFactory => fixture.LoggerFactory;
    public TestFileStorageService FileStorageService => fixture.FileStorageService;
    public ILogger<DeletePersonAndChildRecordsWithoutATrnJob> TestOutputLogger { get; } = new TestOutputLogger<DeletePersonAndChildRecordsWithoutATrnJob>(outputHelper);

    public async Task InitializeAsync()
    {
        await DbFixture.DbHelper.ClearDataAsync();
        FileStorageService.Clear();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Execute_WithDryRunTrue_DoesNotDeleteAnyPersons()
    {
        // Arrange
        var (personsWithNoTrn, _, _, _) = await CreatePersonsWithNoTrnAsync(3);
        var (personsWithTrn, _, _, _) = await CreatePersonsWithTrnAsync(3);

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(true, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();

        AssertEx.ContainsAll(personsWithNoTrn, personsAfterDelete);

        AssertEx.ContainsAll(personsWithTrn, personsAfterDelete);
    }

    [Fact]
    public async Task Execute_WithDryRunFalse_OnlyDeletesPersonsWithNullTrn()
    {
        // Arrange
        var (personsWithNoTrn, _, _, _) = await CreatePersonsWithNoTrnAsync(3);
        var (personsWithTrn, _, _, _) = await CreatePersonsWithTrnAsync(3);

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();

        AssertEx.DoesNotContainAny(personsWithNoTrn, personsAfterDelete);

        AssertEx.ContainsAll(personsWithTrn, personsAfterDelete);
    }

    [Fact]
    public async Task Execute_WithDryRunTrue_UploadsFileWithPersonIdsThatWouldHaveBeenDeleted()
    {
        // Arrange
        var (personsWithNoTrn, _, _, _) = await CreatePersonsWithNoTrnAsync(3);
        var (personsWithTrn, _, _, _) = await CreatePersonsWithTrnAsync(3);

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 2);

        // Act
        await job.ExecuteAsync(true, CancellationToken.None);

        // Assert
        var file = FileStorageService.GetLastUploadedFile();

        // Assert
        Assert.NotNull(file);
        Assert.Equal(DeletePersonAndChildRecordsWithoutATrnJob.ContainerName, file.ContainerName);
        Assert.StartsWith($"{DeletePersonAndChildRecordsWithoutATrnJob.OutputFolderName}/{DeletePersonAndChildRecordsWithoutATrnJob.OutputFileNamePrefix}", file.FileName, StringComparison.InvariantCultureIgnoreCase);

        Assert.NotNull(file.Content);
        var deleted = ReadAsCsvRows(file.Content);
        Assert.Equivalent(personsWithNoTrn.Select(p => new CsvRow(p)), deleted);
    }

    [Fact]
    public async Task Execute_WithDryRunFalse_UploadsFileWithPersonIdsThatWereDeleted()
    {
        // Arrange
        var (personsWithNoTrn, _, _, _) = await CreatePersonsWithNoTrnAsync(3);
        var (personsWithTrn, _, _, _) = await CreatePersonsWithTrnAsync(3);

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 2);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var file = FileStorageService.GetLastUploadedFile();

        // Assert
        Assert.NotNull(file);
        Assert.Equal(DeletePersonAndChildRecordsWithoutATrnJob.ContainerName, file.ContainerName);
        Assert.StartsWith($"{DeletePersonAndChildRecordsWithoutATrnJob.OutputFolderName}/{DeletePersonAndChildRecordsWithoutATrnJob.OutputFileNamePrefix}", file.FileName, StringComparison.InvariantCultureIgnoreCase);

        Assert.NotNull(file.Content);
        var deleted = ReadAsCsvRows(file.Content);
        Assert.Equivalent(personsWithNoTrn.Select(p => new CsvRow(p)), deleted);
    }

    [Fact]
    public async Task Execute_WhenPersonsReferencedBySupportTaskViaPersonId_DeletesSupportTasksForPersonsWithNoTrn()
    {
        // Arrange
        var (personsWithNoTrn, _, _, _) = await CreatePersonsWithNoTrnAsync(3);
        var (personsWithTrn, _, _, _) = await CreatePersonsWithTrnAsync(3);

        var supportTasksForPersonsWithNoTrn = await CreateSupportTasksReferencingPersonViaPersonIdAsync(personsWithNoTrn);
        var supportTasksForPersonsWithTrn = await CreateSupportTasksReferencingPersonViaPersonIdAsync(personsWithTrn);

        Assert.Empty(await GetTrnRequestsAsync());
        Assert.Empty(await GetTrnRequestMetadataAsync());

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();
        var supportTasksAfterDelete = await GetSupportTasksAsync();

        AssertEx.DoesNotContainAny(personsWithNoTrn, personsAfterDelete);
        AssertEx.DoesNotContainAny(supportTasksForPersonsWithNoTrn, supportTasksAfterDelete);

        AssertEx.ContainsAll(personsWithTrn, personsAfterDelete);
        AssertEx.ContainsAll(supportTasksForPersonsWithTrn, supportTasksAfterDelete);
    }

    [Fact]
    public async Task Execute_WhenPersonsReferencedByTrnRequestMetadataViaResolvedPersonId_DeletesSupportTasksRequestsAndRequestMetadataForPersonsWithNoTrn()
    {
        // Arrange
        var (personsWithNoTrn, _, _, _) = await CreatePersonsWithNoTrnAsync(3);
        var (personsWithTrn, _, _, _) = await CreatePersonsWithTrnAsync(3);

        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTasksForPersonsWithNoTrn, requestIdsForPersonsWithNoTrn) = await CreateSupportTasksReferencingPersonViaResolvedPersonIdAsync(personsWithNoTrn, applicationUser.UserId);
        var (supportTasksForPersonsWithTrn, requestIdsForPersonsWithTrn) = await CreateSupportTasksReferencingPersonViaResolvedPersonIdAsync(personsWithTrn, applicationUser.UserId);

        Assert.NotEmpty(await GetTrnRequestsAsync());
        Assert.NotEmpty(await GetTrnRequestMetadataAsync());

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();
        var supportTasksAfterDelete = await GetSupportTasksAsync();
        var trnRequestsAfterDelete = await GetTrnRequestsAsync();
        var trnRequestMetaDataAfterDelete = await GetTrnRequestMetadataAsync();

        AssertEx.DoesNotContainAny(personsWithNoTrn, personsAfterDelete);
        AssertEx.DoesNotContainAny(supportTasksForPersonsWithNoTrn, supportTasksAfterDelete);
        AssertEx.DoesNotContainAny(requestIdsForPersonsWithNoTrn, trnRequestsAfterDelete);
        AssertEx.DoesNotContainAny(requestIdsForPersonsWithNoTrn, trnRequestMetaDataAfterDelete);

        AssertEx.ContainsAll(personsWithTrn, personsAfterDelete);
        AssertEx.ContainsAll(supportTasksForPersonsWithTrn, supportTasksAfterDelete);
        AssertEx.ContainsAll(requestIdsForPersonsWithTrn, trnRequestsAfterDelete);
        AssertEx.ContainsAll(requestIdsForPersonsWithTrn, trnRequestMetaDataAfterDelete);

        Assert.Contains(applicationUser.UserId, await GetApplicationUsersAsync());
    }

    [Fact]
    public async Task Execute_WhenPersonsReferencedByTrnRequestMetadataViaResolvedPersonId_AndOtherPersonReferencesTrnRequestMetadataViaSourceTrnRequestId_DeletesSupportTasksRequestsAndRequestMetadataForPersonsWithNoTrn()
    {
        // Arrange
        var (personsWithNoTrn, _, _, _) = await CreatePersonsWithNoTrnAsync(3);
        var (personsWithTrn, _, _, _) = await CreatePersonsWithTrnAsync(3);

        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTasksForPersonsWithNoTrn, requestIdsForPersonsWithNoTrn) = await CreateSupportTasksReferencingPersonViaResolvedPersonIdAsync(personsWithNoTrn, applicationUser.UserId);
        var (supportTasksForPersonsWithTrn, requestIdsForPersonsWithTrn) = await CreateSupportTasksReferencingPersonViaResolvedPersonIdAsync(personsWithTrn, applicationUser.UserId);

        var (otherPersonsWithNoTrn, _, _, _) = await CreatePersonsWithNoTrnAsync(6, sourceRequestIds: requestIdsForPersonsWithNoTrn.Concat(requestIdsForPersonsWithTrn));
        var (otherPersonsWithTrn, _, _, _) = await CreatePersonsWithTrnAsync(6, sourceRequestIds: requestIdsForPersonsWithNoTrn.Concat(requestIdsForPersonsWithTrn));

        Assert.NotEmpty(await GetTrnRequestsAsync());
        Assert.NotEmpty(await GetTrnRequestMetadataAsync());

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();
        var supportTasksAfterDelete = await GetSupportTasksAsync();
        var trnRequestsAfterDelete = await GetTrnRequestsAsync();
        var trnRequestMetaDataAfterDelete = await GetTrnRequestMetadataAsync();

        AssertEx.DoesNotContainAny(personsWithNoTrn, personsAfterDelete);
        AssertEx.DoesNotContainAny(otherPersonsWithNoTrn, personsAfterDelete);
        AssertEx.DoesNotContainAny(supportTasksForPersonsWithNoTrn, supportTasksAfterDelete);
        AssertEx.DoesNotContainAny(requestIdsForPersonsWithNoTrn, trnRequestsAfterDelete);
        AssertEx.DoesNotContainAny(requestIdsForPersonsWithNoTrn, trnRequestMetaDataAfterDelete);

        AssertEx.ContainsAll(personsWithTrn, personsAfterDelete);
        AssertEx.ContainsAll(otherPersonsWithTrn, personsAfterDelete);
        AssertEx.ContainsAll(supportTasksForPersonsWithTrn, supportTasksAfterDelete);
        AssertEx.ContainsAll(requestIdsForPersonsWithTrn, trnRequestsAfterDelete);
        AssertEx.ContainsAll(requestIdsForPersonsWithTrn, trnRequestMetaDataAfterDelete);

        Assert.Contains(applicationUser.UserId, await GetApplicationUsersAsync());
    }

    [Fact]
    public async Task Execute_WhenPersonsReferencedByTrnRequestMetadataViaResolvedPersonIdAndPersonId_DeletesSupportTasksRequestsAndRequestMetadataForPersonsWithNoTrn()
    {
        // Arrange
        var (personsWithNoTrn, _, _, _) = await CreatePersonsWithNoTrnAsync(3);
        var (personsWithTrn, _, _, _) = await CreatePersonsWithTrnAsync(3);

        var applicationUser = await TestData.CreateApplicationUserAsync();

        var olusForPersonsWithNoTrn = await CreateOneLoginUsersAsync(personsWithNoTrn, applicationUser.UserId);
        var olusForPersonsWithTrn = await CreateOneLoginUsersAsync(personsWithTrn, applicationUser.UserId);

        var (supportTasksForPersonsWithNoTrn, requestIdsForPersonsWithNoTrn) = await CreateSupportTasksReferencingPersonViaResolvedPersonIdAndPersonIdAsync(personsWithNoTrn.Zip(olusForPersonsWithNoTrn), applicationUser.UserId);
        var (supportTasksForPersonsWithTrn, requestIdsForPersonsWithTrn) = await CreateSupportTasksReferencingPersonViaResolvedPersonIdAndPersonIdAsync(personsWithTrn.Zip(olusForPersonsWithTrn), applicationUser.UserId);

        Assert.NotEmpty(await GetTrnRequestsAsync());
        Assert.NotEmpty(await GetTrnRequestMetadataAsync());

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();
        var supportTasksAfterDelete = await GetSupportTasksAsync();
        var trnRequestsAfterDelete = await GetTrnRequestsAsync();
        var trnRequestMetaDataAfterDelete = await GetTrnRequestMetadataAsync();
        var olusAfterDelete = await GetOneLoginUsersAsync();

        AssertEx.DoesNotContainAny(personsWithNoTrn, personsAfterDelete);
        AssertEx.DoesNotContainAny(supportTasksForPersonsWithNoTrn, supportTasksAfterDelete);
        AssertEx.DoesNotContainAny(requestIdsForPersonsWithNoTrn, trnRequestsAfterDelete);
        AssertEx.DoesNotContainAny(requestIdsForPersonsWithNoTrn, trnRequestMetaDataAfterDelete);
        AssertEx.ContainsAll(olusForPersonsWithNoTrn, olusAfterDelete);

        AssertEx.ContainsAll(personsWithTrn, personsAfterDelete);
        AssertEx.ContainsAll(supportTasksForPersonsWithTrn, supportTasksAfterDelete);
        AssertEx.ContainsAll(requestIdsForPersonsWithTrn, trnRequestsAfterDelete);
        AssertEx.ContainsAll(requestIdsForPersonsWithTrn, trnRequestMetaDataAfterDelete);
        AssertEx.ContainsAll(olusForPersonsWithTrn, olusAfterDelete);

        Assert.Contains(applicationUser.UserId, await GetApplicationUsersAsync());
    }

    [Fact]
    public async Task Execute_WhenPersonsReferencedByTrnRequestMetadataViaResolvedPersonIdAndPersonId_AndOtherPersonReferencesTrnRequestMetadataViaSourceTrnRequestId_DeletesSupportTasksRequestsAndRequestMetadataForPersonsWithNoTrn()
    {
        // Arrange
        var (personsWithNoTrn, _, _, _) = await CreatePersonsWithNoTrnAsync(3);
        var (personsWithTrn, _, _, _) = await CreatePersonsWithTrnAsync(3);

        var applicationUser = await TestData.CreateApplicationUserAsync();

        var olusForPersonsWithNoTrn = await CreateOneLoginUsersAsync(personsWithNoTrn, applicationUser.UserId);
        var olusForPersonsWithTrn = await CreateOneLoginUsersAsync(personsWithTrn, applicationUser.UserId);

        var (supportTasksForPersonsWithNoTrn, requestIdsForPersonsWithNoTrn) = await CreateSupportTasksReferencingPersonViaResolvedPersonIdAndPersonIdAsync(personsWithNoTrn.Zip(olusForPersonsWithNoTrn), applicationUser.UserId);
        var (supportTasksForPersonsWithTrn, requestIdsForPersonsWithTrn) = await CreateSupportTasksReferencingPersonViaResolvedPersonIdAndPersonIdAsync(personsWithTrn.Zip(olusForPersonsWithTrn), applicationUser.UserId);

        var (otherPersonsWithNoTrn, _, _, _) = await CreatePersonsWithNoTrnAsync(6, sourceRequestIds: requestIdsForPersonsWithNoTrn.Concat(requestIdsForPersonsWithTrn));
        var (otherPersonsWithTrn, _, _, _) = await CreatePersonsWithTrnAsync(6, sourceRequestIds: requestIdsForPersonsWithNoTrn.Concat(requestIdsForPersonsWithTrn));

        Assert.NotEmpty(await GetTrnRequestsAsync());
        Assert.NotEmpty(await GetTrnRequestMetadataAsync());

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();
        var supportTasksAfterDelete = await GetSupportTasksAsync();
        var trnRequestsAfterDelete = await GetTrnRequestsAsync();
        var trnRequestMetaDataAfterDelete = await GetTrnRequestMetadataAsync();
        var olusAfterDelete = await GetOneLoginUsersAsync();

        AssertEx.DoesNotContainAny(personsWithNoTrn, personsAfterDelete);
        AssertEx.DoesNotContainAny(supportTasksForPersonsWithNoTrn, supportTasksAfterDelete);
        AssertEx.DoesNotContainAny(requestIdsForPersonsWithNoTrn, trnRequestsAfterDelete);
        AssertEx.DoesNotContainAny(requestIdsForPersonsWithNoTrn, trnRequestMetaDataAfterDelete);
        AssertEx.ContainsAll(olusForPersonsWithNoTrn, olusAfterDelete);

        AssertEx.ContainsAll(personsWithTrn, personsAfterDelete);
        AssertEx.ContainsAll(supportTasksForPersonsWithTrn, supportTasksAfterDelete);
        AssertEx.ContainsAll(requestIdsForPersonsWithTrn, trnRequestsAfterDelete);
        AssertEx.ContainsAll(requestIdsForPersonsWithTrn, trnRequestMetaDataAfterDelete);
        AssertEx.ContainsAll(olusForPersonsWithTrn, olusAfterDelete);

        Assert.Contains(applicationUser.UserId, await GetApplicationUsersAsync());
    }

    [Fact]
    public async Task Execute_WhenPersonsReferencedByTrnRequestMetadataViaMatchedPersons_DoesNotDeleteSupportTaskOrMetadataForPersonsWithNoTrn()
    {
        // Arrange
        var (personsWithNoTrn, _, _, _) = await CreatePersonsWithNoTrnAsync(3);
        var (personsWithTrn, _, _, _) = await CreatePersonsWithTrnAsync(3);

        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTasksForPersonsWithNoTrn, requestIdsForPersonWithNoTrn) = await CreateSupportTasksReferencingPersonViaMatchedPersonsAsync(personsWithNoTrn, applicationUser.UserId);
        var (supportTasksForPersonWithTrn, requestIdsForPersonWithTrn) = await CreateSupportTasksReferencingPersonViaMatchedPersonsAsync(personsWithTrn, applicationUser.UserId);

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();
        var supportTasksAfterDelete = await GetSupportTasksAsync();
        var trnRequestsAfterDelete = await GetTrnRequestsAsync();
        var trnRequestMetaDataAfterDelete = await GetTrnRequestMetadataAsync();

        AssertEx.DoesNotContainAny(personsWithNoTrn, personsAfterDelete);
        AssertEx.ContainsAll(supportTasksForPersonsWithNoTrn, supportTasksAfterDelete);
        AssertEx.ContainsAll(requestIdsForPersonWithNoTrn, trnRequestsAfterDelete);
        AssertEx.ContainsAll(requestIdsForPersonWithNoTrn, trnRequestMetaDataAfterDelete);

        AssertEx.ContainsAll(personsWithTrn, personsAfterDelete);
        AssertEx.ContainsAll(supportTasksForPersonWithTrn, supportTasksAfterDelete);
        AssertEx.ContainsAll(requestIdsForPersonWithTrn, trnRequestsAfterDelete);
        AssertEx.ContainsAll(requestIdsForPersonWithTrn, trnRequestMetaDataAfterDelete);

        Assert.Contains(applicationUser.UserId, await GetApplicationUsersAsync());
    }

    [Fact]
    public async Task Execute_WhenPersonsReferencedByTrnRequestMetadataViaMatchedPersons_AndOtherPersonReferencesTrnRequestMetadataViaSourceTrnRequestId_DoesNotDeleteSupportTaskOrMetadataForPersonsWithNoTrn()
    {
        // Arrange
        var (personsWithNoTrn, _, _, _) = await CreatePersonsWithNoTrnAsync(3);
        var (personsWithTrn, _, _, _) = await CreatePersonsWithTrnAsync(3);

        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTasksForPersonsWithNoTrn, requestIdsForPersonsWithNoTrn) = await CreateSupportTasksReferencingPersonViaMatchedPersonsAsync(personsWithNoTrn, applicationUser.UserId);
        var (supportTasksForPersonWithTrn, requestIdsForPersonsWithTrn) = await CreateSupportTasksReferencingPersonViaMatchedPersonsAsync(personsWithTrn, applicationUser.UserId);

        var (otherPersonsWithNoTrn, _, _, _) = await CreatePersonsWithNoTrnAsync(6, sourceRequestIds: requestIdsForPersonsWithNoTrn.Concat(requestIdsForPersonsWithTrn));
        var (otherPersonsWithTrn, _, _, _) = await CreatePersonsWithTrnAsync(6, sourceRequestIds: requestIdsForPersonsWithNoTrn.Concat(requestIdsForPersonsWithTrn));

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();
        var supportTasksAfterDelete = await GetSupportTasksAsync();
        var trnRequestsAfterDelete = await GetTrnRequestsAsync();
        var trnRequestMetaDataAfterDelete = await GetTrnRequestMetadataAsync();

        AssertEx.DoesNotContainAny(personsWithNoTrn, personsAfterDelete);
        AssertEx.ContainsAll(supportTasksForPersonsWithNoTrn, supportTasksAfterDelete);
        AssertEx.ContainsAll(requestIdsForPersonsWithNoTrn, trnRequestsAfterDelete);
        AssertEx.ContainsAll(requestIdsForPersonsWithNoTrn, trnRequestMetaDataAfterDelete);

        AssertEx.ContainsAll(personsWithTrn, personsAfterDelete);
        AssertEx.ContainsAll(supportTasksForPersonWithTrn, supportTasksAfterDelete);
        AssertEx.ContainsAll(requestIdsForPersonsWithTrn, trnRequestsAfterDelete);
        AssertEx.ContainsAll(requestIdsForPersonsWithTrn, trnRequestMetaDataAfterDelete);

        Assert.Contains(applicationUser.UserId, await GetApplicationUsersAsync());
    }

    [Fact]
    public async Task Execute_WhenPersonsMergedWithOtherPersonsWithTrn_DoesNotDeleteOtherPersonsWithTrn()
    {
        // Arrange
        var (otherPersonsWithTrn, _, _, _) = await CreatePersonsWithTrnAsync(1);
        var (personsWithNoTrn, _, _, _) = await CreatePersonsWithNoTrnAsync(3, mergedWithPersonId: otherPersonsWithTrn.Single());
        var (personsWithTrn, _, _, _) = await CreatePersonsWithTrnAsync(3, mergedWithPersonId: otherPersonsWithTrn.Single());

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();

        AssertEx.DoesNotContainAny(personsWithNoTrn, personsAfterDelete);

        AssertEx.ContainsAll([.. personsWithTrn, .. otherPersonsWithTrn], personsAfterDelete);
    }

    [Fact]
    public async Task Execute_WhenPersonsMergedWithOtherPersonsWithNoTrn_DoesNotDeleteOtherPersonsWithTrn()
    {
        // Arrange
        var (otherPersonWithNoTrn, _, _, _) = await CreatePersonsWithNoTrnAsync(1);
        var (personsWithNoTrn, _, _, _) = await CreatePersonsWithNoTrnAsync(3, mergedWithPersonId: otherPersonWithNoTrn.Single());
        var (personsWithTrn, _, _, _) = await CreatePersonsWithTrnAsync(3, mergedWithPersonId: otherPersonWithNoTrn.Single());

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();

        AssertEx.DoesNotContainAny([.. personsWithNoTrn, .. otherPersonWithNoTrn], personsAfterDelete);

        AssertEx.ContainsAll(personsWithTrn, personsAfterDelete);
    }

    [Fact]
    public async Task Execute_WhenOtherPersonsWithTrnMergedWithPersonsWithTrn_DoesNotDeleteOtherPersonsWithTrn()
    {
        // Arrange
        var (personsWithNoTrn, _, _, _) = await CreatePersonsWithNoTrnAsync(1);
        var (personsWithTrn, _, _, _) = await CreatePersonsWithTrnAsync(1);
        var (otherPersonsWithTrn1, _, _, _) = await CreatePersonsWithTrnAsync(3, mergedWithPersonId: personsWithNoTrn.Single());
        var (otherPersonsWithTrn2, _, _, _) = await CreatePersonsWithTrnAsync(3, mergedWithPersonId: personsWithTrn.Single());

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();

        AssertEx.DoesNotContainAny(personsWithNoTrn, personsAfterDelete);

        AssertEx.ContainsAll([.. personsWithTrn, .. otherPersonsWithTrn1, .. otherPersonsWithTrn2], personsAfterDelete);
    }

    [Fact]
    public async Task Execute_WhenOtherPersonsWithNoTrnMergedWithPersonsWithTrn_DoesNotDeleteOtherPersonsWithTrn()
    {
        // Arrange
        var (personsWithNoTrn, _, _, _) = await CreatePersonsWithNoTrnAsync(1);
        var (personsWithTrn, _, _, _) = await CreatePersonsWithTrnAsync(1);
        var (otherPersonsWithNoTrn1, _, _, _) = await CreatePersonsWithNoTrnAsync(3, mergedWithPersonId: personsWithNoTrn.Single());
        var (otherPersonsWithNoTrn2, _, _, _) = await CreatePersonsWithNoTrnAsync(3, mergedWithPersonId: personsWithTrn.Single());

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();

        AssertEx.DoesNotContainAny([.. personsWithNoTrn, .. otherPersonsWithNoTrn1, .. otherPersonsWithNoTrn2], personsAfterDelete);

        AssertEx.ContainsAll(personsWithTrn, personsAfterDelete);
    }

    [Fact]
    public async Task Execute_WhenPersonsReferencedByIntegrationTransactionRecord_DeletesIntegrationTransactionRecordsForPersonsWithNoTrn()
    {
        // Arrange
        var (personsWithNoTrn, _, _, _) = await CreatePersonsWithNoTrnAsync(3);
        var (personsWithTrn, _, _, _) = await CreatePersonsWithTrnAsync(3);

        var itrsForPersonsWithNoTrn = await CreateIntegrationTransactionRecordsAsync(personsWithNoTrn);
        var itrsForPersonsWithTrn = await CreateIntegrationTransactionRecordsAsync(personsWithTrn);

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();
        var itrsAfterDelete = await GetIntegrationTransactionRecordsAsync();

        AssertEx.DoesNotContainAny(personsWithNoTrn, personsAfterDelete);
        AssertEx.DoesNotContainAny(itrsForPersonsWithNoTrn, itrsAfterDelete);

        AssertEx.ContainsAll(personsWithTrn, personsAfterDelete);
        AssertEx.ContainsAll(itrsForPersonsWithTrn, itrsAfterDelete);
    }

    [Fact]
    public async Task Execute_WhenPersonsReferencedByOneLoginUsers_DoesNotDeleteOneLoginUsers()
    {
        // Arrange
        var (personsWithNoTrn, _, _, _) = await CreatePersonsWithNoTrnAsync(3);
        var (personsWithTrn, _, _, _) = await CreatePersonsWithTrnAsync(3);

        var applicationUser = await TestData.CreateApplicationUserAsync();

        var olusForPersonsWithNoTrn = await CreateOneLoginUsersAsync(personsWithNoTrn, applicationUser.UserId);
        var olusForPersonsWithTrn = await CreateOneLoginUsersAsync(personsWithTrn, applicationUser.UserId);

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();
        var olusAfterDelete = await GetOneLoginUsersAsync();

        AssertEx.DoesNotContainAny(personsWithNoTrn, personsAfterDelete);
        AssertEx.ContainsAll(olusForPersonsWithNoTrn, olusAfterDelete);

        AssertEx.ContainsAll(personsWithTrn, personsAfterDelete);
        AssertEx.ContainsAll(olusForPersonsWithTrn, olusAfterDelete);

        Assert.Contains(applicationUser.UserId, await GetApplicationUsersAsync());
    }

    [Fact]
    public async Task Execute_WhenPersonsHaveAlerts_DeletesAlertsForPersonsWithNoTrn()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();

        var (personsWithNoTrn, alertsForPersonsWithNoTrn, _, _) = await CreatePersonsWithNoTrnAsync(3, p => p
            .WithAlert(a => a
                .WithAlertTypeId(AlertType.DbsAlertTypeId)
                .WithCreatedByUser(user.UserId)
                .WithStartDate(DateOnly.Parse("1 Jan 2000"))));

        var (personsWithTrn, alertsForPersonsWithTrn, _, _) = await CreatePersonsWithTrnAsync(3, p => p
            .WithAlert(a => a
                .WithAlertTypeId(AlertType.DbsAlertTypeId)
                .WithCreatedByUser(user.UserId)
                .WithStartDate(DateOnly.Parse("1 Jan 2000"))));

        var allPersons = new[] { personsWithNoTrn };
        var allAlertTypes = await GetAlertTypesAsync();

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();
        var alertsAfterDelete = await GetAlertsAsync();

        AssertEx.DoesNotContainAny(personsWithNoTrn, personsAfterDelete);
        AssertEx.DoesNotContainAny(alertsForPersonsWithNoTrn, alertsAfterDelete);

        AssertEx.ContainsAll(personsWithTrn, personsAfterDelete);
        AssertEx.ContainsAll(alertsForPersonsWithTrn, alertsAfterDelete);

        Assert.Equivalent(allAlertTypes, await GetAlertTypesAsync());

        Assert.Contains(user.UserId, await GetUsersAsync());
    }

    [Fact]
    public async Task Execute_WhenPersonsHaveNotes_DeletesNotesForPersonsWithNoTrn()
    {
        // Arrange
        var (personsWithNoTrn, _, _, _) = await CreatePersonsWithNoTrnAsync(3);
        var (personsWithTrn, _, _, _) = await CreatePersonsWithTrnAsync(3);

        var user = await TestData.CreateUserAsync();

        var notesForPersonsWithNoTrn = await CreateNotesAsync(personsWithNoTrn, user.UserId);
        var notesForPersonsWithTrn = await CreateNotesAsync(personsWithTrn, user.UserId);

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();
        var notesAfterDelete = await GetNotesAsync();

        AssertEx.DoesNotContainAny(personsWithNoTrn, personsAfterDelete);
        AssertEx.DoesNotContainAny(notesForPersonsWithNoTrn, notesAfterDelete);

        AssertEx.ContainsAll(personsWithTrn, personsAfterDelete);
        AssertEx.ContainsAll(notesForPersonsWithTrn, notesAfterDelete);

        Assert.Contains(user.UserId, await GetUsersAsync());
    }

    [Fact]
    public async Task Execute_WhenPersonsHavePreviousNames_DeletesPreviousNamesForPersonsWithNoTrn()
    {
        var date = DateTime.Parse("1 Jan 2000").ToUniversalTime();
        // Arrange
        var (personsWithNoTrn, _, previousNamesForPersonsWithNoTrn, _) = await CreatePersonsWithNoTrnAsync(3, p => p
            .WithPreviousNames(("Joan", "Of", "Arc", date), ("Winnie", "The", "Pooh", date)));
        var (personsWithTrn, _, previousNamesForPersonsWithTrn, _) = await CreatePersonsWithTrnAsync(3, p => p
            .WithPreviousNames(("Joan", "Of", "Arc", date), ("Winnie", "The", "Pooh", date)));

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();
        var previousNamesAfterDelete = await GetPreviousNamesAsync();

        AssertEx.DoesNotContainAny(personsWithNoTrn, personsAfterDelete);
        AssertEx.DoesNotContainAny(previousNamesForPersonsWithNoTrn, previousNamesAfterDelete);

        AssertEx.ContainsAll(personsWithTrn, personsAfterDelete);
        AssertEx.ContainsAll(previousNamesForPersonsWithTrn, previousNamesAfterDelete);
    }

    [Fact]
    public async Task Execute_WhenPersonsHaveQualifications_DeletesQualificationsForPersonsWithNoTrn()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();

        var (personsWithNoTrn, _, _, qualificationsForPersonsWithNoTrn) = await CreatePersonsWithNoTrnAsync(3, p => p
            .WithMandatoryQualification(mq => mq
                .WithCreatedByUser(EventModels.RaisedByUserInfo.FromUserId(user.UserId))));
        var (personsWithTrn, _, _, qualificationsForPersonsWithTrn) = await CreatePersonsWithTrnAsync(3, p => p
            .WithMandatoryQualification(mq => mq
                .WithCreatedByUser(EventModels.RaisedByUserInfo.FromUserId(user.UserId))));

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();
        var qualificationsAfterDelete = await GetQualificationsAsync();

        AssertEx.DoesNotContainAny(personsWithNoTrn, personsAfterDelete);
        AssertEx.DoesNotContainAny(qualificationsForPersonsWithNoTrn, qualificationsAfterDelete);

        AssertEx.ContainsAll(personsWithTrn, personsAfterDelete);
        AssertEx.ContainsAll(qualificationsForPersonsWithTrn, qualificationsAfterDelete);
    }

    [Fact]
    public async Task Execute_WhenPersonsHaveTpsEmployments_DeletesTpsEmploymentsForPersonsWithNoTrn()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();

        var (personsWithNoTrn, _, _, _) = await CreatePersonsWithNoTrnAsync(3);
        var (personsWithTrn, _, _, _) = await CreatePersonsWithTrnAsync(3);

        var establishment = await TestData.CreateEstablishmentAsync("001");

        var employmentsForPersonsWithNoTrn = await CreateEmploymentsAsync(establishment.EstablishmentId, personsWithNoTrn);
        var employmentsForPersonsWithTrn = await CreateEmploymentsAsync(establishment.EstablishmentId, personsWithTrn);

        var job = CreateDeletePersonAndChildRecordsWithoutATrnJob(batchSize: 1);

        // Act
        await job.ExecuteAsync(false, CancellationToken.None);

        // Assert
        var personsAfterDelete = await GetPersonsAsync();
        var employmentsAfterDelete = await GetTpsEmploymentsAsync();
        var establishmentsAfterDelete = await GetEstablishmentsAsync();

        AssertEx.DoesNotContainAny(personsWithNoTrn, personsAfterDelete);
        AssertEx.DoesNotContainAny(employmentsForPersonsWithNoTrn, employmentsAfterDelete);

        AssertEx.ContainsAll(personsWithTrn, personsAfterDelete);
        AssertEx.ContainsAll(employmentsForPersonsWithTrn, employmentsAfterDelete);

        Assert.Contains(establishment.EstablishmentId, establishmentsAfterDelete);
    }

    private DeletePersonAndChildRecordsWithoutATrnJob CreateDeletePersonAndChildRecordsWithoutATrnJob(int batchSize) =>
        new DeletePersonAndChildRecordsWithoutATrnJob(
            CreateJobOptions(batchSize),
            DbContext,
            FileStorageService,
            Clock,
            TestOutputLogger);

    private IOptions<DeletePersonAndChildRecordsWithoutATrnOptions> CreateJobOptions(int batchSize) =>
        Options.Create(
            new DeletePersonAndChildRecordsWithoutATrnOptions
            {
                BatchSize = batchSize
            });

    private List<CsvRow> ReadAsCsvRows(string fileContents)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContents));
        using var sr = new StreamReader(stream);
        using var csv = new CsvHelper.CsvReader(sr, System.Globalization.CultureInfo.InvariantCulture);

        return csv.GetRecords<CsvRow>().ToList();
    }

    private async Task<(IEnumerable<Guid>, IEnumerable<Guid>, IEnumerable<Guid>, IEnumerable<Guid>)> CreatePersonsWithTrnAsync(int count, Action<CreatePersonBuilder>? configure = null, Guid? mergedWithPersonId = null, IEnumerable<string?>? sourceRequestIds = null)
    {
        sourceRequestIds ??= Enumerable.Repeat<string?>(null, count);

        List<Person> persons = [];

        for (var i = 0; i < count; i++)
        {
            var person = await TestData.CreatePersonAsync();
            persons.Add(person.Person);
        }

        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            foreach (var (p, sourceRequestId) in persons.Zip(sourceRequestIds))
            {
                dbContext.Attach(p);
                if (mergedWithPersonId is Guid id)
                {
                    p.MergedWithPersonId = id;
                }
                if (sourceRequestId is string requestId)
                {
                    p.SourceTrnRequestId = requestId;
                }
            }

            await dbContext.SaveChangesAsync();
        });

        return (
            persons.Select(p => p.PersonId),
            persons.SelectMany(p => p.Alerts?.Select(a => a.AlertId) ?? []),
            persons.SelectMany(p => p.PreviousNames?.Select(p => p.PreviousNameId) ?? []),
            persons.SelectMany(p => p.Qualifications?.Select(q => q.QualificationId) ?? [])
        );
    }

    private async Task<(IEnumerable<Guid>, IEnumerable<Guid>, IEnumerable<Guid>, IEnumerable<Guid>)> CreatePersonsWithNoTrnAsync(int count, Action<CreatePersonBuilder>? configure = null, Guid? mergedWithPersonId = null, IEnumerable<string?>? sourceRequestIds = null)
    {
        sourceRequestIds ??= Enumerable.Repeat<string?>(null, count);

        List<Person> persons = [];

        for (var i = 0; i < count; i++)
        {
            var person = await TestData.CreatePersonAsync();
            persons.Add(person.Person);
        }

        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            foreach (var (p, sourceRequestId) in persons.Zip(sourceRequestIds))
            {
                dbContext.Attach(p);
                p.Trn = null;

                if (mergedWithPersonId is Guid id)
                {
                    p.MergedWithPersonId = id;
                }
                if (sourceRequestId is string requestId)
                {
                    p.SourceTrnRequestId = requestId;
                }
            }

            await dbContext.SaveChangesAsync();
        });

        return (
            persons.Select(p => p.PersonId),
            persons.SelectMany(p => p.Alerts?.Select(a => a.AlertId) ?? []),
            persons.SelectMany(p => p.PreviousNames?.Select(p => p.PreviousNameId) ?? []),
            persons.SelectMany(p => p.Qualifications?.Select(q => q.QualificationId) ?? [])
        );
    }

    private async Task<IEnumerable<string>> CreateSupportTasksReferencingPersonViaPersonIdAsync(IEnumerable<Guid> personIds)
    {
        List<SupportTask> supportTasks = [];

        foreach (var personId in personIds)
        {
            var supportTask1 = await TestData.CreateChangeNameRequestSupportTaskAsync(
                personId,
                b => b.WithFirstName("XXX"));

            var supportTask2 = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
                personId,
                b => b.WithDateOfBirth(DateOnly.Parse("1 Jan 1900")));

            supportTasks.AddRange(supportTask1, supportTask2);
        }

        return supportTasks.Select(st => st.SupportTaskReference);
    }

    private async Task<(IEnumerable<string>, IEnumerable<string?>)> CreateSupportTasksReferencingPersonViaResolvedPersonIdAsync(IEnumerable<Guid> personIds, Guid applicationUserId)
    {
        List<SupportTask> supportTasks = [];

        foreach (var personId in personIds)
        {
            var supportTask1 = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUserId, t => t
                .WithCreatedOn(DateTime.Parse("1 Jan 2000"))
                .WithResolvedPersonId(personId));

            var supportTask2 = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUserId, t => t
                .WithCreatedOn(DateTime.Parse("1 Jan 2000")));

            await DbFixture.WithDbContextAsync(async dbContext =>
            {
                dbContext.Attach(supportTask2);

                supportTask2.TrnRequestMetadata!.SetResolvedPerson(personId, TrnRequestStatus.Completed);
                await dbContext.SaveChangesAsync();
            });

            supportTasks.AddRange(supportTask1, supportTask2);
        }

        return (supportTasks.Select(s => s.SupportTaskReference), supportTasks.Select(s => s.TrnRequestId));
    }

    private async Task<(IEnumerable<string>, IEnumerable<string?>)> CreateSupportTasksReferencingPersonViaResolvedPersonIdAndPersonIdAsync(IEnumerable<(Guid, string)> personIdWithOneLoginUserSubject, Guid applicationUserId)
    {
        List<SupportTask> supportTasks = [];

        foreach (var (personId, oneLoginUserSubject) in personIdWithOneLoginUserSubject)
        {
            var trnRequest1Id = Guid.NewGuid().ToString();
            var trnRequest2Id = Guid.NewGuid().ToString();
            var emailAddress = TestData.GenerateUniqueEmail();
            var firstName = TestData.GenerateFirstName();
            var middleName = "";
            var lastName = TestData.GenerateLastName();
            var dateOfBirth = TestData.GenerateDateOfBirth();
            var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
            var gender = TestData.GenerateGender();
            var createdOn = TestData.Clock.UtcNow;

            var request1 = new TrnRequest
            {
                ClientId = "TEST",
                RequestId = trnRequest1Id,
                TeacherId = Guid.NewGuid()
            };

            var request2 = new TrnRequest
            {
                ClientId = "TEST",
                RequestId = trnRequest2Id,
                TeacherId = Guid.NewGuid()
            };

            var metadata1 = new TrnRequestMetadata
            {
                ApplicationUserId = applicationUserId,
                RequestId = trnRequest1Id,
                CreatedOn = createdOn,
                IdentityVerified = false,
                EmailAddress = emailAddress,
                OneLoginUserSubject = oneLoginUserSubject,
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                PreviousFirstName = null,
                PreviousLastName = null,
                Name = (new List<string?> { firstName, middleName, lastName }).OfType<string>().ToArray(),
                DateOfBirth = dateOfBirth,
                PotentialDuplicate = false,
                NationalInsuranceNumber = nationalInsuranceNumber,
                Gender = gender,
                AddressLine1 = null,
                AddressLine2 = null,
                AddressLine3 = null,
                City = null,
                Postcode = null,
                Country = null,
                TrnToken = null,
                Matches = new TrnRequestMatches() { MatchedPersons = [] }
            };

            var metadata2 = new TrnRequestMetadata
            {
                ApplicationUserId = applicationUserId,
                RequestId = trnRequest2Id,
                CreatedOn = createdOn,
                IdentityVerified = false,
                EmailAddress = emailAddress,
                OneLoginUserSubject = oneLoginUserSubject,
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                PreviousFirstName = null,
                PreviousLastName = null,
                Name = (new List<string?> { firstName, middleName, lastName }).OfType<string>().ToArray(),
                DateOfBirth = dateOfBirth,
                PotentialDuplicate = false,
                NationalInsuranceNumber = nationalInsuranceNumber,
                Gender = gender,
                AddressLine1 = null,
                AddressLine2 = null,
                AddressLine3 = null,
                City = null,
                Postcode = null,
                Country = null,
                TrnToken = null,
                Matches = new TrnRequestMatches() { MatchedPersons = [] }
            };

            metadata1.SetResolvedPerson(personId, TrnRequestStatus.Completed);
            metadata2.SetResolvedPerson(personId, TrnRequestStatus.Completed);

            var data = new ChangeNameRequestData
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                EvidenceFileId = Guid.NewGuid(),
                EvidenceFileName = "filename.jpg",
                EmailAddress = emailAddress,
                ChangeRequestOutcome = null
            };

            var supportTask1 = SupportTask.Create(
                SupportTaskType.ChangeNameRequest,
                data,
                personId: personId,
                oneLoginUserSubject: oneLoginUserSubject,
                trnRequestApplicationUserId: applicationUserId,
                trnRequestId: trnRequest1Id,
                createdBy: SystemUser.SystemUserId,
                createdOn,
                out var _);

            var supportTask2 = SupportTask.Create(
                SupportTaskType.ChangeNameRequest,
                data,
                personId: personId,
                oneLoginUserSubject: oneLoginUserSubject,
                trnRequestApplicationUserId: applicationUserId,
                trnRequestId: trnRequest2Id,
                createdBy: SystemUser.SystemUserId,
                createdOn,
                out var _);

            await DbFixture.WithDbContextAsync(async dbContext =>
            {
                dbContext.TrnRequests.AddRange(request1, request2);
                dbContext.TrnRequestMetadata.AddRange(metadata1, metadata2);
                dbContext.SupportTasks.AddRange(supportTask1, supportTask2);

                await dbContext.SaveChangesAsync();
            });

            supportTasks.AddRange(supportTask1, supportTask2);
        }

        return (supportTasks.Select(s => s.SupportTaskReference), supportTasks.Select(s => s.TrnRequestId));
    }

    private async Task<(IEnumerable<string>, IEnumerable<string?>)> CreateSupportTasksReferencingPersonViaMatchedPersonsAsync(IEnumerable<Guid> personIds, Guid applicationUserId)
    {
        List<SupportTask> supportTasks = [];

        foreach (var personId in personIds)
        {
            var supportTask1 = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUserId, t => t
               .WithMatchedPersons(personId));

            var supportTask2 = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUserId, t => t
                .WithMatchedPersons(personId));

            supportTasks.Add(supportTask1);
            supportTasks.Add(supportTask2);
        }

        return (supportTasks.Select(s => s.SupportTaskReference), supportTasks.Select(s => s.TrnRequestId));
    }

    private async Task<IEnumerable<long>> CreateIntegrationTransactionRecordsAsync(IEnumerable<Guid> personIds)
    {
        List<long> ids = [];

        foreach (var personId in personIds)
        {
            await DbFixture.WithDbContextAsync(async dbContext =>
            {
                var itr1 = new IntegrationTransactionRecord
                {
                    IntegrationTransactionRecordId = Faker.RandomNumber.Next(1, long.MaxValue),
                    CreatedDate = Clock.UtcNow,
                    Duplicate = false,
                    FailureMessage = "TEST",
                    HasActiveAlert = false,
                    PersonId = personId,
                    RowData = "XXX",
                    Status = IntegrationTransactionRecordStatus.Failure
                };

                var itr2 = new IntegrationTransactionRecord
                {
                    IntegrationTransactionRecordId = Faker.RandomNumber.Next(1, long.MaxValue),
                    CreatedDate = Clock.UtcNow,
                    Duplicate = false,
                    FailureMessage = "TEST",
                    HasActiveAlert = false,
                    PersonId = personId,
                    RowData = "XXX",
                    Status = IntegrationTransactionRecordStatus.Failure
                };

                dbContext.IntegrationTransactionRecords.AddRange(itr1, itr2);

                await dbContext.SaveChangesAsync();

                ids.AddRange(itr1.IntegrationTransactionRecordId, itr2.IntegrationTransactionRecordId);
            });
        }

        return ids;
    }

    private async Task<IEnumerable<string>> CreateOneLoginUsersAsync(IEnumerable<Guid> personIds, Guid applicationUserId)
    {
        List<string> subjects = [];

        foreach (var personId in personIds)
        {
            var subject = await DbFixture.WithDbContextAsync(async dbContext =>
            {
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId, verifiedInfo: (["XXX"], DateOnly.Parse("1 Jan 2000")));

                dbContext.Attach(oneLoginUser);
                oneLoginUser.SetVerified(Clock.UtcNow, OneLoginUserVerificationRoute.External, applicationUserId, null, null);

                await dbContext.SaveChangesAsync();

                return oneLoginUser.Subject;
            });

            subjects.Add(subject);
        }

        return subjects;
    }

    private async Task<IEnumerable<Guid>> CreateNotesAsync(IEnumerable<Guid> personIds, Guid userId)
    {
        List<Guid> ids = [];

        foreach (var personId in personIds)
        {
            await DbFixture.WithDbContextAsync(async dbContext =>
            {
                var note1 = new Note
                {
                    NoteId = Guid.NewGuid(),
                    PersonId = personId,
                    Content = "TEST",
                    FileId = null,
                    OriginalFileName = null,
                    CreatedByUserId = userId,
                    CreatedOn = Clock.UtcNow,
                    UpdatedOn = Clock.UtcNow
                };

                var note2 = new Note
                {
                    NoteId = Guid.NewGuid(),
                    PersonId = personId,
                    Content = "TEST",
                    FileId = null,
                    OriginalFileName = null,
                    CreatedByUserId = userId,
                    CreatedOn = Clock.UtcNow,
                    UpdatedOn = Clock.UtcNow
                };

                dbContext.Notes.AddRange(note1, note2);

                await dbContext.SaveChangesAsync();

                ids.AddRange(note1.NoteId, note2.NoteId);
            });
        }

        return ids;
    }

    private async Task<IEnumerable<Guid>> CreateEmploymentsAsync(Guid establishmentId, IEnumerable<Guid> personIds)
    {
        List<Guid> ids = [];

        foreach (var personId in personIds)
        {
            await DbFixture.WithDbContextAsync(async dbContext =>
            {
                var employment1 = new TpsEmployment()
                {
                    CreatedOn = Clock.UtcNow,
                    EmployerEmailAddress = null,
                    EmployerPostcode = null,
                    EmploymentType = EmploymentType.FullTime,
                    EndDate = null,
                    EstablishmentId = establishmentId,
                    Key = "TEST",
                    LastExtractDate = DateOnly.Parse("1 Jan 2000"),
                    LastKnownTpsEmployedDate = DateOnly.Parse("1 Jan 2000"),
                    NationalInsuranceNumber = null,
                    PersonEmailAddress = null,
                    PersonId = personId,
                    PersonPostcode = null,
                    StartDate = DateOnly.Parse("1 Jan 2000"),
                    TpsEmploymentId = Guid.NewGuid(),
                    UpdatedOn = Clock.UtcNow,
                    WithdrawalConfirmed = false
                };

                var employment2 = new TpsEmployment()
                {
                    CreatedOn = Clock.UtcNow,
                    EmployerEmailAddress = null,
                    EmployerPostcode = null,
                    EmploymentType = EmploymentType.FullTime,
                    EndDate = null,
                    EstablishmentId = establishmentId,
                    Key = "TEST",
                    LastExtractDate = DateOnly.Parse("1 Jan 2000"),
                    LastKnownTpsEmployedDate = DateOnly.Parse("1 Jan 2000"),
                    NationalInsuranceNumber = null,
                    PersonEmailAddress = null,
                    PersonId = personId,
                    PersonPostcode = null,
                    StartDate = DateOnly.Parse("1 Jan 2000"),
                    TpsEmploymentId = Guid.NewGuid(),
                    UpdatedOn = Clock.UtcNow,
                    WithdrawalConfirmed = false
                };

                dbContext.TpsEmployments.AddRange(employment1, employment2);

                await dbContext.SaveChangesAsync();

                ids.AddRange(employment1.TpsEmploymentId, employment2.TpsEmploymentId);
            });
        }

        return ids;
    }

    private async Task<IEnumerable<Guid>> GetPersonsAsync() => await DbFixture.WithDbContextAsync(dbContext =>
        dbContext.Persons
            .Select(p => p.PersonId)
            .ToArrayAsync());

    private async Task<IEnumerable<Guid>> GetAlertsAsync() => await DbFixture.WithDbContextAsync(dbContext =>
        dbContext.Alerts
            .Select(a => a.AlertId)
            .ToArrayAsync());

    private async Task<IEnumerable<Guid>> GetNotesAsync() => await DbFixture.WithDbContextAsync(dbContext =>
        dbContext.Notes
            .Select(n => n.NoteId)
            .ToArrayAsync());

    private async Task<IEnumerable<Guid>> GetPreviousNamesAsync() => await DbFixture.WithDbContextAsync(dbContext =>
        dbContext.PreviousNames
            .Select(p => p.PreviousNameId)
            .ToArrayAsync());

    private async Task<IEnumerable<Guid>> GetQualificationsAsync() => await DbFixture.WithDbContextAsync(dbContext =>
        dbContext.Qualifications
            .Select(p => p.QualificationId)
            .ToArrayAsync());

    private async Task<IEnumerable<Guid>> GetTpsEmploymentsAsync() => await DbFixture.WithDbContextAsync(dbContext =>
        dbContext.TpsEmployments
            .Select(e => e.TpsEmploymentId)
            .ToArrayAsync());

    private async Task<IEnumerable<Guid>> GetEstablishmentsAsync() => await DbFixture.WithDbContextAsync(dbContext =>
        dbContext.Establishments
            .Select(e => e.EstablishmentId)
            .ToArrayAsync());

    private async Task<IEnumerable<Guid>> GetAlertTypesAsync() => await DbFixture.WithDbContextAsync(dbContext =>
        dbContext.AlertTypes
            .Select(a => a.AlertTypeId)
            .ToArrayAsync());

    private async Task<IEnumerable<string>> GetSupportTasksAsync() => await DbFixture.WithDbContextAsync(dbContext =>
        dbContext.SupportTasks
            .Select(s => s.SupportTaskReference)
            .ToArrayAsync());

    private async Task<IEnumerable<string>> GetTrnRequestsAsync() => await DbFixture.WithDbContextAsync(dbContext =>
        dbContext.TrnRequestMetadata
            .Select(m => m.RequestId)
            .ToArrayAsync());

    private async Task<IEnumerable<string>> GetTrnRequestMetadataAsync() => await DbFixture.WithDbContextAsync(dbContext =>
        dbContext.TrnRequestMetadata
            .Select(m => m.RequestId)
            .ToArrayAsync());

    private async Task<IEnumerable<long>> GetIntegrationTransactionRecordsAsync() => await DbFixture.WithDbContextAsync(dbContext =>
        dbContext.IntegrationTransactionRecords
            .Select(itr => itr.IntegrationTransactionRecordId)
            .ToArrayAsync());

    private async Task<IEnumerable<string>> GetOneLoginUsersAsync() => await DbFixture.WithDbContextAsync(dbContext =>
        dbContext.OneLoginUsers
            .Select(olu => olu.Subject)
            .ToArrayAsync());

    private async Task<IEnumerable<Guid>> GetUsersAsync() => await DbFixture.WithDbContextAsync(dbContext =>
        dbContext.Users
            .Select(u => u.UserId)
            .ToArrayAsync());

    private async Task<IEnumerable<Guid>> GetApplicationUsersAsync() => await DbFixture.WithDbContextAsync(dbContext =>
        dbContext.ApplicationUsers
            .Select(u => u.UserId)
            .ToArrayAsync());

    public record CsvRow(Guid PersonId);
}

public class DeletePersonAndChildRecordsWithoutATrnJobFixture : IAsyncLifetime
{
    public DeletePersonAndChildRecordsWithoutATrnJobFixture(
        DbFixture dbFixture,
        ReferenceDataCache referenceDataCache,
        FakeTrnGenerator trnGenerator,
        ILoggerFactory loggerFactory)
    {
        DbFixture = dbFixture;
        LoggerFactory = loggerFactory;
        Clock = new();

        TestData = new TestData(
            dbFixture.GetDbContextFactory(),
            referenceDataCache,
            Clock,
            trnGenerator);

        DbContext = dbFixture.GetDbContextFactory().CreateDbContext();
    }

    public TestableClock Clock { get; }
    public DbFixture DbFixture { get; }
    public ILoggerFactory LoggerFactory { get; }
    public TestData TestData { get; }
    public TestFileStorageService FileStorageService { get; } = new();
    public TrsDbContext DbContext { get; }

    public async Task InitializeAsync()
    {
        await DbFixture.DbHelper.ClearDataAsync();
        FileStorageService.Clear();
    }

    public async Task DisposeAsync() => await DbContext.DisposeAsync();
}
