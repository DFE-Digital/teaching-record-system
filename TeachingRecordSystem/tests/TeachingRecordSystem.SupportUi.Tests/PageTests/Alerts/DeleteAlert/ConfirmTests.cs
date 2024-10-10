using TeachingRecordSystem.SupportUi.Pages.Alerts.DeleteAlert;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.DeleteAlert;

public class ConfirmTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithAlertIdForNonExistentAlert_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/delete/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_MissingDataInJourneyState_RedirectsToIndexPage()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithAlert());
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/delete/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alertId}/delete", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_WithValidJourneyState_ReturnsOk(bool populateOptional)
    {
        // Arrange
        var alertType = await TestData.ReferenceDataCache.GetAlertTypeById(Guid.Parse("ed0cd700-3fb2-4db0-9403-ba57126090ed")); // Prohibition by the Secretary of State - misconduct
        var startDate = TestData.Clock.Today.AddDays(-50);
        var details = "Some details";
        var link = populateOptional ? TestData.GenerateUrl() : null;
        var endDate = populateOptional ? TestData.Clock.Today.AddDays(-5) : (DateOnly?)null;
        var person = await TestData.CreatePerson(
            b => b.WithAlert(
                a => a.WithAlertTypeId(alertType.AlertTypeId)
                    .WithDetails(details)
                    .WithExternalLink(link)
                    .WithStartDate(startDate)
                    .WithEndDate(endDate)));
        var alert = person.Alerts.Single();
        var journeyInstance = await CreateJourneyInstance(
            alert.AlertId,
            new DeleteAlertState
            {
                ConfirmDelete = true
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/delete/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        Assert.Equal(alertType.Name, doc.GetElementByTestId("alert-type")!.TextContent);
        Assert.Equal(details, doc.GetElementByTestId("details")!.TextContent);
        Assert.Equal(populateOptional ? $"{link} (opens in new tab)" : "-", doc.GetElementByTestId("link")!.TextContent);
        Assert.Equal(startDate.ToString("d MMMM yyyy"), doc.GetElementByTestId("start-date")!.TextContent);
        Assert.Equal(populateOptional ? endDate?.ToString("d MMMM yyyy") : "-", doc.GetElementByTestId("end-date")!.TextContent);
    }

    [Fact]
    public async Task Post_WithAlertIdForNonExistentAlert_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/delete/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_MissingDataInJourneyState_RedirectsToIndexPage()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithAlert());
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/delete/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alertId}/delete", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_WhenNoAdditionalDetailOptionIsSelected_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithAlert());
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new DeleteAlertState
            {
                ConfirmDelete = true
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/delete/confirm?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["UploadEvidence"] = "False"
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "HasAdditionalDetail", "Select yes if you want to add additional detail");
    }

    [Fact]
    public async Task Post_WhenAdditionalDetailOptionIsYesAndNoAdditionalDetailIsEntered_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithAlert());
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new DeleteAlertState
            {
                ConfirmDelete = true
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/delete/confirm?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["HasAdditionalDetail"] = "True",
                ["UploadEvidence"] = "False"
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "AdditionalDetail", "Add additional detail");
    }

    [Fact]
    public async Task Post_WhenNoUploadEvidenceOptionIsSelected_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithAlert());
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new DeleteAlertState
            {
                ConfirmDelete = true
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/delete/confirm?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["HasAdditonalDetail"] = "False"
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "UploadEvidence", "Select yes if you want to upload evidence");
    }

    [Fact]
    public async Task Post_WhenUploadEvidenceOptionIsYesAndNoFileIsSelected_ReturnsError()
    {
        // Arrange
        var startDate = TestData.Clock.Today.AddDays(-50);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new DeleteAlertState
            {
                ConfirmDelete = true
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/delete/confirm?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["HasAdditonalDetail"] = "False",
                ["UploadEvidence"] = "True"
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "EvidenceFile", "Select a file");
    }

    [Fact]
    public async Task Post_WhenEvidenceFileIsInvalidType_ReturnsError()
    {
        // Arrange
        var startDate = TestData.Clock.Today.AddDays(-50);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new DeleteAlertState
            {
                ConfirmDelete = true
            });

        var multipartContent = CreateFormFileUpload(".cs");
        multipartContent.Add(new StringContent("False"), "HasAdditionalDetail");
        multipartContent.Add(new StringContent("True"), "UploadEvidence");

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/delete/confirm?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = multipartContent
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "EvidenceFile", "The selected file must be a BMP, CSV, DOC, DOCX, EML, JPEG, JPG, MBOX, MSG, ODS, ODT, PDF, PNG, TIF, TXT, XLS or XLSX");
    }

    [Fact]
    public async Task Post_WhenValidInput_SoftDeletesAlertCreatesCreatesEventCompletesJourneyAndRedirectsWithFlashMessage()
    {
        // Arrange
        var startDate = TestData.Clock.Today.AddDays(-50);
        var additionalDetail = "My additional detail";
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate)));
        var originalAlert = person.Alerts.Single();
        var alertId = originalAlert.AlertId;

        EventPublisher.Clear();

        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new DeleteAlertState
            {
                ConfirmDelete = true
            });

        var multipartContent = CreateFormFileUpload(".pdf");
        multipartContent.Add(new StringContent("True"), "HasAdditionalDetail");
        multipartContent.Add(new StringContent(additionalDetail), "AdditionalDetail");
        multipartContent.Add(new StringContent("True"), "UploadEvidence");

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/delete/confirm?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = multipartContent
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirect(HttpClient);
        var redirectDoc = await redirectResponse.GetDocument();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Alert deleted");

        await WithDbContext(async dbContext =>
        {
            var alertExists = await dbContext.Alerts.AnyAsync(a => a.AlertId == alertId);
            Assert.False(alertExists);
        });

        EventPublisher.AssertEventsSaved(e =>
        {
            var expectedAlertDeletedEvent = new AlertDeletedEvent()
            {
                EventId = Guid.Empty,
                CreatedUtc = Clock.UtcNow,
                RaisedBy = GetCurrentUserId(),
                PersonId = person.PersonId,
                DeletionReasonDetail = additionalDetail,
                Alert = new()
                {
                    AlertId = Guid.Empty,
                    AlertTypeId = originalAlert.AlertTypeId,
                    Details = originalAlert.Details,
                    ExternalLink = originalAlert.ExternalLink,
                    StartDate = startDate,
                    EndDate = null
                },
                EvidenceFile = new()
                {
                    FileId = Guid.Empty,
                    Name = "evidence.pdf"
                }
            };

            var actualAlertDeletedEvent = Assert.IsType<AlertDeletedEvent>(e);
            Assert.Equivalent(expectedAlertDeletedEvent with { EventId = actualAlertDeletedEvent.EventId }, actualAlertDeletedEvent);
        });

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    private MultipartFormDataContent CreateFormFileUpload(string fileExtension)
    {
        var byteArrayContent = new ByteArrayContent(new byte[] { });
        byteArrayContent.Headers.Add("Content-Type", "application/octet-stream");

        var multipartContent = new MultipartFormDataContent
        {
            { byteArrayContent, "EvidenceFile", $"evidence{fileExtension}" }
        };

        return multipartContent;
    }

    private async Task<JourneyInstance<DeleteAlertState>> CreateJourneyInstance(Guid alertId, DeleteAlertState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.DeleteAlert,
            state ?? new DeleteAlertState(),
            new KeyValuePair<string, object>("alertId", alertId));
}
