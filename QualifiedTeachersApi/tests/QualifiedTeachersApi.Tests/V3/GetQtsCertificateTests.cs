using System;
using System.IO;
using System.Net.Http;
using Azure;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Moq;
using QualifiedTeachersApi.DataStore.Crm.Models;
using Xunit;

namespace QualifiedTeachersApi.Tests.V3;

public class GetQtsCertificateTests : ApiTestBase
{
    public GetQtsCertificateTests(ApiFixture apiFixture)
        : base(apiFixture)
    {
        SetupMockBlobClient();
    }

    [Fact]
    public async Task Get_QtsCertificateWithTrnDoesNotExist_ReturnsBadRequest()
    {
        // Arrange
        var trn = "1234567";
        var httpClient = GetHttpClientWithIdentityAccessToken(trn);

        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/certificates/qts");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_QtsCertificateWithQtsDateDoesNotExist_ReturnsBadRequest()
    {
        // Arrange
        var trn = "1234567";
        var httpClient = GetHttpClientWithIdentityAccessToken(trn);

        var firstName = Faker.Name.First();
        var lastName = Faker.Name.Last();

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetTeacherByTrn(trn, /* columnNames: */ It.IsAny<string[]>(), /* activeOnly: */ true))
            .ReturnsAsync(new Contact()
            {
                Id = Guid.NewGuid(),
                dfeta_TRN = trn,
                FirstName = firstName,
                LastName = lastName,
            });

        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/certificates/qts");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequest_ReturnsExpectedResponse()
    {
        // Arrange
        var trn = "1234567";
        var httpClient = GetHttpClientWithIdentityAccessToken(trn);

        var firstName = Faker.Name.First();
        var lastName = Faker.Name.Last();
        var qtsDate = new DateOnly(1997, 4, 23);

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetTeacherByTrn(trn, /* columnNames: */ It.IsAny<string[]>(), /* activeOnly: */ true))
            .ReturnsAsync(new Contact()
            {
                Id = Guid.NewGuid(),
                dfeta_TRN = trn,
                FirstName = firstName,
                LastName = lastName,
                dfeta_QTSDate = qtsDate.ToDateTime()
            });

        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/certificates/qts");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    private void SetupMockBlobClient()
    {
        var mockBlobClient = new Mock<BlobClient>();
        var response = new Mock<Response>();

        string projectDirectory = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName;
        var pdfPath = Path.Combine(projectDirectory!, "Resources", "TestQtsCertificate.pdf");
        var pdfBytes = File.ReadAllBytes(pdfPath);

        mockBlobClient
            .Setup(c => c.DownloadToAsync(It.IsAny<Stream>()))
            .Callback<Stream>(stream =>
            {
                stream.Write(pdfBytes, 0, pdfBytes.Length);
            })
            .ReturnsAsync(response.Object);

        var mockBlobContainerClient = new Mock<BlobContainerClient>();
        mockBlobContainerClient
            .Setup(mock => mock.GetBlobClient("QTS certificate.pdf"))
            .Returns(mockBlobClient.Object);

        ApiFixture.BlobServiceClient
            .Setup(mock => mock.GetBlobContainerClient("certificates"))
            .Returns(mockBlobContainerClient.Object);
    }
}
