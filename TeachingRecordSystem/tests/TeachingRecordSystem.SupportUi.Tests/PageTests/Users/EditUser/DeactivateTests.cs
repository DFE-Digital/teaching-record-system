using System.Text.Encodings.Web;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.Events.Legacy;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Users.EditUser;

public class DeactivateTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
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

    [Fact]
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

    [Fact]
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
                { "Evidence.UploadEvidence", false }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
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
                { "Evidence.UploadEvidence", false }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
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
                { "Evidence.UploadEvidence", false }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
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
                { "Evidence.UploadEvidence", false }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
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
                { "Evidence.UploadEvidence", false }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
    }

    [Fact]
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
                { "Evidence.UploadEvidence", false }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        await AssertEx.HtmlResponseHasErrorAsync(response, "HasAdditionalReason", "Select a reason for deactivating this user");
    }

    [Fact]
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
                { "Evidence.UploadEvidence", false }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        await AssertEx.HtmlResponseHasErrorAsync(response, "AdditionalReasonDetail", "Enter a reason");
    }

    [Fact]
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
                { "Evidence.UploadEvidence", false }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        await AssertEx.HtmlResponseHasErrorAsync(response, "HasMoreInformation", "Select yes if you want to provide more details");
    }

    [Fact]
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
                { "Evidence.UploadEvidence", false }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        await AssertEx.HtmlResponseHasErrorAsync(response, "MoreInformationDetail", "Enter more details");
    }

    [Fact]
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
                { "HasMoreInformation", false },
                { "Evidence.UploadEvidence", "" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        await AssertEx.HtmlResponseHasErrorAsync(response, "Evidence.UploadEvidence", "Select yes if you want to upload evidence");
    }

    [Fact]
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
                { "Evidence.UploadEvidence", true }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        await AssertEx.HtmlResponseHasErrorAsync(response, "Evidence.EvidenceFile", "Select a file");
    }

    [Fact]
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
                { "Evidence.UploadEvidence", true },
                { "Evidence.EvidenceFile", (CreateEvidenceFileBinaryContent(), "invalidfile.cs") }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        await AssertEx.HtmlResponseHasErrorAsync(response, "Evidence.EvidenceFile", "The selected file must be a BMP, CSV, DOC, DOCX, EML, JPEG, JPG, MBOX, MSG, ODS, ODT, PDF, PNG, TIF, TXT, XLS or XLSX");
    }

    [Fact]
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
                { "Evidence.UploadEvidence", true },
                { "Evidence.EvidenceFile", (CreateEvidenceFileBinaryContent(new byte[1230]), "validfile.png") }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        var doc = await AssertEx.HtmlResponseAsync(response, 400);

        var evidenceFileId = await FileServiceMock.AssertFileWasUploadedAsync();
        var urlEncoder = UrlEncoder.Default;
        var expectedBlobStorageFileUrl = urlEncoder.Encode($"{TestScopedServices.FakeBlobStorageFileUrlBase}{evidenceFileId}");
        var expectedFileUrl = $"http://localhost/files/validfile.png?fileUrl={expectedBlobStorageFileUrl}";

        var link = Assert.IsAssignableFrom<IHtmlAnchorElement>(doc.GetElementByTestId("uploaded-evidence-file-link"));
        Assert.Equal("validfile.png (1.2 KB)", link.TrimmedText());
        Assert.Equal(expectedFileUrl, link.Href);

        Assert.Equal(evidenceFileId.ToString(), doc.GetHiddenInputValue("Evidence.UploadedEvidenceFile.FileId"));
        Assert.Equal("validfile.png", doc.GetHiddenInputValue("Evidence.UploadedEvidenceFile.FileName"));
        Assert.Equal("1.2 KB", doc.GetHiddenInputValue("Evidence.UploadedEvidenceFile.FileSizeDescription"));
    }

    [Fact]
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
                { "Evidence.UploadEvidence", true },
                { "Evidence.UploadedEvidenceFile.FileId", evidenceFileId },
                { "Evidence.UploadedEvidenceFile.FileName", "testfile.jpg" },
                { "Evidence.UploadedEvidenceFile.FileSizeDescription", "3 KB" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        var doc = await AssertEx.HtmlResponseAsync(response, 400);

        var urlEncoder = UrlEncoder.Default;
        var expectedBlobStorageFileUrl = urlEncoder.Encode($"{TestScopedServices.FakeBlobStorageFileUrlBase}{evidenceFileId}");
        var expectedFileUrl = $"http://localhost/files/testfile.jpg?fileUrl={expectedBlobStorageFileUrl}";

        var link = Assert.IsAssignableFrom<IHtmlAnchorElement>(doc.GetElementByTestId("uploaded-evidence-file-link"));
        Assert.Equal("testfile.jpg (3 KB)", link.TrimmedText());
        Assert.Equal(expectedFileUrl, link.Href);

        Assert.Equal(evidenceFileId.ToString(), doc.GetHiddenInputValue("Evidence.UploadedEvidenceFile.FileId"));
        Assert.Equal("testfile.jpg", doc.GetHiddenInputValue("Evidence.UploadedEvidenceFile.FileName"));
        Assert.Equal("3 KB", doc.GetHiddenInputValue("Evidence.UploadedEvidenceFile.FileSizeDescription"));
    }

    [Fact]
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
                { "Evidence.UploadEvidence", true },
                { "Evidence.UploadedEvidenceFile.FileId", evidenceFileId },
                { "Evidence.UploadedEvidenceFile.FileName", "validfile.png" },
                { "Evidence.UploadedEvidenceFile.FileSizeDescription", "5MB" },
                { "Evidence.EvidenceFile", (CreateEvidenceFileBinaryContent(new byte[1230]), "validfile.png") },
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);

        FileServiceMock.AssertFileWasDeleted(evidenceFileId);
    }

    [Fact]
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
                { "Evidence.UploadEvidence", false },
                { "Evidence.UploadedEvidenceFile.FileId", evidenceFileId },
                { "Evidence.UploadedEvidenceFile.FileName", "validfile.png" },
                { "Evidence.UploadedEvidenceFile.FileSizeDescription", "5MB" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);

        FileServiceMock.AssertFileWasDeleted(evidenceFileId);
    }

    [Fact]
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

    [Fact]
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
                { "Evidence.UploadedEvidenceFile.FileId", evidenceFileId },
                { "Evidence.UploadedEvidenceFile.FileName", "validfile.png" },
                { "Evidence.UploadedEvidenceFile.FileSizeDescription", "5MB" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/users/{existingUser.UserId}", response.Headers.Location?.OriginalString);

        FileServiceMock.AssertFileWasDeleted(evidenceFileId);
    }

    [Fact]
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
                { "Evidence.UploadEvidence", false }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedUser = await WithDbContextAsync(dbContext =>
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

    [Fact]
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
                { "Evidence.UploadEvidence", true },
                { "Evidence.EvidenceFile", (CreateEvidenceFileBinaryContent(new byte[1230]), "validfile.png") }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedUser = await WithDbContextAsync(dbContext =>
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

    [Fact]
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
                { "Evidence.UploadEvidence", true },
                { "Evidence.UploadedEvidenceFile.FileId", evidenceFileId },
                { "Evidence.UploadedEvidenceFile.FileName", "validfile.png" },
                { "Evidence.UploadedEvidenceFile.FileSizeDescription", "5MB" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedUser = await WithDbContextAsync(dbContext =>
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

    [Fact]
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
                { "Evidence.UploadEvidence", false },
                { "Evidence.EvidenceFile", (CreateEvidenceFileBinaryContent(new byte[1230]), "validfile.png") }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedUser = await WithDbContextAsync(dbContext =>
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
