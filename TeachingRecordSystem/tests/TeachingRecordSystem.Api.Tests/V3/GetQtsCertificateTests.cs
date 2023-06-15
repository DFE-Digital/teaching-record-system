using Moq;
using TeachingRecordSystem.Api.DataStore.Crm.Models;
using TeachingRecordSystem.Api.Tests.Services.Certificates;
using Xunit;

namespace TeachingRecordSystem.Api.Tests.V3;

public class GetQtsCertificateTests : ApiTestBase
{
    public GetQtsCertificateTests(ApiFixture apiFixture)
        : base(apiFixture)
    {
    }

    [Fact]
    public async Task Get_QtsCertificateWithTrnDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var trn = "1234567";
        var httpClient = GetHttpClientWithIdentityAccessToken(trn);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/certificates/qts");
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_QtsCertificateWithQtsDateDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var trn = "1234567";
        var httpClient = GetHttpClientWithIdentityAccessToken(trn);

        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();

        ApiFixture.DataverseAdapter
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
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/certificates/qts");
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequest_ReturnsExpectedResponse()
    {
        // Arrange
        var trn = "1234567";
        var httpClient = GetHttpClientWithIdentityAccessToken(trn);

        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var qtsDate = new DateOnly(1997, 4, 23);

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetTeacherByTrn(trn, /* columnNames: */ It.IsAny<string[]>(), /* activeOnly: */ true))
            .ReturnsAsync(new Contact()
            {
                Id = Guid.NewGuid(),
                dfeta_TRN = trn,
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                dfeta_QTSDate = qtsDate.ToDateTime()
            });

        using var pdfStream = typeof(CertificateGeneratorTests).Assembly.GetManifestResourceStream("TeachingRecordSystem.Api.Tests.Resources.TestCertificate.pdf") ??
            throw new Exception("Failed to find TestCertificate.pdf.");

        ApiFixture.CertificateGenerator
            .Setup(g => g.GenerateCertificate(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, string>>()))
            .ReturnsAsync(pdfStream);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/certificates/qts");
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var downloadFilename = response!.Content?.Headers?.ContentDisposition?.FileNameStar;
        Assert.Equal("QTSCertificate.pdf", downloadFilename);
    }
}
