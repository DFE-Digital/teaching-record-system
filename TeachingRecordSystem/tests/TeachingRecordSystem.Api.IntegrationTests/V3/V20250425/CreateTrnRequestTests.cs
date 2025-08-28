using System.Net;
using TeachingRecordSystem.Api.Properties;
using TeachingRecordSystem.Api.V3.V20250425.Requests;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;
using TeachingRecordSystem.Core.Services.TrnRequests;

namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20250425;

[Collection(nameof(DisableParallelization))]
public class CreateTrnRequestTests : TestBase
{
    public CreateTrnRequestTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentApiClient([ApiRoles.CreateTrn]);
        FeatureProvider.Features.Add(FeatureNames.ContactsMigrated);

        GetAnIdentityApiClientMock
            .Setup(mock => mock.CreateTrnTokenAsync(It.IsAny<CreateTrnTokenRequest>()))
            .ReturnsAsync((CreateTrnTokenRequest req) => new CreateTrnTokenResponse()
            {
                Email = req.Email,
                ExpiresUtc = Clock.UtcNow.AddDays(1),
                Trn = req.Trn,
                TrnToken = Guid.NewGuid().ToString()
            });
    }

    public override Task InitializeAsync() => DbHelper.DeleteAllPersonsAsync();

    [Theory, RoleNamesData(except: ApiRoles.CreateTrn)]
    public async Task Post_ClientDoesNotHavePermission_ReturnsForbidden(string[] roles)
    {
        // Arrange
        SetCurrentApiClient(roles);

        var requestBody = CreateJsonContent(CreateDummyRequest());

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
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(
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
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(
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
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(
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
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(
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
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(
            response,
            "Person.NationalInsuranceNumber",
            StringResources.ErrorMessages_EnterNinoNumberInCorrectFormat);
    }

    [Fact]
    public async Task Post_RequestWithExistingRequestInCrm_ReturnsConflict()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

        var existingContact = await TestData.CreatePersonAsync(p => p
            .WithTrnRequest(ApplicationUserId, requestId)
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(email)
            .WithNationalInsuranceNumber(nationalInsuranceNumber: nationalInsuranceNumber));

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Add(new TrnRequest()
            {
                ClientId = ApplicationUserId.ToString(),
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
                EmailAddresses = [email],
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
        await AssertEx.JsonResponseIsErrorAsync(response, expectedErrorCode: 10029, expectedStatusCode: StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task Post_RequestWithExistingRequestInDb_ReturnsConflict()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

        var existingContact = await TestData.CreatePersonAsync(p => p
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(email)
            .WithNationalInsuranceNumber(nationalInsuranceNumber: nationalInsuranceNumber)
            .WithTrnRequest(ApplicationUserId, requestId));

        var requestBody = CreateJsonContent(CreateDummyRequest() with
        {
            RequestId = requestId,
            Person = new()
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                EmailAddresses = [email],
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
        await AssertEx.JsonResponseIsErrorAsync(response, expectedErrorCode: 10029, expectedStatusCode: StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task Post_RequestWithInvalidNino_ReturnsError()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var invalidNino = "IvalidNi";

        var existingContact = await TestData.CreatePersonAsync(p => p
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(email)
            .WithNationalInsuranceNumber());

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Add(new TrnRequest()
            {
                ClientId = ApplicationUserId.ToString(),
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
                EmailAddresses = [email],
                NationalInsuranceNumber = invalidNino,
            }
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(
            response,
            "Person.NationalInsuranceNumber",
            StringResources.ErrorMessages_EnterNinoNumberInCorrectFormat);
    }

    [Fact]
    public async Task Post_RequestWithoutNino_ReturnsOk()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();

        var requestBody = CreateJsonContent(CreateDummyRequest() with
        {
            RequestId = requestId,
            Person = new()
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                EmailAddresses = [email]
            }
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Post_NotMatchedToExistingRecord_ReturnsCompletedStatusAndTrn()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var firstNames = new string[] { Faker.Name.First(), Faker.Name.First() };
        var firstName = string.Join(" ", firstNames);
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
                EmailAddresses = [email],
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
        var person = await WithDbContextAsync(dbContext => dbContext.Persons.SingleOrDefaultAsync());
        Assert.NotNull(person);
        Assert.NotNull(person.Trn);

        var aytqLink = await GetAccessYourTeachingQualificationsLinkAsync(requestId);

        await AssertEx.JsonResponseEqualsAsync(
            response,
            expected: new
            {
                requestId,
                trn = person.Trn,
                status = "Completed",
                potentialDuplicate = false,
                accessYourTeachingQualificationsLink = aytqLink
            },
            expectedStatusCode: 200);
    }

    [Fact]
    public async Task Post_PotentialDuplicateContact_ReturnsPendingStatus()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

        await TestData.CreatePersonAsync(p => p
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
                EmailAddresses = [email],
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
        await AssertEx.JsonResponseEqualsAsync(
            response,
            expected: new
            {
                requestId,
                trn = (string?)null,
                status = "Pending",
                potentialDuplicate = true,
                accessYourTeachingQualificationsLink = (string?)null
            },
            expectedStatusCode: 200);
    }

    [Fact]
    public async Task Post_RequestWithoutEmail_ReturnsOk()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);

        var requestBody = CreateJsonContent(CreateDummyRequest() with
        {
            RequestId = requestId,
            Person = new()
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                DateOfBirth = dateOfBirth
            }
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Post_RequestWithNullEmail_ReturnsError()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);

        var requestBody = CreateJsonContent(CreateDummyRequest() with
        {
            RequestId = requestId,
            Person = new()
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                EmailAddresses = new[] { (string?)null }!
            }
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "v3/trn-requests")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, "person.emailAddresses[0]", "Email address cannot be null.");
    }

    private static CreateTrnRequestRequest CreateDummyRequest() => new()
    {
        RequestId = Guid.NewGuid().ToString(),
        Person = CreateDummyRequestPerson()
    };

    private static CreateTrnRequestRequestPerson CreateDummyRequestPerson() => new()
    {
        FirstName = "Minnie",
        MiddleName = "Van",
        LastName = "Ryder",
        DateOfBirth = new(1990, 5, 23),
        EmailAddresses = ["minnie.van.ryder@example.com"],
        NationalInsuranceNumber = "1234567D"
    };

    private async Task<string> GetAccessYourTeachingQualificationsLinkAsync(string requestId)
    {
        var trnToken = await WithDbContextAsync(async dbContext =>
        {
            var metadata = await dbContext.TrnRequestMetadata.SingleAsync(r => r.ApplicationUserId == ApplicationUserId && r.RequestId == requestId);

            if (metadata.TrnToken is null)
            {
                throw new InvalidOperationException("TRN request does not have a TRN token.");
            }

            return metadata.TrnToken!;
        });

        return HostFixture.Services.GetRequiredService<TrnRequestService>()
            .GetAccessYourTeachingQualificationsLink(trnToken);
    }
}
