// namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20240814;
//
// [Collection(nameof(DisableParallelization))]
// public class FindPersonByLastNameAndDateOfBirthTests : TestBase
// {
//     public FindPersonByLastNameAndDateOfBirthTests(HostFixture hostFixture)
//         : base(hostFixture)
//     {
//         XrmFakedContext.DeleteAllEntities<Contact>();
//         SetCurrentApiClient([ApiRoles.GetPerson]);
//     }
//
//     [Theory, RoleNamesData(except: [ApiRoles.GetPerson])]
//     public async Task Get_ClientDoesNotHavePermission_ReturnsForbidden(string[] roles)
//     {
//         // Arrange
//         SetCurrentApiClient(roles);
//
//         var findBy = "LastNameAndDateOfBirth";
//         var lastName = "Smith";
//         var dateOfBirth = new DateOnly(1990, 1, 1);
//
//         var person1 = await TestData.CreatePersonAsync(p => p.WithTrn().WithLastName(lastName).WithDateOfBirth(dateOfBirth));
//         var person2 = await TestData.CreatePersonAsync(p => p.WithTrn().WithLastName(lastName).WithDateOfBirth(dateOfBirth));
//
//         var request = new HttpRequestMessage(
//             HttpMethod.Get,
//             $"/v3/persons?findBy={findBy}&lastName={lastName}&dateOfBirth={dateOfBirth:yyyy-MM-dd}");
//
//         // Act
//         var response = await GetHttpClientWithApiKey().SendAsync(request);
//
//         // Assert
//         Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
//     }
//
//     [Theory]
//     [InlineData("", "Invalid matching policy.")]
//     [InlineData("BadFindBy", "The value 'BadFindBy' is not valid for FindBy.")]
//     public async Task Get_InvalidFindBy_ReturnsError(string findBy, string expectedErrorMessage)
//     {
//         // Arrange
//         var lastName = "Smith";
//         var dateOfBirth = "1990-01-01";
//
//         var request = new HttpRequestMessage(
//             HttpMethod.Get,
//             $"/v3/persons?findBy={findBy}&lastName={lastName}&dateOfBirth={dateOfBirth}");
//
//         // Act
//         var response = await GetHttpClientWithApiKey().SendAsync(request);
//
//         // Assert
//         await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, "findBy", expectedErrorMessage);
//     }
//
//     [Theory]
//     [InlineData("", "1990-01-01", "lastName", "A value is required when findBy is 'LastNameAndDateOfBirth'.")]
//     [InlineData("Smith", "", "dateOfBirth", "A value is required when findBy is 'LastNameAndDateOfBirth'.")]
//     public async Task Get_MissingPropertiesForFindBy_ReturnsError(
//         string lastName,
//         string dateOfBirth,
//         string expectedErrorPropertyName,
//         string expectedErrorMessage)
//     {
//         // Arrange
//         var request = new HttpRequestMessage(
//             HttpMethod.Get,
//             $"/v3/persons?findBy=LastNameAndDateOfBirth&lastName={lastName}&dateOfBirth={dateOfBirth}");
//
//         // Act
//         var response = await GetHttpClientWithApiKey().SendAsync(request);
//
//         // Assert
//         await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, expectedErrorPropertyName, expectedErrorMessage);
//     }
//
//     [Fact]
//     public async Task Get_ValidRequestWithMatchesOnLastName_ReturnsExpectedResponse()
//     {
//         // Arrange
//         var findBy = "LastNameAndDateOfBirth";
//         var lastName = "Smith";
//         var dateOfBirth = new DateOnly(1990, 1, 1);
//
//         var alertTypes = await TestData.ReferenceDataCache.GetAlertTypesAsync();
//         var alertType = alertTypes.Where(at => Api.V3.Constants.LegacyExposableSanctionCodes.Contains(at.DqtSanctionCode)).RandomOne();
//
//         var person1 = await TestData.CreatePersonAsync(p => p
//             .WithTrn()
//             .WithLastName(lastName)
//             .WithDateOfBirth(dateOfBirth)
//             .WithAlert(a => a.WithAlertTypeId(alertType.AlertTypeId).WithEndDate(null))
//             .WithInductionStatus(i => i.WithStatus(InductionStatus.Passed).WithStartDate(new(2022, 1, 1)).WithCompletedDate(new DateOnly(2023, 1, 1)))
//             .WithQts(holdsFrom: new(2021, 7, 1))
//             .WithEyts(eytsDate: new(2021, 8, 1), eytsStatusValue: "222"));
//
//         var person2 = await TestData.CreatePersonAsync(p => p
//             .WithTrn()
//             .WithLastName(lastName)
//             .WithDateOfBirth(dateOfBirth)
//             .WithAlert(a => a.WithAlertTypeId(alertType.AlertTypeId).WithEndDate(null)));
//
//         var request = new HttpRequestMessage(
//             HttpMethod.Get,
//             $"/v3/persons?findBy={findBy}&lastName={lastName}&dateOfBirth={dateOfBirth:yyyy-MM-dd}");
//
//         // Act
//         var response = await GetHttpClientWithApiKey().SendAsync(request);
//
//         // Assert
//         await AssertEx.JsonResponseEqualsAsync(
//             response,
//             new
//             {
//                 total = 2,
//                 query = new
//                 {
//                     findBy,
//                     lastName,
//                     dateOfBirth
//                 },
//                 results = new[]
//                 {
//                     new
//                     {
//                         trn = person1.Trn,
//                         dateOfBirth = person1.DateOfBirth,
//                         firstName = person1.FirstName,
//                         middleName = person1.MiddleName ?? "",
//                         lastName = person1.LastName,
//                         sanctions = new[]
//                         {
//                             new
//                             {
//                                 code = person1.Alerts.Single().AlertType!.DqtSanctionCode,
//                                 startDate = person1.Alerts.Single().StartDate
//                             }
//                         },
//                         previousNames = Array.Empty<object>(),
//                         inductionStatus = (object?)new
//                         {
//                             status = "Pass",
//                             statusDescription = "Pass"
//                         },
//                         qts = (object?)new
//                         {
//                             awarded = person1.QtsDate,
//                             statusDescription = "Qualified"
//                         },
//                         eyts = (object?)new
//                         {
//                             awarded = person1.EytsDate,
//                             statusDescription = "Early years professional status"
//                         }
//                     },
//                     new
//                     {
//                         trn = person2.Trn,
//                         dateOfBirth = person2.DateOfBirth,
//                         firstName = person2.FirstName,
//                         middleName = person2.MiddleName,
//                         lastName = person2.LastName,
//                         sanctions = new[]
//                         {
//                             new
//                             {
//                                 code = person2.Alerts.Single().AlertType!.DqtSanctionCode,
//                                 startDate = person2.Alerts.Single().StartDate
//                             }
//                         },
//                         previousNames = Array.Empty<object>(),
//                         inductionStatus = (object?)null,
//                         qts = (object?)null,
//                         eyts = (object?)null,
//                     }
//                 }
//             });
//     }
//
//     [Fact]
//     public async Task Get_ValidRequestWithMatchOnPreviousName_ReturnsExpectedPersons()
//     {
//         // Arrange
//         var findBy = "LastNameAndDateOfBirth";
//         var lastName = TestData.GenerateLastName();
//         var dateOfBirth = new DateOnly(1990, 1, 1);
//
//         var person1 = await TestData.CreatePersonAsync(p => p
//             .WithTrn()
//             .WithLastName(lastName)
//             .WithDateOfBirth(dateOfBirth));
//
//         var person2 = await TestData.CreatePersonAsync(p => p
//             .WithTrn()
//             .WithLastName(lastName)
//             .WithDateOfBirth(dateOfBirth));
//
//         var updatedLastName = TestData.GenerateChangedLastName(lastName);
//         await TestData.UpdatePersonAsync(p => p
//             .WithPersonId(person2.PersonId)
//             .WithUpdatedName(person2.FirstName, person2.MiddleName, updatedLastName));
//
//         var request = new HttpRequestMessage(
//             HttpMethod.Get,
//             $"/v3/persons?findBy={findBy}&lastName={lastName}&dateOfBirth={dateOfBirth:yyyy-MM-dd}");
//
//         // Act
//         var response = await GetHttpClientWithApiKey().SendAsync(request);
//
//         // Assert
//         var jsonResponse = await AssertEx.JsonResponseAsync(response);
//         var results = jsonResponse.RootElement.GetProperty("results").EnumerateArray();
//         Assert.Collection(
//             results,
//             person =>
//             {
//                 Assert.Equal(person1.Trn, person.GetProperty("trn").GetString());
//             },
//             person =>
//             {
//                 Assert.Equal(person2.Trn, person.GetProperty("trn").GetString());
//                 Assert.Equal(updatedLastName, person.GetProperty("lastName").GetString());
//                 var previousNames = person.GetProperty("previousNames").EnumerateArray();
//                 Assert.Collection(
//                     previousNames,
//                     pn =>
//                     {
//                         Assert.Equal(person2.FirstName, pn.GetProperty("firstName").GetString());
//                         Assert.Equal(person2.MiddleName, pn.GetProperty("middleName").GetString());
//                         Assert.Equal(person2.LastName, pn.GetProperty("lastName").GetString());
//                     });
//             });
//     }
//
//     [Fact]
//     public async Task Get_NonExposableSanctionCode_IsNotReturned()
//     {
//         // Arrange
//         var findBy = "LastNameAndDateOfBirth";
//         var lastName = "Smith";
//         var dateOfBirth = new DateOnly(1990, 1, 1);
//
//         var alertTypes = await TestData.ReferenceDataCache.GetAlertTypesAsync();
//         var alertType = alertTypes.Where(at => !Api.V3.Constants.LegacyExposableSanctionCodes.Contains(at.DqtSanctionCode)).RandomOne();
//
//         var person = await TestData.CreatePersonAsync(p => p
//             .WithTrn()
//             .WithLastName(lastName)
//             .WithDateOfBirth(dateOfBirth)
//             .WithAlert(a => a.WithAlertTypeId(alertType.AlertTypeId).WithEndDate(null)));
//
//         var request = new HttpRequestMessage(
//             HttpMethod.Get,
//             $"/v3/persons?findBy={findBy}&lastName={lastName}&dateOfBirth={dateOfBirth:yyyy-MM-dd}");
//
//         // Act
//         var response = await GetHttpClientWithApiKey().SendAsync(request);
//
//         // Assert
//         var jsonResponse = await AssertEx.JsonResponseAsync(response);
//         var responseSanctions = jsonResponse.RootElement.GetProperty("results").EnumerateArray().First().GetProperty("sanctions").EnumerateArray();
//         Assert.Empty(responseSanctions);
//     }
// }
