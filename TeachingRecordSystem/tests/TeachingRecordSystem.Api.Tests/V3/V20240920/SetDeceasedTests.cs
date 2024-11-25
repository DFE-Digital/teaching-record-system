using System.Net;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.Api.Tests.V3.V20240920;

[Collection(nameof(DisableParallelization))]
public class SetDeceasedTests : TestBase
{
    public SetDeceasedTests(HostFixture hostFixture)
        : base(hostFixture)
    {
        SetCurrentApiClient([ApiRoles.UpdatePerson]);
    }

    [Theory, RoleNamesData(except: ApiRoles.UpdatePerson)]
    public async Task Put_ClientDoesNotHavePermission_ReturnsForbidden(string[] roles)
    {
        // Arrange
        SetCurrentApiClient(roles);
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());
        var requestBody = CreateJsonContent(new { DateOfDeath = new DateOnly(1990, 01, 01) });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/deceased/{person.Trn}")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Put_DateOfDeathInFuture_ReturnsErrror()
    {
        // Arrange
        var futureDate = Clock.UtcNow.AddYears(1).ToDateOnlyWithDqtBstFix(isLocalTime: false);
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());
        var requestBody = CreateJsonContent(new { DateOfDeath = futureDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/deceased/{person.Trn}")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(
            response,
            propertyName: nameof(SetDeceasedCommand.DateOfDeath),
            expectedError: "Date cannot be in the future.");
    }

    [Fact]
    public async Task Put_WithoutDateOfDeath_ReturnsBadRequest()
    {
        // Arrange
        var futureDate = Clock.UtcNow.AddYears(1).ToDateOnlyWithDqtBstFix(isLocalTime: false);
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());
        var requestBody = CreateJsonContent(new { DateOfDeath = default(DateOnly?) });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/deceased/{person.Trn}")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_TrnNotFound_ReturnsBadRequest()
    {
        // Arrange
        var nonExistentTrn = "1234567";
        var requestBody = CreateJsonContent(new { DateOfDeath = new DateOnly(1990, 01, 01) });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/deceased/{nonExistentTrn}")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_ExistingDateOfDeath_ReturnsNoContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());
        var requestBody1 = CreateJsonContent(new { DateOfDeath = new DateOnly(1990, 01, 01) });
        var requestBody2 = CreateJsonContent(new { DateOfDeath = new DateOnly(1980, 01, 01) });
        var request1 = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/deceased/{person.Trn}")
        {
            Content = requestBody1
        };
        var request2 = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/deceased/{person.Trn}")
        {
            Content = requestBody2
        };

        // Act
        var response1 = await GetHttpClientWithApiKey().SendAsync(request1);
        var response2 = await GetHttpClientWithApiKey().SendAsync(request2);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response1.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, response2.StatusCode);
    }

    [Fact]
    public async Task Put_ValidDateOfDeath_ReturnsNoContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());
        var requestBody = CreateJsonContent(new { DateOfDeath = new DateOnly(1990, 01, 01) });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/deceased/{person.Trn}")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
