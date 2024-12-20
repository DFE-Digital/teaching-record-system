using TeachingRecordSystem.Api.V3.Implementation.Dtos;

namespace TeachingRecordSystem.Api.Tests.V3.VNext;

public class GetPersonTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithNonNullDqtInductionStatus_ReturnsExpectedInduction()
    {
        // Arrange
        var dqtStatus = dfeta_InductionStatus.Pass;
        var status = dqtStatus.ToInductionStatus();
        var startDate = new DateOnly(1996, 2, 3);
        var completedDate = new DateOnly(1996, 6, 7);

        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithDqtInduction(
                dqtStatus,
                inductionExemptionReason: null,
                startDate,
                completedDate));

        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/person?include=Induction");

        // Act
        var response = await GetHttpClientWithIdentityAccessToken(person.Trn!).SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var responseInduction = jsonResponse.RootElement.GetProperty("induction");

        AssertEx.JsonObjectEquals(
            new
            {
                status = status.ToString(),
                startDate = startDate.ToString("yyyy-MM-dd"),
                completedDate = completedDate.ToString("yyyy-MM-dd"),
                certificateUrl = "/v3/certificates/induction"
            },
            responseInduction);
    }

    [Fact]
    public async Task Get_WithNullDqtInductionStatus_ReturnsNoneInductionStatus()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());

        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/person?include=Induction");

        // Act
        var response = await GetHttpClientWithIdentityAccessToken(person.Trn!).SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var responseInduction = jsonResponse.RootElement.GetProperty("induction");

        AssertEx.JsonObjectEquals(
            new
            {
                status = InductionStatus.None.ToString(),
                startDate = (DateOnly?)null,
                completedDate = (DateOnly?)null,
                certificateUrl = (string?)null
            },
            responseInduction);
    }

    [Fact]
    public async Task Get_WithQtlsDate_ReturnsActiveQtlsStatus()
    {
        // Arrange
        var qtlsDate = new DateOnly(2020, 01, 01);
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQtlsDate(qtlsDate));

        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/person");

        // Act
        var response = await GetHttpClientWithIdentityAccessToken(person.Trn!).SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var qtlsStatus = jsonResponse.RootElement.GetProperty("qtlsStatus").GetString();
        Assert.Equal(QtlsStatus.Active.ToString(), qtlsStatus!);
    }

    [Fact]
    public async Task Get_WithoutQtlsDate_ReturnsNoneQtlsStatus()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn());

        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/person");

        // Act
        var response = await GetHttpClientWithIdentityAccessToken(person.Trn!).SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var qtlsStatus = jsonResponse.RootElement.GetProperty("qtlsStatus").GetString();
        Assert.Equal(QtlsStatus.None.ToString(), qtlsStatus!);
    }

    [Fact]
    public async Task Get_WithExpiredQtlsDate_ReturnsExpiredQtlsStatus()
    {
        // Arrange
        var qtlsDate = new DateOnly(2020, 01, 01);
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQtlsDate(qtlsDate));

        var entity = new Microsoft.Xrm.Sdk.Entity() { Id = person.PersonId, LogicalName = Contact.EntityLogicalName };
        entity[Contact.Fields.dfeta_qtlsdate] = null;
        await TestData.OrganizationService.UpdateAsync(entity);

        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/person");

        // Act
        var response = await GetHttpClientWithIdentityAccessToken(person.Trn!).SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var qtlsStatus = jsonResponse.RootElement.GetProperty("qtlsStatus").GetString();
        Assert.Equal(QtlsStatus.Expired.ToString(), qtlsStatus!);
    }
}
