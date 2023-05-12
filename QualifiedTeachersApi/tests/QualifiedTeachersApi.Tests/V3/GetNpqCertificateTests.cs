using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Xrm.Sdk;
using Moq;
using QualifiedTeachersApi.DataStore.Crm.Models;
using QualifiedTeachersApi.Tests.Services.Certificates;
using Xunit;

namespace QualifiedTeachersApi.Tests.V3;

public class GetNpqCertificateTests : ApiTestBase
{
    public GetNpqCertificateTests(ApiFixture apiFixture)
        : base(apiFixture)
    {
    }

    [Fact]
    public async Task Get_NpqCertificateWhenQualificationDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var trn = "1234567";
        var qualificationId = Guid.NewGuid();

        var httpClient = GetHttpClientWithIdentityAccessToken(trn);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/certificates/npq/{qualificationId}");
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_NpqCertificateWhenQualificationIsNotNpq_ReturnsNotFound()
    {
        // Arrange
        var trn = "1234567";
        var qualificationId = Guid.NewGuid();

        ApiFixture.DataverseAdapter
            .Setup(d => d.GetQualificationById(qualificationId, It.IsAny<string[]>(), It.IsAny<string[]>()))
            .ReturnsAsync(new dfeta_qualification()
            {
                dfeta_Type = dfeta_qualification_dfeta_Type.HigherEducation,
                dfeta_CompletionorAwardDate = new DateTime(2021, 10, 11)
            });

        var httpClient = GetHttpClientWithIdentityAccessToken(trn);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/certificates/npq/{qualificationId}");
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_NpqCertificateWhenQualificationHasNoAwardDate_ReturnsNotFound()
    {
        // Arrange
        var trn = "1234567";
        var qualificationId = Guid.NewGuid();

        ApiFixture.DataverseAdapter
            .Setup(d => d.GetQualificationById(qualificationId, It.IsAny<string[]>(), It.IsAny<string[]>()))
            .ReturnsAsync(new dfeta_qualification()
            {
                dfeta_Type = dfeta_qualification_dfeta_Type.NPQLT
            });

        var httpClient = GetHttpClientWithIdentityAccessToken(trn);

        // Act        
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/certificates/npq/{qualificationId}");
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_NpqCertificateWhenQualificationIsNotActive_ReturnsNotFound()
    {
        // Arrange
        var trn = "1234567";
        var qualificationId = Guid.NewGuid();

        ApiFixture.DataverseAdapter
            .Setup(d => d.GetQualificationById(qualificationId, It.IsAny<string[]>(), It.IsAny<string[]>()))
            .ReturnsAsync(new dfeta_qualification()
            {
                dfeta_Type = dfeta_qualification_dfeta_Type.NPQLT,
                dfeta_CompletionorAwardDate = new DateTime(2021, 10, 11),
                StateCode = dfeta_qualificationState.Inactive
            });

        var httpClient = GetHttpClientWithIdentityAccessToken(trn);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/certificates/npq/{qualificationId}");
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_NpqCertificateWhenNoAssociatedTeacher_ReturnsNotFound()
    {
        // Arrange
        var trn = "1234567";
        var qualificationId = Guid.NewGuid();

        ApiFixture.DataverseAdapter
            .Setup(d => d.GetQualificationById(qualificationId, It.IsAny<string[]>(), It.IsAny<string[]>()))
            .ReturnsAsync(new dfeta_qualification()
            {
                dfeta_Type = dfeta_qualification_dfeta_Type.NPQLT,
                dfeta_CompletionorAwardDate = new DateTime(2021, 10, 11),
                StateCode = dfeta_qualificationState.Active
            });

        var httpClient = GetHttpClientWithIdentityAccessToken(trn);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/certificates/npq/{qualificationId}");
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequest_ReturnsExpectedResponse()
    {
        // Arrange        
        var trn = "1234567";
        var qualificationId = Guid.NewGuid();
        var teacherId = Guid.NewGuid();

        var teacher = new Contact()
        {
            Id = teacherId,
            FirstName = Faker.Name.First(),
            MiddleName = Faker.Name.Middle(),
            LastName = Faker.Name.Last()
        };

        var qualification = new dfeta_qualification()
        {
            Id = qualificationId,
            dfeta_Type = dfeta_qualification_dfeta_Type.NPQLT,
            dfeta_CompletionorAwardDate = new DateTime(2021, 10, 11),
            StateCode = dfeta_qualificationState.Active,
            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId)
        };

        qualification.Attributes.Add($"contact.{Contact.PrimaryIdAttribute}", new AliasedValue(Contact.EntityLogicalName, Contact.PrimaryIdAttribute, teacherId));
        qualification.Attributes.Add($"contact.{Contact.Fields.FirstName}", new AliasedValue(Contact.EntityLogicalName, Contact.Fields.FirstName, teacher.FirstName));
        qualification.Attributes.Add($"contact.{Contact.Fields.MiddleName}", new AliasedValue(Contact.EntityLogicalName, Contact.Fields.MiddleName, teacher.MiddleName));
        qualification.Attributes.Add($"contact.{Contact.Fields.LastName}", new AliasedValue(Contact.EntityLogicalName, Contact.Fields.FirstName, teacher.LastName));

        ApiFixture.DataverseAdapter
            .Setup(d => d.GetQualificationById(qualificationId, It.IsAny<string[]>(), It.IsAny<string[]>()))
            .ReturnsAsync(qualification);

        using var pdfStream = typeof(CertificateGeneratorTests).Assembly.GetManifestResourceStream("QualifiedTeachersApi.Tests.Resources.TestCertificate.pdf") ??
            throw new Exception("Failed to find TestCertificate.pdf.");

        ApiFixture.CertificateGenerator
            .Setup(g => g.GenerateCertificate(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, string>>()))
            .ReturnsAsync(pdfStream);

        var httpClient = GetHttpClientWithIdentityAccessToken(trn);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/certificates/npq/{qualificationId}");
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var downloadFilename = response!.Content?.Headers?.ContentDisposition?.FileNameStar;
        Assert.Equal($"{qualification.dfeta_Type}Certificate.pdf", downloadFilename);
    }
}
