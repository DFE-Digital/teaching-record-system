// using System.Text.Json;
// using TeachingRecordSystem.Api.V3.Implementation.Dtos;
//
// namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20240606;
//
// public class GetPersonByTrnTests : GetPersonTestBase
// {
//     public GetPersonByTrnTests(HostFixture hostFixture) : base(hostFixture)
//     {
//         SetCurrentApiClient([ApiRoles.GetPerson]);
//     }
//
//     [Fact]
//     public async Task Get_UnauthenticatedRequest_ReturnsUnauthorized()
//     {
//         // Arrange
//         var trn = "1234567";
//
//         var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{trn}");
//
//         // Act
//         var response = await GetHttpClient().SendAsync(request);
//
//         // Assert
//         Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
//     }
//
//     [Theory, RoleNamesData(except: [ApiRoles.GetPerson])]
//     public async Task GetTeacher_ClientDoesNotHavePermission_ReturnsForbidden(string[] roles)
//     {
//         // Arrange
//         SetCurrentApiClient(roles);
//         var trn = "1234567";
//
//         var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{trn}");
//
//         // Act
//         var response = await GetHttpClientWithApiKey().SendAsync(request);
//
//         // Assert
//         Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
//     }
//
//     [Fact]
//     public async Task Get_TrnNotFound_ReturnsNotFound()
//     {
//         // Arrange
//         var trn = "1234567";
//
//         var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{trn}");
//
//         // Act
//         var response = await GetHttpClientWithApiKey().SendAsync(request);
//
//         // Assert
//         Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
//     }
//
//     [Fact]
//     public async Task Get_ValidRequest_ReturnsExpectedResponse()
//     {
//         var contact = await CreateContact();
//         var baseUrl = $"/v3/persons/{contact.dfeta_TRN}";
//
//         await ValidRequestForTeacher_ReturnsExpectedContent(GetHttpClientWithApiKey(), baseUrl, contact, qtsRegistrations: null, expectedQts: null, expectedEyts: null);
//     }
//
//     [Fact]
//     public async Task Get_ValidRequestForContactWithMultiWordFirstName_ReturnsExpectedResponse()
//     {
//         var contact = await CreateContact();
//         var baseUrl = $"/v3/persons/{contact.dfeta_TRN}";
//
//         await ValidRequestForTeacherWithMultiWordFirstName_ReturnsExpectedContent(GetHttpClientWithApiKey(), baseUrl, contact, qtsRegistrations: null, expectedQts: null, expectedEyts: null);
//     }
//
//     [Fact]
//     public async Task Get_ValidRequestWithInduction_ReturnsExpectedInductionContent()
//     {
//         // Arrange
//         var inductionStatus = InductionStatus.Passed;
//         var dqtStatus = dfeta_InductionStatus.Pass;
//         var startDate = new DateOnly(1996, 2, 3);
//         var completedDate = new DateOnly(1996, 6, 7);
//
//         var person = await TestData.CreatePersonAsync(p => p
//             .WithTrn()
//             .WithInductionStatus(i => i.WithStatus(inductionStatus).WithStartDate(startDate).WithCompletedDate(completedDate)));
//
//         // Arrange
//         var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?include=Induction");
//
//         // Act
//         var response = await GetHttpClientWithApiKey().SendAsync(request);
//
//         // Assert
//         var jsonResponse = await AssertEx.JsonResponseAsync(response);
//         var responseInduction = jsonResponse.RootElement.GetProperty("induction");
//
//         AssertEx.JsonObjectEquals(
//             new
//             {
//                 startDate = startDate.ToString("yyyy-MM-dd"),
//                 endDate = completedDate.ToString("yyyy-MM-dd"),
//                 status = dqtStatus.ToString(),
//                 statusDescription = dqtStatus.GetDescription(),
//                 certificateUrl = "/v3/certificates/induction",
//                 periods = Array.Empty<object>()
//             },
//             responseInduction);
//     }
//
//     [Fact]
//     public async Task Get_ValidRequestWithInductionAndPersonHasNullDqtStatus_ReturnsNullInductionContent()
//     {
//         // Arrange
//         var person = await TestData.CreatePersonAsync(p => p.WithTrn());
//
//         // Arrange
//         var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?include=Induction");
//
//         // Act
//         var response = await GetHttpClientWithApiKey().SendAsync(request);
//
//         // Assert
//         var jsonResponse = await AssertEx.JsonResponseAsync(response);
//         var responseInduction = jsonResponse.RootElement.GetProperty("induction");
//         Assert.Equal(JsonValueKind.Null, responseInduction.ValueKind);
//     }
//
//     [Fact]
//     public async Task Get_ValidRequestWithInitialTeacherTraining_ReturnsExpectedInitialTeacherTrainingContent()
//     {
//         var contact = await CreateContact();
//         var baseUrl = $"/v3/persons/{contact.dfeta_TRN}";
//
//         await ValidRequestWithInitialTeacherTraining_ReturnsExpectedInitialTeacherTrainingContent(GetHttpClientWithApiKey(), baseUrl, contact);
//     }
//
//     [Fact]
//     public async Task Get_ValidRequestWithMandatoryQualifications_ReturnsExpectedMandatoryQualificationsContent()
//     {
//         // Arrange
//         var person = await TestData.CreatePersonAsync(p => p
//             .WithTrn()
//             // MQ with no EndDate
//             .WithMandatoryQualification(b => b.WithStatus(MandatoryQualificationStatus.InProgress))
//             // MQ with no Specialism
//             .WithMandatoryQualification(b => b.WithSpecialism(null))
//             // MQ with EndDate and Specialism
//             .WithMandatoryQualification(b => b
//                 .WithStatus(MandatoryQualificationStatus.Passed, endDate: new(2022, 9, 1))
//                 .WithSpecialism(MandatoryQualificationSpecialism.Auditory)));
//
//         var validMq = person.MandatoryQualifications.Last();
//
//         // Arrange
//         var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?include=MandatoryQualifications");
//
//         // Act
//         var response = await GetHttpClientWithApiKey().SendAsync(request);
//
//         // Assert
//         var jsonResponse = await AssertEx.JsonResponseAsync(response);
//         var responseMandatoryQualifications = jsonResponse.RootElement.GetProperty("mandatoryQualifications");
//
//         AssertEx.JsonObjectEquals(
//             new[]
//             {
//                 new
//                 {
//                     awarded = validMq.EndDate?.ToString("yyyy-MM-dd"),
//                     specialism = validMq.Specialism?.GetTitle()
//                 }
//             },
//             responseMandatoryQualifications);
//     }
//
//     [Fact]
//     public async Task Get_ValidRequestForContactWithPendingNameChange_ReturnsPendingNameChangeTrue()
//     {
//         var contact = await CreateContact();
//         var baseUrl = $"/v3/persons/{contact.dfeta_TRN}";
//
//         await ValidRequestForContactWithPendingNameChange_ReturnsPendingNameChangeTrue(GetHttpClientWithApiKey(), baseUrl, contact);
//     }
//
//     [Fact]
//     public async Task Get_ValidRequestForContactWithPendingDateOfBirthChange_ReturnsPendingDateOfBirthChangeTrue()
//     {
//         var contact = await CreateContact();
//         var baseUrl = $"/v3/persons/{contact.dfeta_TRN}";
//
//         await ValidRequestForContactWithPendingDateOfBirthChange_ReturnsPendingDateOfBirthChangeTrue(GetHttpClientWithApiKey(), baseUrl, contact);
//     }
//
//     [Fact]
//     public async Task Get_ValidRequestWithSanctions_ReturnsExpectedSanctionsContent()
//     {
//         // Arrange
//         var alertTypes = await TestData.ReferenceDataCache.GetAlertTypesAsync();
//         var alertType = alertTypes.Where(at => Api.V3.Constants.LegacyExposableSanctionCodes.Contains(at.DqtSanctionCode)).RandomOne();
//
//         var person = await TestData.CreatePersonAsync(b => b
//             .WithTrn()
//             .WithAlert(a => a.WithAlertTypeId(alertType.AlertTypeId).WithEndDate(null)));
//
//         var alert = person.Alerts.Last();
//
//         // Arrange
//         var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?include=Sanctions");
//
//         // Act
//         var response = await GetHttpClientWithApiKey().SendAsync(request);
//
//         // Assert
//         var jsonResponse = await AssertEx.JsonResponseAsync(response);
//         var responseSanctions = jsonResponse.RootElement.GetProperty("sanctions");
//
//         AssertEx.JsonObjectEquals(
//             new[]
//             {
//                 new
//                 {
//                     code = alert.AlertType!.DqtSanctionCode,
//                     startDate = alert.StartDate
//                 }
//             },
//             responseSanctions);
//     }
//
//     [Fact]
//     public async Task Get_ValidRequestWithAlerts_ReturnsExpectedAlertsContent()
//     {
//         // Arrange
//         var alertTypes = await TestData.ReferenceDataCache.GetAlertTypesAsync();
//         var alertType = alertTypes.Where(at => Api.V3.Constants.LegacyProhibitionSanctionCodes.Contains(at.DqtSanctionCode)).RandomOne();
//
//         var person = await TestData.CreatePersonAsync(b => b
//             .WithTrn()
//             .WithAlert(a => a.WithAlertTypeId(alertType.AlertTypeId).WithEndDate(null)));
//
//         var alert = person.Alerts.Last();
//
//         // Arrange
//         var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?include=Alerts");
//
//         // Act
//         var response = await GetHttpClientWithApiKey().SendAsync(request);
//
//         // Assert
//         var jsonResponse = await AssertEx.JsonResponseAsync(response);
//         var responseAlerts = jsonResponse.RootElement.GetProperty("alerts");
//
//         AssertEx.JsonObjectEquals(
//             new[]
//             {
//                 new
//                 {
//                     alertType = "Prohibition",
//                     dqtSanctionCode = alert.AlertType!.DqtSanctionCode,
//                     startDate = alert.StartDate,
//                     endDate = alert.EndDate
//                 }
//             },
//             responseAlerts);
//     }
//
//     [Fact]
//     public async Task Get_ValidRequestWithPreviousNames_ReturnsExpectedPreviousNamesContent()
//     {
//         var contact = await CreateContact();
//         var baseUrl = $"/v3/persons/{contact.dfeta_TRN}";
//
//         await ValidRequestWithPreviousNames_ReturnsExpectedPreviousNamesContent(GetHttpClientWithApiKey(), baseUrl, contact);
//     }
//
//     [Fact]
//     public async Task Get_DateOfBirthDoesNotMatchTeachingRecord_ReturnsNotFound()
//     {
//         // Arrange
//         var person = await TestData.CreatePersonAsync(p => p.WithTrn());
//         var dateOfBirth = person.DateOfBirth.AddDays(1);
//
//         var httpClient = GetHttpClientWithApiKey();
//         var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?dateOfBirth={dateOfBirth:yyyy-MM-dd}");
//
//         // Act
//         var response = await httpClient.SendAsync(request);
//
//         // Assert
//         Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
//     }
//
//     [Fact]
//     public async Task Get_DateOfBirthMatchesTeachingRecord_ReturnsOk()
//     {
//         // Arrange
//         var person = await TestData.CreatePersonAsync(p => p.WithTrn());
//
//         var httpClient = GetHttpClientWithApiKey();
//         var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?dateOfBirth={person.DateOfBirth:yyyy-MM-dd}");
//
//         // Act
//         var response = await httpClient.SendAsync(request);
//
//         // Assert
//         Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
//     }
//
//     [Fact]
//     public async Task Get_DateOfBirthNotProvided_ReturnsOk()
//     {
//         // Arrange
//         var person = await TestData.CreatePersonAsync(p => p.WithTrn());
//
//         var httpClient = GetHttpClientWithApiKey();
//         var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}");
//
//         // Act
//         var response = await httpClient.SendAsync(request);
//
//         // Assert
//         Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
//     }
// }
