namespace TeachingRecordSystem.Api.Tests.V3;

public class FindTeachersTests : ApiTestBase
{
    public FindTeachersTests(ApiFixture apiFixture)
        : base(apiFixture)
    {
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
            $"/v3/teachers?findBy={findBy}&lastName={lastName}&dateOfBirth={dateOfBirth}");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

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
            $"/v3/teachers?findBy=LastNameAndDateOfBirth&lastName={lastName}&dateOfBirth={dateOfBirth}");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(response, expectedErrorPropertyName, expectedErrorMessage);
    }

    [Fact]
    public async Task Get_ValidRequestWithMatchesOnLastName_ReturnsMappedContacts()
    {
        // Arrange
        var findBy = "LastNameAndDateOfBirth";
        var lastName = "Smith";
        var dateOfBirth = new DateOnly(1990, 1, 1);

        var person1 = await TestData.CreatePerson(b => b.WithLastName(lastName).WithDateOfBirth(dateOfBirth).WithSanction("G1"));
        var person2 = await TestData.CreatePerson(b => b.WithLastName(lastName).WithDateOfBirth(dateOfBirth).WithSanction("A17"));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/v3/teachers?findBy={findBy}&lastName={lastName}&dateOfBirth={dateOfBirth:yyyy-MM-dd}");

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
                        trn = person1.Trn,
                        dateOfBirth = person1.DateOfBirth,
                        firstName = person1.FirstName,
                        middleName = person1.MiddleName ?? "",
                        lastName = person1.LastName,
                        sanctions = person1.Sanctions.Select(s => s.SanctionCode)
                    },
                    new
                    {
                        trn = person2.Trn,
                        dateOfBirth = person2.DateOfBirth,
                        firstName = person2.FirstName,
                        middleName = person2.MiddleName,
                        lastName = person2.LastName,
                        sanctions = person2.Sanctions.Select(s => s.SanctionCode)
                    }
                }
            });
    }

    [Fact]
    public async Task Get_ValidRequestWithMatchOnPreviousName_ReturnsMappedContacts()
    {
        // Arrange
        var findBy = "LastNameAndDateOfBirth";
        var lastName = TestData.GenerateLastName();
        var dateOfBirth = new DateOnly(1990, 1, 1);

        var person1 = await TestData.CreatePerson(b => b.WithLastName(lastName).WithDateOfBirth(dateOfBirth).WithSanction("G1"));
        var person2 = await TestData.CreatePerson(b => b.WithLastName(TestData.GenerateChangedLastName(lastName)).WithPreviousLastName(lastName).WithDateOfBirth(dateOfBirth).WithSanction("A17"));
        var person3 = await TestData.CreatePerson(b => b.WithLastName(TestData.GenerateChangedLastName(lastName)).WithDateOfBirth(dateOfBirth));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/v3/teachers?findBy={findBy}&lastName={lastName}&dateOfBirth={dateOfBirth:yyyy-MM-dd}");

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
                        trn = person1.Trn,
                        dateOfBirth = person1.DateOfBirth,
                        firstName = person1.FirstName,
                        middleName = person1.MiddleName ?? "",
                        lastName = person1.LastName,
                        sanctions = person1.Sanctions.Select(s => s.SanctionCode)
                    },
                    new
                    {
                        trn = person2.Trn,
                        dateOfBirth = person2.DateOfBirth,
                        firstName = person2.FirstName,
                        middleName = person2.MiddleName,
                        lastName = person2.LastName,
                        sanctions = person2.Sanctions.Select(s => s.SanctionCode)
                    }
                }
            });
    }
}
