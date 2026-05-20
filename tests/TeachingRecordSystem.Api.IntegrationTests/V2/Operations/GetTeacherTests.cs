// #nullable disable
// using TeachingRecordSystem.Api.Properties;
// using TeachingRecordSystem.Api.V2.ApiModels;
//
// namespace TeachingRecordSystem.Api.IntegrationTests.V2.Operations;
//
// public class GetTeacherTests : TestBase
// {
//     public GetTeacherTests(HostFixture hostFixture)
//         : base(hostFixture)
//     {
//         SetCurrentApiClient(new[] { ApiRoles.GetPerson, ApiRoles.UpdatePerson });
//     }
//
//     [Theory, RoleNamesData(new[] { ApiRoles.GetPerson, ApiRoles.UpdatePerson })]
//     public async Task GetTeacher_ClientDoesNotHavePermission_ReturnsForbidden(string[] roles)
//     {
//         // Arrange
//         SetCurrentApiClient(roles);
//         var trn = "1234567";
//
//         DataverseAdapterMock
//             .Setup(mock => mock.GetTeacherByTrnAsync(trn, It.IsAny<string[]>(), true))
//             .ReturnsAsync((Contact)null);
//
//         var request = new HttpRequestMessage(HttpMethod.Get, $"/v2/teachers/{trn}");
//
//         // Act
//         var response = await GetHttpClientWithApiKey().SendAsync(request);
//
//         // Assert
//         Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
//     }
//
//     [Theory]
//     [InlineData("123456")]
//     [InlineData("12345678")]
//     [InlineData("xxx")]
//     public async Task Given_invalid_trn_returns_error(string trn)
//     {
//         // Arrange
//         var request = new HttpRequestMessage(HttpMethod.Get, $"/v2/teachers/{trn}");
//
//         // Act
//         var response = await GetHttpClientWithApiKey().SendAsync(request);
//
//         // Assert
//         await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, "trn", expectedError: StringResources.ErrorMessages_TRNMustBe7Digits);
//     }
//
//     [Fact]
//     public async Task Given_no_match_found_returns_notfound()
//     {
//         // Arrange
//         var trn = "1234567";
//
//
//         DataverseAdapterMock
//             .Setup(mock => mock.GetTeacherByTrnAsync(trn, It.IsAny<string[]>(), true))
//             .ReturnsAsync((Contact)null);
//
//         var request = new HttpRequestMessage(HttpMethod.Get, $"/v2/teachers/{trn}");
//
//         // Act
//         var response = await GetHttpClientWithApiKey().SendAsync(request);
//
//         // Assert
//         Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
//     }
//
//     [Fact]
//     public async Task Given_match_returns_ok()
//     {
//         // Arrange
//         var trn = "1234567";
//         var birthDate = "1990-04-01";
//         var firstName = Faker.Name.First();
//         var lastName = Faker.Name.Last();
//         var middleName = Faker.Name.Middle();
//         var nino = Faker.Identification.UkNationalInsuranceNumber();
//         var qtsDate = (DateOnly?)null;
//         var eytsDate = (DateOnly?)new DateOnly(2022, 7, 7);
//         var teacherId = Guid.NewGuid();
//         var earlyYearsStatusId = Guid.NewGuid();
//         var earlyYearsStatusName = "Early Years Teacher Status";
//         var earlyYearsStatusValue = "221";
//         var ittStartDate = new DateOnly(2021, 9, 7);
//         var ittEndDate = new DateOnly(2022, 7, 29);
//         var ittProgrammeType = IttProgrammeType.EYITTGraduateEntry;
//         var ittResult = IttOutcome.Pass;
//         var ittProviderUkprn = "12345";
//         var ittTraineeId = "54321";
//         var husId = "987654";
//
//         var contact = new Contact()
//         {
//             Id = teacherId,
//             BirthDate = DateTime.Parse(birthDate),
//             FirstName = firstName,
//             LastName = lastName,
//             MiddleName = middleName,
//             dfeta_TRN = trn,
//             dfeta_NINumber = nino,
//             StateCode = ContactState.Active,
//             dfeta_EYTSDate = eytsDate.ToDateTime(),
//             dfeta_HUSID = husId
//         };
//
//         DataverseAdapterMock
//             .Setup(mock => mock.GetTeacherByTrnAsync(trn, /* columnNames: */ It.IsAny<string[]>(), /* activeOnly: */ true))
//             .ReturnsAsync(contact);
//
//         var request = new HttpRequestMessage(HttpMethod.Get, $"/v2/teachers/{trn}");
//
//         // Act
//         var response = await GetHttpClientWithApiKey().SendAsync(request);
//
//         // Assert
//         await AssertEx.JsonResponseEqualsAsync(
//             response,
//             new
//             {
//                 trn = trn,
//                 firstName = firstName,
//                 lastName = lastName,
//                 middleName = middleName,
//                 dateOfBirth = birthDate,
//                 nationalInsuranceNumber = nino,
//                 hasActiveSanctions = false,
//                 qtsDate = qtsDate?.ToString("yyyy-MM-dd"),
//                 eytsDate = eytsDate?.ToString("yyyy-MM-dd"),
//                 husId = husId,
//                 earlyYearsStatus = new
//                 {
//                     name = earlyYearsStatusName,
//                     value = earlyYearsStatusValue
//                 },
//                 initialTeacherTraining = Array.Empty<object>(),
//                 allowPIIUpdates = false
//             });
//     }
//
//     [Theory]
//     [InlineData(true, true)]
//     [InlineData(false, false)]
//     [InlineData(null, false)]
//     public async Task Given_match_returns_returns_ok_with_correct_allowPIIUpdates(bool? allowPiiUpdates, bool expectedAllowPiiUpdates)
//     {
//         // Arrange
//         var trn = "1234567";
//         var birthDate = "1990-04-01";
//         var firstName = Faker.Name.First();
//         var lastName = Faker.Name.Last();
//         var middleName = Faker.Name.Middle();
//         var nino = Faker.Identification.UkNationalInsuranceNumber();
//         var qtsDate = (DateOnly?)null;
//         var eytsDate = (DateOnly?)new DateOnly(2022, 7, 7);
//         var teacherId = Guid.NewGuid();
//         var husId = "987654";
//
//         var contact = new Contact()
//         {
//             Id = teacherId,
//             BirthDate = DateTime.Parse(birthDate),
//             FirstName = firstName,
//             LastName = lastName,
//             MiddleName = middleName,
//             dfeta_TRN = trn,
//             dfeta_NINumber = nino,
//             StateCode = ContactState.Active,
//             dfeta_EYTSDate = eytsDate.ToDateTime(),
//             dfeta_HUSID = husId,
//             dfeta_AllowPiiUpdatesFromRegister = allowPiiUpdates
//         };
//
//         DataverseAdapterMock
//             .Setup(mock => mock.GetTeacherByTrnAsync(trn, /* columnNames: */ It.IsAny<string[]>(), /* activeOnly: */ true))
//             .ReturnsAsync(contact);
//
//         var request = new HttpRequestMessage(HttpMethod.Get, $"/v2/teachers/{trn}");
//
//         // Act
//         var response = await GetHttpClientWithApiKey().SendAsync(request);
//
//         // Assert
//         await AssertEx.JsonResponseEqualsAsync(
//             response,
//             new
//             {
//                 trn = trn,
//                 firstName = firstName,
//                 lastName = lastName,
//                 middleName = middleName,
//                 dateOfBirth = birthDate,
//                 nationalInsuranceNumber = nino,
//                 hasActiveSanctions = false,
//                 qtsDate = qtsDate?.ToString("yyyy-MM-dd"),
//                 eytsDate = eytsDate?.ToString("yyyy-MM-dd"),
//                 husId = husId,
//                 earlyYearsStatus = (object)null,
//                 initialTeacherTraining = Array.Empty<object>(),
//                 allowPIIUpdates = expectedAllowPiiUpdates
//             });
//     }
// }
//
