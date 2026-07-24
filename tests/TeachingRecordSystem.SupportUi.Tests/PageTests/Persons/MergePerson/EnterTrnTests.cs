using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.MergePerson;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.MergePerson;

public class EnterTrnTests(HostFixture hostFixture) : MergePersonTestBase(hostFixture)
{
    [Fact]
    public async Task Get_PopulatesThisTrnFromPersonRecord()
    {
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person));

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var thisTrn = doc.GetElementByTestId("this-trn");

        Assert.NotNull(thisTrn);
        Assert.Equal(person.Trn, thisTrn.TrimmedText());
    }

    [Fact]
    public async Task Get_OtherTrnAlreadyEntered_ShowsOtherTrn()
    {
        var personA = await TestData.CreatePersonAsync();
        var personB = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
            }));

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var otherTrn = doc.GetChildElementOfTestId<IHtmlInputElement>("other-trn", "input");
        Assert.NotNull(otherTrn);
        Assert.Equal(personB.Trn, otherTrn.Value);
    }

    [Fact]
    public async Task Post_OtherTrnMissing_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(null)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(EnterTrnModel.OtherTrn), "Enter a TRN");
    }

    [Theory]
    [InlineData("A234567")]
    [InlineData("XYZ")]
    public async Task Post_OtherTrnNotNumeric_ShowsPageError(string trn)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(trn)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(EnterTrnModel.OtherTrn), "TRN must be a number");
    }

    [Theory]
    [InlineData("12345678")]
    [InlineData("123456")]
    public async Task Post_OtherTrnNot7DigitsLong_ShowsPageError(string trn)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(trn)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(EnterTrnModel.OtherTrn), "TRN must be 7 digits long");
    }

    [Fact]
    public async Task Post_OtherTrnSameAsThisTrn_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(person.Trn)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(EnterTrnModel.OtherTrn), "TRN must be for a different record");
    }

    [Fact]
    public async Task Post_OtherTrnDoesNotBelongToPerson_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var newTrn = "0000000";

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(newTrn)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(EnterTrnModel.OtherTrn), "No record found with that TRN");
    }

    [Fact]
    public async Task Post_OtherTrnBelongsToDeactivatedPerson_ShowsPageError()
    {
        // Arrange
        var personA = await TestData.CreatePersonAsync();
        var personB = await TestData.CreatePersonAsync();

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(personB.Person);
            personB.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(EnterTrnModel.OtherTrn), "The TRN you entered belongs to a deactivated record");
    }

    [Fact]
    public async Task Post_PersonAIsDeactivated_ReturnsBadRequest()
    {
        // Arrange
        var personA = await TestData.CreatePersonAsync();

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(personA.Person);
            personA.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var personB = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA));

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_PersonAHasOpenAlert_ReturnsBadRequest()
    {
        // Arrange
        var personA = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a.WithEndDate(null)));

        var personB = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA));

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(InductionStatus.InProgress)]
    [InlineData(InductionStatus.Passed)]
    [InlineData(InductionStatus.Failed)]
    public async Task Post_PersonAHasInvalidInductionStatus_ReturnsBadRequest(InductionStatus status)
    {
        // Arrange
        var personA = await TestData.CreatePersonAsync(p => p
            .WithInductionStatus(i => i
                .WithStatus(status)
                .WithStartDate(new DateOnly(2024, 1, 1))
                .WithCompletedDate(new DateOnly(2024, 1, 1))));

        var personB = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA));

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_PersistsDetailsAndRedirectsToNextPage()
    {
        // Arrange
        var personA = await TestData.CreatePersonAsync();
        var personB = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        AssertEx.ResponseIsRedirectTo(response,
            $"/persons/{personA.PersonId}/merge/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        Assert.Equal(personA.PersonId, journeyInstance.State.PersonAId);
        Assert.Equal(personA.Trn, journeyInstance.State.PersonATrn);
        Assert.Equal(personB.PersonId, journeyInstance.State.PersonBId);
        Assert.Equal(personB.Trn, journeyInstance.State.PersonBTrn);
    }

    private string GetRequestPath(TestData.CreatePersonResult person, MergePersonJourneyCoordinator? journeyInstance = null) =>
        $"/persons/{person.PersonId}/merge/enter-trn?{journeyInstance?.GetUniqueIdQueryParameter()}";

    [Theory]
    [InlineData(null)]
    public async Task Get_BacklinkLinksToExpected(string? expectedPage)
    {
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.PersonAttributeSourcesSet = true;
                s.Evidence = new()
                {
                    UploadEvidence = false
                };
            }));

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.NotNull(backlink);
        var expectedBackLink = $"/persons/{personA.PersonId}";
        if (expectedPage is not null)
        {
            expectedBackLink += "/merge/" + expectedPage;
        }
        Assert.Contains(expectedBackLink, backlink.Href);
    }

    [Theory]
    [InlineData("check-answers")]
    public async Task Get_WithReturnUrlToCheckAnswersPage_BacklinkLinksToExpected(string? expectedPage)
    {
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.PersonAttributeSourcesSet = true;
                s.Evidence = new()
                {
                    UploadEvidence = false
                };
            }));

        var checkAnswersUrl = $"/persons/{personA.PersonId}/merge/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}";

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"{GetRequestPath(personA, journeyInstance)}&returnUrl={Uri.EscapeDataString(checkAnswersUrl)}");
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.NotNull(backlink);
        var expectedBackLink = $"/persons/{personA.PersonId}";
        if (expectedPage is not null)
        {
            expectedBackLink += "/merge/" + expectedPage;
        }
        Assert.Contains(expectedBackLink, backlink.Href);
    }

    [Theory]
    [InlineData("Continue", "Cancel and return to record")]
    public async Task Get_ContinueAndCancelButtons_ExistOnPage(string continueButtonText, string cancelButtonText)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.PersonAttributeSourcesSet = true;
                s.Evidence = new()
                {
                    UploadEvidence = false
                };
            }));

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var form = doc.GetElementByTestId("submit-form") as IHtmlFormElement;
        Assert.NotNull(form);
        var buttons = form.GetElementsByTagName("button").OfType<IHtmlButtonElement>();
        Assert.Collection(buttons,
            b => Assert.Equal(continueButtonText, b.TrimmedText()),
            b => Assert.Equal(cancelButtonText, b.TrimmedText()));
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToPersonDetailPage()
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.PersonAttributeSourcesSet = true;
                s.Evidence = new()
                {
                    UploadEvidence = false
                };
            }));

        var pageUrl = GetRequestPath(personA, journeyInstance);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, pageUrl);
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var cancelButton = doc.GetElementByTestId("cancel-button") as IHtmlButtonElement;
        Assert.NotNull(cancelButton);
        Assert.Equal("Cancel", cancelButton.Name);

        // Act
        var redirectRequest = new HttpRequestMessage(HttpMethod.Post, pageUrl)
        {
            Content = new FormUrlEncodedContentBuilder().Add("Cancel", bool.TrueString)
        };
        var redirectResponse = await HttpClient.SendAsync(redirectRequest);

        // Assert
        AssertEx.ResponseIsRedirectTo(redirectResponse, $"/persons/{personA.PersonId}");

        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }

    [Theory]
    [InlineData("matches")]
    public async Task Post_RedirectsToExpected(string? expectedPage)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.PersonAttributeSourcesSet = true;
                s.Evidence = new()
                {
                    UploadEvidence = false
                };
            }));

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryPersonId(personB.PersonId)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var expectedRedirect = $"/persons/{personA.PersonId}";
        if (expectedPage is not null)
        {
            expectedRedirect = $"{expectedRedirect}/merge/{expectedPage}?{journeyInstance.GetUniqueIdQueryParameter()}";
        }

        AssertEx.ResponseIsRedirectTo(response, expectedRedirect);
    }

    [Theory]
    [InlineData("check-answers")]
    public async Task Post_WithReturnUrlToCheckAnswersPage_RedirectsToCheckAnswersPage(string? expectedPage)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.PersonAttributeSourcesSet = true;
                s.Evidence = new()
                {
                    UploadEvidence = false
                };
            }));

        var checkAnswersUrl = $"/persons/{personA.PersonId}/merge/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}";
        var request = new HttpRequestMessage(HttpMethod.Post, $"{GetRequestPath(personA, journeyInstance)}&returnUrl={Uri.EscapeDataString(checkAnswersUrl)}")
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryPersonId(personB.PersonId)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var expectedRedirect = $"/persons/{personA.PersonId}";
        if (expectedPage is not null)
        {
            expectedRedirect = $"{expectedRedirect}/merge/{expectedPage}?{journeyInstance.GetUniqueIdQueryParameter()}";
        }

        AssertEx.ResponseIsRedirectTo(response, expectedRedirect);
    }
}
