using System.Diagnostics;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.Core.Services.TrnGeneration;
using TeachingRecordSystem.Core.Services.TrnRequests;

namespace TeachingRecordSystem.Core.Tests.Services.TrnRequests;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public class TrnRequestServiceTests : ServiceTestBase
{
    public TrnRequestServiceTests(ServiceFixture fixture) : base(fixture)
    {
        GetAnIdentityApiClientMock = new();
        GetAnIdentityApiClientMock
            .Setup(mock => mock.CreateTrnTokenAsync(It.IsAny<CreateTrnTokenRequest>()))
            .ReturnsAsync((CreateTrnTokenRequest req) => new CreateTrnTokenResponse
            {
                TrnToken = Guid.NewGuid().ToString(),
                Trn = req.Trn,
                Email = req.Email,
                ExpiresUtc = Clock.UtcNow.AddDays(1)
            });

        TrnGenerator = Mock.Of<ITrnGenerator>();
        Mock.Get(TrnGenerator).Setup(mock => mock.GenerateTrnAsync()).Returns(() => TestData.GenerateTrnAsync());

        AytqOptionsAccessor = Options.Create(new AccessYourTeachingQualificationsOptions()
        {
            BaseAddress = "https://aytq.test/"
        });

        TrnRequestOptionsAccessor = Options.Create(new TrnRequestOptions());
    }

    private Mock<IGetAnIdentityApiClient> GetAnIdentityApiClientMock { get; }

    private ITrnGenerator TrnGenerator { get; }

    private IOptions<AccessYourTeachingQualificationsOptions> AytqOptionsAccessor { get; }

    private IOptions<TrnRequestOptions> TrnRequestOptionsAccessor { get; }

    [Fact]
    public async Task CreateTrnRequestAsync_AddsRequestToDbAndPublishesEvent()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var options = GetCreateTrnRequestOptions(applicationUser.UserId);

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var result = await WithServiceAsync(s => s.CreateTrnRequestAsync(options, processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var trnRequest =
                await dbContext.TrnRequestMetadata.SingleOrDefaultAsync(m =>
                    m.ApplicationUserId == options.ApplicationUserId && m.RequestId == options.RequestId);

            Assert.NotNull(trnRequest);

            Assert.Equal(options.ApplicationUserId, trnRequest.ApplicationUserId);
            Assert.Equal(options.RequestId, trnRequest.RequestId);
            Assert.Equal(processContext.Now, trnRequest.CreatedOn);
            Assert.Equal(options.OneLoginUserInfo?.IdentityVerified, trnRequest.IdentityVerified);
            Assert.Equal(options.EmailAddress, trnRequest.EmailAddress);
            Assert.Equal(options.OneLoginUserInfo?.OneLoginUserSubject, trnRequest.OneLoginUserSubject);
            Assert.Equal(options.FirstName, trnRequest.FirstName);
            Assert.Equal(options.MiddleName, trnRequest.MiddleName);
            Assert.Equal(options.LastName, trnRequest.LastName);
            Assert.Equal(options.PreviousFirstName, trnRequest.PreviousFirstName);
            Assert.Equal(options.PreviousMiddleName, trnRequest.PreviousMiddleName);
            Assert.Equal(options.PreviousLastName, trnRequest.PreviousLastName);
            Assert.Equal(options.DateOfBirth, trnRequest.DateOfBirth);
            Assert.Equal(options.NationalInsuranceNumber, trnRequest.NationalInsuranceNumber);
            Assert.Equal(options.Gender, trnRequest.Gender);
            Assert.Equal(options.NpqWorkingInEducationalSetting, trnRequest.NpqWorkingInEducationalSetting);
            Assert.Equal(options.NpqApplicationId, trnRequest.NpqApplicationId);
            Assert.Equal(options.NpqName, trnRequest.NpqName);
            Assert.Equal(options.NpqTrainingProvider, trnRequest.NpqTrainingProvider);
            Assert.Equal(options.NpqEvidenceFileId, trnRequest.NpqEvidenceFileId);
            Assert.Equal(options.NpqEvidenceFileName, trnRequest.NpqEvidenceFileName);
            Assert.Equal(options.WorkEmailAddress, trnRequest.WorkEmailAddress);
        });
    }

    [Fact]
    public async Task CreateTrnRequestAsync_SetsPotentialDuplicateToFalseForDefiniteMatch()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var dateOfBirth = TestData.GenerateDateOfBirth();
        var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();

        var options = GetCreateTrnRequestOptions(applicationUser.UserId) with
        {
            DateOfBirth = dateOfBirth,
            NationalInsuranceNumber = nationalInsuranceNumber
        };

        var definiteMatch =
            await TestData.CreatePersonAsync(p => p.WithDateOfBirth(dateOfBirth).WithNationalInsuranceNumber(nationalInsuranceNumber));

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var result = await WithServiceAsync(s => s.CreateTrnRequestAsync(options, processContext));

        // Assert
        Assert.False(result.TrnRequest.PotentialDuplicate);
    }

    [Fact]
    public async Task CreateTrnRequestAsync_SetsPotentialDuplicateToFalseForPotentialMatches()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var firstName = TestData.GenerateFirstName();
        var middleName = TestData.GenerateMiddleName();
        var lastName = TestData.GenerateLastName();
        var dateOfBirth = TestData.GenerateDateOfBirth();

        var options = GetCreateTrnRequestOptions(applicationUser.UserId) with
        {
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            DateOfBirth = dateOfBirth
        };

        var potentialMatch = await TestData.CreatePersonAsync(p => p
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth));

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var result = await WithServiceAsync(s => s.CreateTrnRequestAsync(options, processContext));

        // Assert
        Assert.True(result.TrnRequest.PotentialDuplicate);
    }

    [Fact]
    public async Task CreateTrnRequestAsync_SetsPotentialDuplicateToFalseForNoMatches()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var options = GetCreateTrnRequestOptions(applicationUser.UserId);

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var result = await WithServiceAsync(s => s.CreateTrnRequestAsync(options, processContext));

        // Assert
        Assert.False(result.TrnRequest.PotentialDuplicate);
    }

    [Fact]
    public async Task CreateTrnRequestAsync_WithTryResolveAndDefiniteMatch_AssignsResolvedPersonAndSetsStatusToCompleted()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var dateOfBirth = TestData.GenerateDateOfBirth();
        var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();

        var options = GetCreateTrnRequestOptions(applicationUser.UserId, tryResolve: true) with
        {
            DateOfBirth = dateOfBirth,
            NationalInsuranceNumber = nationalInsuranceNumber
        };

        var definiteMatch = await TestData.CreatePersonAsync(p => p
            .WithDateOfBirth(dateOfBirth)
            .WithNationalInsuranceNumber(nationalInsuranceNumber));

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.CreateTrnRequestAsync(options, processContext));

        // Assert
        var trnRequest = await WithDbContextAsync(dbContext =>
            dbContext.TrnRequestMetadata.SingleAsync(m => m.ApplicationUserId == applicationUser.UserId && m.RequestId == options.RequestId));

        Assert.Equal(definiteMatch.PersonId, trnRequest.ResolvedPersonId);
        Assert.Equal(TrnRequestStatus.Completed, trnRequest.Status);
        Assert.NotNull(trnRequest.TrnToken);
    }

    [Fact]
    public async Task CreateTrnRequestAsync_WithTryResolveAndDefiniteMatchAndFurtherChecksRequired_AssignsResolvedPersonAndCreatesFurtherChecksTask()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        TrnRequestOptionsAccessor.Value.FlagFurtherChecksRequiredFromUserIds = [applicationUser.UserId];

        var dateOfBirth = TestData.GenerateDateOfBirth();
        var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();

        var options = GetCreateTrnRequestOptions(applicationUser.UserId, tryResolve: true) with
        {
            DateOfBirth = dateOfBirth,
            NationalInsuranceNumber = nationalInsuranceNumber
        };

        var definiteMatch = await TestData.CreatePersonAsync(p => p
            .WithDateOfBirth(dateOfBirth)
            .WithNationalInsuranceNumber(nationalInsuranceNumber)
            .WithAlert());

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.CreateTrnRequestAsync(options, processContext));

        // Assert
        var trnRequest = await WithDbContextAsync(dbContext =>
            dbContext.TrnRequestMetadata.SingleAsync(m => m.ApplicationUserId == applicationUser.UserId && m.RequestId == options.RequestId));

        Assert.Equal(definiteMatch.PersonId, trnRequest.ResolvedPersonId);
        Assert.Equal(TrnRequestStatus.Pending, trnRequest.Status);

        await WithDbContextAsync(async dbContext =>
        {
            var supportTask = await dbContext.SupportTasks
                .SingleOrDefaultAsync(t => t.PersonId == definiteMatch.PersonId && t.SupportTaskType == SupportTaskType.TrnRequestManualChecksNeeded);

            Assert.NotNull(supportTask);
            Assert.Equal(SupportTaskStatus.Open, supportTask.Status);
        });
    }

    [Fact]
    public async Task CompleteTrnRequestWithMatchedPersonAsync_FurtherChecksNotRequired_AssignsResolvedPersonAndTrnTokenAndSetsStatusToCompleted()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (trnRequest, matchedPerson) = await CreatePendingTrnRequestAndMatchingPerson(applicationUser.UserId);

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.CompleteTrnRequestWithMatchedPersonAsync(
            trnRequest,
            (matchedPerson.PersonId, matchedPerson.Trn!),
            processContext));

        // Assert
        Assert.Equal(matchedPerson.PersonId, trnRequest.ResolvedPersonId);
        Assert.Equal(TrnRequestStatus.Completed, trnRequest.Status);
        Assert.NotNull(trnRequest.TrnToken);
    }

    [Fact]
    public async Task CompleteTrnRequestWithMatchedPersonAsync_FurtherChecksRequired_AssignsResolvedPersonAndCreatesFurtherChecksTask()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        TrnRequestOptionsAccessor.Value.FlagFurtherChecksRequiredFromUserIds = [applicationUser.UserId];

        var (trnRequest, matchedPerson) = await CreatePendingTrnRequestAndMatchingPerson(applicationUser.UserId, matchedPersonHasFurtherChecksRequiredFlag: true);

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.CompleteTrnRequestWithMatchedPersonAsync(
            trnRequest,
            (matchedPerson.PersonId, matchedPerson.Trn!),
            processContext));

        // Assert
        Assert.Equal(matchedPerson.PersonId, trnRequest.ResolvedPersonId);
        Assert.Equal(TrnRequestStatus.Pending, trnRequest.Status);

        await WithDbContextAsync(async dbContext =>
        {
            var supportTask = await dbContext.SupportTasks
                .SingleOrDefaultAsync(t => t.PersonId == matchedPerson.PersonId && t.SupportTaskType == SupportTaskType.TrnRequestManualChecksNeeded);

            Assert.NotNull(supportTask);
            Assert.Equal(SupportTaskStatus.Open, supportTask.Status);
        });
    }

    [Fact]
    public async Task CompleteTrnRequestWithNewRecordAsync_AddsNewPersonToDbCreatesEventAndSetsStatusToCompleted()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (trnRequest, _) = await CreatePendingTrnRequestAndMatchingPerson(applicationUser.UserId);

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.CompleteTrnRequestWithNewRecordAsync(trnRequest, processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var person = await dbContext.Persons.SingleOrDefaultAsync(p =>
                p.SourceApplicationUserId == trnRequest.ApplicationUserId && p.SourceTrnRequestId == trnRequest.RequestId);
            Assert.NotNull(person);
            Assert.Equal(trnRequest.FirstName, person.FirstName);
            Assert.Equal(trnRequest.MiddleName, person.MiddleName);
            Assert.Equal(trnRequest.LastName, person.LastName);
            Assert.Equal(trnRequest.DateOfBirth, person.DateOfBirth);
            Assert.Equal(trnRequest.NationalInsuranceNumber, person.NationalInsuranceNumber);
            Assert.Equal(trnRequest.Gender, person.Gender);
            Assert.Equal(trnRequest.EmailAddress, person.EmailAddress);

            Assert.Equal(person.PersonId, trnRequest.ResolvedPersonId);
            Assert.Equal(TrnRequestStatus.Completed, trnRequest.Status);
        });

        Events.AssertEventsPublished(e => Assert.IsType<PersonCreatedEvent>(e));
    }

    [Fact]
    public async Task GetTrnRequestAsync_RequestDoesNotExist_ReturnsNull()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var trnRequestId = Guid.NewGuid().ToString();

        // Act
        var result = await WithServiceAsync(s => s.GetTrnRequestAsync(applicationUser.UserId, trnRequestId));

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetTrnRequestAsync_RequestExists_ReturnsRequest()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (trnRequest, matchedPerson) = await CreatePendingTrnRequestAndMatchingPerson(applicationUser.UserId);

        trnRequest.SetResolvedPerson(matchedPerson.PersonId, TrnRequestStatus.Completed);
        Debug.Assert(trnRequest.TrnToken is null);

        // Act
        var result = await WithServiceAsync(s => s.GetTrnRequestAsync(trnRequest.ApplicationUserId, trnRequest.RequestId));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(applicationUser.UserId, result.TrnRequest.ApplicationUserId);
        Assert.Equal(trnRequest.RequestId, result.TrnRequest.RequestId);
    }

    [Fact]
    public async Task GetTrnRequestAsync_RequestExistsAndIsCompletedWithoutTrnToken_AssignsTrnToken()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (trnRequest, matchedPerson) = await CreatePendingTrnRequestAndMatchingPerson(applicationUser.UserId);

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(trnRequest);

            trnRequest.SetResolvedPerson(matchedPerson.PersonId, TrnRequestStatus.Completed);
            Debug.Assert(trnRequest.TrnToken is null);

            await dbContext.SaveChangesAsync();
        });

        // Act
        var result = await WithServiceAsync(s => s.GetTrnRequestAsync(trnRequest.ApplicationUserId, trnRequest.RequestId));

        // Assert
        Assert.NotNull(result?.TrnRequest.TrnToken);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task RequiresFurtherChecksNeededSupportTaskAsync_ResolvedPersonHasActiveAlert_ReturnsExpectedResult(
        bool applicationUserRequiresFurtherChecks,
        bool expectedResult)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        TrnRequestOptionsAccessor.Value.FlagFurtherChecksRequiredFromUserIds = applicationUserRequiresFurtherChecks ? [applicationUser.UserId] : [];

        var person = await TestData.CreatePersonAsync(p => p.WithAlert());
        Debug.Assert(person.Alerts.Count != 0);

        // Act
        var result = await WithServiceAsync(s => s.RequiresFurtherChecksNeededSupportTaskAsync(person.PersonId, applicationUser.UserId));

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task RequiresFurtherChecksNeededSupportTaskAsync_ResolvedPersonHasQts_ReturnsExpectedResult(
        bool applicationUserRequiresFurtherChecks,
        bool expectedResult)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        TrnRequestOptionsAccessor.Value.FlagFurtherChecksRequiredFromUserIds = applicationUserRequiresFurtherChecks ? [applicationUser.UserId] : [];

        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        Debug.Assert(person.QtsDate is not null);

        // Act
        var result = await WithServiceAsync(s => s.RequiresFurtherChecksNeededSupportTaskAsync(person.PersonId, applicationUser.UserId));

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task RequiresFurtherChecksNeededSupportTaskAsync_ResolvedPersonHasActiveEyts_ReturnsExpectedResult(
        bool applicationUserRequiresFurtherChecks,
        bool expectedResult)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        TrnRequestOptionsAccessor.Value.FlagFurtherChecksRequiredFromUserIds = applicationUserRequiresFurtherChecks ? [applicationUser.UserId] : [];

        var person = await TestData.CreatePersonAsync(p => p.WithEyts());
        Debug.Assert(person.EytsDate is not null);

        // Act
        var result = await WithServiceAsync(s => s.RequiresFurtherChecksNeededSupportTaskAsync(person.PersonId, applicationUser.UserId));

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RequiresFurtherChecksNeededSupportTaskAsync_ResolvedPersonHasNeitherAnActiveAlertNorQtsNorEyts_ReturnsFalse(bool applicationUserRequiresFurtherChecks)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        TrnRequestOptionsAccessor.Value.FlagFurtherChecksRequiredFromUserIds = applicationUserRequiresFurtherChecks ? [applicationUser.UserId] : [];

        var person = await TestData.CreatePersonAsync();
        Debug.Assert(person.Alerts.Count == 0);
        Debug.Assert(person.QtsDate is null);
        Debug.Assert(person.EytsDate is null);

        // Act
        var result = await WithServiceAsync(s => s.RequiresFurtherChecksNeededSupportTaskAsync(person.PersonId, applicationUser.UserId));

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task TryEnsureTrnTokenAsync_StatusIsNotCompleted_DoesNotAssignToken()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var emailAddress = TestData.GenerateUniqueEmail();
        var (trnRequest, person) = await CreatePendingTrnRequestAndMatchingPerson(
            applicationUser.UserId,
            configureOptions: r => r with { EmailAddress = emailAddress });

        Debug.Assert(trnRequest.Status is not TrnRequestStatus.Completed);

        // Act
        var result = await WithServiceAsync(s => s.TryEnsureTrnTokenAsync(trnRequest, person.Trn!));

        // Assert
        VerifyTrnTokenApiNotCalled();
        Assert.Null(trnRequest.TrnToken);
    }

    [Fact]
    public async Task TryEnsureTrnTokenAsync_TrnRequestDoesNotHaveEmail_DoesNotAssignToken()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var (trnRequest, person) = await CreatePendingTrnRequestAndMatchingPerson(
            applicationUser.UserId,
            configureOptions: r => r with { EmailAddress = null! });

        trnRequest.SetResolvedPerson(person.PersonId, TrnRequestStatus.Completed);

        // Act
        var result = await WithServiceAsync(s => s.TryEnsureTrnTokenAsync(trnRequest, person.Trn!));

        // Assert
        VerifyTrnTokenApiNotCalled();
    }

    [Fact]
    public async Task TryEnsureTrnTokenAsync_TokenAlreadyAssigned_DoesNotCreateToken()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var emailAddress = TestData.GenerateUniqueEmail();
        var (trnRequest, person) = await CreatePendingTrnRequestAndMatchingPerson(
            applicationUser.UserId,
            configureOptions: r => r with { EmailAddress = emailAddress });

        trnRequest.SetResolvedPerson(person.PersonId, TrnRequestStatus.Completed);
        var initialToken = trnRequest.TrnToken = Guid.NewGuid().ToString();

        // Act
        var result = await WithServiceAsync(s => s.TryEnsureTrnTokenAsync(trnRequest, person.Trn!));

        // Assert
        VerifyTrnTokenApiNotCalled();
        Assert.Equal(initialToken, trnRequest.TrnToken);
    }

    [Fact]
    public async Task TryEnsureTrnTokenAsync_StatusIsCompletedAndNoTokenAssignedYet_CreatesAndAssignsToken()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var emailAddress = TestData.GenerateUniqueEmail();
        var (trnRequest, person) = await CreatePendingTrnRequestAndMatchingPerson(
            applicationUser.UserId,
            configureOptions: r => r with { EmailAddress = emailAddress });

        Debug.Assert(trnRequest.TrnToken is null);
        trnRequest.SetResolvedPerson(person.PersonId, TrnRequestStatus.Completed);
        Debug.Assert(trnRequest.Status is TrnRequestStatus.Completed);

        // Act
        var result = await WithServiceAsync(s => s.TryEnsureTrnTokenAsync(trnRequest, person.Trn!));

        // Assert
        VerifyTrnTokenApiCalled(trnRequest.EmailAddress!, person.Trn!);
        Assert.NotNull(trnRequest.TrnToken);
    }

    private async Task<(TrnRequestMetadata TrnRequest, Person Person)> CreatePendingTrnRequestAndMatchingPerson(
        Guid applicationUserId,
        bool matchedPersonHasFurtherChecksRequiredFlag = false,
        Func<CreateTrnRequestOptions, CreateTrnRequestOptions>? configureOptions = null)
    {
        var options = GetCreateTrnRequestOptions(applicationUserId, tryResolve: false);

        if (configureOptions is not null)
        {
            options = configureOptions(options);
        }

        var trnRequest = new TrnRequestMetadata
        {
            ApplicationUserId = options.ApplicationUserId,
            RequestId = options.RequestId,
            CreatedOn = Clock.UtcNow,
            IdentityVerified = options.OneLoginUserInfo?.IdentityVerified,
            EmailAddress = options.EmailAddress,
            OneLoginUserSubject = options.OneLoginUserInfo?.OneLoginUserSubject,
            FirstName = options.FirstName,
            MiddleName = options.MiddleName,
            LastName = options.LastName,
            PreviousFirstName = options.PreviousFirstName,
            PreviousMiddleName = options.PreviousMiddleName,
            PreviousLastName = options.PreviousLastName,
            Name = new[] { options.FirstName, options.MiddleName, options.LastName }.Where(n => n is not null).ToArray()!,
            DateOfBirth = options.DateOfBirth,
            PotentialDuplicate = false,
            NationalInsuranceNumber = options.NationalInsuranceNumber,
            Gender = options.Gender,
            NpqWorkingInEducationalSetting = options.NpqWorkingInEducationalSetting,
            NpqApplicationId = options.NpqApplicationId,
            NpqName = options.NpqName,
            NpqTrainingProvider = options.NpqTrainingProvider,
            NpqEvidenceFileId = options.NpqEvidenceFileId,
            NpqEvidenceFileName = options.NpqEvidenceFileName,
            WorkEmailAddress = options.WorkEmailAddress
        };

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.TrnRequestMetadata.Add(trnRequest);
            await dbContext.SaveChangesAsync();
        });

        var createPersonResult = await TestData.CreatePersonAsync(p =>
        {
            p
                .WithFirstName(options.FirstName!)
                .WithMiddleName(options.MiddleName)
                .WithLastName(options.LastName!)
                .WithDateOfBirth(options.DateOfBirth)
                .WithEmailAddress(options.EmailAddress)
                .WithNationalInsuranceNumber(options.NationalInsuranceNumber!);

            if (matchedPersonHasFurtherChecksRequiredFlag)
            {
                p.WithAlert();
            }
        });

        return (trnRequest, createPersonResult.Person);
    }

    private CreateTrnRequestOptions GetCreateTrnRequestOptions(Guid applicationUserId, bool tryResolve = true) =>
        new CreateTrnRequestOptions
        {
            TryResolve = tryResolve,
            ApplicationUserId = applicationUserId,
            RequestId = Guid.NewGuid().ToString(),
            OneLoginUserInfo = new(Guid.NewGuid().ToString(), true),
            EmailAddress = TestData.GenerateUniqueEmail(),
            FirstName = TestData.GenerateFirstName(),
            MiddleName = TestData.GenerateMiddleName(),
            LastName = TestData.GenerateLastName(),
            DateOfBirth = TestData.GenerateDateOfBirth(),
            NationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber(),
            Gender = TestData.GenerateGender(),
            NpqWorkingInEducationalSetting = true,
            NpqApplicationId = Guid.NewGuid().ToString(),
            NpqName = "NPQ",
            NpqTrainingProvider = "Provider",
            NpqEvidenceFileId = Guid.NewGuid(),
            NpqEvidenceFileName = "NPQ Evidence.pdf",
            WorkEmailAddress = TestData.GenerateUniqueEmail()
        };

    private void VerifyTrnTokenApiCalled(string emailAddress, string trn) =>
        GetAnIdentityApiClientMock.Verify(mock => mock.CreateTrnTokenAsync(It.Is<CreateTrnTokenRequest>(r => r.Email == emailAddress && r.Trn == trn)));

    private void VerifyTrnTokenApiNotCalled() =>
        GetAnIdentityApiClientMock.Verify(mock => mock.CreateTrnTokenAsync(It.IsAny<CreateTrnTokenRequest>()), Times.Never);

    private Task WithServiceAsync(Func<TrnRequestService, Task> action, params object[] arguments) =>
        WithServiceAsync<TrnRequestService>(action, GetServiceDependencies(arguments));

    private Task<TResult> WithServiceAsync<TResult>(Func<TrnRequestService, Task<TResult>> action, params object[] arguments) =>
        WithServiceAsync<TrnRequestService, TResult>(action, GetServiceDependencies(arguments));

    private object[] GetServiceDependencies(object[] arguments) =>
        [GetAnIdentityApiClientMock.Object, TrnGenerator, AytqOptionsAccessor, TrnRequestOptionsAccessor, .. arguments];
}
