#nullable disable
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Xrm.Sdk;
using Moq;
using QualifiedTeachersApi.DataStore.Crm.Models;
using Xunit;

namespace QualifiedTeachersApi.Tests.V3;

public class GetInductionCertificateTests : ApiTestBase
{
    public GetInductionCertificateTests(ApiFixture apiFixture)
        : base(apiFixture)
    {
    }

    [Fact]
    public async Task Get_InductionCertificateWhenNoTeacherAssociatedWithTrn_ReturnsNotFound()
    {
        // Arrange
        var trn = "1234567";

        var httpClient = GetHttpClientWithIdentityAccessToken(trn);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/certificates/induction");
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_InductionCertificateWhenInductionDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var trn = "1234567";

        ApiFixture.DataverseAdapter
            .Setup(d => d.GetTeacherByTrn(trn, It.IsAny<string[]>(), It.IsAny<bool>()))
            .ReturnsAsync(new Contact()
            {
                Id = Guid.NewGuid(),
                dfeta_TRN = trn
            });

        var httpClient = GetHttpClientWithIdentityAccessToken(trn);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/certificates/induction");
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_InductionCertificateWhenInductionNotPassed_ReturnsNotFound()
    {
        // Arrange
        var trn = "1234567";
        var teacherId = Guid.NewGuid();
        var inductionStartDate = new DateOnly(2000, 5, 6);
        var inductionStatus = dfeta_InductionStatus.NotYetCompleted;

        ApiFixture.DataverseAdapter
            .Setup(d => d.GetTeacherByTrn(trn, It.IsAny<string[]>(), It.IsAny<bool>()))
            .ReturnsAsync(new Contact()
            {
                Id = teacherId,
                dfeta_TRN = trn
            });

        ApiFixture.DataverseAdapter
            .Setup(d => d.GetInductionByTeacher(
                teacherId,
                It.IsAny<string[]>(),
                It.IsAny<string[]>(),
                It.IsAny<string[]>(),
                It.IsAny<string[]>()))
            .ReturnsAsync(
            (new dfeta_induction()
            {
                Id = Guid.NewGuid(),
                dfeta_StartDate = inductionStartDate.ToDateTime(),
                dfeta_InductionStatus = inductionStatus
            },
            new dfeta_inductionperiod[] { }
            ));

        var httpClient = GetHttpClientWithIdentityAccessToken(trn);

        // Act        
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/certificates/induction");
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_InductionCertificateWhenInductionHasNoCompletionDate_ReturnsNotFound()
    {
        // Arrange
        var trn = "1234567";
        var teacherId = Guid.NewGuid();
        var inductionStartDate = new DateOnly(2000, 5, 6);
        var inductionStatus = dfeta_InductionStatus.Pass;

        ApiFixture.DataverseAdapter
            .Setup(d => d.GetTeacherByTrn(trn, It.IsAny<string[]>(), It.IsAny<bool>()))
            .ReturnsAsync(new Contact()
            {
                Id = teacherId,
                dfeta_TRN = trn
            });

        ApiFixture.DataverseAdapter
            .Setup(d => d.GetInductionByTeacher(
                teacherId,
                It.IsAny<string[]>(),
                It.IsAny<string[]>(),
                It.IsAny<string[]>(),
                It.IsAny<string[]>()))
            .ReturnsAsync(
            (new dfeta_induction()
            {
                Id = Guid.NewGuid(),
                dfeta_StartDate = inductionStartDate.ToDateTime(),
                dfeta_InductionStatus = inductionStatus
            },
            new dfeta_inductionperiod[] { }
            ));

        var httpClient = GetHttpClientWithIdentityAccessToken(trn);

        // Act        
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/certificates/induction");
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequest_ReturnsExpectedResponse()
    {
        // Arrange
        var trn = "1234567";
        var teacherId = Guid.NewGuid();
        var inductionStartDate = new DateOnly(2000, 5, 6);
        var inductionEndDate = new DateOnly(2000, 7, 9);
        var inductionStatus = dfeta_InductionStatus.Pass;

        var teacher = new Contact()
        {
            Id = teacherId,
            FirstName = Faker.Name.First(),
            MiddleName = Faker.Name.Middle(),
            LastName = Faker.Name.Last(),
            dfeta_TRN = trn
        };

        ApiFixture.DataverseAdapter
            .Setup(d => d.GetTeacherByTrn(trn, It.IsAny<string[]>(), It.IsAny<bool>()))
            .ReturnsAsync(teacher);

        var induction = new dfeta_induction()
        {
            Id = Guid.NewGuid(),
            dfeta_StartDate = inductionStartDate.ToDateTime(),
            dfeta_CompletionDate = inductionEndDate.ToDateTime(),
            dfeta_InductionStatus = inductionStatus,
            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId)
        };

        induction.Attributes.Add($"contact.{Contact.PrimaryIdAttribute}", new AliasedValue(Contact.EntityLogicalName, Contact.PrimaryIdAttribute, teacherId));
        induction.Attributes.Add($"contact.{Contact.Fields.FirstName}", new AliasedValue(Contact.EntityLogicalName, Contact.Fields.FirstName, teacher.FirstName));
        induction.Attributes.Add($"contact.{Contact.Fields.MiddleName}", new AliasedValue(Contact.EntityLogicalName, Contact.Fields.MiddleName, teacher.MiddleName));
        induction.Attributes.Add($"contact.{Contact.Fields.LastName}", new AliasedValue(Contact.EntityLogicalName, Contact.Fields.FirstName, teacher.LastName));

        ApiFixture.DataverseAdapter
            .Setup(d => d.GetInductionByTeacher(
                teacherId,
                It.IsAny<string[]>(),
                It.IsAny<string[]>(),
                It.IsAny<string[]>(),
                It.IsAny<string[]>()))
            .ReturnsAsync((induction, new dfeta_inductionperiod[] { }));

        string projectDirectory = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName;
        var pdfPath = Path.Combine(projectDirectory!, "Resources", "TestCertificate.pdf");
        var pdfStream = File.OpenRead(pdfPath);

        ApiFixture.CertificateGenerator
            .Setup(g => g.GenerateCertificate(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, string>>()))
            .ReturnsAsync(pdfStream);

        var httpClient = GetHttpClientWithIdentityAccessToken(trn);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/certificates/induction");
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var downloadFilename = response!.Content?.Headers?.ContentDisposition?.FileNameStar;
        Assert.Equal($"InductionCertificate.pdf", downloadFilename);
    }
}
