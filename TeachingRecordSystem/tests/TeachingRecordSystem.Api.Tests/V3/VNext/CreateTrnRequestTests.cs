using System.Net;
using TeachingRecordSystem.Api.Properties;
using TeachingRecordSystem.Api.V3.VNext.ApiModels;
using TeachingRecordSystem.Api.V3.VNext.Requests;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.Tests.V3.VNext;

[Collection(nameof(DisableParallelization))]
public class CreateTrnRequestTests : TestBase
{
    public CreateTrnRequestTests(HostFixture hostFixture)
        : base(hostFixture)
    {
        SetCurrentApiClient(new[] { ApiRoles.CreateTrn });
    }

    [Theory, RoleNamesData(except: ApiRoles.CreateTrn)]
    public async Task Post_ClientDoesNotHavePermission_ReturnsForbidden(string[] roles)
    {
        // Arrange
        SetCurrentApiClient(roles);

        var requestId = new string('x', 101);  // Limit is 100
        var requestBody = CreateJsonContent(CreateDummyRequest() with { RequestId = requestId });

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Post_RequestWithInvalidId_ReturnsError()
    {
        // Arrange
        var requestId = "$";
        var requestBody = CreateJsonContent(CreateDummyRequest() with { RequestId = requestId });

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: nameof(CreateTrnRequestRequest.RequestId),
            expectedError: StringResources.ErrorMessages_RequestIdCanOnlyContainCharactersDigitsUnderscoresAndDashes);
    }

    [Fact]
    public async Task Post_RequestIdExceedingCharacterLimit_ReturnsError()
    {
        // Arrange
        var requestId = new string('x', 101);  // Limit is 100
        var requestBody = CreateJsonContent(CreateDummyRequest() with { RequestId = requestId });

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: nameof(CreateTrnRequestRequest.RequestId),
            expectedError: StringResources.ErrorMessages_RequestIdMustBe100CharactersOrFewer);
    }

    [Theory]
    [InlineData(1900, 1, 1)]
    public async Task Post_DateOfBirthBefore01011940_ReturnsError(int year, int month, int day)
    {
        // Arrange
        var requestBody = CreateJsonContent(CreateDummyRequest() with
        {
            Person = CreateDummyRequestPerson() with { DateOfBirth = new DateOnly(year, month, day) }
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            "Person.DateOfBirth",
            StringResources.ErrorMessages_BirthDateIsOutOfRange);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task Post_RequestWithDateOfBirthEqualOrAfterToday_ReturnsError(int daysAfterToday)
    {
        // Arrange
        var dateOfBirth = Clock.Today.AddDays(daysAfterToday);

        var requestBody = CreateJsonContent(CreateDummyRequest() with
        {
            Person = CreateDummyRequestPerson() with { DateOfBirth = dateOfBirth }
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

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
        var nationalInsuranceNumber = new string('x', 10);

        var requestBody = CreateJsonContent(CreateDummyRequest() with
        {
            Person = CreateDummyRequestPerson() with { NationalInsuranceNumber = nationalInsuranceNumber }
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

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
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

        var existingContact = await TestData.CreatePerson(p => p
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(email)
            .WithNationalInsuranceNumber(nationalInsuranceNumber: nationalInsuranceNumber));

        await WithDbContext(async dbContext =>
        {
            dbContext.Add(new TrnRequest()
            {
                ClientId = ClientId,
                RequestId = requestId,
                TeacherId = existingContact.ContactId
            });

            await dbContext.SaveChangesAsync();
        });

        var requestBody = CreateJsonContent(CreateDummyRequest() with
        {
            RequestId = requestId,
            Person = new()
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                Email = email,
                NationalInsuranceNumber = nationalInsuranceNumber,
            }
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseIsError(response, expectedErrorCode: 10029, expectedStatusCode: StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task Post_NotMatchedToExistingRecord_CreatesTeacherWithTrnAndReturnsCompletedStatus()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

        var requestBody = CreateJsonContent(CreateDummyRequest() with
        {
            RequestId = requestId,
            Person = new()
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                Email = email,
                NationalInsuranceNumber = nationalInsuranceNumber,
            }
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var (_, createdContactId) = CrmQueryDispatcherSpy.GetSingleQuery<CreateContactQuery, Guid>();
        var contact = XrmFakedContext.CreateQuery<Contact>().SingleOrDefault(c => c.Id == createdContactId);
        Assert.NotNull(contact);
        Assert.NotEmpty(contact.dfeta_TRN);

        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                requestId,
                person = new
                {
                    firstName,
                    middleName,
                    lastName,
                    email,
                    dateOfBirth,
                    nationalInsuranceNumber,
                },
                trn = contact.dfeta_TRN,
                status = "Completed"
            },
            expectedStatusCode: 200);
    }

    [Fact]
    public async Task Post_PotentialDuplicateRequest_CreatesContactWithoutTrnAndReturnsPendingStatus()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

        await TestData.CreatePerson(p => p
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth));

        var requestBody = CreateJsonContent(CreateDummyRequest() with
        {
            RequestId = requestId,
            Person = new()
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                Email = email,
                NationalInsuranceNumber = nationalInsuranceNumber,
            }
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var (_, createdContactId) = CrmQueryDispatcherSpy.GetSingleQuery<CreateContactQuery, Guid>();
        var contact = XrmFakedContext.CreateQuery<Contact>().SingleOrDefault(c => c.Id == createdContactId);
        Assert.NotNull(contact);
        Assert.Null(contact.dfeta_TRN);

        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                requestId,
                person = new
                {
                    firstName,
                    middleName,
                    lastName,
                    email,
                    dateOfBirth,
                    nationalInsuranceNumber,
                },
                trn = (string?)null,
                status = "Pending"
            },
            expectedStatusCode: 200);
    }

    private static CreateTrnRequestRequest CreateDummyRequest() => new()
    {
        RequestId = Guid.NewGuid().ToString(),
        Person = CreateDummyRequestPerson()
    };

    private static TrnRequestPerson CreateDummyRequestPerson() => new()
    {
        FirstName = "Minnie",
        MiddleName = "Van",
        LastName = "Ryder",
        DateOfBirth = new(1990, 5, 23),
        Email = "minnie.van.ryder@example.com",
        NationalInsuranceNumber = "1234567D"
    };
}
