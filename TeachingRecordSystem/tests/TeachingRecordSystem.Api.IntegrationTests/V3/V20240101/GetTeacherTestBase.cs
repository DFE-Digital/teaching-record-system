using System.Text.Json;
using TeachingRecordSystem.Core.Dqt;
using static TeachingRecordSystem.TestCommon.TestData;

namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20240101;

public abstract class GetTeacherTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected async Task ValidRequestForTeacher_ReturnsExpectedContent(
        HttpClient httpClient,
        string baseUrl,
        Contact contact,
        (DateTime? QTSDate, string StatusDescription)? expectedQts,
        (DateTime? EYTSDate, string StatusDescription)? expectedEyts)
    {
        // Arrange
        await ConfigureMocks(contact);

        var request = new HttpRequestMessage(HttpMethod.Get, baseUrl);

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        var expectedJson = JsonSerializer.SerializeToNode(new
        {
            firstName = contact.FirstName,
            lastName = contact.LastName,
            middleName = contact.MiddleName,
            trn = contact.dfeta_TRN,
            dateOfBirth = contact.BirthDate?.ToString("yyyy-MM-dd"),
            nationalInsuranceNumber = contact.dfeta_NINumber,
            qts = new
            {
                awarded = expectedQts?.QTSDate?.ToString("yyyy-MM-dd"),
                certificateUrl = "/v3/certificates/qts",
                statusDescription = expectedQts?.StatusDescription
            },
            eyts = new
            {
                awarded = expectedEyts?.EYTSDate?.ToString("yyyy-MM-dd"),
                certificateUrl = "/v3/certificates/eyts",
                statusDescription = expectedEyts?.StatusDescription
            },
            email = contact.EMailAddress1
        })!;

        if (expectedQts == null)
        {
            expectedJson["qts"] = null;
        }

        if (expectedEyts == null)
        {
            expectedJson["eyts"] = null;
        }

        await AssertEx.JsonResponseEqualsAsync(
            response,
            expectedJson,
            StatusCodes.Status200OK);
    }

    protected async Task ValidRequestForTeacherWithMultiWordFirstName_ReturnsExpectedContent(
        HttpClient httpClient,
        string baseUrl,
        Contact contact,
        (DateTime? QtsDate, string StatusDescription)? expectedQts,
        (DateTime? EytsDate, string StatusDescription)? expectedEyts)
    {
        // Arrange
        await ConfigureMocks(contact);

        var request = new HttpRequestMessage(HttpMethod.Get, baseUrl);

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        var expectedJson = JsonSerializer.SerializeToNode(new
        {
            firstName = contact.dfeta_StatedFirstName,
            lastName = contact.dfeta_StatedLastName,
            middleName = contact.dfeta_StatedMiddleName,
            trn = contact.dfeta_TRN,
            dateOfBirth = contact.BirthDate?.ToString("yyyy-MM-dd"),
            nationalInsuranceNumber = contact.dfeta_NINumber,
            qts = new
            {
                awarded = expectedQts?.QtsDate?.ToString("yyyy-MM-dd"),
                certificateUrl = "/v3/certificates/qts",
                statusDescription = expectedQts?.StatusDescription
            },
            eyts = new
            {
                awarded = expectedEyts?.EytsDate?.ToString("yyyy-MM-dd"),
                certificateUrl = "/v3/certificates/eyts",
                statusDescription = expectedEyts?.StatusDescription
            },
            email = contact.EMailAddress1
        })!;

        if (expectedQts == null)
        {
            expectedJson["qts"] = null;
        }
        if (expectedEyts == null)
        {
            expectedJson["eyts"] = null;
        }

        await AssertEx.JsonResponseEqualsAsync(
            response,
            expectedJson,
            StatusCodes.Status200OK);
    }

    protected async Task ValidRequestForContactWithPendingNameChange_ReturnsPendingNameChangeTrue(
        HttpClient httpClient,
        string baseUrl,
        Contact contact)
    {
        // Arrange
        var changeOfNameSubject = await TestData.ReferenceDataCache.GetSubjectByTitleAsync("Change of Name");

        var incidents = new[]
        {
            new Incident()
            {
                CustomerId = contact.Id.ToEntityReference(Contact.EntityLogicalName),
                Title = "Name change request",
                SubjectId = changeOfNameSubject.Id.ToEntityReference(Subject.EntityLogicalName)
            }
        };

        await ConfigureMocks(contact, incidents: incidents);

        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}?include=PendingDetailChanges");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        Assert.True(jsonResponse.RootElement.GetProperty("pendingNameChange").GetBoolean());
    }

    protected async Task ValidRequestForContactWithPendingDateOfBirthChange_ReturnsPendingDateOfBirthChangeTrue(
        HttpClient httpClient,
        string baseUrl,
        Contact contact)
    {
        // Arrange
        var changeOfDateOfBirthSubject = await TestData.ReferenceDataCache.GetSubjectByTitleAsync("Change of Date of Birth");

        var incidents = new[]
        {
            new Incident()
            {
                CustomerId = contact.Id.ToEntityReference(Contact.EntityLogicalName),
                Title = "DOB change request",
                SubjectId = changeOfDateOfBirthSubject.Id.ToEntityReference(Subject.EntityLogicalName)
            }
        };

        await ConfigureMocks(contact, incidents: incidents);

        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}?include=PendingDetailChanges");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        Assert.True(jsonResponse.RootElement.GetProperty("pendingDateOfBirthChange").GetBoolean());
    }

    protected async Task ValidRequestWithPreviousNames_ReturnsExpectedPreviousNamesContent(
        HttpClient httpClient,
        string baseUrl,
        Contact contact)
    {
        // Arrange
        var updatedFirstName = TestData.GenerateFirstName();
        var updatedMiddleName = TestData.GenerateMiddleName();
        var updatedLastName = TestData.GenerateLastName();
        var updatedNames = new[]
        {
            (FirstName: updatedFirstName, MiddleName: updatedMiddleName, LastName: contact.LastName),
            (FirstName: updatedFirstName, MiddleName: updatedMiddleName, LastName: updatedLastName)
        };

        await ConfigureMocks(contact, updatedNames: updatedNames!);

        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}?include=PreviousNames");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var responsePreviousNames = jsonResponse.RootElement.GetProperty("previousNames");

        AssertEx.JsonObjectEquals(
            new[]
            {
                new
                {
                    firstName = updatedFirstName,
                    middleName = updatedMiddleName,
                    lastName = contact.LastName,
                },
                new
                {
                    firstName = contact.FirstName,
                    middleName = contact.MiddleName,
                    lastName = contact.LastName,
                }
            },
            responsePreviousNames);
    }

    protected async Task<Contact> CreateContact(
        QtsRegistration[]? qtsRegistrations = null,
        Qualification[]? qualifications = null,
        bool hasMultiWordFirstName = false)
    {
        var firstName = hasMultiWordFirstName ? $"{Faker.Name.First()} {Faker.Name.First()}" : Faker.Name.First();

        var person = await TestData.CreatePersonAsync(
            b =>
            {
                b.WithFirstName(firstName).WithTrn();

                foreach (var item in qtsRegistrations ?? Array.Empty<QtsRegistration>())
                {
                    b.WithQtsRegistration(item!.QtsDate, item!.TeacherStatusValue, item.CreatedOn, item!.EytsDate, item!.EytsStatusValue);
                }

                foreach (var item in qualifications ?? Array.Empty<Qualification>())
                {
                    b.WithQualification(item.QualificationId, item.Type, item.CompletionOrAwardDate, item.IsActive, item.HeQualificationValue, item.HeSubject1Value, item.HeSubject2Value, item.HeSubject3Value);
                }
            });

        return person.Contact;
    }

    private async Task ConfigureMocks(
        Contact contact,
        Incident[]? incidents = null,
        (string FirstName, string? MiddleName, string LastName)[]? updatedNames = null)
    {
        DataverseAdapterMock
            .Setup(mock => mock.GetTeacherByTrnAsync(contact.dfeta_TRN, /* columnNames: */ It.IsAny<string[]>(), /* activeOnly: */ true))
            .ReturnsAsync(contact);

        DataverseAdapterMock
            .Setup(mock => mock.GetIncidentsByContactIdAsync(contact.Id, IncidentState.Active, It.IsAny<string[]>()))
            .ReturnsAsync(incidents ?? Array.Empty<Incident>());

        using var ctx = new DqtCrmServiceContext(TestData.OrganizationService);
        var qtsRegs = ctx.dfeta_qtsregistrationSet
            .Where(c => c.GetAttributeValue<Guid>(dfeta_qtsregistration.Fields.dfeta_PersonId) == contact.Id)
            .ToArray();

        foreach (var updatedName in updatedNames ?? Array.Empty<(string, string?, string)>())
        {
            await TestData.UpdatePersonAsync(b => b.WithPersonId(contact.Id).WithUpdatedName(updatedName.FirstName, updatedName.MiddleName, updatedName.LastName));
            await Task.Delay(2000);
        }
    }
}
