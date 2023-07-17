namespace TeachingRecordSystem.Api.Tests.V3;

[TestClass]
public class GetEytsCertificateTests : ApiTestBase
{
    public GetEytsCertificateTests(ApiFixture apiFixture)
        : base(apiFixture)
    {
    }

    [Test]
    public async Task Get_EytsCertificateWithTrnDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var trn = "1234567";
        var httpClient = GetHttpClientWithIdentityAccessToken(trn);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/certificates/eyts");
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_EytsCertificateWithEytsDateDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var trn = "1234567";
        var httpClient = GetHttpClientWithIdentityAccessToken(trn);

        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();

        DataverseAdapter
            .Setup(mock => mock.GetTeacherByTrn(trn, /* columnNames: */ It.IsAny<string[]>(), /* activeOnly: */ true))
            .ReturnsAsync(new Contact()
            {
                Id = Guid.NewGuid(),
                dfeta_TRN = trn,
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
            });

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/certificates/eyts");
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_ValidRequest_ReturnsExpectedResponse()
    {
        // Arrange
        var trn = "1234567";
        var httpClient = GetHttpClientWithIdentityAccessToken(trn);

        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var eytsDate = new DateOnly(1997, 4, 23);

        DataverseAdapter
            .Setup(mock => mock.GetTeacherByTrn(trn, /* columnNames: */ It.IsAny<string[]>(), /* activeOnly: */ true))
            .ReturnsAsync(new Contact()
            {
                Id = Guid.NewGuid(),
                dfeta_TRN = trn,
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                dfeta_EYTSDate = eytsDate.ToDateTime()
            });

        using var pdfStream = typeof(GetEytsCertificateTests).Assembly.GetManifestResourceStream("TeachingRecordSystem.Api.Tests.Resources.TestCertificate.pdf") ??
            throw new Exception("Failed to find TestCertificate.pdf.");

        CertificateGenerator
            .Setup(g => g.GenerateCertificate(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, string>>()))
            .ReturnsAsync(pdfStream);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/certificates/eyts");
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var downloadFilename = response!.Content?.Headers?.ContentDisposition?.FileNameStar;
        Assert.Equal("EYTSCertificate.pdf", downloadFilename);
    }
}
