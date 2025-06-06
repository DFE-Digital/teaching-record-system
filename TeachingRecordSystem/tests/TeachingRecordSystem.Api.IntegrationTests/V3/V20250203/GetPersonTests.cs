using QtlsStatus = TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos.QtlsStatus;

namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20250203;

public class GetPersonTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithNonNullDqtInductionStatus_ReturnsExpectedInduction()
    {
        // Arrange
        var status = InductionStatus.Passed;
        var startDate = new DateOnly(1996, 2, 3);
        var completedDate = new DateOnly(1996, 6, 7);

        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithInductionStatus(i => i
                .WithStatus(status)
                .WithStartDate(startDate)
                .WithCompletedDate(completedDate)));

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
                completedDate = completedDate.ToString("yyyy-MM-dd")
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
                completedDate = (DateOnly?)null
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
            .WithQtlsDateInDqt(qtlsDate));

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
            .WithQtlsDateInDqt(qtlsDate));

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

    [Theory]
    [InlineData("01/01/2011", "01/01/2022", "Qualified Teacher Learning and Skills status", "2011-01-01")]
    [InlineData("01/01/2019", "01/01/1999", "Qualified", "1999-01-01")]
    public async Task Get_QtsAndActiveQtls_ReturnsQtsStatusOfEarliestOfDates(string qtlsDateStr, string qtsDateStr, string expectedStatusDescription, string expectedAwardedDate)
    {
        // Arrange
        var qtlsDate = DateOnly.Parse(qtlsDateStr);
        var qtsDate = DateOnly.Parse(qtsDateStr);
        var activeHeQualificationWithNoSubjectsId = Guid.NewGuid();
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQts(qtsDate)
            .WithQtlsDateInDqt(qtlsDate));
        var status = await ReferenceCache.GetTeacherStatusByValueAsync("71"); //qualified teacher
        var qtsRegistration = new dfeta_qtsregistration() { dfeta_QTSDate = qtsDate.ToDateTime(), dfeta_TeacherStatusId = status.ToEntityReference() };
        DataverseAdapterMock.Setup(x => x.GetQtsRegistrationsByTeacherAsync(It.IsAny<Guid>(), It.IsAny<string[]>())).ReturnsAsync(new[] { qtsRegistration });

        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/person");

        // Act
        var response = await GetHttpClientWithIdentityAccessToken(person.Trn!).SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var qts = jsonResponse.RootElement.GetProperty("qts");
        var statusDescription = qts.GetProperty("statusDescription").GetString();
        var awardedDate = qts.GetProperty("awarded").GetString();
        Assert.Equal(expectedStatusDescription, statusDescription!);
        Assert.Equal(expectedAwardedDate, awardedDate!);
    }
}
