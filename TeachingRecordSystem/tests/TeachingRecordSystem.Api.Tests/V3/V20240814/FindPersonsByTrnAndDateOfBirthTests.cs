using System.Diagnostics;

namespace TeachingRecordSystem.Api.Tests.V3.V20240814;

[Collection(nameof(DisableParallelization))]
public class FindPersonsByTrnAndDateOfBirthTests : TestBase
{
    public FindPersonsByTrnAndDateOfBirthTests(HostFixture hostFixture)
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

        var dateOfBirth = new DateOnly(1990, 1, 1);

        var person = await TestData.CreatePerson(b => b
            .WithDateOfBirth(dateOfBirth));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/v3/persons/find")
        {
            Content = JsonContent.Create(new
            {
                persons = new[]
                {
                    new
                    {
                        trn = person.Trn,
                        dateOfBirth = person.DateOfBirth
                    }
                }
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_TooManyPeopleRequested_ReturnsError()
    {
        // Arrange
        var limit = 500;

        var request = new HttpRequestMessage(HttpMethod.Post, $"/v3/persons/find")
        {
            Content = JsonContent.Create(new
            {
                persons = await Enumerable.Range(0, limit + 1)
                    .ToAsyncEnumerable()
                    .SelectAwait(async i => new
                    {
                        trn = await TestData.GenerateTrn(),
                        dateOfBirth = new DateOnly(1990, 1, 1).AddDays(i)
                    })
                    .ToArrayAsync()
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(response, "persons", "Only 500 persons or less can be specified.");
    }

    [Fact]
    public async Task Get_IncorrectDateOfBirth_DoesNotReturnRecord()
    {
        // Arrange
        var dateOfBirth = new DateOnly(1990, 1, 1);

        var person = await TestData.CreatePerson(b => b
            .WithDateOfBirth(dateOfBirth)
            .WithSanction("G1")
            .WithInduction(dfeta_InductionStatus.Pass, inductionExemptionReason: null, inductionStartDate: new(2022, 1, 1), completedDate: new DateOnly(2023, 1, 1))
            .WithQts(qtsDate: new(2021, 7, 1))
            .WithEyts(eytsDate: new(2021, 8, 1), eytsStatusValue: "222"));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/v3/persons/find")
        {
            Content = JsonContent.Create(new
            {
                persons = new[]
                {
                    new
                    {
                        trn = person.Trn,
                        dateOfBirth = person.DateOfBirth.AddDays(1)
                    }
                }
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            new
            {
                total = 0,
                results = Array.Empty<object>()
            });
    }

    [Fact]
    public async Task Get_ValidRequest_ReturnsMatchedRecord()
    {
        // Arrange
        var dateOfBirth = new DateOnly(1990, 1, 1);

        var person = await TestData.CreatePerson(b => b
            .WithDateOfBirth(dateOfBirth)
            .WithSanction("G1")
            .WithInduction(dfeta_InductionStatus.Pass, inductionExemptionReason: null, inductionStartDate: new(2022, 1, 1), completedDate: new DateOnly(2023, 1, 1))
            .WithQts(qtsDate: new(2021, 7, 1))
            .WithEyts(eytsDate: new(2021, 8, 1), eytsStatusValue: "222"));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/v3/persons/find")
        {
            Content = JsonContent.Create(new
            {
                persons = new[]
                {
                    new
                    {
                        trn = person.Trn,
                        dateOfBirth = person.DateOfBirth
                    }
                }
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            new
            {
                total = 1,
                results = new[]
                {
                    new
                    {
                        trn = person.Trn,
                        dateOfBirth = person.DateOfBirth,
                        firstName = person.FirstName,
                        middleName = person.MiddleName ?? "",
                        lastName = person.LastName,
                        sanctions = new[]
                        {
                            new
                            {
                                code = person.Sanctions.First().SanctionCode,
                                startDate = person.Sanctions.First().StartDate
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
                            awarded = person.QtsDate,
                            statusDescription = "Qualified"
                        },
                        eyts = (object?)new
                        {
                            awarded = person.EytsDate,
                            statusDescription = "Early years professional status"
                        }
                    }
                }
            });
    }

    [Fact]
    public async Task Get_NonExposableSanctionCode_IsNotReturned()
    {
        // Arrange
        var dateOfBirth = new DateOnly(1990, 1, 1);

        var sanctionCode = "A17";
        Debug.Assert(!Api.V3.Constants.LegacyExposableSanctionCodes.Contains(sanctionCode));
        var person = await TestData.CreatePerson(b => b
            .WithDateOfBirth(dateOfBirth)
            .WithSanction(sanctionCode));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/v3/persons/find")
        {
            Content = JsonContent.Create(new
            {
                persons = new[]
                {
                    new
                    {
                        trn = person.Trn,
                        dateOfBirth = person.DateOfBirth
                    }
                }
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponse(response);
        var responseSanctions = jsonResponse.RootElement.GetProperty("results").EnumerateArray().First().GetProperty("sanctions").EnumerateArray();
        Assert.Empty(responseSanctions);
    }
}
