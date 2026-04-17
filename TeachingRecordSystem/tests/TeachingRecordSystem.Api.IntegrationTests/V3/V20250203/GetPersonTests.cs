namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20250203;

[Collection(nameof(DisableParallelization)), ClearDbBeforeTest]
public class GetPersonTests : TestBase
{
    public GetPersonTests(HostFixture hostFixture) : base(hostFixture)
    {
    }

    [Fact]
    public async Task Get_WithTrnClaim_ReturnsPersonDetails()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var httpClient = GetHttpClientWithAuthorizeAccessToken(person.Trn!, Version);
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/person");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        Assert.Equal(person.Trn, jsonResponse.RootElement.GetProperty("trn").GetString());
    }

    [Fact]
    public async Task Get_WithTrnRequestIdClaimButUnresolvedRequest_ReturnsForbidden()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var trnRequestId = Guid.NewGuid().ToString();

        await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId, t => t
            .WithRequestId(trnRequestId));

        var httpClient = GetHttpClientWithAuthorizeAccessTokenForTrnRequest(applicationUser.UserId, trnRequestId, Version);
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/person");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithTrnRequestIdClaimAndResolvedRequest_ReturnsPersonDetails()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var trnRequestId = Guid.NewGuid().ToString();

        await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId, t => t
            .WithRequestId(trnRequestId)
            .WithResolvedPersonId(person.PersonId));

        var httpClient = GetHttpClientWithAuthorizeAccessTokenForTrnRequest(applicationUser.UserId, trnRequestId, Version);
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/person");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        Assert.Equal(person.Trn, jsonResponse.RootElement.GetProperty("trn").GetString());
    }
}

//         // Assert
//         var jsonResponse = await AssertEx.JsonResponseAsync(response);
//         var qtlsStatus = jsonResponse.RootElement.GetProperty("qtlsStatus").GetString();
//         Assert.Equal(QtlsStatus.None.ToString(), qtlsStatus!);
//     }
//
//     [Fact]
//     public async Task Get_WithExpiredQtlsDate_ReturnsExpiredQtlsStatus()
//     {
//         // Arrange
//         var qtlsDate = new DateOnly(2020, 01, 01);
//         var person = await TestData.CreatePersonAsync(p => p
//             .WithTrn().WithQtls(qtlsDate));
//
//         var entity = new Microsoft.Xrm.Sdk.Entity() { Id = person.PersonId, LogicalName = Contact.EntityLogicalName };
//         entity[Contact.Fields.dfeta_qtlsdate] = null;
//         await TestData.OrganizationService.UpdateAsync(entity);
//
//         // Arrange
//         var request = new HttpRequestMessage(HttpMethod.Get, "/v3/person");
//
//         // Act
//         var response = await GetHttpClientWithIdentityAccessToken(person.Trn!).SendAsync(request);
//
//         // Assert
//         var jsonResponse = await AssertEx.JsonResponseAsync(response);
//         var qtlsStatus = jsonResponse.RootElement.GetProperty("qtlsStatus").GetString();
//         Assert.Equal(QtlsStatus.Expired.ToString(), qtlsStatus!);
//     }
//
//     [Theory]
//     [InlineData("01/01/2011", "01/01/2022", "Qualified Teacher Learning and Skills status", "2011-01-01")]
//     [InlineData("01/01/2019", "01/01/1999", "Qualified", "1999-01-01")]
//     public async Task Get_QtsAndActiveQtls_ReturnsQtsStatusOfEarliestOfDates(string qtlsDateStr, string qtsDateStr, string expectedStatusDescription, string expectedAwardedDate)
//     {
//         // Arrange
//         var qtlsDate = DateOnly.Parse(qtlsDateStr);
//         var qtsDate = DateOnly.Parse(qtsDateStr);
//         var activeHeQualificationWithNoSubjectsId = Guid.NewGuid();
//         var person = await TestData.CreatePersonAsync(p => p
//             .WithTrn()
//             .WithQts(qtsDate).WithQtls(qtlsDate));
//         var status = await ReferenceCache.GetTeacherStatusByValueAsync("71"); //qualified teacher
//         var qtsRegistration = new dfeta_qtsregistration() { dfeta_QTSDate = qtsDate.ToDateTime(), dfeta_TeacherStatusId = status.ToEntityReference() };
//         DataverseAdapterMock.Setup(x => x.GetQtsRegistrationsByTeacherAsync(It.IsAny<Guid>(), It.IsAny<string[]>())).ReturnsAsync(new[] { qtsRegistration });
//
//         var request = new HttpRequestMessage(HttpMethod.Get, "/v3/person");
//
//         // Act
//         var response = await GetHttpClientWithIdentityAccessToken(person.Trn!).SendAsync(request);
//
//         // Assert
//         var jsonResponse = await AssertEx.JsonResponseAsync(response);
//         var qts = jsonResponse.RootElement.GetProperty("qts");
//         var statusDescription = qts.GetProperty("statusDescription").GetString();
//         var awardedDate = qts.GetProperty("awarded").GetString();
//         Assert.Equal(expectedStatusDescription, statusDescription!);
//         Assert.Equal(expectedAwardedDate, awardedDate!);
//     }
// }
