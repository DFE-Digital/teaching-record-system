using System.Diagnostics;

namespace TeachingRecordSystem.Api.Tests.V3.V20240814;

[Collection(nameof(DisableParallelization))]
public class FindPersonByLastNameAndDateOfBirthTests : TestBase
{
    public FindPersonByLastNameAndDateOfBirthTests(HostFixture hostFixture)
        : base(hostFixture)
    {
        XrmFakedContext.DeleteAllEntities<Contact>();
        SetCurrentApiClient([ApiRoles.GetPerson]);
    }

    [Theory, RoleNamesData(except: [ApiRoles.GetPerson])]
    public async Task Get_ClientDoesNotHavePermission_ReturnsForbidden(string[] roles)
    {
        // Arrange
        SetCurrentApiClient(roles);

        var findBy = "LastNameAndDateOfBirth";
        var lastName = "Smith";
        var dateOfBirth = new DateOnly(1990, 1, 1);

        var person1 = await TestData.CreatePerson(p => p.WithTrn().WithLastName(lastName).WithDateOfBirth(dateOfBirth).WithSanction("G1"));
        var person2 = await TestData.CreatePerson(p => p.WithTrn().WithLastName(lastName).WithDateOfBirth(dateOfBirth).WithSanction("G1"));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/v3/persons?findBy={findBy}&lastName={lastName}&dateOfBirth={dateOfBirth:yyyy-MM-dd}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Theory]
    [InlineData("", "Invalid matching policy.")]
    [InlineData("BadFindBy", "The value 'BadFindBy' is not valid for FindBy.")]
    public async Task Get_InvalidFindBy_ReturnsError(string findBy, string expectedErrorMessage)
    {
        // Arrange
        var lastName = "Smith";
        var dateOfBirth = "1990-01-01";

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/v3/persons?findBy={findBy}&lastName={lastName}&dateOfBirth={dateOfBirth}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(response, "findBy", expectedErrorMessage);
    }

    [Theory]
    [InlineData("", "1990-01-01", "lastName", "A value is required when findBy is 'LastNameAndDateOfBirth'.")]
    [InlineData("Smith", "", "dateOfBirth", "A value is required when findBy is 'LastNameAndDateOfBirth'.")]
    public async Task Get_MissingPropertiesForFindBy_ReturnsError(
        string lastName,
        string dateOfBirth,
        string expectedErrorPropertyName,
        string expectedErrorMessage)
    {
        // Arrange
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/v3/persons?findBy=LastNameAndDateOfBirth&lastName={lastName}&dateOfBirth={dateOfBirth}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(response, expectedErrorPropertyName, expectedErrorMessage);
    }

    [Fact]
    public async Task Get_ValidRequestWithMatchesOnLastName_ReturnsExpectedResponse()
    {
        // Arrange
        var findBy = "LastNameAndDateOfBirth";
        var lastName = "Smith";
        var dateOfBirth = new DateOnly(1990, 1, 1);

        var person1 = await TestData.CreatePerson(p => p
            .WithTrn()
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithSanction("A21B")
            .WithDqtInduction(dfeta_InductionStatus.Pass, inductionExemptionReason: null, inductionStartDate: new(2022, 1, 1), completedDate: new DateOnly(2023, 1, 1))
            .WithQts(qtsDate: new(2021, 7, 1))
            .WithEyts(eytsDate: new(2021, 8, 1), eytsStatusValue: "222"));

        var person2 = await TestData.CreatePerson(p => p
            .WithTrn()
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithSanction("A21B"));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/v3/persons?findBy={findBy}&lastName={lastName}&dateOfBirth={dateOfBirth:yyyy-MM-dd}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            new
            {
                total = 2,
                query = new
                {
                    findBy,
                    lastName,
                    dateOfBirth
                },
                results = new[]
                {
                    new
                    {
                        trn = person1.Trn,
                        dateOfBirth = person1.DateOfBirth,
                        firstName = person1.FirstName,
                        middleName = person1.MiddleName ?? "",
                        lastName = person1.LastName,
                        sanctions = new[]
                        {
                            new
                            {
                                code = person1.Sanctions.First().SanctionCode,
                                startDate = person1.Sanctions.First().StartDate
                            }
                        },
                        previousNames = Array.Empty<object>(),
                        inductionStatus = (object?)new
                        {
                            status = "Pass",
                            statusDescription = "Pass"
                        },
                        qts = (object?)new
                        {
                            awarded = person1.QtsDate,
                            statusDescription = "Qualified"
                        },
                        eyts = (object?)new
                        {
                            awarded = person1.EytsDate,
                            statusDescription = "Early years professional status"
                        }
                    },
                    new
                    {
                        trn = person2.Trn,
                        dateOfBirth = person2.DateOfBirth,
                        firstName = person2.FirstName,
                        middleName = person2.MiddleName,
                        lastName = person2.LastName,
                        sanctions = new[]
                        {
                            new
                            {
                                code = person2.Sanctions.First().SanctionCode,
                                startDate = person2.Sanctions.First().StartDate
                            }
                        },
                        previousNames = Array.Empty<object>(),
                        inductionStatus = (object?)null,
                        qts = (object?)null,
                        eyts = (object?)null,
                    }
                }
            });
    }

    [Fact]
    public async Task Get_ValidRequestWithMatchOnPreviousName_ReturnsExpectedPersons()
    {
        // Arrange
        var findBy = "LastNameAndDateOfBirth";
        var lastName = TestData.GenerateLastName();
        var dateOfBirth = new DateOnly(1990, 1, 1);

        var person1 = await TestData.CreatePerson(p => p.WithTrn().WithLastName(lastName).WithDateOfBirth(dateOfBirth));
        var person2 = await TestData.CreatePerson(p => p.WithTrn().WithLastName(lastName).WithDateOfBirth(dateOfBirth));
        var updatedLastName = TestData.GenerateChangedLastName(lastName);
        await TestData.UpdatePerson(p => p.WithPersonId(person2.PersonId).WithUpdatedName(person2.FirstName, person2.MiddleName, updatedLastName));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/v3/persons?findBy={findBy}&lastName={lastName}&dateOfBirth={dateOfBirth:yyyy-MM-dd}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponse(response);
        var results = jsonResponse.RootElement.GetProperty("results").EnumerateArray();
        Assert.Collection(
            results,
            person =>
            {
                Assert.Equal(person1.Trn, person.GetProperty("trn").GetString());
            },
            person =>
            {
                Assert.Equal(person2.Trn, person.GetProperty("trn").GetString());
                Assert.Equal(updatedLastName, person.GetProperty("lastName").GetString());
                var previousNames = person.GetProperty("previousNames").EnumerateArray();
                Assert.Collection(
                    previousNames,
                    pn =>
                    {
                        Assert.Equal(person2.FirstName, pn.GetProperty("firstName").GetString());
                        Assert.Equal(person2.MiddleName, pn.GetProperty("middleName").GetString());
                        Assert.Equal(person2.LastName, pn.GetProperty("lastName").GetString());
                    });
            });
    }

    [Fact]
    public async Task Get_NonExposableSanctionCode_IsNotReturned()
    {
        // Arrange
        var findBy = "LastNameAndDateOfBirth";
        var lastName = "Smith";
        var dateOfBirth = new DateOnly(1990, 1, 1);

        var sanctionCode = "A17";
        Debug.Assert(!Api.V3.Constants.LegacyExposableSanctionCodes.Contains(sanctionCode));
        var person = await TestData.CreatePerson(p => p.WithTrn().WithLastName(lastName).WithDateOfBirth(dateOfBirth).WithSanction(sanctionCode));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/v3/persons?findBy={findBy}&lastName={lastName}&dateOfBirth={dateOfBirth:yyyy-MM-dd}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponse(response);
        var responseSanctions = jsonResponse.RootElement.GetProperty("results").EnumerateArray().First().GetProperty("sanctions").EnumerateArray();
        Assert.Empty(responseSanctions);
    }
}
