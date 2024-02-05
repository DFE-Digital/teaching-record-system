using System.Net;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Api.Tests.V3;

public class GetTrnRequestTests : TestBase
{
    public GetTrnRequestTests(HostFixture hostFixture)
        : base(hostFixture)
    {
        SetCurrentApiClient(new[] { ApiRoles.CreateTrn });
    }

    [Theory]
    [InlineData(ApiRoles.UnlockPerson)]
    [InlineData(ApiRoles.UpdateNpq)]
    public async Task Get_TrnRequestWithoutPermission_returns_NotPermitted(string role)
    {
        // Arrange
        SetCurrentApiClient(new[] { role });
        var teacherId = Guid.NewGuid();
        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var existing = await TestData.CreatePerson(p =>
        {
            p.WithFirstName(firstName);
            p.WithMiddleName(middleName);
            p.WithLastName(lastName);
            p.WithDateOfBirth(dateOfBirth);
            p.WithEmail(email);
            p.WithTrn(hasTrn: true);
            p.WithNationalInsuranceNumber(hasNationalInsuranceNumber: true);
        });
        await WithDbContext(async dbContext =>
        {
            dbContext.Add(new TrnRequest()
            {
                ClientId = ClientId,
                RequestId = requestId,
                TeacherId = existing.ContactId
            });

            await dbContext.SaveChangesAsync();
        });

        // Act
        var response = await GetHttpClientWithApiKey().GetAsync($"v3/trn-requests?requestid={requestId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidCompletedTrnRequest_ReturnsExpectedResponse()
    {
        // Arrange
        var teacherId = Guid.NewGuid();
        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var existing = await TestData.CreatePerson(p =>
        {
            p.WithFirstName(firstName);
            p.WithMiddleName(middleName);
            p.WithLastName(lastName);
            p.WithDateOfBirth(dateOfBirth);
            p.WithEmail(email);
            p.WithTrn(hasTrn: true);
            p.WithNationalInsuranceNumber(hasNationalInsuranceNumber: true);
        });
        await WithDbContext(async dbContext =>
        {
            dbContext.Add(new TrnRequest()
            {
                ClientId = ClientId,
                RequestId = requestId,
                TeacherId = existing.ContactId
            });

            await dbContext.SaveChangesAsync();
        });

        // Act
        var response = await GetHttpClientWithApiKey().GetAsync($"v3/trn-requests?requestid={requestId}");
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
                    NationalInsuranceNumber = contact!.dfeta_NINumber,
                },
                trn = contact.dfeta_TRN,
                status = "Completed"
            },
            expectedStatusCode: 200);
    }

    [Fact]
    public async Task Get_TrnRequestNotFound_ReturnsNotFound()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();

        // Act
        var response = await GetHttpClientWithApiKey().GetAsync($"v3/trn-requests?requestid={requestId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidIncompleteTrnRequest_ReturnsExpectedResponse()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();

        var existing = await TestData.CreatePerson(p =>
        {
            p.WithFirstName(firstName);
            p.WithMiddleName(middleName);
            p.WithLastName(lastName);
            p.WithDateOfBirth(dateOfBirth);
            p.WithEmail(email);
            p.WithTrn(hasTrn: false);
            p.WithNationalInsuranceNumber(hasNationalInsuranceNumber: true);
        });

        await WithDbContext(async dbContext =>
        {
            dbContext.Add(new TrnRequest()
            {
                ClientId = ClientId,
                RequestId = requestId,
                TeacherId = existing.ContactId
            });
            await dbContext.SaveChangesAsync();
        });


        // Act
        var response = await GetHttpClientWithApiKey().GetAsync($"v3/trn-requests?requestid={requestId}");

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
                    NationalInsuranceNumber = existing.NationalInsuranceNumber,
                },
                trn = default(string),
                status = "Pending"
            },
            expectedStatusCode: 200);
    }
}
