using System.Net;
using TeachingRecordSystem.Api.V3.V20240606.Requests;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.Tests.V3.V20240606;

[Collection(nameof(DisableParallelization))]
public class CreateTrnRequestTests : TestBase
{
    public CreateTrnRequestTests(HostFixture hostFixture)
        : base(hostFixture)
    {
        SetCurrentApiClient([ApiRoles.CreateTrn]);
    }

    [Fact]
    public async Task Post_WithMultipleEmailAddresses_MatchesByEmail()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email1 = Faker.Internet.Email();
        var email2 = Faker.Internet.Email();

        await TestData.CreatePerson(p => p
            .WithTrn()
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(email2));

        var requestBody = CreateJsonContent(CreateDummyRequest() with
        {
            RequestId = requestId,
            Person = new()
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                EmailAddresses = [email1, email2]
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
        Assert.Equal(email1, contact.EMailAddress1);

        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                requestId,
                trn = (string?)null,
                status = "Pending"
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
}
