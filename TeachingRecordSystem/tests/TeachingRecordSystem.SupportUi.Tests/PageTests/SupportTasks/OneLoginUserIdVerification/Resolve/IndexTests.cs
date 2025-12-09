using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification.Resolve;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.OneLoginUserIdVerification.Resolve;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_SupportTaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/one-login-user-id-verification/TRS-000/resolve");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_SupportTaskIsNotOpen_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        await WithDbContextAsync(dbContext => dbContext.SupportTasks
            .Where(t => t.SupportTaskReference == supportTask.SupportTaskReference)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.Status, _ => SupportTaskStatus.Closed)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_ValidRequest_RendersExpectedContent(bool evidenceIsPdf)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null);
        var statedNationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject,
            b => b
                .WithStatedNationalInsuranceNumber(statedNationalInsuranceNumber)
                .WithEvidenceFileName(evidenceIsPdf ? "evidence.pdf" : "evidence.jpg"));
        var requestData = supportTask.GetData<OneLoginUserIdVerificationData>();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve?ffiid=", response.Headers.Location?.OriginalString);
        var redirectRequest = new HttpRequestMessage(HttpMethod.Get, response.Headers.Location!.OriginalString);
        var redirectResponse = await HttpClient.SendAsync(redirectRequest);

        Assert.Equal(StatusCodes.Status200OK, (int)redirectResponse.StatusCode);

        var doc = await AssertEx.HtmlResponseAsync(redirectResponse);
        Assert.Equal($"{requestData.StatedFirstName} {requestData.StatedLastName}", doc.GetSummaryListValueByKey("Name"));
        Assert.Equal(requestData.StatedDateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), doc.GetSummaryListValueByKey("Date of birth"));
        Assert.Equal(oneLoginUser.EmailAddress, doc.GetSummaryListValueByKey("Email address"));
        Assert.Equal(requestData.StatedTrn, doc.GetSummaryListValueByKey("TRN"));
        Assert.Equal(requestData.StatedNationalInsuranceNumber, doc.GetSummaryListValueByKey("National Insurance number"));
        if (evidenceIsPdf)
        {
            Assert.NotNull(doc.GetElementByTestId($"pdf-{requestData.EvidenceFileId}"));
            Assert.Null(doc.GetElementByTestId($"image-{requestData.EvidenceFileId}"));
        }
        else
        {
            Assert.NotNull(doc.GetElementByTestId($"image-{requestData.EvidenceFileId}"));
            Assert.Null(doc.GetElementByTestId($"pdf-{requestData.EvidenceFileId}"));
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_WhereStateIsPopulated_SetsInputFields(bool canIdentityBeVerified)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null);
        var statedNationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);
        var requestData = supportTask.GetData<OneLoginUserIdVerificationData>();
        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask.SupportTaskReference,
            new ResolveOneLoginUserIdVerificationState
            {
                CanIdentityBeVerified = canIdentityBeVerified
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await AssertEx.HtmlResponseAsync(response);
        AssertCheckedRadioOption("CanIdentityBeVerified", canIdentityBeVerified.ToString());

        void AssertCheckedRadioOption(string name, string expectedCheckedValue)
        {
            var selectedOption = doc.GetElementsByName(name).SingleOrDefault(r => r.HasAttribute("checked"));
            Assert.Equal(expectedCheckedValue, selectedOption?.GetAttribute("value"));
        }
    }

    [Fact]
    public async Task Post_SupportTaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/one-login-user-id-verification/TRS-000/resolve")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_SupportTaskIsNotOpen_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        await WithDbContextAsync(dbContext => dbContext.SupportTasks
            .Where(t => t.SupportTaskReference == supportTask.SupportTaskReference)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.Status, _ => SupportTaskStatus.Closed)));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WhenNoCanIdentityBeVerifiedOptionIsSelected_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null);
        var statedNationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);
        var requestData = supportTask.GetData<OneLoginUserIdVerificationData>();
        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask.SupportTaskReference,
            new ResolveOneLoginUserIdVerificationState());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "CanIdentityBeVerified", "Select yes if you can verify this person’s identity");
    }

    [Fact]
    public async Task Post_CanIdentityBeVerifiedIsFalse_UpdatesStateAndRedirectsToRejectPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null);
        var statedNationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);
        var requestData = supportTask.GetData<OneLoginUserIdVerificationData>();
        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask.SupportTaskReference,
            new ResolveOneLoginUserIdVerificationState());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "CanIdentityBeVerified", "False" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/reject?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.False(journeyInstance.State.CanIdentityBeVerified);
    }

    [Fact]
    public async Task Post_CanIdentityBeVerifiedIsTrueAndNoMatches_UpdatesStateAndRedirectsToNoMatchesPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null);
        var statedNationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject,
            b => b
                .WithStatedFirstName(TestData.GenerateChangedFirstName(person.FirstName))
                .WithStatedLastName(TestData.GenerateChangedLastName(person.LastName))
                .WithStatedDateOfBirth(TestData.GenerateChangedDateOfBirth(person.DateOfBirth))
        );
        var requestData = supportTask.GetData<OneLoginUserIdVerificationData>();
        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask.SupportTaskReference,
            new ResolveOneLoginUserIdVerificationState());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "CanIdentityBeVerified", "True" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/no-matches?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.State.CanIdentityBeVerified);
    }

    [Fact]
    public async Task Post_CanIdentityBeVerifiedIsTrueAndMatches_UpdatesStateAndRedirectsToMatchesPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null);
        var statedNationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject,
            b => b
                .WithStatedFirstName(person.FirstName)
                .WithStatedLastName(person.LastName)
                .WithStatedDateOfBirth(person.DateOfBirth)
        );
        var requestData = supportTask.GetData<OneLoginUserIdVerificationData>();
        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask.SupportTaskReference,
            new ResolveOneLoginUserIdVerificationState());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "CanIdentityBeVerified", "True" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.State.CanIdentityBeVerified);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null);
        var statedNationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);
        var requestData = supportTask.GetData<OneLoginUserIdVerificationData>();
        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask.SupportTaskReference,
            new ResolveOneLoginUserIdVerificationState());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/cancel?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/support-tasks/one-login-user-id-verification", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    private Task<JourneyInstance<ResolveOneLoginUserIdVerificationState>> CreateJourneyInstanceAsync(
            string supportTaskReference,
            ResolveOneLoginUserIdVerificationState state) =>
        CreateJourneyInstance(
            JourneyNames.ResolveOneLoginUserIdVerification,
            state,
            new KeyValuePair<string, object>("supportTaskReference", supportTaskReference));
}
