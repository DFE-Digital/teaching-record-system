using System.Text.Json;
using TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;
using TeachingRecordSystem.Core.Models.SupportTaskData;
using PostgresModels = TeachingRecordSystem.Core.DataStore.Postgres.Models;
namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests.RequestTrn;

[Collection(nameof(DisableParallelization))]
public class CheckAnswersTests(HostFixture hostFixture) : TestBase(hostFixture), IAsyncLifetime
{
    public Task InitializeAsync() => HostFixture.Services.GetRequiredService<DbHelper>().ClearDataAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Get_HasPendingTrnRequestSetTrue_RedirectsToSubmitted()
    {
        // Arrange
        var state = CreateNewState();
        state.HasPendingTrnRequest = true;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/submitted?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_HasWorkEmailMissingFromState_RedirectsToWorkEmail()
    {
        // Arrange
        var state = CreateNewState();
        state.WorkEmail = null;

        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/work-email?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_HasPersonalEmailMissingFromState_RedirectsToPersonalEmail()
    {
        // Arrange
        var state = CreateNewState();
        state.PersonalEmail = null;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/personal-email?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_HasNameMissingFromState_RedirectsToName()
    {
        // Arrange
        var state = CreateNewState();
        state.LastName = null;

        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/name?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_HasHasPreviousNameMissingFromState_RedirectsToPreviousName()
    {
        // Arrange
        var state = CreateNewState();
        state.HasPreviousName = null;

        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/previous-name?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_HasDateOfBirthMissingFromState_RedirectsToDateOfBirth()
    {
        // Arrange
        var state = CreateNewState();
        state.DateOfBirth = null;

        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/date-of-birth?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_HasEvidenceFileIdMissingFromState_RedirectsToIdentity()
    {
        // Arrange
        var state = CreateNewState();
        state.EvidenceFileId = null;
        state.EvidenceFileName = null;
        state.EvidenceFileSizeDescription = null;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/identity?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_HasHasNationalInsuranceNumberMissingFromState_RedirectsToNationalInsuranceNumber()
    {
        // Arrange
        var state = CreateNewState();
        state.HasNationalInsuranceNumber = null;
        state.NationalInsuranceNumber = null;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_HasNationalInsuranceNumberTrueAndNationalInsuranceNumberMissingFromState_RedirectsToNationalInsuranceNumber()
    {
        // Arrange
        var state = CreateNewState();
        state.HasNationalInsuranceNumber = true;
        state.NationalInsuranceNumber = null;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_HasNationalInsuranceNumberFalseAndAddressLine1MissingFromState_RedirectsToAddress()
    {
        // Arrange
        var state = CreateNewState();
        state.HasNationalInsuranceNumber = false;
        state.NationalInsuranceNumber = null;
        state.AddressLine1 = null;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/address?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_ValidRequest_PopulatesModelFromJourneyState(bool hasNationalInsuranceNumber)
    {
        // Arrange
        var state = CreateNewState();
        state.HasPreviousName = true;
        state.PreviousFirstName = TestData.GenerateFirstName();
        state.PreviousMiddleName = TestData.GenerateMiddleName();
        state.PreviousLastName = TestData.GenerateLastName();

        state.HasNationalInsuranceNumber = hasNationalInsuranceNumber;
        state.NationalInsuranceNumber = hasNationalInsuranceNumber ? Faker.Identification.UkNationalInsuranceNumber() : null;

        if (!hasNationalInsuranceNumber)
        {
            state.AddressLine1 = Faker.Address.StreetAddress();
            state.AddressLine2 = Faker.Address.SecondaryAddress();
            state.TownOrCity = Faker.Address.City();
            state.Country = TestData.GenerateCountry();
            state.PostalCode = Faker.Address.ZipCode();
        }
        state.HasPendingTrnRequest = false;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(state.WorkEmail, doc.GetSummaryListValueForKey("Work email"));
        Assert.Equal(state.PersonalEmail, doc.GetSummaryListValueForKey("Personal email"));
        Assert.Equal(StringHelper.JoinNonEmpty(' ', new string?[] { state.FirstName, state.MiddleName, state.LastName }), doc.GetSummaryListValueForKey("Name"));
        Assert.Equal(StringHelper.JoinNonEmpty(' ', new string?[] { state.PreviousFirstName, state.PreviousMiddleName, state.PreviousLastName }), doc.GetSummaryListValueForKey("Previous name"));
        Assert.Equal(state.DateOfBirth?.ToString("d MMMM yyyy"), doc.GetSummaryListValueForKey("Date of birth"));
        Assert.Contains(state.EvidenceFileName!, doc.GetSummaryListValueForKey("Proof of identity"));
        if (hasNationalInsuranceNumber)
        {
            Assert.Equal(state.NationalInsuranceNumber, doc.GetSummaryListValueForKey("National Insurance number"));
        }
        else
        {
            Assert.Equal("None", doc.GetSummaryListValueForKey("National Insurance number"));
            var address = doc.GetSummaryListValueForKey("Address");
            Assert.Contains(state.AddressLine1!, address);
            Assert.Contains(state.AddressLine2!, address);
            Assert.Contains(state.TownOrCity!, address);
            Assert.Contains(state.PostalCode!, address);
            Assert.Contains(state.Country!, address);
        }
    }

    [Fact]
    public async Task Get_GetValidRequestWithNpqNameAndProvider_ShowsNpqNameAndProviderAndHidesNpqApplicationId()
    {
        // Arrange
        var npqApplicationId = default(string?);
        var npqName = "Some NPQ Name";
        var npqTrainingProvider = "NPQ TRAINING PROVIDER";
        var state = CreateNewState();
        state.NpqApplicationId = npqApplicationId;
        state.NpqName = npqName;
        state.NpqTrainingProvider = npqTrainingProvider;

        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Null(doc.GetSummaryListValueForKey("NPQ application ID"));
        Assert.Equal(npqName, doc.GetSummaryListValueForKey("NPQ"));
        Assert.Equal(npqTrainingProvider, doc.GetSummaryListValueForKey("NPQ provider"));
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_GetValidRequestNpqApplicationIdProvided_HidesNpqNameAndProvider()
    {
        // Arrange
        var npqApplicationId = "ApplicationId-12345";
        var state = CreateNewState();
        state.NpqApplicationId = npqApplicationId;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(state.NpqApplicationId, doc.GetSummaryListValueForKey("NPQ application ID"));
        Assert.Null(doc.GetSummaryListValueForKey("NPQ"));
        Assert.Null(doc.GetSummaryListValueForKey("NPQ Provider"));
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_GetValidRequestWorkEmailIsNull_HidesWorkEmail()
    {
        // Arrange
        var state = CreateNewState();
        state.WorkingInSchoolOrEducationalSetting = false;
        state.WorkEmail = null;

        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(state.NpqApplicationId, doc.GetSummaryListValueForKey("NPQ application ID"));
        Assert.Null(doc.GetSummaryListValueForKey("Work email"));
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_HasPendingTrnRequestSetTrue_RedirectsToSubmitted()
    {
        // Arrange
        var state = CreateNewState();
        state.HasPendingTrnRequest = true;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/submitted?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_HasWorkEmailMissingFromState_RedirectsToWorkEmail()
    {
        // Arrange
        var state = CreateNewState();
        state.WorkingInSchoolOrEducationalSetting = true;
        state.WorkEmail = null;

        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/work-email?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_HasPersonalEmailMissingFromState_RedirectsToPersonalEmail()
    {
        // Arrange
        var state = CreateNewState();
        state.PersonalEmail = null;

        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/personal-email?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_HasNameMissingFromState_RedirectsToName()
    {
        // Arrange
        var state = CreateNewState();
        state.LastName = null;

        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/name?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_HasPreviousNameMissingFromState_RedirectsToPreviousName()
    {
        // Arrange
        var state = CreateNewState();
        state.HasPreviousName = true;
        state.PreviousFirstName = TestData.GenerateFirstName();
        state.PreviousMiddleName = null;
        state.PreviousLastName = null;

        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/previous-name?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_HasDateOfBirthMissingFromState_RedirectsToDateOfBirth()
    {
        // Arrange
        var state = CreateNewState();
        state.DateOfBirth = null;

        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/date-of-birth?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_HasEvidenceFileIdMissingFromState_RedirectsToIdentity()
    {
        // Arrange
        var state = CreateNewState();
        state.EvidenceFileId = null;
        state.EvidenceFileName = null;
        state.EvidenceFileSizeDescription = null;

        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/identity?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_HasHasNationalInsuranceNumberMissingFromState_RedirectsToNationalInsuranceNumber()
    {
        // Arrange
        var state = CreateNewState();
        state.HasNationalInsuranceNumber = null;
        state.NationalInsuranceNumber = null;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_HasNationalInsuranceNumberTrueAndNationalInsuranceNumberMissingFromState_RedirectsToNationalInsuranceNumber()
    {
        // Arrange
        var state = CreateNewState();
        state.HasNationalInsuranceNumber = true;
        state.NationalInsuranceNumber = null;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_HasNationalInsuranceNumberFalseAndAddressLine1MissingFromState_RedirectsToAddress()
    {
        // Arrange
        var state = CreateNewState();
        state.HasNationalInsuranceNumber = false;
        state.NationalInsuranceNumber = null;
        state.AddressLine1 = null;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/address?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Post_ValidRequest_UpdatesStateAndAndRedirectsToSubmitted(bool hasNationalInsuranceNumber)
    {
        // Arrange
        var state = CreateNewState();
        state.HasNationalInsuranceNumber = hasNationalInsuranceNumber;
        state.NationalInsuranceNumber = hasNationalInsuranceNumber ? Faker.Identification.UkNationalInsuranceNumber() : null;
        if (!hasNationalInsuranceNumber)
        {
            state.AddressLine1 = Faker.Address.StreetAddress();
            state.AddressLine2 = Faker.Address.SecondaryAddress();
            state.TownOrCity = Faker.Address.City();
            state.Country = TestData.GenerateCountry();
            state.PostalCode = Faker.Address.ZipCode();
        }
        state.HasPendingTrnRequest = false;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/submitted?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        var reloadedJourneyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(reloadedJourneyInstance.State.HasPendingTrnRequest);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Post_NoMatches_SavesSupportTask(bool hasNationalInsuranceNumber)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");

        var state = CreateNewState();
        state.HasNationalInsuranceNumber = hasNationalInsuranceNumber;
        state.NationalInsuranceNumber = hasNationalInsuranceNumber ? Faker.Identification.UkNationalInsuranceNumber() : null;
        if (!hasNationalInsuranceNumber)
        {
            state.AddressLine1 = Faker.Address.StreetAddress();
            state.AddressLine2 = Faker.Address.SecondaryAddress();
            state.TownOrCity = Faker.Address.City();
            state.Country = TestData.GenerateCountry();
            state.PostalCode = Faker.Address.ZipCode();
        }
        state.HasPendingTrnRequest = false;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertSupportTaskCreatedAsync();
        await AssertMetadataExpectedAsync(state, false);
    }

    [Fact]
    public async Task Post_Matches_SavesSupportTask()
    {
        // Arrange
        var state = CreateNewState();
        state.NationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();
        state.HasNationalInsuranceNumber = true;
        state.HasPendingTrnRequest = false;
        var journeyInstance = await CreateJourneyInstance(state);

        var person1 = await TestData.CreatePersonAsync(p => p
            .WithFirstName(state.FirstName!)
            .WithMiddleName(state.MiddleName)
            .WithLastName(state!.LastName!));
        var person2 = await TestData.CreatePersonAsync(p => p
            .WithFirstName(state.FirstName!)
            .WithMiddleName(state.MiddleName)
            .WithLastName(state!.LastName!));
        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertSupportTaskCreatedAsync();
        await AssertMetadataExpectedAsync(state, true, new List<Guid>() { person1.PersonId, person2.PersonId });
    }

    [Fact]
    public async Task Post_DefiniteMatch_SavesSupportTask()
    {
        // Arrange
        var state = CreateNewState();
        state.NationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();
        state.HasNationalInsuranceNumber = true;
        state.HasPendingTrnRequest = false;
        var journeyInstance = await CreateJourneyInstance(state);

        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName(state.FirstName!)
            .WithMiddleName(state.MiddleName)
            .WithLastName(state!.LastName!)
            .WithDateOfBirth(state.DateOfBirth!.Value)
            .WithNationalInsuranceNumber(state.NationalInsuranceNumber!));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertSupportTaskCreatedAsync();
        await AssertMetadataExpectedAsync(state, true, new List<Guid>() { person.PersonId });
    }

    [Fact]
    public async Task Post_CreatesEvent()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");

        var state = CreateNewState();

        state.NationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();
        state.HasNationalInsuranceNumber = true;
        state.HasPendingTrnRequest = false;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEventCreatedAsync();
    }

    private Task AssertMetadataExpectedAsync(RequestTrnJourneyState request, bool expectedPotentialDuplicate, List<Guid>? potentialDuplicates = null) =>
        base.WithDbContext(async dbContext =>
    {
        if (expectedPotentialDuplicate && potentialDuplicates is null)
        {
            throw new ArgumentException("Define the list of expected potential duplicates");
        }

        var applicationUserId = PostgresModels.ApplicationUser.NpqApplicationUserGuid;

        var metadata = await dbContext.TrnRequestMetadata
            .SingleOrDefaultAsync(m => m.ApplicationUserId == applicationUserId);
        Assert.NotNull(metadata);

        var expectedEmailAddress = request.PersonalEmail;

        var expectedName = new[] { request.FirstName, request.MiddleName, request.LastName }
            .Where(part => !string.IsNullOrEmpty(part));

        Assert.Equal(TrnRequestStatus.Pending, metadata.Status);
        Assert.False(metadata.IdentityVerified);
        Assert.Equal(request.PersonalEmail, metadata.EmailAddress);
        Assert.True(expectedName.SequenceEqual(metadata.Name));
        Assert.Equal(request.FirstName, metadata.FirstName);
        Assert.Equal(request.MiddleName, metadata.MiddleName);
        Assert.Equal(request.LastName, metadata.LastName);
        Assert.Equal(request.DateOfBirth, metadata.DateOfBirth);
        Assert.Equal(expectedPotentialDuplicate, metadata.PotentialDuplicate);
        Assert.Equal(request.NationalInsuranceNumber, metadata.NationalInsuranceNumber);
        Assert.Equal(request.AddressLine1, metadata.AddressLine1);
        Assert.Equal(request.AddressLine2, metadata.AddressLine2);
        Assert.Equal(request.TownOrCity, metadata.City);
        Assert.Equal(request.PostalCode, metadata.Postcode);
        Assert.Equal(request.Country, metadata.Country);
        Assert.Null(metadata.OneLoginUserSubject);
        Assert.Null(metadata.Gender);
        Assert.Null(metadata.AddressLine3);
        Assert.NotNull(metadata.RequestId);
        Assert.Equal(PostgresModels.ApplicationUser.NpqApplicationUserGuid, metadata.ApplicationUserId);
        Assert.Equal(request.NpqApplicationId, metadata.NpqApplicationId);
        Assert.Equal(request.NpqName, metadata.NpqName);
        Assert.Equal(request.NpqTrainingProvider, metadata.NpqTrainingProvider);
        Assert.Equal(request.PreviousFirstName, metadata.PreviousFirstName);
        Assert.Equal(request.PreviousMiddleName, metadata.PreviousMiddleName);
        Assert.Equal(request.PreviousLastName, metadata.PreviousLastName);
        Assert.NotNull(metadata.NpqEvidenceFileId);
        Assert.NotNull(metadata.NpqEvidenceFileName);
        Assert.Equal(request.WorkEmail, metadata.WorkEmailAddress);

        if (expectedPotentialDuplicate)
        {
            Assert.Equivalent(potentialDuplicates!.Select(x => new PostgresModels.TrnRequestMatchedPerson() { PersonId = x }), metadata.Matches!.MatchedPersons);
        }
        else
        {
            Assert.Equivalent(new List<PostgresModels.TrnRequestMatchedPerson>().AsReadOnly(), metadata.Matches!.MatchedPersons);
        }
    });

    private Task AssertSupportTaskCreatedAsync() =>
        base.WithDbContext(async dbContext =>
    {
        var applicationUserId = PostgresModels.ApplicationUser.NpqApplicationUserGuid;
        var supportTask = await dbContext.SupportTasks
            .SingleOrDefaultAsync(t => t.SupportTaskTypeId == PostgresModels.SupportTaskType.NpqTrnRequest.SupportTaskTypeId &&
                t.TrnRequestMetadata!.ApplicationUserId == applicationUserId);

        Assert.NotNull(supportTask);
        Assert.Equal(SupportTaskStatus.Open, supportTask.Status);
    });

    private Task AssertEventCreatedAsync() =>
        base.WithDbContext(async dbContext =>
    {
        var applicationUserId = PostgresModels.ApplicationUser.NpqApplicationUserGuid;
        var supportTask = await dbContext.SupportTasks
            .SingleOrDefaultAsync(t => t.SupportTaskType == PostgresModels.SupportTaskType.NpqTrnRequest &&
                t.TrnRequestMetadata!.ApplicationUserId == applicationUserId);
        var events = await dbContext.Events
            .Where(e => e.EventName == nameof(SupportTaskCreatedEvent))
            .ToListAsync();
        Assert.Single(events);
        var @event = events.Single();

        var supportTaskCreatedEvent = JsonSerializer.Deserialize<SupportTaskCreatedEvent>(@event.Payload);
        Assert.Equal(supportTask!.SupportTaskReference, supportTaskCreatedEvent!.SupportTask.SupportTaskReference);

        var data = supportTaskCreatedEvent.SupportTask.Data as NpqTrnRequestData;
        Assert.Null(data!.ResolvedAttributes);
        Assert.Null(data.SelectedPersonAttributes);
        Assert.Null(data.SupportRequestOutcome);
    });
}
