using TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.Api.Tests.V3.VNext;

public class SetPIITests : TestBase
{
    public SetPIITests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentApiClient([ApiRoles.UpdatePerson]);
    }

    [Theory, RoleNamesData(except: ApiRoles.UpdatePerson)]
    public async Task Put_UserDoesNotHavePermission_ReturnsForbidden(string[] roles)
    {
        // Arrange
        SetCurrentApiClient(roles);

        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQts());

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v3/persons/{person.Trn}/set-pii")
        {
            Content = CreateJsonContent(new
            {
                firstName = Faker.Name.First(),
                middleName = Faker.Name.Middle(),
                lastName = Faker.Name.Last(),
                dateOfBirth = Faker.Identification.DateOfBirth().ToShortDateString(),
                emailAddresses = new string[0],
                nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber(),
                gender = Gender.Male
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Act
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Put_PIIUpdatesNotPermitted_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQts());
        XrmFakedContext.UpdateEntity(new Contact()
        {
            ContactId = person.ContactId,
            dfeta_AllowPiiUpdatesFromRegister = false
        });

        var content = CreateJsonContent(new
        {
            firstName = Faker.Name.First(),
            middleName = Faker.Name.Middle(),
            lastName = Faker.Name.Last(),
            dateOfBirth = Faker.Identification.DateOfBirth().ToDateOnlyWithDqtBstFix(isLocalTime: false),
            emailAddresses = new[] { Faker.Internet.Email() },
            nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber(),
            gender = "1"
        });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}/set-pii", content);

        // Act
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.PIIUpdatesForbidden, StatusCodes.Status400BadRequest);
    }
}
