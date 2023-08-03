﻿#nullable disable
using System.Net;
using Optional;
using TeachingRecordSystem.Api.Properties;
using TeachingRecordSystem.Api.V2.ApiModels;
using TeachingRecordSystem.Api.V2.Requests;
using TeachingRecordSystem.Api.Validation;

namespace TeachingRecordSystem.Api.Tests.V2.Operations;

[TestClass]
public class UpdateTeacherTests : ApiTestBase
{
    public UpdateTeacherTests(ApiFixture apiFixture) : base(apiFixture)
    {
    }

    [Test]
    public async Task Given_missing_initialteachertraining_providerukprn_returns_error()
    {
        // Arrange
        var trn = "1234567";

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate=1985-01-01",
            CreateRequest(req => req.InitialTeacherTraining.ProviderUkprn = ""));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: $"{nameof(UpdateTeacherRequest.InitialTeacherTraining)}.{nameof(UpdateTeacherRequest.InitialTeacherTraining.ProviderUkprn)}",
            expectedError: "Initial TeacherTraining ProviderUkprn is required.");
    }

    [Test]
    public async Task Given_missing_birthdate_returns_error()
    {
        // Arrange
        var trn = "1234567";
        var ukprn = "123456";

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}",
            CreateRequest(req => req.InitialTeacherTraining.ProviderUkprn = ukprn));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: "Birthdate",
            expectedError: "Birthdate is required.");
    }

    [Test]
    public async Task Given_invalid_qualification_subject2_returns_error()
    {
        // Arrange
        var trn = "12345";
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var dob = new DateOnly(1987, 01, 01);

        DataverseAdapter
            .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
            .ReturnsAsync(UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.QualificationSubject2NotFound));

        DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob, /* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
            .ReturnsAsync(contactList);

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}",
            CreateRequest(req => req.Qualification.Subject2 = "some invalid subject"));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(response, $"{nameof(UpdateTeacherRequest.Qualification)}.{nameof(UpdateTeacherRequest.Qualification.Subject2)}", ErrorRegistry.SubjectNotFound().Title);
    }

    [Test]
    public async Task Given_invalid_qualification_subject3_returns_error()
    {
        // Arrange
        var trn = "12345";
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var dob = new DateOnly(1987, 01, 01);

        DataverseAdapter
            .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
            .ReturnsAsync(UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.QualificationSubject3NotFound));

        DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob, /* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
            .ReturnsAsync(contactList);

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}",
            CreateRequest(req => req.Qualification.Subject3 = "some invalid"));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(response, $"{nameof(UpdateTeacherRequest.Qualification)}.{nameof(UpdateTeacherRequest.Qualification.Subject3)}", ErrorRegistry.SubjectNotFound().Title);
    }


    [Test]
    public async Task Given_InitialTeacherTraining_is_empty_return_error()
    {
        // Arrange
        var trn = "12345";
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var dob = new DateOnly(1987, 01, 01);

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}",
            CreateRequest(req => req.InitialTeacherTraining = null));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(response, $"{nameof(UpdateTeacherRequest.InitialTeacherTraining)}", $"'Initial Teacher Training' must not be empty.");
    }

    [Test]
    public async Task Given_invalid_itt_provider_returns_error()
    {
        // Arrange
        var ukprn = "xxx";
        var trn = "12345";
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var dob = new DateOnly(1987, 01, 01);

        DataverseAdapter
            .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
            .ReturnsAsync(UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.NoMatchingIttRecord));

        DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob, /* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
            .ReturnsAsync(contactList);

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}",
            CreateRequest(req => req.InitialTeacherTraining.ProviderUkprn = ukprn));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(response, $"{nameof(UpdateTeacherRequest.InitialTeacherTraining)}.{nameof(UpdateTeacherRequest.InitialTeacherTraining.ProviderUkprn)}", ErrorRegistry.TeacherHasNoIncompleteIttRecord().Title);
    }

    [Test]
    public async Task Given_invalid_itt_subject1_returns_error()
    {
        // Arrange
        var subject = "xxx";
        var trn = "123456";
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var dob = new DateOnly(1987, 01, 01);

        DataverseAdapter
            .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
                .ReturnsAsync(UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.Subject1NotFound));

        DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob,/* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
                .ReturnsAsync(contactList);

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}",
            CreateRequest(req => req.InitialTeacherTraining.Subject1 = subject));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(response, $"{nameof(UpdateTeacherRequest.InitialTeacherTraining)}.{nameof(UpdateTeacherRequest.InitialTeacherTraining.Subject1)}", ErrorRegistry.SubjectNotFound().Title);
    }

    [Test]
    public async Task Given_invalid_itt_subject2_returns_error()
    {
        // Arrange
        var subject = "xxx";
        var trn = "12345";
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var dob = new DateOnly(1987, 01, 01);

        DataverseAdapter
            .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
                .ReturnsAsync(UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.Subject2NotFound));

        DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob, /* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
                .ReturnsAsync(contactList);

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}",
            CreateRequest(req => req.InitialTeacherTraining.Subject2 = subject));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(response, $"{nameof(UpdateTeacherRequest.InitialTeacherTraining)}.{nameof(UpdateTeacherRequest.InitialTeacherTraining.Subject2)}", ErrorRegistry.SubjectNotFound().Title);
    }

    [Test]
    public async Task Given_invalid_itt_subject3_returns_error()
    {
        // Arrange
        var subject = "xxx";
        var trn = "12345";
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var dob = new DateOnly(1987, 01, 01);

        DataverseAdapter
            .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
                .ReturnsAsync(UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.Subject3NotFound));

        DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob, /* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
                .ReturnsAsync(contactList);

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}",
            CreateRequest(req => req.InitialTeacherTraining.Subject3 = subject));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(response, $"{nameof(UpdateTeacherRequest.InitialTeacherTraining)}.{nameof(UpdateTeacherRequest.InitialTeacherTraining.Subject3)}", ErrorRegistry.SubjectNotFound().Title);
    }

    [Test]
    public async Task Given_invalid_itt_qualification_returns_error()
    {
        // Arrange
        var trn = "12345";
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var dob = new DateOnly(1987, 01, 01);
        var ittQualificationType = (IttQualificationType)(-1);

        DataverseAdapter
            .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
            .ReturnsAsync(UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.IttQualificationNotFound));

        DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob, /* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
                .ReturnsAsync(contactList);

        var request = CreateRequest(req => req.InitialTeacherTraining.IttQualificationType = ittQualificationType);

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob:yyyy-MM-dd)}",
            request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Test]
    public async Task Given_invalid_qualification_country_returns_error()
    {
        // Arrange
        var trn = "123456";
        var country = "some non existent country country";
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var dob = new DateOnly(1987, 01, 01);

        DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob, /* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
                .ReturnsAsync(contactList);

        DataverseAdapter
            .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
                .ReturnsAsync(UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.QualificationCountryNotFound));

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}",
            CreateRequest(req => req.Qualification.CountryCode = country));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(response, $"{nameof(UpdateTeacherRequest.Qualification)}.{nameof(UpdateTeacherRequest.Qualification.CountryCode)}", ErrorRegistry.CountryNotFound().Title);
    }

    [Test]
    public async Task Given_invalid_qualification_type_returns_error()
    {
        // Arrange
        var trn = "123456";
        var heQualificationType = (HeQualificationType)(-1);
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var dob = new DateOnly(1987, 01, 01);

        DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob, /* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
                .ReturnsAsync(contactList);

        DataverseAdapter
            .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
                .ReturnsAsync(UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.QualificationNotFound));

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob:yyyy-MM-dd}",
            CreateRequest(req => req.Qualification.HeQualificationType = heQualificationType));

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Test]
    public async Task Given_multiple_lookups_failed_returns_error()
    {
        // Arrange
        var subject = "xxx";
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var dob = new DateOnly(1987, 01, 01);
        var trn = "xxx";

        DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob,/* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
                .ReturnsAsync(contactList);

        DataverseAdapter
            .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
                .ReturnsAsync(UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.QualificationSubjectNotFound | UpdateTeacherFailedReasons.Subject2NotFound));

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}",
            CreateRequest(req => req.Qualification.Subject = subject));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorsForProperties(
            response,
            new Dictionary<string, string>()
            {
                { $"{nameof(UpdateTeacherRequest.Qualification)}.{nameof(UpdateTeacherRequest.Qualification.Subject)}", ErrorRegistry.SubjectNotFound().Title },
                { $"{nameof(UpdateTeacherRequest.InitialTeacherTraining)}.{nameof(UpdateTeacherRequest.InitialTeacherTraining.Subject2)}", ErrorRegistry.SubjectNotFound().Title }
            });
    }

    [Test]
    public async Task Given_invalid_qualification_subject_returns_error()
    {
        // Arrange
        var subject = "xxx";
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var dob = new DateOnly(1987, 01, 01);
        var trn = "xxx";

        DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob,/* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
                .ReturnsAsync(contactList);

        DataverseAdapter
            .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
                .ReturnsAsync(UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.QualificationSubjectNotFound));

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}",
            CreateRequest(req => req.Qualification.Subject = subject));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(response, $"{nameof(UpdateTeacherRequest.Qualification)}.{nameof(UpdateTeacherRequest.Qualification.Subject)}", ErrorRegistry.SubjectNotFound().Title);
    }

    [Test]
    public async Task Given_valid_update_without_qualification_succeeds()
    {
        // Arrange
        var trn = "123456";
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var result = UpdateTeacherResult.Success(Guid.NewGuid(), "some trn");
        var dob = new DateOnly(1987, 01, 01);

        DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob, /* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
                .ReturnsAsync(contactList);

        DataverseAdapter
            .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
                .ReturnsAsync(result);

        var request = CreateRequest(req => req.Qualification = null);

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob:yyyy-MM-dd}",
            request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }

    [Test]
    public async Task Given_qts_registration_not_matched_return_error()
    {
        // Arrange
        var subject = "xxx";
        var trn = "123456";
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var result = UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.NoMatchingQtsRecord);
        var dob = new DateOnly(1987, 01, 01);

        DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob, /* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
                .ReturnsAsync(contactList);

        DataverseAdapter
            .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
                .ReturnsAsync(result);

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}",
            CreateRequest(req => req.Qualification.Subject = subject));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(response, $"{nameof(UpdateTeacherRequest.InitialTeacherTraining)}.{nameof(UpdateTeacherRequest.InitialTeacherTraining.Outcome)}", StringResources.Errors_10006_Title);
    }

    [Test]
    public async Task Given_valid_update_with_slugid_return_nocontent()
    {
        // Arrange
        var slugid = Guid.NewGuid().ToString();
        var subject = "xxx";
        var trn = "123456";
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var result = UpdateTeacherResult.Success(Guid.NewGuid(), "some trn");
        var dob = new DateOnly(1987, 01, 01);

        DataverseAdapter
            .Setup(mock => mock.GetTeachersBySlugIdAndTrn(slugid, trn,  /* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
                .ReturnsAsync(contactList);

        DataverseAdapter
            .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
                .ReturnsAsync(result);

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}&slugid={slugid}",
            CreateRequest(req => req.Qualification.Subject = subject));

        // Assert
        DataverseAdapter
            .Verify(mock => mock.GetTeachersBySlugIdAndTrn(slugid, trn,/* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true), Times.Once);
        DataverseAdapter
            .Verify(mock => mock.GetTeachersByTrnAndDoB(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<string[]>(), /* columnNames: */ true /* activeOnly: */), Times.Never);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Test]
    public async Task Given_slugid_does_not_return_contact_fallback_to_trn_and_dob_return_nocontent()
    {
        // Arrange
        var slugid = Guid.NewGuid().ToString();
        var subject = "xxx";
        var trn = "123456";
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var result = UpdateTeacherResult.Success(Guid.NewGuid(), "some trn");
        var dob = new DateOnly(1987, 01, 01);

        DataverseAdapter
            .Setup(mock => mock.GetTeachersBySlugIdAndTrn(slugid, trn,  /* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
                .ReturnsAsync(Array.Empty<Contact>());

        DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob, /* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
                .ReturnsAsync(contactList);

        DataverseAdapter
            .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
                .ReturnsAsync(result);

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}&slugid={slugid}",
            CreateRequest(req => req.Qualification.Subject = subject));

        // Assert
        DataverseAdapter
            .Verify(mock => mock.GetTeachersBySlugIdAndTrn(slugid, trn,/* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true), Times.Once);
        DataverseAdapter
            .Verify(mock => mock.GetTeachersByTrnAndDoB(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<string[]>(), /* columnNames: */ true /* activeOnly: */), Times.Once);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Test]
    public async Task Given_valid_update_with_trn_and_dob_succeeds_return_nocontent()
    {
        // Arrange
        var subject = "xxx";
        var trn = "123456";
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var result = UpdateTeacherResult.Success(Guid.NewGuid(), "some trn");
        var dob = new DateOnly(1987, 01, 01);

        DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob, /* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
                .ReturnsAsync(contactList);

        DataverseAdapter
            .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
                .ReturnsAsync(result);

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}",
            CreateRequest(req => req.Qualification.Subject = subject));

        // Assert
        DataverseAdapter
            .Verify(mock => mock.GetTeachersByTrnAndDoB(trn, dob, It.IsAny<string[]>(), /* columnNames: */ true /* activeOnly: */), Times.Once);
        DataverseAdapter
            .Verify(mock => mock.GetTeachersBySlugIdAndTrn(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), /* columnNames: */ true /* activeOnly: */), Times.Never);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Test]
    public async Task Given_update_with_lastname_provided_without_firstname_return_error()
    {
        // Arrange
        var subject = "xxx";
        var trn = "123456";
        var dob = new DateOnly(1987, 01, 01);

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}",
            CreateRequest(req =>
            {
                req.Qualification.Subject = subject;
                req.LastName = Option.Some("lastname");
            }
        ));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(response, $"{nameof(UpdateTeacherRequest.FirstName)}", "'First Name' must not be empty.");
    }

    [Test]
    public async Task Given_update_with_firstname_provided_without_lastname_return_error()
    {
        // Arrange
        var subject = "xxx";
        var trn = "123456";
        var dob = new DateOnly(1987, 01, 01);

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}",
            CreateRequest(req =>
            {
                req.Qualification.Subject = subject;
                req.FirstName = Option.Some("FirstName");
            }
        ));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(response, $"{nameof(UpdateTeacherRequest.LastName)}", "'Last Name' must not be empty.");
    }

    [Test]
    public async Task Given_update_with_middlename_provided_without_lastname_return_error()
    {
        // Arrange
        var subject = "xxx";
        var trn = "123456";
        var dob = new DateOnly(1987, 01, 01);

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}",
            CreateRequest(req =>
            {
                req.Qualification.Subject = subject;
                req.MiddleName = Option.Some("SomeMiddleName");
                req.FirstName = Option.Some("FirstName");
            }
        ));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(response, $"{nameof(UpdateTeacherRequest.LastName)}", "'Last Name' must not be empty.");
    }

    [Test]
    public async Task Given_update_with_middlename_provided_without_firstname_return_error()
    {
        // Arrange
        var subject = "xxx";
        var trn = "123456";
        var dob = new DateOnly(1987, 01, 01);

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}",
            CreateRequest(req =>
            {
                req.Qualification.Subject = subject;
                req.MiddleName = Option.Some("SomeMiddleName");
                req.LastName = Option.Some("Lastname");
            }
        ));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(response, $"{nameof(UpdateTeacherRequest.FirstName)}", "'First Name' must not be empty.");
    }

    [Test]
    [MemberData(nameof(InvalidAgeCombinationsData))]
    public async Task Given_invalid_age_combination_returns_error(
        int? ageRangeFrom,
        int? ageRangeTo,
        string expectedErrorPropertyName,
        string expectedErrorMessage)
    {
        // Arrange
        var trn = "123456";
        var dob = new DateOnly(1980, 01, 01);
        var request = CreateRequest(cmd =>
        {
            cmd.InitialTeacherTraining.AgeRangeFrom = ageRangeFrom;
            cmd.InitialTeacherTraining.AgeRangeTo = ageRangeTo;
        });

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            expectedErrorPropertyName,
            expectedErrorMessage);
    }

    [Test]
    public async Task Given_a_teacher_that_does_not_exist_returns_notfound()
    {
        // Arrange
        var trn = "1234567";
        var dob = new DateOnly(1987, 01, 01);

        DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob,/* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
            .ReturnsAsync(Array.Empty<Contact>());

        var requestBody = new UpdateTeacherRequest()
        {
            InitialTeacherTraining = new UpdateTeacherRequestInitialTeacherTraining()
            {
                ProviderUkprn = "123456",
                ProgrammeStartDate = new DateOnly(2011, 11, 01),
                ProgrammeEndDate = new DateOnly(2012, 11, 01),
                ProgrammeType = IttProgrammeType.EYITTUndergraduate,
                Subject1 = "Mathematics"
            },
            Qualification = new UpdateTeacherRequestQualification()
            {
                ProviderUkprn = "123456",
                CountryCode = "XK",
                Subject = "Computer Science",
                Class = ClassDivision.Pass,
                Date = new DateOnly(2022, 01, 01)
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Patch, $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}")
        {
            Content = CreateJsonContent(requestBody)
        };

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Test]
    public async Task Given_a_two_teachers_have_the_same_trn_return_conflict()
    {
        // Arrange
        var trn = "1000000";

        var contact1 = new Contact();
        var contact2 = new Contact();
        var contactList = new[] { contact1, contact2 };
        var dob = new DateOnly(1987, 01, 01);

        var requestBody = new UpdateTeacherRequest()
        {
            InitialTeacherTraining = new UpdateTeacherRequestInitialTeacherTraining()
            {
                ProviderUkprn = "123456",
                ProgrammeStartDate = new DateOnly(2011, 11, 01),
                ProgrammeEndDate = new DateOnly(2012, 11, 01),
                ProgrammeType = IttProgrammeType.EYITTUndergraduate,
                Subject1 = "Mathematics",
                Subject2 = "Computer Science",
                AgeRangeFrom = 1,
                AgeRangeTo = 10
            },
            Qualification = new UpdateTeacherRequestQualification()
            {
                ProviderUkprn = "123456",
                CountryCode = "XK",
                Subject = "Computer Science",
                Class = ClassDivision.Pass,
                Date = new DateOnly(2022, 01, 01)
            }
        };

        DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob,/* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
            .ReturnsAsync(contactList);

        var request = new HttpRequestMessage(HttpMethod.Patch, $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}")
        {
            Content = CreateJsonContent(requestBody)
        };

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseIsError(response, expectedErrorCode: 10002, expectedStatusCode: StatusCodes.Status409Conflict);
    }

    [Test]
    public async Task Given_invalid_outcome_return_error()
    {
        // Arrange
        var trn = "1000000";

        var contact1 = new Contact();
        var contactList = new[] { contact1 };
        var dob = new DateOnly(1987, 01, 01);

        var requestBody = new UpdateTeacherRequest()
        {
            InitialTeacherTraining = new UpdateTeacherRequestInitialTeacherTraining()
            {
                ProviderUkprn = "123456",
                ProgrammeStartDate = new DateOnly(2011, 11, 01),
                ProgrammeEndDate = new DateOnly(2012, 11, 01),
                ProgrammeType = IttProgrammeType.EYITTUndergraduate,
                Subject1 = "Mathematics",
                Subject2 = "Computer Science",
                AgeRangeFrom = 1,
                AgeRangeTo = 10,
                Outcome = (IttOutcome)(-1)
            },
            Qualification = new UpdateTeacherRequestQualification()
            {
                ProviderUkprn = "123456",
                CountryCode = "XK",
                Subject = "Computer Science",
                Class = ClassDivision.Pass,
                Date = new DateOnly(2022, 01, 01)
            }
        };

        DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob,/* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
            .ReturnsAsync(contactList);

        var request = new HttpRequestMessage(HttpMethod.Patch, $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}")
        {
            Content = CreateJsonContent(requestBody)
        };

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(response, nameof(UpdateTeacherRequestInitialTeacherTraining.Outcome), StringResources.ErrorMessages_OutcomeMustBeDeferredInTrainingOrUnderAssessment);
    }


    [Test]
    public async Task Given_asessmentonlyroute_programmetype_then_intraining_outcome_is_not_permitted()
    {
        // Arrange
        var trn = "1000000";

        var contact1 = new Contact();
        var contactList = new[] { contact1 };
        var dob = new DateOnly(1987, 01, 01);

        var requestBody = new UpdateTeacherRequest()
        {
            InitialTeacherTraining = new UpdateTeacherRequestInitialTeacherTraining()
            {
                ProviderUkprn = "123456",
                ProgrammeStartDate = new DateOnly(2011, 11, 01),
                ProgrammeEndDate = new DateOnly(2012, 11, 01),
                ProgrammeType = IttProgrammeType.AssessmentOnlyRoute,
                Subject1 = "Mathematics",
                Subject2 = "Computer Science",
                AgeRangeFrom = 1,
                AgeRangeTo = 10,
                Outcome = IttOutcome.InTraining
            },
            Qualification = new UpdateTeacherRequestQualification()
            {
                ProviderUkprn = "123456",
                CountryCode = "XK",
                Subject = "Computer Science",
                Class = ClassDivision.Pass,
                Date = new DateOnly(2022, 01, 01)
            }
        };

        DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob,/* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
            .ReturnsAsync(contactList);

        var request = new HttpRequestMessage(HttpMethod.Patch, $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}")
        {
            Content = CreateJsonContent(requestBody)
        };

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(response, nameof(UpdateTeacherRequestInitialTeacherTraining.Outcome), StringResources.ErrorMessages_InTrainingOutcomeNotValidForAssessmentOnlyRoute);
    }

    [Test]
    [InlineData(IttProgrammeType.Apprenticeship)]
    [InlineData(IttProgrammeType.Core)]
    [InlineData(IttProgrammeType.CoreFlexible)]
    [InlineData(IttProgrammeType.EYITTGraduateEmploymentBased)]
    [InlineData(IttProgrammeType.EYITTGraduateEntry)]
    [InlineData(IttProgrammeType.EYITTSchoolDirectEarlyYears)]
    [InlineData(IttProgrammeType.HEI)]
    [InlineData(IttProgrammeType.FutureTeachingScholars)]
    [InlineData(IttProgrammeType.InternationalQualifiedTeacherStatus)]
    [InlineData(IttProgrammeType.OverseasTrainedTeacherProgramme)]
    [InlineData(IttProgrammeType.UndergraduateOptIn)]
    [InlineData(IttProgrammeType.LicensedTeacherProgramme)]
    [InlineData(IttProgrammeType.ProviderLedPostgrad)]
    public async Task Given_non_asessmentonlyroute_programmetypes_then_underassessment_outcome_is_not_permitted(IttProgrammeType programmeType)
    {
        // Arrange
        var trn = "1000000";

        var contact1 = new Contact();
        var contactList = new[] { contact1 };
        var dob = new DateOnly(1987, 01, 01);

        var requestBody = new UpdateTeacherRequest()
        {
            InitialTeacherTraining = new UpdateTeacherRequestInitialTeacherTraining()
            {
                ProviderUkprn = "123456",
                ProgrammeStartDate = new DateOnly(2011, 11, 01),
                ProgrammeEndDate = new DateOnly(2012, 11, 01),
                ProgrammeType = programmeType,
                Subject1 = "Mathematics",
                Subject2 = "Computer Science",
                AgeRangeFrom = 1,
                AgeRangeTo = 10,
                Outcome = IttOutcome.UnderAssessment
            },
            Qualification = new UpdateTeacherRequestQualification()
            {
                ProviderUkprn = "123456",
                CountryCode = "XK",
                Subject = "Computer Science",
                Class = ClassDivision.Pass,
                Date = new DateOnly(2022, 01, 01)
            }
        };

        DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob,/* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
            .ReturnsAsync(contactList);

        var request = new HttpRequestMessage(HttpMethod.Patch, $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}")
        {
            Content = CreateJsonContent(requestBody)
        };

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(response, nameof(UpdateTeacherRequestInitialTeacherTraining.Outcome), StringResources.ErrorMessages_UnderAssessmentOutcomeOnlyValidForAssessmentOnlyRoute);
    }

    [Test]
    public async Task Given_teacher_has_multiple_incomplete_itt_records_return_error()
    {
        // Arrange
        var trn = "123456";
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var ittRecords = new[] { new dfeta_initialteachertraining(), new dfeta_initialteachertraining() };
        var result = UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.MultipleInTrainingIttRecords);
        var dob = new DateOnly(1987, 01, 01);

        DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob, /* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
                .ReturnsAsync(contactList);


        DataverseAdapter
            .Setup(mock => mock.GetInitialTeacherTrainingByTeacher(It.IsAny<Guid>(), It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<string[]>(), true))
                .ReturnsAsync(ittRecords);

        DataverseAdapter
            .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
                .ReturnsAsync(result);

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}",
            CreateRequest());

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            $"{nameof(GetOrCreateTrnRequest.InitialTeacherTraining)}",
            StringResources.Errors_10004_Title);
    }

    [Test]
    public async Task Given_request_with_existing_husid_for_another_teacher_returns_error()
    {
        // Arrange
        var trn = "123456";
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var result = UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.DuplicateHusId);
        var dob = new DateOnly(1987, 01, 01);

        DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob, /* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
                .ReturnsAsync(contactList);

        DataverseAdapter
            .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
                .ReturnsAsync(result);

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}",
            CreateRequest());

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            $"{nameof(GetOrCreateTrnRequest.HusId)}.{nameof(GetOrCreateTrnRequest.HusId)}",
            StringResources.Errors_10018_Title);
    }

    [Test]
    public async Task Given_request_slugid_exceeding_maxlength_return_error()
    {
        // Arrange
        var trn = "123456";
        var slugId = new string('x', 155);  // Limit is 150
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var result = UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.DuplicateHusId);
        var dob = new DateOnly(1987, 01, 01);

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}&slugid={slugId}",
            CreateRequest());

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: nameof(UpdateTeacherRequest.SlugId),
            expectedError: Properties.StringResources.ErrorMessages_SlugIdMustBe150CharactersOrFewer);
    }

    [Test]
    public async Task Given_emailaddress_exceeeds_maxlength_returns_error()
    {
        // Arrange
        var trn = "1234567";
        var emailAddress = $"{new string('x', 99)}@test.com";  // Limit is 100

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate=1985-01-01",
            CreateRequest(req =>
            {
                req.EmailAddress = Option.Some(emailAddress);
            }
            ));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: $"{nameof(UpdateTeacherRequest.EmailAddress)}",
            expectedError: $"The length of 'Email Address' must be 100 characters or fewer. You entered {emailAddress.Length} characters.");
    }

    [Test]
    public async Task Given_firstname_exceeeds_maxlength_returns_error()
    {
        // Arrange
        var trn = "1234567";
        var firstName = new string('x', 150);

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate=1985-01-01",
            CreateRequest(req =>
            {
                req.FirstName = Option.Some(firstName);
            }
            ));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: $"{nameof(UpdateTeacherRequest.FirstName)}",
            expectedError: $"The length of 'First Name' must be 100 characters or fewer. You entered {firstName.Length} characters.");
    }

    [Test]
    public async Task Given_middlename_exceeeds_maxlength_returns_error()
    {
        // Arrange
        var trn = "1234567";
        var middleName = new string('x', 150);

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate=1985-01-01",
            CreateRequest(req =>
            {
                req.MiddleName = Option.Some(middleName);
            }
            ));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: $"{nameof(UpdateTeacherRequest.MiddleName)}",
            expectedError: $"The length of 'Middle Name' must be 100 characters or fewer. You entered {middleName.Length} characters.");
    }

    [Test]
    public async Task Given_lastname_exceeeds_maxlength_returns_error()
    {
        // Arrange
        var trn = "1234567";
        var lastName = new string('x', 150);

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate=1985-01-01",
            CreateRequest(req =>
            {
                req.LastName = Option.Some(lastName);
            }
            ));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: $"{nameof(UpdateTeacherRequest.LastName)}",
            expectedError: $"The length of 'Last Name' must be 100 characters or fewer. You entered {lastName.Length} characters.");
    }

    [Test]
    public async Task Given_firstname_empty_returns_error()
    {
        // Arrange
        var trn = "1234567";
        var firstName = string.Empty;

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate=1985-01-01",
            CreateRequest(req =>
            {
                req.FirstName = Option.Some(firstName);
            }
            ));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: $"{nameof(UpdateTeacherRequest.FirstName)}",
            expectedError: $"'First Name' must not be empty.");
    }

    [Test]
    public async Task Given_middlename_empty_returns_error()
    {
        // Arrange
        var trn = "1234567";
        var middleName = string.Empty;

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate=1985-01-01",
            CreateRequest(req =>
            {
                req.MiddleName = Option.Some(middleName);
            }
            ));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: $"{nameof(UpdateTeacherRequest.MiddleName)}",
            expectedError: $"'Middle Name' must not be empty.");
    }

    [Test]
    public async Task Given_lastname_empty_returns_error()
    {
        // Arrange
        var trn = "1234567";
        var lastname = string.Empty;

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate=1985-01-01",
            CreateRequest(req =>
            {
                req.LastName = Option.Some(lastname);
            }
            ));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: $"{nameof(UpdateTeacherRequest.LastName)}",
            expectedError: $"'Last Name' must not be empty.");
    }

    [Test]
    public async Task Given_invalid_email_returns_error()
    {
        // Arrange
        var trn = "1234567";
        var email = "invalid email";

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate=1985-01-01",
            CreateRequest(req =>
            {
                req.EmailAddress = Option.Some(email);
            }
            ));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: $"{nameof(UpdateTeacherRequest.EmailAddress)}",
            expectedError: $"'Email Address' is not a valid email address.");
    }

    [Test]
    public async Task Given_invalid_gendercode_returns_error()
    {
        // Arrange
        var trn = "1234567";
        var gender = ((Gender)(-1));

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate=1985-01-01",
            CreateRequest(req =>
            {
                req.GenderCode = Option.Some(gender);
            }
            ));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: $"{nameof(UpdateTeacherRequest.GenderCode)}",
            expectedError: $"'Gender Code' has a range of values which does not include '-1'.");
    }

    [Test]
    public async Task Given_invalid_dateofbirth_returns_error()
    {
        // Arrange
        var trn = "1234567";
        var dateonly = new DateOnly(01, 01, 01);

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate=1985-01-01",
            CreateRequest(req =>
            {
                req.DateOfBirth = Option.Some(dateonly);
            }
            ));

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: $"{nameof(UpdateTeacherRequest.DateOfBirth)}",
            StringResources.ErrorMessages_BirthDateIsOutOfRange);
    }

    [Test]
    public async Task Given_update_pii_request_returns_nocontent()
    {
        // Arrange
        DateOnly dateonly = new DateOnly(1998, 01, 01);
        string firstname = "Bob";
        string middlename = "bob";
        string lastname = "builder";
        string emailaddress = "bob.builder@test.com";
        Gender gender = Gender.Male;
        var trn = "123456";
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var result = UpdateTeacherResult.Success(Guid.NewGuid(), "some trn");
        var dob = new DateOnly(1987, 01, 01);

        DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob, /* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
                .ReturnsAsync(contactList);

        DataverseAdapter
            .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
                .ReturnsAsync(result);

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate=1987-01-01",
            CreateRequest(req =>
            {
                req.GenderCode = Option.Some(gender);
                req.DateOfBirth = Option.Some(dateonly);
                req.FirstName = Option.Some(firstname);
                req.MiddleName = Option.Some(middlename);
                req.LastName = Option.Some(lastname);
                req.EmailAddress = Option.Some(emailaddress);
            }
            ));

        // Assert
        DataverseAdapter
            .Verify(mock => mock.GetTeachersByTrnAndDoB(trn, dob, It.IsAny<string[]>(), /* columnNames: */ true /* activeOnly: */), Times.Once);
        DataverseAdapter
            .Verify(mock => mock.GetTeachersBySlugIdAndTrn(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), /* columnNames: */ true /* activeOnly: */), Times.Never);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private JsonContent CreateRequest(Action<UpdateTeacherRequest> configureRequest = null)
    {
        var request = new UpdateTeacherRequest()
        {
            InitialTeacherTraining = new()
            {
                ProviderUkprn = "10044534",
                ProgrammeStartDate = new(2020, 4, 1),
                ProgrammeEndDate = new(2020, 10, 10),
                ProgrammeType = IttProgrammeType.GraduateTeacherProgramme,
                Subject1 = "100366",  // computer science
                Subject2 = "100403",  // mathematics
                Subject3 = "100302",  // history
                AgeRangeFrom = 5,
                AgeRangeTo = 11,
                IttQualificationAim = IttQualificationAim.ProfessionalStatusAndAcademicAward
            },
            Qualification = new()
            {
                ProviderUkprn = "10044534",
                CountryCode = "UK",
                Subject = "Computing",
                Class = ClassDivision.FirstClassHonours,
                Date = new(2021, 5, 3),
                Subject2 = "X300", // Academic Studies in Education
                Subject3 = "N400"  // Accounting
            },
            HusId = new Random().NextInt64(2000000000000, 2999999999999).ToString(),
        };

        configureRequest?.Invoke(request);

        return CreateJsonContent(request);
    }

    public static TestArguments<int?, int?, string, string> InvalidAgeCombinationsData { get; } = new()
    {
        {
            -1,
            1,
            $"{nameof(GetOrCreateTrnRequest.InitialTeacherTraining)}.{nameof(GetOrCreateTrnRequest.InitialTeacherTraining.AgeRangeFrom)}",
            StringResources.ErrorMessages_AgeMustBe0To19Inclusive
        },
        {
            1,
            -1,
            $"{nameof(GetOrCreateTrnRequest.InitialTeacherTraining)}.{nameof(GetOrCreateTrnRequest.InitialTeacherTraining.AgeRangeTo)}",
            StringResources.ErrorMessages_AgeMustBe0To19Inclusive
        },
        {
            5,
            4,
            $"{nameof(GetOrCreateTrnRequest.InitialTeacherTraining)}.{nameof(GetOrCreateTrnRequest.InitialTeacherTraining.AgeRangeTo)}",
            StringResources.ErrorMessages_AgeToCannotBeLessThanAgeFrom
        }
    };
}
