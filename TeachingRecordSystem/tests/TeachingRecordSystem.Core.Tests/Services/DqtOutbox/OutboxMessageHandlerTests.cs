using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Services.DqtOutbox;
using TeachingRecordSystem.Core.Services.DqtOutbox.Messages;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Tests.Services.DqtOutbox;

public class OutboxMessageHandlerTests : IClassFixture<OutboxMessageHandlerFixture>
{
    public OutboxMessageHandlerTests(OutboxMessageHandlerFixture fixture)
    {
        Fixture = fixture;
        Handler = fixture.ServiceProvider.GetRequiredService<OutboxMessageHandler>();
    }

    public OutboxMessageHandlerFixture Fixture { get; }

    public DbFixture DbFixture => Fixture.DbFixture;

    public IClock Clock => Fixture.Clock;

    public MessageSerializer MessageSerializer => Fixture.MessageSerializer;

    public OutboxMessageHandler Handler { get; }

    public TestData TestData => Fixture.TestData;

    [Fact]
    public async Task HandleOutboxMessage_ForTrnRequestMetadataMessageWithCompletedTrnRequest_AddsOneLoginUserToDb()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var applicationUser = await TestData.CreateApplicationUser();
        var oneLoginUserSubject = TestData.CreateOneLoginUserSubject();

        var person = await TestData.CreatePerson(p => p
            .WithTrn()
            .WithTrnRequestId(TrnRequestHelper.GetCrmTrnRequestId(applicationUser.UserId, requestId)));

        var message = new TrnRequestMetadataMessage()
        {
            ApplicationUserId = applicationUser.UserId,
            RequestId = requestId,
            VerifiedOneLoginUserSubject = oneLoginUserSubject
        };

        var outboxMessage = new dfeta_TrsOutboxMessage()
        {
            dfeta_Payload = MessageSerializer.SerializeMessage(message, out var messageName),
            dfeta_MessageName = messageName
        };

        // Act
        await Handler.HandleOutboxMessage(outboxMessage);

        // Assert
        await DbFixture.WithDbContext(async dbContext =>
        {
            var oneLoginUser = await dbContext.OneLoginUsers.SingleOrDefaultAsync(u => u.Subject == oneLoginUserSubject);
            Assert.NotNull(oneLoginUser);
            Assert.Equal(person.PersonId, oneLoginUser.PersonId);
            Assert.Equal(Clock.UtcNow, oneLoginUser.VerifiedOn);
            Assert.Equal(OneLoginUserVerificationRoute.External, oneLoginUser.VerificationRoute);
            Assert.Equal(applicationUser.UserId, oneLoginUser.VerifiedByApplicationUserId);
        });
    }

    [Fact]
    public async Task HandleOutboxMessage_ForTrnRequestMetadataMessageWithPendingTrnRequest_DoesNotAddOneLoginButDoesAddTrnRequestMetadataToDb()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var applicationUser = await TestData.CreateApplicationUser();
        var oneLoginUserSubject = TestData.CreateOneLoginUserSubject();

        var person = await TestData.CreatePerson(p => p
            .WithoutTrn()
            .WithTrnRequestId(TrnRequestHelper.GetCrmTrnRequestId(applicationUser.UserId, requestId)));

        var message = new TrnRequestMetadataMessage()
        {
            ApplicationUserId = applicationUser.UserId,
            RequestId = requestId,
            VerifiedOneLoginUserSubject = oneLoginUserSubject
        };

        var outboxMessage = new dfeta_TrsOutboxMessage()
        {
            dfeta_Payload = MessageSerializer.SerializeMessage(message, out var messageName),
            dfeta_MessageName = messageName
        };

        // Act
        await Handler.HandleOutboxMessage(outboxMessage);

        // Assert
        await DbFixture.WithDbContext(async dbContext =>
        {
            var oneLoginUser = await dbContext.OneLoginUsers.SingleOrDefaultAsync(u => u.Subject == oneLoginUserSubject);
            Assert.Null(oneLoginUser);

            var trnRequestMetadata = await dbContext.TrnRequestMetadata.SingleOrDefaultAsync(m => m.ApplicationUserId == applicationUser.UserId && m.RequestId == requestId);
            Assert.NotNull(trnRequestMetadata);
            Assert.Equal(oneLoginUserSubject, trnRequestMetadata.VerifiedOneLoginUserSubject);
        });
    }
}

public class OutboxMessageHandlerFixture
{
    public OutboxMessageHandlerFixture(
        DbFixture dbFixture,
        IOrganizationServiceAsync2 organizationService,
        IDbContextFactory<TrsDbContext> dbContextFactory,
        ReferenceDataCache referenceDataCache,
        FakeTrnGenerator trnGenerator)
    {
        Clock = new TestableClock();
        DbFixture = dbFixture;
        MessageSerializer = new MessageSerializer();

        var testDataSyncHelper = new TrsDataSyncHelper(
            dbFixture.GetDataSource(),
            organizationService,
            referenceDataCache,
            Clock);

        TestData = new TestData(
            dbFixture.GetDbContextFactory(),
            organizationService,
            referenceDataCache,
            Clock,
            trnGenerator,
            TestDataSyncConfiguration.Sync(testDataSyncHelper));

        var services = new ServiceCollection()
            .AddSingleton<IClock>(Clock)
            .AddSingleton(MessageSerializer)
            .AddSingleton<TestData>()
            .AddSingleton<OutboxMessageHandler>()
            .AddSingleton(dbContextFactory)
            .AddTransient<TrsDbContext>(sp => sp.GetRequiredService<IDbContextFactory<TrsDbContext>>().CreateDbContext())
            .AddSingleton(TestData)
            .AddTransient<TrnRequestHelper>()
            .AddCrmQueries()
            .AddDefaultServiceClient(ServiceLifetime.Singleton, _ => organizationService);

        ServiceProvider = services.BuildServiceProvider();
    }

    public IClock Clock { get; }

    public DbFixture DbFixture { get; }

    public MessageSerializer MessageSerializer { get; }

    public IServiceProvider ServiceProvider { get; }

    public TestData TestData { get; private set; }
}