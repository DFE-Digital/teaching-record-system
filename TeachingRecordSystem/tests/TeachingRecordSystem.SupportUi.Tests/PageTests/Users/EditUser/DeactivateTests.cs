using AngleSharp.Html.Dom;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Users.EditUser;

public class DeactivateTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Test]
    public async Task Get_UserWithoutAccessManagerRole_ReturnsForbidden()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.RecordManager);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(existingUser.UserId));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert

        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_UserIdDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(Guid.NewGuid()));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_UserWithoutAccessManagerRole_ReturnsForbidden()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.RecordManager);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(existingUser.UserId))
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "HasAdditionalReason", false },
                { "HasMoreInformation", false },
                { "UploadEvidence", false }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_UserIdDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(Guid.NewGuid()))
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "HasAdditionalReason", false },
                { "HasMoreInformation", false },
                { "UploadEvidence", false }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_UserExistsButIsAlreadyDeactivated_ReturnsBadRequest()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync(active: false, role: UserRoles.RecordManager);

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(existingUser.UserId))
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "HasAdditionalReason", false },
                { "HasMoreInformation", false },
                { "UploadEvidence", false }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_UserWithoutAdministratorRole_DeactivatingUserWithAdministratorRole_ReturnsBadRequest()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync(role: UserRoles.Administrator);

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(existingUser.UserId))
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "HasAdditionalReason", false },
                { "HasMoreInformation", false },
                { "UploadEvidence", false }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_UserWithAdministratorRole_DeactivatingUserWithAdministratorRole_ReturnsFound()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.Administrator);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync(role: UserRoles.Administrator);

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(existingUser.UserId))
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "HasAdditionalReason", false },
                { "HasMoreInformation", false },
                { "UploadEvidence", false }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_HasAdditionalReasonNotSelected_RendersError()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(existingUser.UserId))
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "HasMoreInformation", false },
                { "UploadEvidence", false }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        await AssertEx.HtmlResponseHasErrorAsync(response, "HasAdditionalReason", "Select a reason for deactivating this user");
    }

    [Test]
    public async Task Post_HasAdditionalReasonSetToYes_ButAdditionalReasonDetailNotEntered_RendersError()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(existingUser.UserId))
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "HasAdditionalReason", true },
                { "AdditionalReasonDetail", "" },
                { "HasMoreInformation", false },
                { "UploadEvidence", false }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        await AssertEx.HtmlResponseHasErrorAsync(response, "AdditionalReasonDetail", "Enter a reason");
    }

    [Test]
    public async Task Post_HasMoreInformationNotSelected_RendersError()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(existingUser.UserId))
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "HasAdditionalReason", false },
                { "UploadEvidence", false }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        await AssertEx.HtmlResponseHasErrorAsync(response, "HasMoreInformation", "Select yes if you want to provide more details");
    }

    [Test]
    public async Task Post_HasMoreInformationSetToYes_ButMoreInformationDetailNotEntered_RendersError()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(existingUser.UserId))
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "HasAdditionalReason", false },
                { "HasMoreInformation", true },
                { "MoreInformationDetail", "" },
                { "UploadEvidence", false }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        await AssertEx.HtmlResponseHasErrorAsync(response, "MoreInformationDetail", "Enter more details");
    }

    [Test]
    public async Task Post_UploadEvidenceNotSelected_RendersError()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(existingUser.UserId))
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "HasAdditionalReason", false },
                { "HasMoreInformation", false }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        await AssertEx.HtmlResponseHasErrorAsync(response, "UploadEvidence", "Select yes if you want to upload evidence");
    }

    [Test]
    public async Task Post_UploadEvidenceSetToYes_ButNoEvidenceFileSelected_RendersError()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(existingUser.UserId))
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "HasAdditionalReason", false },
                { "HasMoreInformation", false },
                { "UploadEvidence", true }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        await AssertEx.HtmlResponseHasErrorAsync(response, "EvidenceFile", "Select a file");
    }

    [Test]
    public async Task Post_UploadEvidenceSetToYes_ButEvidenceFileIsInvalidType_RendersError()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(existingUser.UserId))
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "HasAdditionalReason", false },
                { "HasMoreInformation", false },
                { "UploadEvidence", true },
                { "EvidenceFile", CreateEvidenceFileBinaryContent(), "invalidfile.cs" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        await AssertEx.HtmlResponseHasErrorAsync(response, "EvidenceFile", "The selected file must be a BMP, CSV, DOC, DOCX, EML, JPEG, JPG, MBOX, MSG, ODS, ODT, PDF, PNG, TIF, TXT, XLS or XLSX");
    }

    [Test]
    public async Task Post_UploadEvidenceSetToYes_AndEvidenceFileIsSelected_ButOtherFieldsInvalid_ShowsUploadedFile()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(existingUser.UserId))
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "HasMoreInformation", false },
                { "UploadEvidence", true },
                { "EvidenceFile", CreateEvidenceFileBinaryContent(new byte[1230]), "validfile.png" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        var doc = await AssertEx.HtmlResponseAsync(response, 400);

        var evidenceFileId = await FileServiceMock.AssertFileWasUploadedAsync();
        var expectedFileUrl = $"{TestScopedServices.FakeBlobStorageFileUrlBase}{evidenceFileId}";

        var link = Assert.IsAssignableFrom<IHtmlAnchorElement>(doc.GetElementByTestId("uploaded-evidence-file-link"));
        Assert.Equal("validfile.png (1.2 KB)", link.TrimmedText());
        Assert.Equal(expectedFileUrl, link.Href);

        Assert.Equal(evidenceFileId.ToString(), doc.GetHiddenInputValue("EvidenceFileId"));
        Assert.Equal("validfile.png", doc.GetHiddenInputValue("EvidenceFileName"));
        Assert.Equal("1.2 KB", doc.GetHiddenInputValue("EvidenceFileSizeDescription"));
        Assert.Equal(expectedFileUrl, doc.GetHiddenInputValue("UploadedEvidenceFileUrl"));
    }

    [Test]
    public async Task Post_UploadEvidenceSetToYes_AndEvidenceFilePreviouslyUploaded_ButOtherFieldsInvalid_RemembersUploadedFile()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync();
        var evidenceFileId = Guid.NewGuid();

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(existingUser.UserId))
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "HasMoreInformation", false },
                { "UploadEvidence", true },
                { "EvidenceFileId", evidenceFileId },
                { "EvidenceFileName", "testfile.jpg" },
                { "EvidenceFileSizeDescription", "3 KB" },
                { "UploadedEvidenceFileUrl", "http://test.com/file" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        var doc = await AssertEx.HtmlResponseAsync(response, 400);

        var expectedFileUrl = $"{TestScopedServices.FakeBlobStorageFileUrlBase}{evidenceFileId}";

        var link = Assert.IsAssignableFrom<IHtmlAnchorElement>(doc.GetElementByTestId("uploaded-evidence-file-link"));
        Assert.Equal("testfile.jpg (3 KB)", link.TrimmedText());
        Assert.Equal("http://test.com/file", link.Href);

        Assert.Equal(evidenceFileId.ToString(), doc.GetHiddenInputValue("EvidenceFileId"));
        Assert.Equal("testfile.jpg", doc.GetHiddenInputValue("EvidenceFileName"));
        Assert.Equal("3 KB", doc.GetHiddenInputValue("EvidenceFileSizeDescription"));
        Assert.Equal("http://test.com/file", doc.GetHiddenInputValue("UploadedEvidenceFileUrl"));
    }

    [Test]
    public async Task Post_UploadEvidenceSetToYes_AndEvidenceFilePreviouslyUploaded_AndNewFileUploaded_ButOtherFieldsInvalid_DeletesPreviouslyUploadedFile()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync();
        var evidenceFileId = Guid.NewGuid();

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(existingUser.UserId))
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "HasMoreInformation", false },
                { "UploadEvidence", true },
                { "EvidenceFile", CreateEvidenceFileBinaryContent(new byte[1230]), "validfile.png" },
                { "EvidenceFileId", evidenceFileId }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);

        FileServiceMock.AssertFileWasDeleted(evidenceFileId);
    }

    [Test]
    public async Task Post_UploadEvidenceSetToNo_ButEvidenceFilePreviouslyUploaded_AndOtherFieldsInvalid_DeletesPreviouslyUploadedFile()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync();
        var evidenceFileId = Guid.NewGuid();

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(existingUser.UserId))
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "HasMoreInformation", false },
                { "UploadEvidence", false },
                { "EvidenceFileId", evidenceFileId }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);

        FileServiceMock.AssertFileWasDeleted(evidenceFileId);
    }

    [Test]
    public async Task PostCancel_RedirectsToEditUserPage()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync();
        var evidenceFileId = Guid.NewGuid();

        var request = new HttpRequestMessage(HttpMethod.Post, $"{GetRequestPath(existingUser.UserId)}/cancel");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/users/{existingUser.UserId}", response.Headers.Location?.OriginalString);
    }

    [Test]
    public async Task PostCancel_EvidenceFilePreviouslyUploaded_DeletesPreviouslyUploadedFileAndRedirectsToEditUserPage()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync();
        var evidenceFileId = Guid.NewGuid();

        var request = new HttpRequestMessage(HttpMethod.Post, $"{GetRequestPath(existingUser.UserId)}/cancel")
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "EvidenceFileId", evidenceFileId }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/users/{existingUser.UserId}", response.Headers.Location?.OriginalString);

        FileServiceMock.AssertFileWasDeleted(evidenceFileId);
    }

    [Test]
    public async Task Post_ValidRequest_DeactivatesUserEmitsEventAndRedirectsWithFlashMessage()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync(role: UserRoles.RecordManager);
        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(existingUser.UserId))
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "HasAdditionalReason", false },
                { "HasMoreInformation", false },
                { "UploadEvidence", false }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedUser = await WithDbContext(dbContext =>
            dbContext.Users.SingleOrDefaultAsync(u => u.UserId == existingUser.UserId));
        Assert.NotNull(updatedUser);

        Assert.False(updatedUser.Active);

        EventObserver.AssertEventsSaved(e =>
        {
            var userCreatedEvent = Assert.IsType<UserDeactivatedEvent>(e);
            Assert.Equal(Clock.UtcNow, userCreatedEvent.CreatedUtc);
            Assert.Equal(userCreatedEvent.RaisedBy.UserId, GetCurrentUserId());
            Assert.Null(userCreatedEvent.DeactivatedReason);
            Assert.Null(userCreatedEvent.DeactivatedReasonDetail);
            Assert.Null(userCreatedEvent.EvidenceFileId);
        });

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, expectedHeading: $"{existingUser.Name}\u2019s account has been deactivated");
    }

    [Test]
    public async Task Post_ValidRequest_WithAdditionalReasonMoreInformationAndEvidenceFile_DeactivatesUserUploadsEvidenceFileEmitsEventAndRedirectsWithFlashMessage()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync(role: UserRoles.RecordManager);
        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(existingUser.UserId))
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "HasAdditionalReason", true },
                { "AdditionalReasonDetail", "Some additional reason" },
                { "HasMoreInformation", true },
                { "MoreInformationDetail", "Some more information" },
                { "UploadEvidence", true },
                { "EvidenceFile", CreateEvidenceFileBinaryContent(new byte[1230]), "validfile.png" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedUser = await WithDbContext(dbContext =>
            dbContext.Users.SingleOrDefaultAsync(u => u.UserId == existingUser.UserId));
        Assert.NotNull(updatedUser);
        Assert.False(updatedUser.Active);

        var evidenceFileId = await FileServiceMock.AssertFileWasUploadedAsync();

        EventObserver.AssertEventsSaved(e =>
        {
            var userCreatedEvent = Assert.IsType<UserDeactivatedEvent>(e);
            Assert.Equal(Clock.UtcNow, userCreatedEvent.CreatedUtc);
            Assert.Equal(userCreatedEvent.RaisedBy.UserId, GetCurrentUserId());
            Assert.Equal("Some additional reason", userCreatedEvent.DeactivatedReason);
            Assert.Equal("Some more information", userCreatedEvent.DeactivatedReasonDetail);
            Assert.Equal(evidenceFileId, userCreatedEvent.EvidenceFileId);
        });

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, expectedHeading: $"{existingUser.Name}\u2019s account has been deactivated");
    }

    [Test]
    public async Task Post_ValidRequest_WithPreviouslyUploadedEvidenceFile_DeactivatesUserEmitsEventAndRedirectsWithFlashMessage()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync(role: UserRoles.RecordManager);
        var evidenceFileId = Guid.NewGuid();
        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(existingUser.UserId))
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "HasAdditionalReason", false },
                { "HasMoreInformation", false },
                { "UploadEvidence", true },
                { "EvidenceFileId", evidenceFileId }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedUser = await WithDbContext(dbContext =>
            dbContext.Users.SingleOrDefaultAsync(u => u.UserId == existingUser.UserId));
        Assert.NotNull(updatedUser);
        Assert.False(updatedUser.Active);

        FileServiceMock.AssertFileWasNotUploaded();

        EventObserver.AssertEventsSaved(e =>
        {
            var userCreatedEvent = Assert.IsType<UserDeactivatedEvent>(e);
            Assert.Equal(Clock.UtcNow, userCreatedEvent.CreatedUtc);
            Assert.Equal(userCreatedEvent.RaisedBy.UserId, GetCurrentUserId());
            Assert.Null(userCreatedEvent.DeactivatedReason);
            Assert.Null(userCreatedEvent.DeactivatedReasonDetail);
            Assert.Equal(evidenceFileId, userCreatedEvent.EvidenceFileId);
        });

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, expectedHeading: $"{existingUser.Name}\u2019s account has been deactivated");
    }

    [Test]
    public async Task Post_ValidRequest_WithAdditionalInfo_ButAdditionalInfoRadioButtonsNotSetToYes_DeactivatesUserAndDiscardsAdditionalInfo()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync(role: UserRoles.RecordManager);
        var evidenceFileId = Guid.NewGuid();
        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(existingUser.UserId))
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "HasAdditionalReason", false },
                { "AdditionalReasonDetail", "Some additional reason" },
                { "HasMoreInformation", false },
                { "MoreInformationDetail", "Some more information" },
                { "UploadEvidence", false },
                { "EvidenceFile", CreateEvidenceFileBinaryContent(new byte[1230]), "validfile.png" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedUser = await WithDbContext(dbContext =>
            dbContext.Users.SingleOrDefaultAsync(u => u.UserId == existingUser.UserId));
        Assert.NotNull(updatedUser);
        Assert.False(updatedUser.Active);

        FileServiceMock.AssertFileWasNotUploaded();

        EventObserver.AssertEventsSaved(e =>
        {
            var userCreatedEvent = Assert.IsType<UserDeactivatedEvent>(e);
            Assert.Equal(Clock.UtcNow, userCreatedEvent.CreatedUtc);
            Assert.Equal(userCreatedEvent.RaisedBy.UserId, GetCurrentUserId());
            Assert.Null(userCreatedEvent.DeactivatedReason);
            Assert.Null(userCreatedEvent.DeactivatedReasonDetail);
            Assert.Null(userCreatedEvent.EvidenceFileId);
        });

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, expectedHeading: $"{existingUser.Name}\u2019s account has been deactivated");
    }

    private static string GetRequestPath(Guid userId) => $"/users/{userId}/deactivate";
}
