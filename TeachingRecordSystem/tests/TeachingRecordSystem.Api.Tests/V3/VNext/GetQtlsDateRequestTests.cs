using System.Net;

namespace TeachingRecordSystem.Api.Tests.V3.VNext;

public class GetQtlsDateRequestTests : TestBase
{
    public GetQtlsDateRequestTests(HostFixture hostFixture)
        : base(hostFixture)
    {
        SetCurrentApiClient(new[] { ApiRoles.AssignQtls });
    }

    [Theory, RoleNamesData(except: ApiRoles.AssignQtls)]
    public async Task Get_ClientDoesNotHavePermission_ReturnsForbidden(string[] roles)
    {
        // Arrange
        SetCurrentApiClient(roles);
        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true));

        var request = new HttpRequestMessage(HttpMethod.Get, $"v3/persons/{existingContact.Trn}/qtls");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Trn_TrnNotFound_ReturnsNotFound()
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
    public async Task Get_NoQTLS_ReturnsExpectedResult()
    {
        // Arrange  
        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true));

        var request = new HttpRequestMessage(HttpMethod.Get, $"v3/persons/{existingContact.Trn}/qtls");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                existingContact.Trn,
                QtsDate = default(DateOnly?)

            },
            expectedStatusCode: 200);
    }

    [Fact]
    public async Task Get_WithQTLS_ReturnsExpectedResult()
    {
        // Arrange
        var qtlsDate = new DateOnly(2020, 01, 01);
        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithQtlsDate(qtlsDate));
        var request = new HttpRequestMessage(HttpMethod.Get, $"v3/persons/{existingContact.Trn}/qtls");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                existingContact.Trn,
                QtsDate = qtlsDate

            },
            expectedStatusCode: 200);
    }
}
