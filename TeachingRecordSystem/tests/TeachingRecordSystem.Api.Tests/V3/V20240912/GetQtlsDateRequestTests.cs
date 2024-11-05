using System.Net;

namespace TeachingRecordSystem.Api.Tests.V3.V20240912;

public class GetQtlsDateRequestTests : TestBase
{
    public GetQtlsDateRequestTests(HostFixture hostFixture)
        : base(hostFixture)
    {
        SetCurrentApiClient([ApiRoles.AssignQtls]);
    }

    [Theory, RoleNamesData(except: ApiRoles.AssignQtls)]
    public async Task Get_ClientDoesNotHavePermission_ReturnsForbidden(string[] roles)
    {
        // Arrange
        SetCurrentApiClient(roles);
        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn());

        var request = new HttpRequestMessage(HttpMethod.Get, $"v3/persons/{existingContact.Trn}/qtls");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Get_TrnNotFound_ReturnsNotFound()
    {
        // Arrange
        var nonExistentTrn = "1234567";

        var request = new HttpRequestMessage(HttpMethod.Get, $"v3/persons/{nonExistentTrn}/qtls");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_NoQtls_ReturnsExpectedResult()
    {
        // Arrange
        var person = await TestData.CreatePerson(p => p.WithTrn());
        var request = new HttpRequestMessage(HttpMethod.Get, $"v3/persons/{person.Trn}/qtls");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                person.Trn,
                qtsDate = (DateOnly?)null

            },
            expectedStatusCode: 200);
    }

    [Fact]
    public async Task Get_WithQtls_ReturnsExpectedResult()
    {
        // Arrange
        var qtlsDate = new DateOnly(2020, 01, 01);
        var person = await TestData.CreatePerson(p => p
            .WithTrn()
            .WithQtlsDate(qtlsDate));
        var request = new HttpRequestMessage(HttpMethod.Get, $"v3/persons/{person.Trn}/qtls");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                person.Trn,
                qtsDate = qtlsDate

            },
            expectedStatusCode: 200);
    }
}
