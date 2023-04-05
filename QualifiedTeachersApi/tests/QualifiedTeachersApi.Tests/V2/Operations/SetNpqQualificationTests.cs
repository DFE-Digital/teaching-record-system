﻿#nullable disable
using System;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Moq;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.DataStore.Crm.Models;
using QualifiedTeachersApi.TestCommon;
using QualifiedTeachersApi.V2.ApiModels;
using QualifiedTeachersApi.V2.Requests;
using Xunit;

namespace QualifiedTeachersApi.Tests.V2.Operations;

public class SetNpqQualificationTests : ApiTestBase
{
    public SetNpqQualificationTests(ApiFixture apiFixture) : base(apiFixture)
    {
    }

    [Fact]
    public async Task Given_request_without_trn_return_error()
    {
        // Arrange
        Clock.UtcNow = new DateTime(2021, 12, 04);

        // Act
        var response = await HttpClientWithApiKey.PutAsync(
            $"v2/npq-qualifications",
            CreateRequest(req => req.CompletionDate = new DateOnly(Clock.UtcNow.Year, Clock.UtcNow.Month, Clock.UtcNow.Day)));

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Given_completeddate_before_provider_earliest_completiondate_return_error()
    {
        // Arrange
        Clock.UtcNow = new DateTime(2021, 10, 31);
        var trn = "1234567";
        ApiFixture.DataverseAdapter
           .Setup(mock => mock.GetTeacherByTrn(trn, /* columnNames: */ It.IsAny<string[]>(), /* activeOnly: */ true))
           .ReturnsAsync((Contact)null);

        // Act
        var response = await HttpClientWithApiKey.PutAsync(
            $"v2/npq-qualifications?trn={trn}",
            CreateRequest(req => req.CompletionDate = new DateOnly(Clock.UtcNow.Year, Clock.UtcNow.Month, Clock.UtcNow.Day)));

        // Assert
        await AssertEx.ResponseIsValidationErrorForProperty(
            response,
            propertyName: $"{nameof(SetNpqQualificationRequest.CompletionDate)}",
            expectedError: Properties.StringResources.Errors_10022_Title);
    }

    [Fact]
    public async Task Given_contact_for_trn_not_found_return_error()
    {
        // Arrange
        Clock.UtcNow = new DateTime(2022, 01, 01);
        var trn = "1234567";
        ApiFixture.DataverseAdapter
           .Setup(mock => mock.GetTeacherByTrn(trn, /* columnNames: */ It.IsAny<string[]>(), /* activeOnly: */ true))
           .ReturnsAsync((Contact)null);

        // Act
        var response = await HttpClientWithApiKey.PutAsync(
            $"v2/npq-qualifications?trn={trn}",
            CreateRequest(req => req.CompletionDate = new DateOnly(Clock.UtcNow.Year, Clock.UtcNow.Month, Clock.UtcNow.Day)));

        // Assert
        await AssertEx.ResponseIsError(response, errorCode: 10001, expectedStatusCode: StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Given_valid_request_for_qualification_not_createdbyapi_return_error()
    {
        // Arrange
        Clock.UtcNow = new DateTime(2023, 10, 31);
        var trn = "1234567";
        var id = Guid.NewGuid();
        var contact = new Contact()
        {
            dfeta_TRN = trn,
            Id = Guid.NewGuid()
        };
        var qualifications = new dfeta_qualification[]
        {
           new dfeta_qualification
           {
               dfeta_Type = dfeta_qualification_dfeta_Type.NPQH,
               dfeta_createdbyapi = false
           }
        };

        ApiFixture.DataverseAdapter
           .Setup(mock => mock.GetTeacherByTrn(trn, /* columnNames: */ It.IsAny<string[]>(), /* activeOnly: */ true))
           .ReturnsAsync(contact);

        ApiFixture.DataverseAdapter
           .Setup(mock => mock.GetQualificationsForTeacher(id, It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<string[]>()))
           .ReturnsAsync(qualifications);

        ApiFixture.DataverseAdapter
           .Setup(mock => mock.SetNpqQualification(It.IsAny<SetNpqQualificationCommand>()))
           .ReturnsAsync(SetNpqQualificationResult.Failed(SetNpqQualificationFailedReasons.NpqQualificationNotCreatedByApi));

        // Act
        var response = await HttpClientWithApiKey.PutAsync(
            $"v2/npq-qualifications?trn={trn}",
            CreateRequest(req => req.CompletionDate = new DateOnly(Clock.UtcNow.Year, Clock.UtcNow.Month, Clock.UtcNow.Day)));

        // Assert
        await AssertEx.ResponseIsValidationErrorForProperty(
            response,
            propertyName: $"{nameof(SetNpqQualificationRequest.QualificationType)}",
            expectedError: Properties.StringResources.Errors_10021_Title);
    }

    [Fact]
    public async Task Given_invalid_qualificationtype_return_error()
    {
        // Arrange
        var trn = "1234567";

        // Act
        var response = await HttpClientWithApiKey.PutAsync(
            $"v2/npq-qualifications?trn={trn}",
            CreateRequest(req => req.QualificationType = (QualificationType)(-1)));

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Given_valid_request_return_nocontent()
    {
        // Arrange
        Clock.UtcNow = new DateTime(2023, 10, 31);
        var result = SetNpqQualificationResult.Success();
        var trn = "1234567";
        var contact = new Contact()
        {
            dfeta_TRN = trn
        };

        ApiFixture.DataverseAdapter
           .Setup(mock => mock.GetTeacherByTrn(trn, /* columnNames: */ It.IsAny<string[]>(), /* activeOnly: */ true))
           .ReturnsAsync(contact);

        ApiFixture.DataverseAdapter
           .Setup(mock => mock.SetNpqQualification(It.IsAny<SetNpqQualificationCommand>()))
           .ReturnsAsync(result);

        // Act
        var response = await HttpClientWithApiKey.PutAsync(
            $"v2/npq-qualifications?trn={trn}",
            CreateRequest(req => req.CompletionDate = new DateOnly(Clock.UtcNow.Year - 1, Clock.UtcNow.Month, Clock.UtcNow.Day)));

        // Assert
        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);
    }

    private JsonContent CreateRequest(Action<SetNpqQualificationRequest> configureRequest = null)
    {
        var request = new SetNpqQualificationRequest()
        {
            CompletionDate = new DateOnly(Clock.UtcNow.Year, Clock.UtcNow.Month, Clock.UtcNow.Day),
            QualificationType = QualifiedTeachersApi.V2.ApiModels.QualificationType.NPQEL
        };
        configureRequest?.Invoke(request);
        return CreateJsonContent(request);
    }
}
