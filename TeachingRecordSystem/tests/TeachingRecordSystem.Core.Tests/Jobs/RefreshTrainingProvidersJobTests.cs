using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Services.PublishApi;

namespace TeachingRecordSystem.Core.Tests.Jobs;

[Collection(nameof(DisableParallelization))]
public class RefreshTrainingProvidersJobTests(CoreFixture fixture) : IAsyncLifetime
{
    private IDbContextFactory<TrsDbContext> DbContextFactory => fixture.DbContextFactory;

    async ValueTask IAsyncLifetime.InitializeAsync()
    {
        await fixture.DbHelper.ClearDataAsync();

        // training_providers is excluded from the reset done by ClearDataAsync so we need to it ourselves
        await DbContextFactory.WithDbContextAsync(dbContext => dbContext.TrainingProviders.ExecuteDeleteAsync());
    }

    ValueTask IAsyncDisposable.DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task RefreshTrainingProvidersJob_WhenCalledForNewUkprn_AddsNewTrainingProvider()
    {
        // Arrange
        var dbContextFactory = fixture.DbContextFactory;
        var publishApiClient = new Mock<IPublishApiClient>();
        var provider1Name = "Test Training Provider 1";
        var provider1Ukprn = "12345678";
        var provider2Name = "Test Training Provider 2";
        var provider2Ukprn = "87654321";
        var providersExpected = new List<ProviderResource>
        {
            new ProviderResource
            {
                Attributes = new ProviderAttributes
                {
                    Ukprn = provider1Ukprn,
                    Name = provider1Name
                }
            },
            new ProviderResource
            {
                Attributes = new ProviderAttributes
                {
                    Ukprn = provider2Ukprn,
                    Name = provider2Name
                }
            }
        };

        publishApiClient
            .Setup(x => x.GetAccreditedProvidersAsync())
            .ReturnsAsync(providersExpected);

        var job = new RefreshTrainingProvidersJob(publishApiClient.Object, dbContextFactory);

        // Act
        await job.ExecuteAsync(CancellationToken.None);

        // Assert
        await DbContextFactory.WithDbContextAsync(async dbContext =>
        {
            var trainingProvidersActual = await dbContext.TrainingProviders.Where(p => p.Ukprn == provider1Ukprn || p.Ukprn == provider2Ukprn).OrderBy(p => p.Ukprn).ToListAsync();
            Assert.Collection(trainingProvidersActual,
                p =>
                {
                    Assert.Equal(provider1Ukprn, p.Ukprn);
                    Assert.Equal(provider1Name, p.Name);
                    Assert.True(p.IsActive);
                },
                p =>
                {
                    Assert.Equal(provider2Ukprn, p.Ukprn);
                    Assert.Equal(provider2Name, p.Name);
                    Assert.True(p.IsActive);
                });
        });
    }

    [Fact]
    public async Task RefreshTrainingProvidersJob_WhenCalledForExistingUkprn_UpdatesTrainingProvider()
    {
        // Arrange
        var publishApiClient = new Mock<IPublishApiClient>();
        var existingProvider = new TrainingProvider
        {
            TrainingProviderId = Guid.NewGuid(),
            Ukprn = "12345679",
            Name = "Test Training Provider 1",
            IsActive = false
        };

        await DbContextFactory.WithDbContextAsync(async dbContext =>
        {
            dbContext.TrainingProviders.Add(existingProvider);
            await dbContext.SaveChangesAsync();
        });

        var newProviderName = "Updated Test Training Provider 1";
        var providersExpected = new List<ProviderResource>
        {
            new ProviderResource
            {
                Attributes = new ProviderAttributes
                {
                    Ukprn = existingProvider.Ukprn,
                    Name = newProviderName
                }
            }
        };

        publishApiClient
            .Setup(x => x.GetAccreditedProvidersAsync())
            .ReturnsAsync(providersExpected);

        var job = new RefreshTrainingProvidersJob(publishApiClient.Object, DbContextFactory);

        // Act
        await job.ExecuteAsync(CancellationToken.None);

        // Assert
        await DbContextFactory.WithDbContextAsync(async dbContext =>
        {
            var trainingProvidersActual = await dbContext.TrainingProviders.Where(p => p.Ukprn == existingProvider.Ukprn).ToListAsync();
            Assert.Single(trainingProvidersActual);
            Assert.Equal(newProviderName, trainingProvidersActual.Single().Name);
            Assert.True(trainingProvidersActual.Single().IsActive);
        });
    }

    [Fact]
    public async Task RefreshTrainingProvidersJob_WhenCalledForWithExistingUkprnMissing_DeactivatesTrainingProvider()
    {
        // Arrange
        var publishApiClient = new Mock<IPublishApiClient>();
        var newProviderName = "New Training Provider";
        var newProviderUkprn = "76543210";

        var existingProvider = new TrainingProvider
        {
            TrainingProviderId = Guid.NewGuid(),
            Ukprn = "12345670",
            Name = "Test Training Provider 1",
            IsActive = true
        };

        await DbContextFactory.WithDbContextAsync(async dbContext =>
        {
            dbContext.TrainingProviders.Add(existingProvider);
            await dbContext.SaveChangesAsync();
        });

        var providersExpected = new List<ProviderResource>()
        {
            new ProviderResource
            {
                Attributes = new ProviderAttributes
                {
                    Ukprn = newProviderUkprn,
                    Name = newProviderName
                }
            }
        };

        publishApiClient
            .Setup(x => x.GetAccreditedProvidersAsync())
            .ReturnsAsync(providersExpected);

        var job = new RefreshTrainingProvidersJob(publishApiClient.Object, DbContextFactory);

        // Act
        await job.ExecuteAsync(CancellationToken.None);

        // Assert
        await DbContextFactory.WithDbContextAsync(async dbContext =>
        {
            var trainingProvidersActual = await dbContext.TrainingProviders.Where(p => p.Ukprn == existingProvider.Ukprn || p.Ukprn == newProviderUkprn).OrderBy(p => p.Ukprn).ToListAsync();
            Assert.Collection(trainingProvidersActual,
                p =>
                {
                    Assert.Equal(existingProvider.Ukprn, p.Ukprn);
                    Assert.Equal(existingProvider.Name, p.Name);
                    Assert.False(p.IsActive);
                },
                p =>
                {
                    Assert.Equal(newProviderUkprn, p.Ukprn);
                    Assert.Equal(newProviderName, p.Name);
                    Assert.True(p.IsActive);
                });
        });
    }
}
