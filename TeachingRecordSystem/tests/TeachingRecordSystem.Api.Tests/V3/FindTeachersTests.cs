namespace TeachingRecordSystem.Api.Tests.V3;

public class FindTeachersTests : ApiTestBase
{
    public FindTeachersTests(ApiFixture apiFixture)
        : base(apiFixture)
    {
    }

    [Theory]
    [InlineData("")]
    [InlineData("BadFindBy")]
    public async Task Get_InvalidFindBy_ReturnsError(string findBy)
    {
        // Arrange
        var lastName = "Smith";
        var dateOfBirth = "1990-01-01";

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/v3/teachers?findBy={findBy}&lastName={lastName}&dateOfBirth={dateOfBirth}");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(response, "findBy", $"'{findBy}' is not valid.");
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
            $"/v3/teachers?findBy=LastNameAndDateOfBirth&lastName={lastName}&dateOfBirth={dateOfBirth}");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(response, expectedErrorPropertyName, expectedErrorMessage);
    }

    [Fact]
    public async Task Get_ValidRequest_ReturnsMappedContacts()
    {
        // Arrange
        var findBy = "LastNameAndDateOfBirth";
        var lastName = "Smith";
        var dateOfBirth = "1990-01-01";

        var resultWithoutStatedNames = new Contact()
        {
            Id = Guid.NewGuid(),
            FirstName = Faker.Name.First(),
            MiddleName = Faker.Name.Middle(),
            LastName = Faker.Name.Last(),
            dfeta_TRN = "1234567",
            BirthDate = Faker.Identification.DateOfBirth()
        };

        var resultWithStatedNames = new Contact()
        {
            Id = Guid.NewGuid(),
            FirstName = "Mary",
            MiddleName = "Jane",
            LastName = "Smith",
            dfeta_StatedFirstName = "Mary Jane",
            dfeta_StatedMiddleName = null,
            dfeta_StatedLastName = "Smith",
            dfeta_TRN = "2345678",
            BirthDate = Faker.Identification.DateOfBirth()
        };

        DataverseAdapterMock
            .Setup(mock => mock.FindTeachersByLastNameAndDateOfBirth(
                lastName,
                DateOnly.Parse(dateOfBirth),
                /* columnNames: */ It.IsAny<string[]>()))
            .ReturnsAsync(new[] { resultWithoutStatedNames, resultWithStatedNames });

        DataverseAdapterMock
            .Setup(mock => mock.GetSanctionsByContactIds(It.IsAny<IEnumerable<Guid>>(), /* liveOnly: */ true))
            .ReturnsAsync(new Dictionary<Guid, string[]>()
            {
                { resultWithoutStatedNames.Id, new[] { "G1" } },
                { resultWithStatedNames.Id, new[] { "A17" } }
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/v3/teachers?findBy={findBy}&lastName={lastName}&dateOfBirth={dateOfBirth}");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

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
                        trn = resultWithoutStatedNames.dfeta_TRN,
                        dateOfBirth = DateOnly.FromDateTime(resultWithoutStatedNames.BirthDate!.Value),
                        firstName = resultWithoutStatedNames.FirstName,
                        middleName = resultWithoutStatedNames.MiddleName ?? "",
                        lastName = resultWithoutStatedNames.LastName,
                        sanctions = new[] { "G1" }
                    },
                    new
                    {
                        trn = resultWithStatedNames.dfeta_TRN,
                        dateOfBirth = DateOnly.FromDateTime(resultWithStatedNames.BirthDate!.Value),
                        firstName = resultWithStatedNames.dfeta_StatedFirstName,
                        middleName = resultWithStatedNames.dfeta_StatedMiddleName ?? "",
                        lastName = resultWithStatedNames.dfeta_StatedLastName,
                        sanctions = new[] { "A17" }
                    }
                }
            });
    }
}
