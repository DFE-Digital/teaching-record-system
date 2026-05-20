using System.Diagnostics;

namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20260120;

public class GetPersonTests : TestBase
{
    public GetPersonTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentApiClient(roles: [ApiRoles.GetPerson]);
    }

    [Fact]
    public async Task HandleAsync_PersonDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var trn = "0000000";

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{trn}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.PersonNotFound, StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task HandleAsync_PersonExistsButIsNotActive_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        await WithDbContextAsync(dbContext =>
            dbContext.Persons
                .Where(p => p.PersonId == person.PersonId)
                .ExecuteUpdateAsync(u => u.SetProperty(p => p.Status, _ => PersonStatus.Deactivated)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.RecordIsDeactivated, StatusCodes.Status410Gone);
    }

    [Fact]
    public async Task HandleAsync_PersonIsMerged_ReturnsRedirect()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var anotherPerson = await TestData.CreatePersonAsync();

        await WithDbContextAsync(dbContext =>
            dbContext.Persons
                .Where(p => p.PersonId == person.PersonId)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(p => p.Status, _ => PersonStatus.Deactivated)
                    .SetProperty(p => p.MergedWithPersonId, _ => anotherPerson.PersonId)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status308PermanentRedirect, (int)response.StatusCode);
        Assert.Equal($"/v3/persons/{anotherPerson.Trn}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task HandleAsync_PersonExistsAndIsActive_ReturnsOk()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        Debug.Assert(person.Person.Status is PersonStatus.Active);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }
}
