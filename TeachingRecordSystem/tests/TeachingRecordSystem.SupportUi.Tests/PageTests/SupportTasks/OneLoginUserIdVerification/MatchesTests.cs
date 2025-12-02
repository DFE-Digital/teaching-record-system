using AngleSharp.Dom;
using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification.Resolve;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.OneLoginUserIdVerification;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public class MatchesTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    private OneLoginUser[]? OneLoginUsers { get; set; }
    private SupportTask[]? SupportTasks { get; set; }

    private async Task CreateSupportTasksWithOneLoginUsersAsync()
    {
        OneLoginUsers = new OneLoginUser[]
        {
            await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null),
            await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null)
        };

        var supportTask1 = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(OneLoginUsers[0].Subject);
        Clock.Advance(TimeSpan.FromDays(1));
        var supportTask2 = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(OneLoginUsers[1].Subject);

        SupportTasks = new SupportTask[]
        {
            supportTask1,
            supportTask2
        };
    }

    [Fact]
    public async Task Get_TaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var taskReference = SupportTask.GenerateSupportTaskReference();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/one-login-user-id-verification/{taskReference}/resolve/matches");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_TaskIsClosed_ReturnsNotFound()
    {
        // Arrange
        await CreateSupportTasksWithOneLoginUsersAsync();
        var supportTask = SupportTasks![0];
        supportTask.Status = SupportTaskStatus.Closed;

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequest_ShowsRequestDetails()
    {
        // Arrange
        await CreateSupportTasksWithOneLoginUsersAsync();
        var supportTask = SupportTasks![0];
        var oneLoginRequestData = supportTask.Data as OneLoginUserIdVerificationData;
        var journeyState = new ResolveOneLoginUserIdVerificationState
        {
            CanIdentityBeVerified = true
        };
        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ResolveOneLoginUserIdVerification,
            journeyState,
            new KeyValuePair<string, object>("supportTaskReference", supportTask.SupportTaskReference));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var requestDetails = doc.GetElementByTestId("request");
        Assert.NotNull(requestDetails);
        Assert.Equal(StringHelper.JoinNonEmpty(' ', oneLoginRequestData!.StatedFirstName, oneLoginRequestData.StatedLastName), requestDetails.GetSummaryListValueByKey("Name"));
        Assert.Equal(oneLoginRequestData.StatedDateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), requestDetails.GetSummaryListValueByKey("Date of birth"));
        Assert.Equal(OneLoginUsers![0].EmailAddress, requestDetails.GetSummaryListValueByKey("Email address"));
        Assert.Equal(oneLoginRequestData.StatedNationalInsuranceNumber, requestDetails.GetSummaryListValueByKey("NI number"));
        Assert.Equal(oneLoginRequestData.StatedTrn, requestDetails.GetSummaryListValueByKey("TRN"));
    }

    //[Fact]
    //public async Task Get_ValidRequest_ShowsDetailsOfMatchedRecords()
    //{
    //    // Arrange
    //    await CreateSupportTasksWithOneLoginUsersAsync();
    //    var supportTask = SupportTasks![0];
    //    var oneLoginRequestData = supportTask.Data as OneLoginUserIdVerificationData;
    //    var journeyInstance = await CreateJourneyInstance(supportTask);

    //    var request = new HttpRequestMessage(
    //        HttpMethod.Get,
    //        $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

    //    // Act
    //    var response = await HttpClient.SendAsync(request);

    //    // Assert
    //    var doc = await response.GetDocumentAsync();
    //    var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
    //    Assert.NotNull(firstMatchDetails);
    //    Assert.Equal(matchedPerson.FirstName, firstMatchDetails.GetSummaryListValueByKey("First name"));
    //    Assert.Equal(matchedPerson.MiddleName, firstMatchDetails.GetSummaryListValueByKey("Middle name"));
    //    Assert.Equal(matchedPerson.LastName, firstMatchDetails.GetSummaryListValueByKey("Last name"));
    //    Assert.Equal(matchedPerson.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), firstMatchDetails.GetSummaryListValueByKey("Date of birth"));
    //    Assert.Equal(matchedPerson.EmailAddress, firstMatchDetails.GetSummaryListValueByKey("Email address"));
    //    Assert.Equal(matchedPerson.NationalInsuranceNumber, firstMatchDetails.GetSummaryListValueByKey("NI number"));
    //    Assert.Equal(matchedPerson.Gender?.GetDisplayName(), firstMatchDetails.GetSummaryListValueByKey("Gender"));
    //}

    [Fact]
    public async Task Get_MatchedRecords_NullableFieldsEmptyInRecordButPopulatedInRequest_ShowsHighlightedNotProvided()
    {
        // Arrange
        var OneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(OneLoginUser.Subject, options =>
        {
            options.WithStatedNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber());
        });
        var supportTaskData = supportTask.Data as OneLoginUserIdVerificationData;

        // Person who matches on last name & DOB
        var person1 = await TestData.CreatePersonAsync(p => p.WithLastName(supportTaskData!.StatedLastName).WithDateOfBirth(supportTaskData.StatedDateOfBirth));

        // Person who matches on NINO
        var person2 = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber(supportTaskData!.StatedNationalInsuranceNumber!));

        var journeyState = new ResolveOneLoginUserIdVerificationState
        {
            CanIdentityBeVerified = true
        };
        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ResolveOneLoginUserIdVerification,
            journeyState,
            new KeyValuePair<string, object>("supportTaskReference", supportTask.SupportTaskReference));


        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var firstMatchDetails = doc.GetAllElementsByTestId("match")[0];
        Assert.NotNull(firstMatchDetails);
        Assert.Equal(StringHelper.JoinNonEmpty(' ', person1.FirstName, person1.LastName), firstMatchDetails.GetSummaryListValueByKey("Name"));
        Assert.Equal(UiDefaults.EmptyDisplayContent, firstMatchDetails.GetSummaryListValueByKey("National Insurance number"));
        AssertMatchRowHasExpectedHighlight(firstMatchDetails, "National Insurance number", true);

        var nextMatchDetails = doc.GetAllElementsByTestId("match")[1];
        Assert.NotNull(nextMatchDetails);
        Assert.Equal(StringHelper.JoinNonEmpty(' ', person2.FirstName, person2.LastName), nextMatchDetails.GetSummaryListValueByKey("Name"));
        Assert.Equal(person2.NationalInsuranceNumber, nextMatchDetails.GetSummaryListValueByKey("National Insurance number"));
        Assert.Equal(UiDefaults.EmptyDisplayContent, nextMatchDetails.GetSummaryListValueByKey("Date of birth"));
        AssertMatchRowHasExpectedHighlight(nextMatchDetails, "Name", true);
        AssertMatchRowHasExpectedHighlight(nextMatchDetails, "Date of birth", true);
    }

    //[Fact]
    //public async Task Get_MatchedRecords_NullableFieldsEmptyInRecordAndEmptyInRequest_ShowsNotProvidedNotHighlighted()
    //{
    //    // Arrange
    //    //var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(OneLoginUsers![0].Subject);
    //    var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(OneLoginUsers![0].Subject, options => 
    //    {
    //        options.WithStatedNationalInsuranceNumber(null);
    //    });
    //    var supportTaskData = supportTask.Data as OneLoginUserIdVerificationData;
    //    var matchingPerson = await TestData.CreatePersonAsync(p => p
    //        .WithNationalInsuranceNumber(false)
    //        .WithFirstName(supportTaskData!.StatedFirstName)
    //        .WithLastName(supportTaskData.StatedLastName)
    //        .WithMiddleName(""));

    //    var journeyState = new ResolveOneLoginUserIdVerificationState
    //    {
    //        CanIdentityBeVerified = true
    //    };
    //    var journeyInstance = await CreateJourneyInstance(
    //        JourneyNames.ResolveOneLoginUserIdVerification,
    //        journeyState,
    //        new KeyValuePair<string, object>("supportTaskReference", SupportTasks![0].SupportTaskReference));


    //    var request = new HttpRequestMessage(
    //        HttpMethod.Get,
    //        $"/support-tasks/one-login-user-id-verification/{SupportTasks![0].SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

    //    // Act
    //    var response = await HttpClient.SendAsync(request);

    //    // Assert
    //    var doc = await response.GetDocumentAsync();
    //    var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
    //    Assert.NotNull(firstMatchDetails);
    //    Assert.Equal(matchedPerson.FirstName, firstMatchDetails.GetSummaryListValueByKey("First name"));
    //    Assert.Equal(UiDefaults.EmptyDisplayContent, firstMatchDetails.GetSummaryListValueByKey("Middle name"));
    //    Assert.Equal(matchedPerson.LastName, firstMatchDetails.GetSummaryListValueByKey("Last name"));
    //    Assert.Equal(UiDefaults.EmptyDisplayContent, firstMatchDetails.GetSummaryListValueByKey("NI number"));

    //    AssertMatchRowHasExpectedHighlight(firstMatchDetails, "Middle name", false);
    //    AssertMatchRowHasExpectedHighlight(firstMatchDetails, "NI number", false);
    //}

    //[Fact]
    //public async Task Get_WithDefiniteMatch_ShowsMergeRecordButton()
    //{
    //    // Arrange
    //    var applicationUser = await TestData.CreateApplicationUserAsync();
    //    var firstName = TestData.GenerateFirstName();
    //    var middleName = TestData.GenerateMiddleName();
    //    var lastName = TestData.GenerateLastName();
    //    var dateOfBirth = TestData.GenerateDateOfBirth();
    //    var emailAddress = TestData.GenerateUniqueEmail();
    //    var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();

    //    var matchedPerson = await TestData.CreatePersonAsync(p =>
    //    {
    //        p.WithFirstName(firstName);
    //        p.WithMiddleName(middleName);
    //        p.WithLastName(lastName);
    //        p.WithDateOfBirth(dateOfBirth);
    //        p.WithEmailAddress(emailAddress);
    //        p.WithNationalInsuranceNumber(nationalInsuranceNumber);
    //    });

    //    var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(
    //        applicationUser.UserId,
    //        t => t
    //            .WithMatchedPersons(matchedPerson.PersonId)
    //            .WithStatus(SupportTaskStatus.Open)
    //            .WithFirstName(firstName)
    //            .WithMiddleName(middleName)
    //            .WithLastName(lastName)
    //            .WithDateOfBirth(dateOfBirth)
    //            .WithGender(matchedPerson.Gender)
    //            .WithEmailAddress(emailAddress)
    //            .WithNationalInsuranceNumber(nationalInsuranceNumber)
    //        );

    //    var journeyInstance = await CreateJourneyInstance(supportTask);

    //    var request = new HttpRequestMessage(
    //        HttpMethod.Get,
    //        $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

    //    // Act
    //    var response = await HttpClient.SendAsync(request);

    //    // Assert
    //    var doc = await response.GetDocumentAsync();
    //    Assert.NotEmpty(doc.GetAllElementsByTestId("merge-record-button"));
    //}

    //[Fact]
    //public async Task Get_WithNoMatches_RedirectsToNoMatchesPage()
    //{
    //    // Arrange
    //    var applicationUser = await TestData.CreateApplicationUserAsync();
    //    var firstName = TestData.GenerateFirstName();
    //    var middleName = TestData.GenerateMiddleName();
    //    var lastName = TestData.GenerateLastName();
    //    var dateOfBirth = TestData.GenerateDateOfBirth();
    //    var emailAddress = TestData.GenerateUniqueEmail();
    //    var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();

    //    var matchedPerson = await TestData.CreatePersonAsync(p =>
    //    {
    //        p.WithFirstName(firstName);
    //        p.WithMiddleName(middleName);
    //        p.WithLastName(lastName);
    //        p.WithDateOfBirth(dateOfBirth);
    //        p.WithEmailAddress(emailAddress);
    //    });

    //    var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(
    //        applicationUser.UserId,
    //        t => t
    //            .WithMatchedPersons(matchedPerson.PersonId)
    //            .WithStatus(SupportTaskStatus.Open)
    //            .WithFirstName(TestData.GenerateChangedFirstName(firstName))
    //            .WithMiddleName(TestData.GenerateChangedMiddleName(middleName))
    //            .WithLastName(TestData.GenerateChangedLastName(lastName))
    //            .WithDateOfBirth(TestData.GenerateChangedDateOfBirth(dateOfBirth))
    //            .WithEmailAddress(TestData.GenerateUniqueEmail())
    //        );

    //    var journeyInstance = await CreateJourneyInstance(supportTask);

    //    var request = new HttpRequestMessage(
    //        HttpMethod.Get,
    //        $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

    //    // Act
    //    var response = await HttpClient.SendAsync(request);

    //    // Assert
    //    var doc = await response.GetDocumentAsync();

    //    // CML TODO: assert redirect
    //}

    private void AssertMatchRowHasExpectedHighlight(IElement matchDetails, string summaryListKey, bool expectHighlight)
    {
        var valueElement = matchDetails.GetSummaryListValueElementByKey(summaryListKey);
        Assert.NotNull(valueElement);
        var highlightElement = valueElement.GetElementsByClassName("hods-highlight").SingleOrDefault();

        if (expectHighlight)
        {
            Assert.False(highlightElement == null, $"{summaryListKey} should be highlighted");
        }
        else
        {
            Assert.True(highlightElement == null, $"{summaryListKey} should not be highlighted");
        }
    }
}
