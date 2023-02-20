using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Moq;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.DataStore.Crm.Models;
using QualifiedTeachersApi.Properties;
using QualifiedTeachersApi.TestCommon;
using QualifiedTeachersApi.V2.ApiModels;
using QualifiedTeachersApi.V2.Requests;
using QualifiedTeachersApi.Validation;
using Xunit;

namespace QualifiedTeachersApi.Tests.V2.Operations;

public class UpdateTeacherTests : ApiTestBase
{
    public UpdateTeacherTests(ApiFixture apiFixture) : base(apiFixture)
    {
    }

    [Fact]
    public async Task Given_missing_initialteachertraining_providerukprn_returns_error()
    {
        // Arrange
        var trn = "1234567";

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate=1985-01-01",
            CreateRequest(req => req.InitialTeacherTraining.ProviderUkprn = ""));

        // Assert
        await AssertEx.ResponseIsValidationErrorForProperty(
            response,
            propertyName: $"{nameof(UpdateTeacherRequest.InitialTeacherTraining)}.{nameof(UpdateTeacherRequest.InitialTeacherTraining.ProviderUkprn)}",
            expectedError: "Initial TeacherTraining ProviderUkprn is required.");
    }

    [Fact]
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
        await AssertEx.ResponseIsValidationErrorForProperty(
            response,
            propertyName: "Birthdate",
            expectedError: "Birthdate is required.");
    }

    [Fact]
    public async Task Given_invalid_qualification_subject2_returns_error()
    {
        // Arrange
        var trn = "12345";
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var dob = new DateOnly(1987, 01, 01);

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
            .ReturnsAsync(UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.QualificationSubject2NotFound));

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob, /* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
            .ReturnsAsync(contactList);

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}",
            CreateRequest(req => req.Qualification.Subject2 = "some invalid subject"));

        // Assert
        await AssertEx.ResponseIsValidationErrorForProperty(response, $"{nameof(UpdateTeacherRequest.Qualification)}.{nameof(UpdateTeacherRequest.Qualification.Subject2)}", ErrorRegistry.SubjectNotFound().Title);
    }

    [Fact]
    public async Task Given_invalid_qualification_subject3_returns_error()
    {
        // Arrange
        var trn = "12345";
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var dob = new DateOnly(1987, 01, 01);

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
            .ReturnsAsync(UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.QualificationSubject3NotFound));

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob, /* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
            .ReturnsAsync(contactList);

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}",
            CreateRequest(req => req.Qualification.Subject3 = "some invalid"));

        // Assert
        await AssertEx.ResponseIsValidationErrorForProperty(response, $"{nameof(UpdateTeacherRequest.Qualification)}.{nameof(UpdateTeacherRequest.Qualification.Subject3)}", ErrorRegistry.SubjectNotFound().Title);
    }

    [Fact]
    public async Task Given_invalid_itt_provider_returns_error()
    {
        // Arrange
        var ukprn = "xxx";
        var trn = "12345";
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var dob = new DateOnly(1987, 01, 01);

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
            .ReturnsAsync(UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.NoMatchingIttRecord));

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob, /* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
            .ReturnsAsync(contactList);

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}",
            CreateRequest(req => req.InitialTeacherTraining.ProviderUkprn = ukprn));

        // Assert
        await AssertEx.ResponseIsValidationErrorForProperty(response, $"{nameof(UpdateTeacherRequest.InitialTeacherTraining)}.{nameof(UpdateTeacherRequest.InitialTeacherTraining.ProviderUkprn)}", ErrorRegistry.TeacherHasNoIncompleteIttRecord().Title);
    }

    [Fact]
    public async Task Given_invalid_itt_subject1_returns_error()
    {
        // Arrange
        var subject = "xxx";
        var trn = "123456";
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var dob = new DateOnly(1987, 01, 01);

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
                .ReturnsAsync(UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.Subject1NotFound));

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob,/* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
                .ReturnsAsync(contactList);

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}",
            CreateRequest(req => req.InitialTeacherTraining.Subject1 = subject));

        // Assert
        await AssertEx.ResponseIsValidationErrorForProperty(response, $"{nameof(UpdateTeacherRequest.InitialTeacherTraining)}.{nameof(UpdateTeacherRequest.InitialTeacherTraining.Subject1)}", ErrorRegistry.SubjectNotFound().Title);
    }

    [Fact]
    public async Task Given_invalid_itt_subject2_returns_error()
    {
        // Arrange
        var subject = "xxx";
        var trn = "12345";
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var dob = new DateOnly(1987, 01, 01);

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
                .ReturnsAsync(UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.Subject2NotFound));

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob, /* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
                .ReturnsAsync(contactList);

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}",
            CreateRequest(req => req.InitialTeacherTraining.Subject2 = subject));

        // Assert
        await AssertEx.ResponseIsValidationErrorForProperty(response, $"{nameof(UpdateTeacherRequest.InitialTeacherTraining)}.{nameof(UpdateTeacherRequest.InitialTeacherTraining.Subject2)}", ErrorRegistry.SubjectNotFound().Title);
    }

    [Fact]
    public async Task Given_invalid_itt_subject3_returns_error()
    {
        // Arrange
        var subject = "xxx";
        var trn = "12345";
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var dob = new DateOnly(1987, 01, 01);

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
                .ReturnsAsync(UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.Subject3NotFound));

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob, /* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
                .ReturnsAsync(contactList);

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}",
            CreateRequest(req => req.InitialTeacherTraining.Subject3 = subject));

        // Assert
        await AssertEx.ResponseIsValidationErrorForProperty(response, $"{nameof(UpdateTeacherRequest.InitialTeacherTraining)}.{nameof(UpdateTeacherRequest.InitialTeacherTraining.Subject3)}", ErrorRegistry.SubjectNotFound().Title);
    }

    [Fact]
    public async Task Given_invalid_itt_qualification_returns_error()
    {
        // Arrange
        var trn = "12345";
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var dob = new DateOnly(1987, 01, 01);
        var ittQualificationType = (IttQualificationType)(-1);

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
            .ReturnsAsync(UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.IttQualificationNotFound));

        ApiFixture.DataverseAdapter
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

    [Fact]
    public async Task Given_invalid_qualification_country_returns_error()
    {
        // Arrange
        var trn = "123456";
        var country = "some non existent country country";
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var dob = new DateOnly(1987, 01, 01);

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob, /* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
                .ReturnsAsync(contactList);

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
                .ReturnsAsync(UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.QualificationCountryNotFound));

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}",
            CreateRequest(req => req.Qualification.CountryCode = country));

        // Assert
        await AssertEx.ResponseIsValidationErrorForProperty(response, $"{nameof(UpdateTeacherRequest.Qualification)}.{nameof(UpdateTeacherRequest.Qualification.CountryCode)}", ErrorRegistry.CountryNotFound().Title);
    }

    [Fact]
    public async Task Given_invalid_qualification_type_returns_error()
    {
        // Arrange
        var trn = "123456";
        var heQualificationType = (HeQualificationType)(-1);
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var dob = new DateOnly(1987, 01, 01);

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob, /* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
                .ReturnsAsync(contactList);

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
                .ReturnsAsync(UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.QualificationNotFound));

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob:yyyy-MM-dd}",
            CreateRequest(req => req.Qualification.HeQualificationType = heQualificationType));

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Given_multiple_lookups_failed_returns_error()
    {
        // Arrange
        var subject = "xxx";
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var dob = new DateOnly(1987, 01, 01);
        var trn = "xxx";

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob,/* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
                .ReturnsAsync(contactList);

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
                .ReturnsAsync(UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.QualificationSubjectNotFound | UpdateTeacherFailedReasons.Subject2NotFound));

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}",
            CreateRequest(req => req.Qualification.Subject = subject));

        // Assert
        await AssertEx.ResponseIsValidationErrorForProperty(response, $"{nameof(UpdateTeacherRequest.Qualification)}.{nameof(UpdateTeacherRequest.Qualification.Subject)}", ErrorRegistry.SubjectNotFound().Title);
        await AssertEx.ResponseIsValidationErrorForProperty(response, $"{nameof(UpdateTeacherRequest.InitialTeacherTraining)}.{nameof(UpdateTeacherRequest.InitialTeacherTraining.Subject2)}", ErrorRegistry.SubjectNotFound().Title);
    }

    [Fact]
    public async Task Given_invalid_qualification_subject_returns_error()
    {
        // Arrange
        var subject = "xxx";
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var dob = new DateOnly(1987, 01, 01);
        var trn = "xxx";

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob,/* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
                .ReturnsAsync(contactList);

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
                .ReturnsAsync(UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.QualificationSubjectNotFound));

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}",
            CreateRequest(req => req.Qualification.Subject = subject));

        // Assert
        await AssertEx.ResponseIsValidationErrorForProperty(response, $"{nameof(UpdateTeacherRequest.Qualification)}.{nameof(UpdateTeacherRequest.Qualification.Subject)}", ErrorRegistry.SubjectNotFound().Title);
    }

    [Fact]
    public async Task Given_valid_update_without_qualification_succeeds()
    {
        // Arrange
        var trn = "123456";
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var result = UpdateTeacherResult.Success(Guid.NewGuid(), "some trn");
        var dob = new DateOnly(1987, 01, 01);

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob, /* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
                .ReturnsAsync(contactList);

        ApiFixture.DataverseAdapter
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

    [Fact]
    public async Task Given_valid_update_succeeds_return_nocontent()
    {
        // Arrange
        var subject = "xxx";
        var trn = "123456";
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var result = UpdateTeacherResult.Success(Guid.NewGuid(), "some trn");
        var dob = new DateOnly(1987, 01, 01);

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob, /* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
                .ReturnsAsync(contactList);

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
                .ReturnsAsync(result);

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}",
            CreateRequest(req => req.Qualification.Subject = subject));

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Theory]
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
        await AssertEx.ResponseIsValidationErrorForProperty(
            response,
            expectedErrorPropertyName,
            expectedErrorMessage);
    }

    [Fact]
    public async Task Given_a_teacher_that_does_not_exist_returns_notfound()
    {
        // Arrange
        var trn = "1234567";
        var dob = new DateOnly(1987, 01, 01);

        ApiFixture.DataverseAdapter
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

    [Fact]
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

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob,/* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
            .ReturnsAsync(contactList);

        var request = new HttpRequestMessage(HttpMethod.Patch, $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}")
        {
            Content = CreateJsonContent(requestBody)
        };

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.ResponseIsError(response, errorCode: 10002, expectedStatusCode: StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task Given_request_with_existing_husid_for_another_teacher_returns_error()
    {
        // Arrange
        var trn = "123456";
        var contact = new Contact() { Id = Guid.NewGuid() };
        var contactList = new[] { contact };
        var result = UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.DuplicateHusId);
        var dob = new DateOnly(1987, 01, 01);

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetTeachersByTrnAndDoB(trn, dob, /* activeOnly: */ It.IsAny<string[]>(), /* columnNames: */ true))
                .ReturnsAsync(contactList);

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.UpdateTeacher(It.IsAny<UpdateTeacherCommand>()))
                .ReturnsAsync(result);

        // Act
        var response = await HttpClientWithApiKey.PatchAsync(
            $"v2/teachers/update/{trn}?birthdate={dob.ToString("yyyy-MM-dd")}",
            CreateRequest());

        // Assert
        await AssertEx.ResponseIsValidationErrorForProperty(
            response,
            $"{nameof(GetOrCreateTrnRequest.HusId)}.{nameof(GetOrCreateTrnRequest.HusId)}",
            StringResources.Errors_10018_Title);
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
            HusId = new Random().NextInt64(2000000000000, 2999999999999).ToString()
        };

        configureRequest?.Invoke(request);

        return CreateJsonContent(request);
    }

    public static TheoryData<int?, int?, string, string> InvalidAgeCombinationsData { get; } = new()
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
