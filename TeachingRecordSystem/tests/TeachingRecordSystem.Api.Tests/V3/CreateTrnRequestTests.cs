using System.Net;
using TeachingRecordSystem.Api.Properties;
using TeachingRecordSystem.Api.V3.Requests;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.Api.Tests.V3;

[Collection(nameof(DisableParallelization))]
public class CreateTrnRequestTests : TestBase
{
    public CreateTrnRequestTests(HostFixture hostFixture)
    : base(hostFixture)
    {
        SetCurrentApiClient(new[] { ApiRoles.CreateTrn });
    }

    [Theory]
    [InlineData(ApiRoles.UnlockPerson)]
    [InlineData(ApiRoles.UpdateNpq)]
    [InlineData(ApiRoles.GetPerson)]
    public async Task Post_ClientDoesNotHavePermission_ReturnsForbidden(string role)
    {
        // Arrange
        SetCurrentApiClient(new[] { role });
        var requestId = new string('x', 101);  // Limit is 100
        var req = CreateRequest(r =>
        {
            r.RequestId = requestId;
        });

        // Act
        var response = await GetHttpClientWithApiKey().PostAsync($"v3/trn-requests", req);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Post_RequestWithInvalidId_ReturnsError()
    {
        // Arrange
        var requestId = "$";
        var req = CreateRequest(r =>
        {
            r.RequestId = requestId;
        });
        var request = new HttpRequestMessage(HttpMethod.Post, $"v3/trn-requests")
        {
            Content = req
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: nameof(CreateTrnRequestBody.RequestId),
            expectedError: Properties.StringResources.ErrorMessages_RequestIdCanOnlyContainCharactersDigitsUnderscoresAndDashes);
    }

    [Fact]
    public async Task Post_RequestIdExceedingCharacterLimit_ReturnsError()
    {
        // Arrange
        var requestId = new string('x', 101);  // Limit is 100
        var req = CreateRequest(r =>
        {
            r.RequestId = requestId;
        });

        // Act
        var response = await GetHttpClientWithApiKey().PostAsync($"v3/trn-requests", req);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: nameof(CreateTrnRequestBody.RequestId),
            expectedError: Properties.StringResources.ErrorMessages_RequestIdMustBe100CharactersOrFewer);
    }

    [Fact]
    public async Task Post__ValidRequestCreatesTeacherWithTrn_ReturnsCompleteStatus()
    {
        // Arrange
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();
        var requestId = Guid.NewGuid().ToString();
        var request = CreateRequest(req =>
        {
            req.RequestId = requestId;
            req.Person.FirstName = firstName;
            req.Person.MiddleName = middleName;
            req.Person.LastName = lastName;
            req.Person.DateOfBirth = dateOfBirth;
            req.Person.Email = email;
            req.Person.NationalInsuranceNumber = nationalInsuranceNumber;
        });

        // Act
        var response = await GetHttpClientWithApiKey().PostAsync($"v3/trn-requests", request);
        var contact = XrmFakedContext.CreateQuery<Contact>().Where(x => x.FirstName == firstName && x.MiddleName == middleName && x.LastName == lastName).FirstOrDefault();

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                requestId = requestId,
                Person = new
                {
                    FirstName = firstName,
                    MiddleName = middleName,
                    LastName = lastName,
                    Email = email,
                    DateOfBirth = dateOfBirth,
                    NationalInsuranceNumber = nationalInsuranceNumber,
                },
                trn = contact!.dfeta_TRN,
                status = "Completed"
            },
            expectedStatusCode: 200);
    }

    [Fact]
    public async Task Post_PotentialDuplicateRequest_ReturnsPendingStatus()
    {
        // Arrange
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();
        var existingTeacherId = Guid.NewGuid();
        var requestId = Guid.NewGuid().ToString();
        var request = CreateRequest(req =>
        {
            req.RequestId = requestId;
            req.Person.FirstName = firstName;
            req.Person.MiddleName = middleName;
            req.Person.LastName = lastName;
            req.Person.DateOfBirth = dateOfBirth;
            req.Person.Email = email;
            req.Person.NationalInsuranceNumber = nationalInsuranceNumber;
        });
        await TestData.CreatePerson(p =>
        {
            p.WithFirstName(firstName);
            p.WithMiddleName(middleName);
            p.WithLastName(lastName);
            p.WithDateOfBirth(dateOfBirth);
        });

        // Act
        var response = await GetHttpClientWithApiKey().PostAsync($"v3/trn-requests", request);

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                requestId = requestId,
                Person = new
                {
                    FirstName = firstName,
                    MiddleName = middleName,
                    LastName = lastName,
                    Email = email,
                    DateOfBirth = dateOfBirth,
                    NationalInsuranceNumber = nationalInsuranceNumber,
                },
                trn = "",
                status = "Pending"
            },
            expectedStatusCode: 200);
    }

    [Theory]
    [InlineData(1900, 1, 1)]
    public async Task Post_Before_1_1_1940_ReturnsError(int year, int month, int day)
    {
        // Arrange
        var dob = new DateOnly(year, month, day);
        var requestId = Guid.NewGuid().ToString();
        Clock.UtcNow = new DateTime(2022, 1, 1);

        var request = CreateRequest(cmd =>
        {
            cmd.Person.DateOfBirth = dob;
        });

        // Act
        var response = await GetHttpClientWithApiKey().PostAsync($"v3/trn-requests", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            "Person.DateOfBirth",
            StringResources.ErrorMessages_BirthDateIsOutOfRange);
    }

    [Theory]
    [InlineData(2022, 1, 1)]
    [InlineData(2023, 1, 1)]
    public async Task Post_RequestWithDOBEqualOrAfterToday_ReturnsError(int year, int month, int day)
    {
        // Arrange
        var dob = new DateOnly(year, month, day);
        var requestId = Guid.NewGuid().ToString();
        var request = CreateRequest(cmd =>
        {
            cmd.Person.DateOfBirth = dob;
        });

        // Act
        var response = await GetHttpClientWithApiKey().PostAsync($"v3/trn-requests", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            "Person.DateOfBirth",
            StringResources.ErrorMessages_BirthDateIsOutOfRange);
    }

    [Fact]
    public async Task Post_NationalInsuranceNumberExceedingMaxLength_ReturnsError()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var request = CreateRequest(cmd =>
        {
            cmd.Person.NationalInsuranceNumber = new string('x', 10);
        });

        // Act
        var response = await GetHttpClientWithApiKey().PostAsync($"v3/trn-requests", request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            "Person.NationalInsuranceNumber",
            StringResources.ErrorMessages_NationalInsuranceNumberMustBe9CharactersOrLess);
    }

    [Fact]
    public async Task Post_RequestWithExistingRequestId_ReturnsConflict()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var teacherId = Guid.NewGuid();
        var firstName = Faker.Name.First();
        var lastName = Faker.Name.Last();
        var middleName = Faker.Name.Middle();
        DateTime? dateOfBirth = new DateTime(1990, 5, 23);
        var email = "minnie.van.ryder@example.com";
        var nationalInsuranceNumber = "1234567D";
        var request = CreateRequest(req =>
        {
            req.RequestId = requestId;
            req.Person.FirstName = firstName;
            req.Person.LastName = lastName;
            req.Person.MiddleName = middleName;
            req.Person.Email = email;
            req.Person.NationalInsuranceNumber = nationalInsuranceNumber;
            req.Person.DateOfBirth = dateOfBirth!.Value.ToDateOnlyWithDqtBstFix(true);
        });
        await WithDbContext(async dbContext =>
        {
            dbContext.Add(new TrnRequest()
            {
                ClientId = ClientId,
                RequestId = requestId,
                TeacherId = teacherId
            });

            await dbContext.SaveChangesAsync();
        });

        // Act
        var response = await GetHttpClientWithApiKey().PostAsync($"v3/trn-requests", request);

        // Assert
        await AssertEx.JsonResponseIsError(response, expectedErrorCode: 10029, expectedStatusCode: StatusCodes.Status409Conflict);
    }

    private JsonContent CreateRequest(Action<CreateTrnRequestBody>? configureRequest = null)
    {
        var request = new CreateTrnRequestBody()
        {
            RequestId = Guid.NewGuid().ToString(),
            Person = new TrnRequestPerson()
            {
                FirstName = "Minnie",
                MiddleName = "Van",
                LastName = "Ryder",
                DateOfBirth = new(1990, 5, 23),
                Email = "minnie.van.ryder@example.com",
                NationalInsuranceNumber = "1234567D"
            }
        };

        configureRequest?.Invoke(request);

        return CreateJsonContent(request);
    }
}

