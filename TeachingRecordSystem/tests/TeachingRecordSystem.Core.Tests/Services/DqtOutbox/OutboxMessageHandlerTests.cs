using Faker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Services.DqtOutbox;
using TeachingRecordSystem.Core.Services.DqtOutbox.Messages;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.Core.Services.TrsDataSync;
using TeachingRecordSystem.Core.Services.Webhooks;
using Country = TeachingRecordSystem.Core.DataStore.Postgres.Models.Country;
using SystemUser = TeachingRecordSystem.Core.DataStore.Postgres.Models.SystemUser;

namespace TeachingRecordSystem.Core.Tests.Services.DqtOutbox;

public class OutboxMessageHandlerTests : IClassFixture<OutboxMessageHandlerFixture>
{
    public OutboxMessageHandlerTests(OutboxMessageHandlerFixture fixture)
    {
        Fixture = fixture;
        Handler = fixture.ServiceProvider.GetRequiredService<OutboxMessageHandler>();
    }

    public OutboxMessageHandlerFixture Fixture { get; }

    public IClock Clock => Fixture.Clock;

    public MessageSerializer MessageSerializer => Fixture.MessageSerializer;

    public OutboxMessageHandler Handler { get; }

    public TestData TestData => Fixture.TestData;

    public ReferenceDataCache ReferenceDataCache => Fixture.ReferenceDataCache;

    private async Task WithDbContextAsync(Func<TrsDbContext, Task> action)
    {
        await using var dbContext = await Fixture.ServiceProvider.GetRequiredService<IDbContextFactory<TrsDbContext>>().CreateDbContextAsync();
        await action(dbContext);
    }

    [Fact]
    public async Task HandleOutboxMessage_ForTrnRequestMetadataMessage_AddsTrnRequestMetadataToDb()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var oneLoginUserSubject = TestData.CreateOneLoginUserSubject();
        var email = TestData.GenerateUniqueEmail();

        var person = await TestData.CreatePersonAsync(p => p
            .WithoutTrn()
            .WithTrnRequest(applicationUser.UserId, requestId, writeMetadata: false));

        var message = new TrnRequestMetadataMessage()
        {
            ApplicationUserId = applicationUser.UserId,
            RequestId = requestId,
            CreatedOn = Clock.UtcNow,
            IdentityVerified = true,
            OneLoginUserSubject = oneLoginUserSubject,
            EmailAddress = email,
            Name = [person.FirstName, person.LastName],
            DateOfBirth = person.DateOfBirth,
        };

        var outboxMessage = new dfeta_TrsOutboxMessage()
        {
            dfeta_Payload = MessageSerializer.SerializeMessage(message, out var messageName),
            dfeta_MessageName = messageName
        };

        // Act
        await Handler.HandleOutboxMessageAsync(outboxMessage);

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var oneLoginUser = await dbContext.OneLoginUsers.SingleOrDefaultAsync(u => u.Subject == oneLoginUserSubject);
            Assert.Null(oneLoginUser);

            var trnRequestMetadata = await dbContext.TrnRequestMetadata.SingleOrDefaultAsync(m => m.ApplicationUserId == applicationUser.UserId && m.RequestId == requestId);
            Assert.NotNull(trnRequestMetadata);
            Assert.Equal(message.IdentityVerified, trnRequestMetadata.IdentityVerified);
            Assert.Equal(message.OneLoginUserSubject, trnRequestMetadata.OneLoginUserSubject);
            Assert.Equal(message.EmailAddress, trnRequestMetadata.EmailAddress);
            Assert.Equal(message.Name, trnRequestMetadata.Name);
            Assert.Equal(message.DateOfBirth, trnRequestMetadata.DateOfBirth);
        });
    }

    [Fact]
    public async Task HandleOutboxMessage_ForAddInductionExemptionMessage_AddsExemptionReason()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithTrn().WithQts());

        var exemptionReasonId = InductionExemptionReason.PassedInWalesId;

        var message = new AddInductionExemptionMessage()
        {
            PersonId = person.PersonId,
            ExemptionReasonId = exemptionReasonId,
            TrsUserId = Core.DataStore.Postgres.Models.SystemUser.SystemUserId
        };

        var outboxMessage = new dfeta_TrsOutboxMessage()
        {
            dfeta_Payload = MessageSerializer.SerializeMessage(message, out var messageName),
            dfeta_MessageName = messageName
        };

        // Act
        await Handler.HandleOutboxMessageAsync(outboxMessage);

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var updatedPerson = await dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId);
            Assert.Equal(InductionStatus.Exempt, updatedPerson.InductionStatus);
            Assert.Collection(updatedPerson.InductionExemptionReasonIds, id => Assert.Equal(exemptionReasonId, id));
        });
    }

    [Fact]
    public async Task HandleOutboxMessage_ForRemoveInductionExemptionMessage_AddsExemptionReason()
    {
        // Arrange
        var exemptionReasonId = InductionExemptionReason.PassedInWalesId;

        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQts()
            .WithInductionStatus(i => i.WithStatus(InductionStatus.RequiredToComplete)));

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);

            person.Person.SetInductionStatus(
                InductionStatus.Exempt,
                startDate: null,
                completedDate: null,
                exemptionReasonIds: [exemptionReasonId],
                changeReason: null,
                changeReasonDetail: null,
                evidenceFile: null,
                updatedBy: SystemUser.SystemUserId,
                now: Clock.UtcNow,
                out _);

            await dbContext.SaveChangesAsync();
        });

        var message = new RemoveInductionExemptionMessage()
        {
            PersonId = person.PersonId,
            ExemptionReasonId = exemptionReasonId,
            TrsUserId = SystemUser.SystemUserId
        };

        var outboxMessage = new dfeta_TrsOutboxMessage()
        {
            dfeta_Payload = MessageSerializer.SerializeMessage(message, out var messageName),
            dfeta_MessageName = messageName
        };

        // Act
        await Handler.HandleOutboxMessageAsync(outboxMessage);

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var updatedPerson = await dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId);
            Assert.Equal(InductionStatus.RequiredToComplete, updatedPerson.InductionStatus);
            Assert.Empty(updatedPerson.InductionExemptionReasonIds);
        });
    }

    [Fact]
    public async Task HandleOutboxMessage_ForSetInductionRequiredToCompleteMessage_UpdatesInductionStatus()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());

        var message = new SetInductionRequiredToCompleteMessage()
        {
            PersonId = person.PersonId,
            TrsUserId = Core.DataStore.Postgres.Models.SystemUser.SystemUserId
        };

        var outboxMessage = new dfeta_TrsOutboxMessage()
        {
            dfeta_Payload = MessageSerializer.SerializeMessage(message, out var messageName),
            dfeta_MessageName = messageName
        };

        // Act
        await Handler.HandleOutboxMessageAsync(outboxMessage);

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var updatedPerson = await dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId);
            Assert.Equal(InductionStatus.RequiredToComplete, updatedPerson.InductionStatus);
        });
    }


    [Fact]
    public async Task HandleOutboxMessage_ForAddWelshRMessage_CreatesProfessionalStatusForTeacher()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());
        var awardedDate = Clock.UtcNow.AddDays(-100).ToDateOnlyWithDqtBstFix(isLocalTime: true);
        var countries = await ReferenceDataCache.GetTrainingCountriesAsync();
        var countryId = countries.FirstOrDefault()?.CountryId;
        var specialism = TrainingAgeSpecialismType.KeyStage3;
        var trainingStartDate = new DateOnly(2011, 04, 05);
        var trainingEndDate = new DateOnly(2014, 02, 05);
        var subjects = await ReferenceDataCache.GetTrainingSubjectsAsync();
        var subject1 = subjects.RandomOne().TrainingSubjectId;
        var ageRangeFrom = 15;
        var ageRangeTo = 21;

        var message = new AddWelshRMessage()
        {
            PersonId = person.PersonId,
            AwardedDate = awardedDate,
            Subjects = new List<Guid>() { subject1 },
            TrainingProviderId = null,
            TrainingStartDate = trainingStartDate,
            TrainingEndDate = trainingEndDate,
            TrainingCountryId = countryId,
            TrainingAgeSpecialismRangeFrom = ageRangeFrom,
            TrainingAgeSpecialismRangeTo = ageRangeTo,
            TrainingAgeSpecialismType = specialism
        };

        var outboxMessage = new dfeta_TrsOutboxMessage()
        {
            dfeta_Payload = MessageSerializer.SerializeMessage(message, out var messageName),
            dfeta_MessageName = messageName
        };

        // Act
        await Handler.HandleOutboxMessageAsync(outboxMessage);

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var professionalStatus = await dbContext.ProfessionalStatuses.SingleAsync(p => p.PersonId == person.PersonId);
            Assert.NotNull(professionalStatus);
            Assert.Equal(awardedDate, professionalStatus.AwardedDate);
            Assert.Equal(countryId, professionalStatus.TrainingCountryId);
            Assert.Equal(specialism, professionalStatus.TrainingAgeSpecialismType);
            Assert.Equal(trainingStartDate, professionalStatus.TrainingStartDate);
            Assert.Equal(trainingEndDate, professionalStatus.TrainingEndDate);
            Assert.Collection(professionalStatus.TrainingSubjectIds,
                sub1 =>
                {
                    Assert.Equal(subject1, sub1);
                });
            Assert.Null(professionalStatus.TrainingProviderId);
            Assert.Equal(ageRangeFrom, professionalStatus.TrainingAgeSpecialismRangeFrom);
            Assert.Equal(ageRangeTo, professionalStatus.TrainingAgeSpecialismRangeTo);
        });
    }
}

public class OutboxMessageHandlerFixture
{
    public OutboxMessageHandlerFixture(
        DbFixture dbFixture,
        IOrganizationServiceAsync2 organizationService,
        IConfiguration configuration,
        ReferenceDataCache referenceDataCache,
        FakeTrnGenerator trnGenerator,
        ILoggerFactory loggerFactory)
    {
        Clock = new TestableClock();
        MessageSerializer = new MessageSerializer();

        var testDataSyncHelper = new TrsDataSyncHelper(
            dbFixture.GetDataSource(),
            organizationService,
            referenceDataCache,
            Clock,
            new TestableAuditRepository(),
            loggerFactory.CreateLogger<TrsDataSyncHelper>(),
            new Mock<IFileService>().Object,
            configuration);

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
            .AddDatabase(configuration.GetPostgresConnectionString())
            .AddTransient<TrsDbContext>(sp => sp.GetRequiredService<IDbContextFactory<TrsDbContext>>().CreateDbContext())
            .AddSingleton(TestData)
            .AddTrnRequestService()
            .AddCrmQueries()
            .AddDefaultServiceClient(ServiceLifetime.Singleton, _ => organizationService)
            .AddSingleton<WebhookMessageFactory>()
            .AddSingleton<EventMapperRegistry>()
            .AddSingleton<PersonInfoCache>()
            .AddMemoryCache()
            .AddSingleton(testDataSyncHelper);

        ReferenceDataCache = referenceDataCache;
        ServiceProvider = services.BuildServiceProvider();
    }

    public IClock Clock { get; }

    public MessageSerializer MessageSerializer { get; }

    public IServiceProvider ServiceProvider { get; }

    public TestData TestData { get; private set; }

    public ReferenceDataCache ReferenceDataCache { get; }
}
