using System.Diagnostics;
using System.Net.Http.Headers;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using static TeachingRecordSystem.AuthorizeAccess.SignInJourneyCoordinator.Vtrs;

namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests;

public class ProofOfIdentityTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public Task Get_ValidRequest_RendersExpectedContent() =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var person = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber());
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, verifiedInfo: null);

                await SetupInstanceStateAsync(coordinator, oneLoginUser);

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.ProofOfIdentity(coordinator.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                await AssertEx.HtmlResponseAsync(response);
            });

    [Fact]
    public Task Post_FileIsNotValidType_ReturnsError() =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var person = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber());
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, verifiedInfo: null);

                await SetupInstanceStateAsync(coordinator, oneLoginUser, person.NationalInsuranceNumber!);

                var fileName = "proof.txt";
                var content = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes("This is a text file."));
                content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "File",
                    FileName = fileName
                };
                content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.ProofOfIdentity(coordinator.InstanceId))
                {
                    Content = new MultipartFormDataContentBuilder
                    {
                        { "File", (content, fileName) }
                    }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                await AssertEx.HtmlResponseHasErrorAsync(response, "File", "The file must be a PDF, JPG, or PNG");
            });

    [Fact]
    public Task Post_FileIsLargerThan10MB_ReturnsError() =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var person = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber());
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, verifiedInfo: null);

                await SetupInstanceStateAsync(coordinator, oneLoginUser, person.NationalInsuranceNumber!);

                var fileName = "proof.jpg";
                var largeFileContent = new byte[11 * 1024 * 1024]; // 11 MB
                var content = new ByteArrayContent(largeFileContent);
                content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "File",
                    FileName = fileName
                };
                content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.ProofOfIdentity(coordinator.InstanceId))
                {
                    Content = new MultipartFormDataContentBuilder
                    {
                        { "File", (content, fileName) }
                    }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                await AssertEx.HtmlResponseHasErrorAsync(response, "File", "The file must be no larger than 10MB");
            });

    [Fact]
    public Task Post_FileContainsAVirus_ReturnsError() =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                SafeFileService
                    .Setup(s => s.TrySafeUploadAsync(
                                It.IsAny<Stream>(),
                                It.IsAny<string?>(),
                                out It.Ref<Guid>.IsAny,
                                null))
                    .ReturnsAsync(false);

                var person = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber());
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, verifiedInfo: null);

                await SetupInstanceStateAsync(coordinator, oneLoginUser, person.NationalInsuranceNumber!);

                var fileName = "proof.jpg";
                var content = new ByteArrayContent(TestData.JpegImage);
                content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "File",
                    FileName = fileName
                };
                content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.ProofOfIdentity(coordinator.InstanceId))
                {
                    Content = new MultipartFormDataContentBuilder
                    {
                        { "File", (content, fileName) }
                    }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                await AssertEx.HtmlResponseHasErrorAsync(response, "File", "The selected file contains a virus");
            });

    [Fact]
    public Task Post_ValidFile_UpdatesStateAndRedirectsToProofOfIdentity() =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var person = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber());
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, verifiedInfo: null);

                await SetupInstanceStateAsync(coordinator, oneLoginUser, person.NationalInsuranceNumber!);

                var fileName = "proof.jpg";
                var content = new ByteArrayContent(TestData.JpegImage);
                content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "File",
                    FileName = fileName
                };
                content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.ProofOfIdentity(coordinator.InstanceId))
                {
                    Content = new MultipartFormDataContentBuilder
                    {
                        { "File", (content, fileName) }
                    }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.CheckAnswers(coordinator.InstanceId), response.Headers.Location?.OriginalString);

                var state = coordinator.State;
                Assert.NotNull(state.ProofOfIdentityFileId);
                Assert.Equal(fileName, state.ProofOfIdentityFileName);
            });

    private async Task SetupInstanceStateAsync(
        SignInJourneyCoordinator coordinator,
        OneLoginUser oneLoginUser,
        string? nationalInsuranceNumber = null,
        string? trn = null)
    {
        var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
        await coordinator.OnOneLoginCallbackAsync(ticket);
        Debug.Assert(!coordinator.State.IdentityVerified);
        AddUrlToPath(coordinator, StepUrls.Name);
        coordinator.UpdateState(s => s.SetName(TestData.GenerateFirstName(), TestData.GenerateLastName()));
        AddUrlToPath(coordinator, StepUrls.DateOfBirth);
        coordinator.UpdateState(s => s.SetDateOfBirth(TestData.GenerateDateOfBirth()));
        AddUrlToPath(coordinator, StepUrls.NationalInsuranceNumber);
        coordinator.UpdateState(s => s.SetNationalInsuranceNumber(true, nationalInsuranceNumber ?? TestData.GenerateNationalInsuranceNumber()));
        AddUrlToPath(coordinator, StepUrls.Trn);
        await coordinator.UpdateStateAsync(async s => s.SetTrn(true, trn ?? await TestData.GenerateTrnAsync()));
        AddUrlToPath(coordinator, StepUrls.ProofOfIdentity);
    }
}
