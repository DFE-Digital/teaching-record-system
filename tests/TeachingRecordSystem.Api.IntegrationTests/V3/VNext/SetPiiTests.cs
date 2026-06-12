namespace TeachingRecordSystem.Api.IntegrationTests.V3.VNext;

[Collection(nameof(DisableParallelization))]
public class SetPiiTests : TestBase
{
    public SetPiiTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentApiClient([ApiRoles.UpdatePerson]);
    }

    private static object CreateRequestBody(
        string? firstName = null,
        string? middleName = null,
        string? lastName = null,
        DateOnly? dateOfBirth = null) => new
        {
            firstName = firstName ?? Faker.Name.First(),
            middleName = middleName ?? Faker.Name.Middle(),
            lastName = lastName ?? Faker.Name.Last(),
            dateOfBirth = dateOfBirth ?? new DateOnly(1990, 1, 1),
            emailAddress = Faker.Internet.Email(),
            nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber()
        };

    // PII updates are only permitted when the person's source application matches the calling client and the
    // person allows detail updates from its source application.
    private async Task<TestData.CreatePersonResult> CreatePiiUpdatablePersonAsync(Action<TestData.CreatePersonBuilder>? configure = null)
    {
        var person = await TestData.CreatePersonAsync(p =>
        {
            p.WithTrnRequest(ApplicationUserId, Guid.NewGuid().ToString());
            configure?.Invoke(p);
        });

        await WithDbContextAsync(dbContext => dbContext.Persons
            .Where(x => x.PersonId == person.PersonId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.AllowDetailsUpdatesFromSourceApplication, true)));

        return person;
    }

    [Theory, RoleNamesData(except: ApiRoles.UpdatePerson)]
    public async Task Put_UserDoesNotHavePermission_ReturnsForbidden(string[] roles)
    {
        // Arrange
        SetCurrentApiClient(roles);

        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v3/persons/{person.Trn}")
        {
            Content = CreateJsonContent(CreateRequestBody())
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Put_PersonDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Put, "/v3/persons/0000001")
        {
            Content = CreateJsonContent(CreateRequestBody())
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Put_PiiUpdatesNotPermittedForSourceApplication_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v3/persons/{person.Trn}")
        {
            Content = CreateJsonContent(CreateRequestBody())
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.PiiUpdatesForbidden, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Put_PersonHasQts_ReturnsError()
    {
        // Arrange
        var person = await CreatePiiUpdatablePersonAsync(p => p.WithQts());

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v3/persons/{person.Trn}")
        {
            Content = CreateJsonContent(CreateRequestBody())
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.PiiUpdatesForbiddenPersonHasQts, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Put_PersonHasEyts_ReturnsError()
    {
        // Arrange
        var person = await CreatePiiUpdatablePersonAsync(p => p.WithEyts(new DateOnly(2000, 01, 10)));

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v3/persons/{person.Trn}")
        {
            Content = CreateJsonContent(CreateRequestBody())
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.PiiUpdatesForbiddenPersonHasEyts, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Put_ValidRequest_ReturnsNoContentAndUpdatesPerson()
    {
        // Arrange
        var person = await CreatePiiUpdatablePersonAsync();

        var updatedFirstName = Faker.Name.First();
        var updatedMiddleName = Faker.Name.Middle();
        var updatedLastName = Faker.Name.Last();
        var updatedDateOfBirth = new DateOnly(1985, 6, 7);

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v3/persons/{person.Trn}")
        {
            Content = CreateJsonContent(CreateRequestBody(updatedFirstName, updatedMiddleName, updatedLastName, updatedDateOfBirth))
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);

        await WithDbContextAsync(async dbContext =>
        {
            var updatedPerson = await dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId);
            Assert.Equal(updatedFirstName, updatedPerson.FirstName);
            Assert.Equal(updatedMiddleName, updatedPerson.MiddleName);
            Assert.Equal(updatedLastName, updatedPerson.LastName);
            Assert.Equal(updatedDateOfBirth, updatedPerson.DateOfBirth);
        });
    }
}
