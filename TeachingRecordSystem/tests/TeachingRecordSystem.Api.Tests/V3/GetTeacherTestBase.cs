using System.Diagnostics;
using System.Text.Json;
using Microsoft.Xrm.Sdk;
using TeachingRecordSystem.Api.V3.ApiModels;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.Api.Tests.V3;

public abstract class GetTeacherTestBase : ApiTestBase
{
    private const string QualifiedTeacherTrainedTeacherStatusValue = "71";
    private const string QtsAwardedInWalesTeacherStatusValue = "213";
    private readonly Guid _qtsAwardedInWalesTeacherStatusId = Guid.NewGuid();

    private readonly Guid _changeOfNameSubjectId = Guid.NewGuid();
    private readonly Guid _changeOfDateOfBirthSubjectId = Guid.NewGuid();

    protected GetTeacherTestBase(ApiFixture apiFixture) : base(apiFixture)
    {
    }

    protected async Task ValidRequestForTeacher_ReturnsExpectedContent(
        HttpClient httpClient,
        string baseUrl,
        Contact contact,
        bool expectQtsCertificateUrl,
        bool expectEysCertificateUrl)
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
                awarded = contact.dfeta_QTSDate?.ToString("yyyy-MM-dd"),
                certificateUrl = "/v3/certificates/qts"
            },
            eyts = new
            {
                awarded = contact.dfeta_EYTSDate?.ToString("yyyy-MM-dd"),
                certificateUrl = "/v3/certificates/eyts"
            },
            email = contact.EMailAddress1
        })!;

        if (!expectQtsCertificateUrl)
        {
            expectedJson["qts"]?.AsObject().Remove("certificateUrl");
        }

        if (!expectEysCertificateUrl)
        {
            expectedJson["eyts"]?.AsObject().Remove("certificateUrl");
        }

        await AssertEx.JsonResponseEquals(
            response,
            expectedJson,
            StatusCodes.Status200OK);
    }

    protected async Task ValidRequestForTeacherWithMultiWordFirstName_ReturnsExpectedContent(
        HttpClient httpClient,
        string baseUrl,
        Contact contact,
        bool expectCertificateUrls)
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
                awarded = contact.dfeta_QTSDate?.ToString("yyyy-MM-dd"),
                certificateUrl = "/v3/certificates/qts"
            },
            eyts = new
            {
                awarded = contact.dfeta_EYTSDate?.ToString("yyyy-MM-dd"),
                certificateUrl = "/v3/certificates/eyts"
            },
            email = contact.EMailAddress1
        })!;

        if (!expectCertificateUrls)
        {
            expectedJson["qts"]?.AsObject().Remove("certificateUrl");
            expectedJson["eyts"]?.AsObject().Remove("certificateUrl");
        }

        await AssertEx.JsonResponseEquals(
            response,
            expectedJson,
            StatusCodes.Status200OK);
    }

    protected async Task ValidRequestWithInduction_ReturnsExpectedInductionContent(
        HttpClient httpClient,
        string baseUrl,
        Contact contact,
        bool expectCertificateUrls)
    {
        // Arrange
        var induction = CreateInduction();
        var inductionPeriods = CreateInductionPeriods();

        await ConfigureMocks(contact, induction: induction, inductionPeriods: inductionPeriods);

        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}?include=Induction");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        var expectedJson = JsonSerializer.SerializeToNode(new
        {
            startDate = induction.dfeta_StartDate?.ToString("yyyy-MM-dd"),
            endDate = induction.dfeta_CompletionDate?.ToString("yyyy-MM-dd"),
            status = induction.dfeta_InductionStatus?.ToString(),
            statusDescription = induction.dfeta_InductionStatus?.GetDescription(),
            certificateUrl = "/v3/certificates/induction",
            periods = new[]
            {
                new
                {
                    startDate = inductionPeriods[0].dfeta_StartDate?.ToString("yyyy-MM-dd"),
                    endDate = inductionPeriods[0].dfeta_EndDate?.ToString("yyyy-MM-dd"),
                    terms = inductionPeriods[0].dfeta_Numberofterms,
                    appropriateBody = new
                    {
                        name = inductionPeriods[0].GetAttributeValue<AliasedValue>($"appropriatebody.{Account.Fields.Name}").Value
                    }
                }
            }
        })!;

        if (!expectCertificateUrls)
        {
            expectedJson.AsObject().Remove("certificateUrl");
        }

        var jsonResponse = await AssertEx.JsonResponse(response);
        var responseInduction = jsonResponse.RootElement.GetProperty("induction");

        AssertEx.JsonObjectEquals(expectedJson, responseInduction);
    }

    protected async Task ValidRequestWithInitialTeacherTraining_ReturnsExpectedInitialTeacherTrainingContent(
        HttpClient httpClient,
        string baseUrl,
        Contact contact)
    {
        // Arrange
        var itt = CreateItt(contact);

        await ConfigureMocks(contact, itt);

        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}?include=InitialTeacherTraining");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponse(response);
        var responseItt = jsonResponse.RootElement.GetProperty("initialTeacherTraining");

        AssertEx.JsonObjectEquals(
            new[]
            {
                new
                {
                    qualification = new
                    {
                        name = itt.GetAttributeValue<AliasedValue>($"qualification.{dfeta_ittqualification.Fields.dfeta_name}").Value
                    },
                    programmeType = itt.dfeta_ProgrammeType.ToString(),
                    programmeTypeDescription = itt.dfeta_ProgrammeType?.ConvertToEnumByValue<dfeta_ITTProgrammeType, IttProgrammeType>().GetDescription(),
                    startDate = itt.dfeta_ProgrammeStartDate?.ToString("yyyy-MM-dd"),
                    endDate = itt.dfeta_ProgrammeEndDate?.ToString("yyyy-MM-dd"),
                    result = itt.dfeta_Result?.ToString(),
                    ageRange = new
                    {
                        description = "11 to 16 years"
                    },
                    provider = new
                    {
                        name = itt.GetAttributeValue<AliasedValue>($"establishment.{Account.Fields.Name}").Value,
                        ukprn = itt.GetAttributeValue<AliasedValue>($"establishment.{Account.Fields.dfeta_UKPRN}").Value
                    },
                    subjects = new[]
                    {
                        new
                        {
                            code = itt.GetAttributeValue<AliasedValue>($"subject1.{dfeta_ittsubject.Fields.dfeta_Value}").Value,
                            name = itt.GetAttributeValue<AliasedValue>($"subject1.{dfeta_ittsubject.Fields.dfeta_name}").Value
                        },
                        new
                        {
                            code = itt.GetAttributeValue<AliasedValue>($"subject2.{dfeta_ittsubject.Fields.dfeta_Value}").Value,
                            name = itt.GetAttributeValue<AliasedValue>($"subject2.{dfeta_ittsubject.Fields.dfeta_name}").Value
                        },
                        new
                        {
                            code = itt.GetAttributeValue<AliasedValue>($"subject3.{dfeta_ittsubject.Fields.dfeta_Value}").Value,
                            name = itt.GetAttributeValue<AliasedValue>($"subject3.{dfeta_ittsubject.Fields.dfeta_name}").Value
                        }
                    }
                }
            },
            responseItt);
    }

    protected async Task ValidRequestWithNpqQualifications_ReturnsExpectedNpqQualificationsContent(
        HttpClient httpClient,
        string baseUrl,
        Contact contact,
        bool expectCertificateUrls)
    {
        // Arrange
        var npqQualificationNoAwardDate = CreateQualification(dfeta_qualification_dfeta_Type.NPQLL, null, dfeta_qualificationState.Active, null);
        var npqQualificationInactive = CreateQualification(dfeta_qualification_dfeta_Type.NPQSL, new DateTime(2022, 5, 6), dfeta_qualificationState.Inactive, null);
        var npqQualificationValid = CreateQualification(dfeta_qualification_dfeta_Type.NPQEYL, new DateTime(2022, 3, 4), dfeta_qualificationState.Active, null);

        var qualifications = new dfeta_qualification[]
        {
            npqQualificationNoAwardDate,
            npqQualificationInactive,
            npqQualificationValid
        };

        await ConfigureMocks(contact, qualifications: qualifications);

        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}?include=NpqQualifications");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        var expectedJson = JsonSerializer.SerializeToNode(new[]
        {
            new
            {
                awarded = npqQualificationValid.dfeta_CompletionorAwardDate?.ToString("yyyy-MM-dd"),
                type = new
                {
                    code = npqQualificationValid.dfeta_Type.ToString(),
                    name = npqQualificationValid.dfeta_Type?.GetName()
                },
                certificateUrl = $"/v3/certificates/npq/{npqQualificationValid.Id}"
            }
        })!;

        if (!expectCertificateUrls)
        {
            expectedJson[0]?.AsObject().Remove("certificateUrl");
        }

        var jsonResponse = await AssertEx.JsonResponse(response);
        var responseNpqQualifications = jsonResponse.RootElement.GetProperty("npqQualifications");

        AssertEx.JsonObjectEquals(expectedJson, responseNpqQualifications);
    }

    protected async Task ValidRequestWithMandatoryQualifications_ReturnsExpectedMandatoryQualificationsContent(
        HttpClient httpClient,
        string baseUrl,
        Contact contact)
    {
        // Arrange
        var mandatoryQualificationNoAwardDate = CreateQualification(dfeta_qualification_dfeta_Type.MandatoryQualification, null, dfeta_qualificationState.Active, "Visual Impairment");
        var mandatoryQualificationNoSpecialism = CreateQualification(dfeta_qualification_dfeta_Type.MandatoryQualification, new DateTime(2022, 2, 3), dfeta_qualificationState.Active, null);
        var mandatoryQualificationValid = CreateQualification(dfeta_qualification_dfeta_Type.MandatoryQualification, new DateTime(2022, 4, 6), dfeta_qualificationState.Active, "Hearing");
        var mandatoryQualificationInactive = CreateQualification(dfeta_qualification_dfeta_Type.MandatoryQualification, new DateTime(2022, 4, 8), dfeta_qualificationState.Inactive, "Multi Sensory Impairment");

        var qualifications = new dfeta_qualification[]
        {
            mandatoryQualificationNoAwardDate,
            mandatoryQualificationNoSpecialism,
            mandatoryQualificationValid,
            mandatoryQualificationInactive
        };

        await ConfigureMocks(contact, qualifications: qualifications);

        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}?include=MandatoryQualifications");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponse(response);
        var responseMandatoryQualifications = jsonResponse.RootElement.GetProperty("mandatoryQualifications");

        AssertEx.JsonObjectEquals(
            new[]
            {
                new
                {
                    awarded = mandatoryQualificationValid.dfeta_MQ_Date?.ToString("yyyy-MM-dd"),
                    specialism = mandatoryQualificationValid.GetAttributeValue<AliasedValue>($"{dfeta_specialism.EntityLogicalName}.{dfeta_specialism.Fields.dfeta_name}").Value
                }
            },
            responseMandatoryQualifications);
    }

    protected async Task ValidRequestWithHigherEducationQualifications_ReturnsExpectedHigherEducationQualificationsContent(
        HttpClient httpClient,
        string baseUrl,
        Contact contact)
    {
        // Arrange
        var qualification1AwardDate = new DateTime(2022, 4, 6);
        var qualification1Name = "My HE Qual 1";
        var qualification1Subject1 = (Code: "Qualification1Subject1", Name: "Qualification 1 Subject 1");
        var qualification1Subject2 = (Code: "Qualification1Subject2", Name: "Qualification 1 Subject 2");
        var qualification1Subject3 = (Code: "Qualification1Subject3", Name: "Qualification 1 Subject 3");

        var qualification2AwardDate = new DateTime(2022, 4, 2);
        var qualification2Name = "My HE Qual 2";
        var qualification2Subject1 = (Code: "Qualification2Subject1", Name: "Qualification 2 Subject 1");

        var qualification3Name = "My HE Qual 3";
        var qualification3Subject1 = (Code: "Qualification3Subject1", Name: "Qualification 3 Subject 1");

        var qualification4AwardDate = new DateTime(2022, 4, 8);
        var qualification4Name = "My HE Qual 4";
        var qualification4Subject1 = (Code: "Qualification4Subject1", Name: "Qualification 4 Subject 1");
        var qualification4Subject2 = (Code: "Qualification4Subject2", Name: "Qualification 4 Subject 2");
        var qualification4Subject3 = (Code: "Qualification4Subject3", Name: "Qualification 4 Subject 3");

        var heQualificationWith3Subjects = CreateQualification(
            dfeta_qualification_dfeta_Type.HigherEducation,
            qualification1AwardDate,
            dfeta_qualificationState.Active,
            null,
            qualification1Name,
            qualification1Subject1,
            qualification1Subject2,
            qualification1Subject3);
        var heQualificationWith1Subject = CreateQualification(
            dfeta_qualification_dfeta_Type.HigherEducation,
            qualification2AwardDate,
            dfeta_qualificationState.Active,
            null,
            qualification2Name,
            qualification2Subject1);
        var heQualificationWithNoAwardDate = CreateQualification(
            dfeta_qualification_dfeta_Type.HigherEducation,
            null,
            dfeta_qualificationState.Active,
            null,
            qualification3Name,
            qualification3Subject1);
        var heQualificationInactive = CreateQualification(
            dfeta_qualification_dfeta_Type.HigherEducation,
            qualification4AwardDate,
            dfeta_qualificationState.Inactive,
            null,
            qualification4Name,
            qualification4Subject1,
            qualification4Subject2,
            qualification4Subject3);

        var qualifications = new dfeta_qualification[]
        {
            heQualificationWith3Subjects,
            heQualificationWith1Subject,
            heQualificationWithNoAwardDate,
            heQualificationInactive
        };

        await ConfigureMocks(contact, qualifications: qualifications);

        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}?include=HigherEducationQualifications");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponse(response);
        var responseHigherEducationQualifications = jsonResponse.RootElement.GetProperty("higherEducationQualifications");

        AssertEx.JsonObjectEquals(
            new[]
            {
                new
                {
                    name = qualification1Name,
                    awarded = (string?)qualification1AwardDate.ToString("yyyy-MM-dd"),
                    subjects = new[]
                    {
                        new { code = qualification1Subject1.Code, name = qualification1Subject1.Name },
                        new { code = qualification1Subject2.Code, name = qualification1Subject2.Name },
                        new { code = qualification1Subject3.Code, name = qualification1Subject3.Name }
                    }
                },
                new
                {
                    name = qualification2Name,
                    awarded = (string?)qualification2AwardDate.ToString("yyyy-MM-dd"),
                    subjects = new[]
                    {
                        new { code = qualification2Subject1.Code, name = qualification2Subject1.Name }
                    }
                },
                new
                {
                    name = qualification3Name,
                    awarded = (string?)null,
                    subjects = new[]
                    {
                        new { code = qualification3Subject1.Code, name = qualification3Subject1.Name }
                    }
                }
            },
            responseHigherEducationQualifications);
    }

    protected async Task ValidRequestForContactWithPendingNameChange_ReturnsPendingNameChangeTrue(
        HttpClient httpClient,
        string baseUrl,
        Contact contact)
    {
        // Arrange
        var incidents = new[]
        {
            new Incident()
            {
                CustomerId = contact.Id.ToEntityReference(Contact.EntityLogicalName),
                Title = "Name change request",
                SubjectId = _changeOfNameSubjectId.ToEntityReference(Subject.EntityLogicalName)
            }
        };

        await ConfigureMocks(contact, incidents: incidents);

        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}?include=PendingDetailChanges");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponse(response);
        Assert.True(jsonResponse.RootElement.GetProperty("pendingNameChange").GetBoolean());
    }

    protected async Task ValidRequestForContactWithPendingDateOfBirthChange_ReturnsPendingDateOfBirthChangeTrue(
        HttpClient httpClient,
        string baseUrl,
        Contact contact)
    {
        // Arrange
        var incidents = new[]
        {
            new Incident()
            {
                CustomerId = contact.Id.ToEntityReference(Contact.EntityLogicalName),
                Title = "DOB change request",
                SubjectId = _changeOfDateOfBirthSubjectId.ToEntityReference(Subject.EntityLogicalName)
            }
        };

        await ConfigureMocks(contact, incidents: incidents);

        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}?include=PendingDetailChanges");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponse(response);
        Assert.True(jsonResponse.RootElement.GetProperty("pendingDateOfBirthChange").GetBoolean());
    }

    protected async Task ValidRequestWithSanctions_ReturnsExpectedSanctionsContent(
        HttpClient httpClient,
        string baseUrl,
        Contact contact)
    {
        // Arrange
        var sanctions = new (string SanctionCode, DateOnly? StartDate)[]
        {
            new("A18", null),
            new("G1", new DateOnly(2022, 4, 1)),
        };
        Debug.Assert(sanctions.Select(s => s.SanctionCode).All(TeachingRecordSystem.Api.V3.Constants.ExposableSanctionCodes.Contains));

        await ConfigureMocks(contact, sanctions: sanctions);

        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}?include=Sanctions");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponse(response);
        var responseSanctions = jsonResponse.RootElement.GetProperty("sanctions");

        AssertEx.JsonObjectEquals(
            new[]
            {
                new
                {
                    code = sanctions[0].SanctionCode,
                    startDate = sanctions[0].StartDate
                },
                new
                {
                    code = sanctions[1].SanctionCode,
                    startDate = sanctions[1].StartDate
                }
            },
            responseSanctions);
    }

    protected async Task ValidRequestWithAlerts_ReturnsExpectedSanctionsContent(
        HttpClient httpClient,
        string baseUrl,
        Contact contact)
    {
        // Arrange
        var sanctions = new (string SanctionCode, DateOnly? StartDate)[]
        {
            new("B1", null),
            new("G1", new DateOnly(2022, 4, 1)),
        };
        Debug.Assert(sanctions.Select(s => s.SanctionCode).All(TeachingRecordSystem.Api.V3.Constants.ProhibitionSanctionCodes.Contains));

        await ConfigureMocks(contact, sanctions: sanctions);

        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}?include=Alerts");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponse(response);
        var responseAlerts = jsonResponse.RootElement.GetProperty("alerts");

        AssertEx.JsonObjectEquals(
            new[]
            {
                new
                {
                    alertType = "Prohibition",
                    dqtSanctionCode = sanctions[0].SanctionCode,
                    startDate = sanctions[0].StartDate,
                    endDate = (DateOnly?)null
                },
                new
                {
                    alertType = "Prohibition",
                    dqtSanctionCode = sanctions[1].SanctionCode,
                    startDate = sanctions[1].StartDate,
                    endDate = (DateOnly?)null
                }
            },
            responseAlerts);
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
        var jsonResponse = await AssertEx.JsonResponse(response);
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
        bool hasMultiWordFirstName = false,
        bool qualifiedInWales = false)
    {
        var qtsDate = new DateOnly(1997, 4, 23);
        var eytsDate = new DateOnly(1995, 5, 14);
        var firstName = hasMultiWordFirstName ? $"{Faker.Name.First()} {Faker.Name.First()}" : Faker.Name.First();

        var person = await TestData.CreatePerson(
            b => b.WithFirstName(firstName)
                .WithTrn()
                .WithQts(qtsDate, qualifiedInWales ? QtsAwardedInWalesTeacherStatusValue : QualifiedTeacherTrainedTeacherStatusValue)
                .WithEyts(eytsDate));

        return person.Contact;
    }

    private async Task ConfigureMocks(
        Contact contact,
        dfeta_initialteachertraining? itt = null,
        dfeta_induction? induction = null,
        dfeta_inductionperiod[]? inductionPeriods = null,
        dfeta_qualification[]? qualifications = null,
        Incident[]? incidents = null,
        (string FirstName, string? MiddleName, string LastName)[]? updatedNames = null,
        (string SanctionCode, DateOnly? StartDate)[]? sanctions = null)
    {
        DataverseAdapterMock
            .Setup(mock => mock.GetSubjectByTitle("Change of Name"))
            .ReturnsAsync(new Subject()
            {
                Id = _changeOfNameSubjectId,
                Title = "Change of Name"
            });

        DataverseAdapterMock
            .Setup(mock => mock.GetSubjectByTitle("Change of Date of Birth"))
            .ReturnsAsync(new Subject()
            {
                Id = _changeOfDateOfBirthSubjectId,
                Title = "Change of Date of Birth"
            });

        DataverseAdapterMock
            .Setup(mock => mock.GetTeacherByTrn(contact.dfeta_TRN, /* columnNames: */ It.IsAny<string[]>(), /* activeOnly: */ true))
            .ReturnsAsync(contact);

        DataverseAdapterMock
            .Setup(mock => mock.GetInitialTeacherTrainingByTeacher(
                contact.Id,
                It.IsAny<string[]>(),
                It.IsAny<string[]>(),
                It.IsAny<string[]>(),
                It.IsAny<string[]>(),
                /*activeOnly: */true))
            .ReturnsAsync(itt != null ? new[] { itt } : Array.Empty<dfeta_initialteachertraining>());

        DataverseAdapterMock
            .Setup(mock => mock.GetInductionByTeacher(
                contact.Id,
                It.IsAny<string[]>(),
                It.IsAny<string[]>(),
                It.IsAny<string[]>(),
                It.IsAny<string[]>()))
            .ReturnsAsync((induction, inductionPeriods));

        DataverseAdapterMock
             .Setup(mock => mock.GetQualificationsForTeacher(
                 contact.Id,
                 It.IsAny<string[]>(),
                 It.IsAny<string[]>(),
                 It.IsAny<string[]>(),
                 It.IsAny<string[]>()))
             .ReturnsAsync(qualifications ?? Array.Empty<dfeta_qualification>());

        DataverseAdapterMock
            .Setup(mock => mock.GetIncidentsByContactId(contact.Id, IncidentState.Active, It.IsAny<string[]>()))
            .ReturnsAsync(incidents ?? Array.Empty<Incident>());

        DataverseAdapterMock
            .Setup(mock => mock.GetTeacherStatus(
                It.Is<string>(s => s == QtsAwardedInWalesTeacherStatusValue),
                It.IsAny<RequestBuilder>()))
            .Returns(async (string s, RequestBuilder b) => await TestData.ReferenceDataCache.GetTeacherStatusByValue(s));

        using var ctx = new DqtCrmServiceContext(TestData.OrganizationService);
        var qtsRegistrations = ctx.dfeta_qtsregistrationSet
            .Where(c => c.GetAttributeValue<Guid>(dfeta_qtsregistration.Fields.dfeta_PersonId) == contact.Id)
            .ToArray();

        DataverseAdapterMock
            .Setup(mock => mock.GetQtsRegistrationsByTeacher(
                contact.Id,
                It.IsAny<string[]>()))
            .ReturnsAsync(qtsRegistrations ?? Array.Empty<dfeta_qtsregistration>());

        foreach (var sanction in sanctions ?? Array.Empty<(string, DateOnly?)>())
        {
            var sanctionCode = await TestData.ReferenceDataCache.GetSanctionCodeByValue(sanction.SanctionCode);

            await TestData.OrganizationService.CreateAsync(new dfeta_sanction()
            {
                Id = Guid.NewGuid(),
                dfeta_PersonId = contact.Id.ToEntityReference(Contact.EntityLogicalName),
                dfeta_SanctionCodeId = sanctionCode.Id.ToEntityReference(dfeta_sanctioncode.EntityLogicalName),
                dfeta_Spent = false,
                dfeta_StartDate = sanction.StartDate?.ToDateTimeWithDqtBstFix(isLocalTime: true)
            });
        }

        foreach (var updatedName in updatedNames ?? Array.Empty<(string, string?, string)>())
        {
            await TestData.UpdatePerson(b => b.WithPersonId(contact.Id).WithUpdatedName(updatedName.FirstName, updatedName.MiddleName, updatedName.LastName));
            await Task.Delay(2000);
        }
    }

    private static dfeta_initialteachertraining CreateItt(Contact teacher)
    {
        var ittStartDate = new DateOnly(2021, 9, 7);
        var ittEndDate = new DateOnly(2022, 7, 29);
        var ittProgrammeType = IttProgrammeType.EYITTGraduateEntry;
        var ittResult = IttOutcome.Pass;
        var ittAgeRangeFrom = dfeta_AgeRange._11;
        var ittAgeRangeTo = dfeta_AgeRange._16;
        var ittProviderName = Faker.Company.Name();
        var ittProviderUkprn = "12345";
        var ittTraineeId = "54321";
        var ittSubject1Value = "12345";
        var ittSubject1Name = "Subject 1";
        var ittSubject2Value = "23456";
        var ittSubject2Name = "Subject 2";
        var ittSubject3Value = "34567";
        var ittSubject3Name = "Subject 3";
        var ittQualificationName = "My test qualification 123";

        var itt = new dfeta_initialteachertraining()
        {
            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacher.Id),
            dfeta_ProgrammeStartDate = ittStartDate.ToDateTime(),
            dfeta_ProgrammeEndDate = ittEndDate.ToDateTime(),
            dfeta_ProgrammeType = ittProgrammeType.ConvertToIttProgrammeType(),
            dfeta_Result = ittResult.ConvertToITTResult(),
            dfeta_AgeRangeFrom = ittAgeRangeFrom,
            dfeta_AgeRangeTo = ittAgeRangeTo,
            dfeta_TraineeID = ittTraineeId,
            StateCode = dfeta_initialteachertrainingState.Active
        };

        itt.Attributes.Add($"qualification.{dfeta_ittqualification.PrimaryIdAttribute}", new AliasedValue(dfeta_ittqualification.EntityLogicalName, dfeta_ittqualification.PrimaryIdAttribute, Guid.NewGuid()));
        itt.Attributes.Add($"qualification.{dfeta_ittqualification.Fields.dfeta_name}", new AliasedValue(dfeta_ittqualification.EntityLogicalName, dfeta_ittqualification.Fields.dfeta_name, ittQualificationName));
        itt.Attributes.Add($"establishment.{Account.PrimaryIdAttribute}", new AliasedValue(Account.EntityLogicalName, Account.PrimaryIdAttribute, Guid.NewGuid()));
        itt.Attributes.Add($"establishment.{Account.Fields.Name}", new AliasedValue(Account.EntityLogicalName, Account.Fields.Name, ittProviderName));
        itt.Attributes.Add($"establishment.{Account.Fields.dfeta_UKPRN}", new AliasedValue(Account.EntityLogicalName, Account.Fields.dfeta_UKPRN, ittProviderUkprn));
        itt.Attributes.Add($"subject1.{dfeta_ittsubject.PrimaryIdAttribute}", new AliasedValue(dfeta_ittsubject.EntityLogicalName, dfeta_ittsubject.PrimaryIdAttribute, Guid.NewGuid()));
        itt.Attributes.Add($"subject1.{dfeta_ittsubject.Fields.dfeta_Value}", new AliasedValue(dfeta_ittsubject.EntityLogicalName, dfeta_ittsubject.Fields.dfeta_Value, ittSubject1Value));
        itt.Attributes.Add($"subject1.{dfeta_ittsubject.Fields.dfeta_name}", new AliasedValue(dfeta_ittsubject.EntityLogicalName, dfeta_ittsubject.Fields.dfeta_name, ittSubject1Name));
        itt.Attributes.Add($"subject2.{dfeta_ittsubject.PrimaryIdAttribute}", new AliasedValue(dfeta_ittsubject.EntityLogicalName, dfeta_ittsubject.PrimaryIdAttribute, Guid.NewGuid()));
        itt.Attributes.Add($"subject2.{dfeta_ittsubject.Fields.dfeta_Value}", new AliasedValue(dfeta_ittsubject.EntityLogicalName, dfeta_ittsubject.Fields.dfeta_Value, ittSubject2Value));
        itt.Attributes.Add($"subject2.{dfeta_ittsubject.Fields.dfeta_name}", new AliasedValue(dfeta_ittsubject.EntityLogicalName, dfeta_ittsubject.Fields.dfeta_name, ittSubject2Name));
        itt.Attributes.Add($"subject3.{dfeta_ittsubject.PrimaryIdAttribute}", new AliasedValue(dfeta_ittsubject.EntityLogicalName, dfeta_ittsubject.PrimaryIdAttribute, Guid.NewGuid()));
        itt.Attributes.Add($"subject3.{dfeta_ittsubject.Fields.dfeta_Value}", new AliasedValue(dfeta_ittsubject.EntityLogicalName, dfeta_ittsubject.Fields.dfeta_Value, ittSubject3Value));
        itt.Attributes.Add($"subject3.{dfeta_ittsubject.Fields.dfeta_name}", new AliasedValue(dfeta_ittsubject.EntityLogicalName, dfeta_ittsubject.Fields.dfeta_name, ittSubject3Name));

        return itt;
    }

    private static dfeta_induction CreateInduction()
    {
        var inductionStartDate = new DateOnly(1996, 2, 3);
        var inductionEndDate = new DateOnly(1996, 6, 7);
        var inductionStatus = dfeta_InductionStatus.Pass;

        return new dfeta_induction()
        {
            dfeta_StartDate = inductionStartDate.ToDateTime(),
            dfeta_CompletionDate = inductionEndDate.ToDateTime(),
            dfeta_InductionStatus = inductionStatus
        };
    }

    private static dfeta_inductionperiod[] CreateInductionPeriods()
    {
        var inductionPeriodStartDate = new DateOnly(1996, 2, 3);
        var inductionPeriodEndDate = new DateOnly(1996, 6, 7);
        var inductionPeriodTerms = 3;
        var inductionPeriodAppropriateBodyName = "My appropriate body";

        var inductionPeriod = new dfeta_inductionperiod()
        {
            dfeta_StartDate = inductionPeriodStartDate.ToDateTime(),
            dfeta_EndDate = inductionPeriodEndDate.ToDateTime(),
            dfeta_Numberofterms = inductionPeriodTerms
        };

        inductionPeriod.Attributes.Add($"appropriatebody.{Account.PrimaryIdAttribute}", new AliasedValue(Account.EntityLogicalName, Account.PrimaryIdAttribute, Guid.NewGuid()));
        inductionPeriod.Attributes.Add($"appropriatebody.{Account.Fields.Name}", new AliasedValue(Account.EntityLogicalName, Account.Fields.Name, inductionPeriodAppropriateBodyName));

        return new[]
        {
            inductionPeriod
        };
    }

    private static dfeta_qualification CreateQualification(
        dfeta_qualification_dfeta_Type type,
        DateTime? date,
        dfeta_qualificationState state,
        string? specialismName,
        string? heName = null,
        (string Code, string Name)? heSubject1 = null,
        (string Code, string Name)? heSubject2 = null,
        (string Code, string Name)? heSubject3 = null)
    {
        var qualification = new dfeta_qualification
        {
            Id = Guid.NewGuid(),
            dfeta_Type = type,
            StateCode = state
        };

        if (type == dfeta_qualification_dfeta_Type.MandatoryQualification)
        {
            qualification.dfeta_MQ_Date = date;
        }
        else
        {
            qualification.dfeta_CompletionorAwardDate = date;
        }

        if (type == dfeta_qualification_dfeta_Type.HigherEducation)
        {
            qualification.Attributes.Add($"{nameof(dfeta_hequalification)}.{dfeta_hequalification.PrimaryIdAttribute}", new AliasedValue(dfeta_hequalification.EntityLogicalName, dfeta_hequalification.PrimaryIdAttribute, Guid.NewGuid()));
            qualification.Attributes.Add($"{nameof(dfeta_hequalification)}.{dfeta_hequalification.Fields.dfeta_name}", new AliasedValue(dfeta_hequalification.EntityLogicalName, dfeta_hequalification.Fields.dfeta_name, heName));

            if (heSubject1 != null)
            {
                qualification.Attributes.Add($"{nameof(dfeta_hesubject)}1.{dfeta_hesubject.PrimaryIdAttribute}", new AliasedValue(dfeta_hesubject.EntityLogicalName, dfeta_hesubject.PrimaryIdAttribute, Guid.NewGuid()));
                qualification.Attributes.Add($"{nameof(dfeta_hesubject)}1.{dfeta_hesubject.Fields.dfeta_name}", new AliasedValue(dfeta_hesubject.EntityLogicalName, dfeta_hesubject.Fields.dfeta_name, heSubject1.Value.Name));
                qualification.Attributes.Add($"{nameof(dfeta_hesubject)}1.{dfeta_hesubject.Fields.dfeta_Value}", new AliasedValue(dfeta_hesubject.EntityLogicalName, dfeta_hesubject.Fields.dfeta_Value, heSubject1.Value.Code));
            }

            if (heSubject2 != null)
            {
                qualification.Attributes.Add($"{nameof(dfeta_hesubject)}2.{dfeta_hesubject.PrimaryIdAttribute}", new AliasedValue(dfeta_hesubject.EntityLogicalName, dfeta_hesubject.PrimaryIdAttribute, Guid.NewGuid()));
                qualification.Attributes.Add($"{nameof(dfeta_hesubject)}2.{dfeta_hesubject.Fields.dfeta_name}", new AliasedValue(dfeta_hesubject.EntityLogicalName, dfeta_hesubject.Fields.dfeta_name, heSubject2.Value.Name));
                qualification.Attributes.Add($"{nameof(dfeta_hesubject)}2.{dfeta_hesubject.Fields.dfeta_Value}", new AliasedValue(dfeta_hesubject.EntityLogicalName, dfeta_hesubject.Fields.dfeta_Value, heSubject2.Value.Code));
            }

            if (heSubject3 != null)
            {
                qualification.Attributes.Add($"{nameof(dfeta_hesubject)}3.{dfeta_hesubject.PrimaryIdAttribute}", new AliasedValue(dfeta_hesubject.EntityLogicalName, dfeta_hesubject.PrimaryIdAttribute, Guid.NewGuid()));
                qualification.Attributes.Add($"{nameof(dfeta_hesubject)}3.{dfeta_hesubject.Fields.dfeta_name}", new AliasedValue(dfeta_hesubject.EntityLogicalName, dfeta_hesubject.Fields.dfeta_name, heSubject3.Value.Name));
                qualification.Attributes.Add($"{nameof(dfeta_hesubject)}3.{dfeta_hesubject.Fields.dfeta_Value}", new AliasedValue(dfeta_hesubject.EntityLogicalName, dfeta_hesubject.Fields.dfeta_Value, heSubject3.Value.Code));
            }
        }

        if (specialismName is not null)
        {
            qualification.Attributes.Add($"{dfeta_specialism.EntityLogicalName}.{dfeta_specialism.PrimaryIdAttribute}", new AliasedValue(dfeta_specialism.EntityLogicalName, dfeta_specialism.PrimaryIdAttribute, Guid.NewGuid()));
            qualification.Attributes.Add($"{dfeta_specialism.EntityLogicalName}.{dfeta_specialism.Fields.dfeta_name}", new AliasedValue(dfeta_specialism.EntityLogicalName, dfeta_specialism.Fields.dfeta_name, specialismName));
        }

        return qualification;
    }
}
