using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.Api.Tests.V3;

public class GetQtsCertificateTests : TestBase
{
    private const string QtsAwardedInWalesTeacherStatusValue = "213";
    private readonly Guid _qtsAwardedInWalesTeacherStatusId = Guid.NewGuid();

    public GetQtsCertificateTests(HostFixture hostFixture)
        : base(hostFixture)
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

        DataverseAdapterMock
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
    public async Task Get_QtsCertificateForTeacherQualifiedInWales_ReturnsNotFound()
    {
        // Arrange
        var trn = "1234567";
        var httpClient = GetHttpClientWithIdentityAccessToken(trn);

        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var qtsDate = new DateOnly(1997, 4, 23);

        var teacher = new Contact()
        {
            Id = Guid.NewGuid(),
            dfeta_TRN = trn,
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            dfeta_QTSDate = qtsDate.ToDateTime()
        };

        DataverseAdapterMock
            .Setup(mock => mock.GetTeacherByTrn(trn, /* columnNames: */ It.IsAny<string[]>(), /* activeOnly: */ true))
            .ReturnsAsync(teacher);

        DataverseAdapterMock
            .Setup(mock => mock.GetTeacherStatus(
                It.Is<string>(s => s == QtsAwardedInWalesTeacherStatusValue),
                It.IsAny<RequestBuilder>()))
            .ReturnsAsync(new dfeta_teacherstatus()
            {
                Id = _qtsAwardedInWalesTeacherStatusId
            });

        DataverseAdapterMock
            .Setup(mock => mock.GetTeacherStatus(
                It.Is<string>(s => s != QtsAwardedInWalesTeacherStatusValue),
                It.IsAny<RequestBuilder>()))
            .ReturnsAsync(new dfeta_teacherstatus()
            {
                Id = Guid.NewGuid()
            });

        var qtsRegistrations = new[]
        {
            new dfeta_qtsregistration()
            {
                dfeta_QTSDate = qtsDate.ToDateTime(),
                dfeta_TeacherStatusId = _qtsAwardedInWalesTeacherStatusId.ToEntityReference(dfeta_teacherstatus.EntityLogicalName)
            }
        };

        DataverseAdapterMock
            .Setup(mock => mock.GetQtsRegistrationsByTeacher(
                teacher.Id,
                It.IsAny<string[]>()))
            .ReturnsAsync(qtsRegistrations);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/certificates/qts");
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Get_ValidRequest_ReturnsExpectedResponse(bool exemptFromInduction)
    {
        // Arrange
        var trn = "1234567";
        var httpClient = GetHttpClientWithIdentityAccessToken(trn);

        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var qtsDate = new DateOnly(1997, 4, 23);

        var teacher = new Contact()
        {
            Id = Guid.NewGuid(),
            dfeta_TRN = trn,
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            dfeta_QTSDate = qtsDate.ToDateTime()
        };

        if (exemptFromInduction)
        {
            teacher.dfeta_InductionStatus = dfeta_InductionStatus.Exempt;
        }

        DataverseAdapterMock
            .Setup(mock => mock.GetTeacherByTrn(trn, /* columnNames: */ It.IsAny<string[]>(), /* activeOnly: */ true))
            .ReturnsAsync(teacher);

        DataverseAdapterMock
            .Setup(mock => mock.GetTeacherStatus(
                It.Is<string>(s => s == QtsAwardedInWalesTeacherStatusValue),
                It.IsAny<RequestBuilder>()))
            .ReturnsAsync(new dfeta_teacherstatus()
            {
                Id = _qtsAwardedInWalesTeacherStatusId
            });

        DataverseAdapterMock
            .Setup(mock => mock.GetTeacherStatus(
                It.Is<string>(s => s != QtsAwardedInWalesTeacherStatusValue),
                It.IsAny<RequestBuilder>()))
            .ReturnsAsync(new dfeta_teacherstatus()
            {
                Id = Guid.NewGuid()
            });

        var qtsRegistrations = new[]
        {
            new dfeta_qtsregistration()
            {
                dfeta_QTSDate = qtsDate.ToDateTime(),
                dfeta_TeacherStatusId = Guid.NewGuid().ToEntityReference(dfeta_teacherstatus.EntityLogicalName)
            }
        };

        DataverseAdapterMock
            .Setup(mock => mock.GetQtsRegistrationsByTeacher(
                teacher.Id,
                It.IsAny<string[]>()))
            .ReturnsAsync(qtsRegistrations);

        using var pdfStream = typeof(GetQtsCertificateTests).Assembly.GetManifestResourceStream("TeachingRecordSystem.Api.Tests.Resources.TestCertificate.pdf") ??
            throw new Exception("Failed to find TestCertificate.pdf.");

        string? templateNameActual = null;
        CertificateGeneratorMock
            .Setup(g => g.GenerateCertificate(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, string>>()))
            .Callback<string, IReadOnlyDictionary<string, string>>(
                (t, f) =>
                {
                    templateNameActual = t;
                })
            .ReturnsAsync(pdfStream);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/certificates/qts");
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        if (exemptFromInduction)
        {
            Assert.Equal("Exempt QTS certificate.pdf", templateNameActual);
        }
        else
        {
            Assert.Equal("QTS certificate.pdf", templateNameActual);
        }

        var downloadFilename = response!.Content?.Headers?.ContentDisposition?.FileNameStar;
        Assert.Equal("QTSCertificate.pdf", downloadFilename);
    }
}
