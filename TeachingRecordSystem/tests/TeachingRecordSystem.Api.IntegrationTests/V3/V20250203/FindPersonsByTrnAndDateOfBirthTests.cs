using QtlsStatus = TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos.QtlsStatus;

namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20250203;

[Collection(nameof(DisableParallelization))]
public class FindPersonsByTrnAndDateOfBirthTests : TestBase
{
    public FindPersonsByTrnAndDateOfBirthTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentApiClient([ApiRoles.GetPerson]);
    }

    [Fact]
    public async Task Get_PersonHasNullDqtInductionStatus_ReturnsNoneInductionStatus()
    {
        // Arrange
        var lastName = "Smith";
        var dateOfBirth = new DateOnly(1990, 1, 1);

        var person = await TestData.CreatePersonAsync(p => p
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/v3/persons/find")
        {
            Content = JsonContent.Create(new
            {
                persons = new[]
                {
                    new
                    {
                        trn = person.Trn,
                        dateOfBirth = person.DateOfBirth
                    }
                }
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var responseInduction = jsonResponse.RootElement.GetProperty("results").EnumerateArray().Single().GetProperty("inductionStatus").GetString();
        Assert.Equal(InductionStatus.None.ToString(), responseInduction);
    }

    [Fact]
    public async Task Get_PersonHasNonNullDqtInductionStatus_ReturnsExpectedStatus()
    {
        // Arrange
        var lastName = "Smith";
        var dateOfBirth = new DateOnly(1990, 1, 1);
        var inductionStatus = InductionStatus.Passed;
        var inductionStartDate = new DateOnly(1996, 2, 3);
        var inductionCompletedDate = new DateOnly(1996, 6, 7);

        var person = await TestData.CreatePersonAsync(p => p
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithInductionStatus(i => i
                .WithStatus(inductionStatus)
                .WithStartDate(inductionStartDate)
                .WithCompletedDate(inductionCompletedDate)));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/v3/persons/find")
        {
            Content = JsonContent.Create(new
            {
                persons = new[]
                {
                    new
                    {
                        trn = person.Trn,
                        dateOfBirth = person.DateOfBirth
                    }
                }
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var responseInduction = jsonResponse.RootElement.GetProperty("results").EnumerateArray().Single().GetProperty("inductionStatus").GetString();
        Assert.Equal(inductionStatus.ToString(), responseInduction);
    }

    [Fact]
    public async Task Get_WithQtlsDate_ReturnsActiveQtlsStatus()
    {
        // Arrange
        var lastName = "Smith";
        var dateOfBirth = new DateOnly(1990, 1, 1);
        var qtlsDate = new DateOnly(2020, 01, 01);

        var person = await TestData.CreatePersonAsync(p => p
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithQtls(qtlsDate));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/v3/persons/find")
        {
            Content = JsonContent.Create(new
            {
                persons = new[]
                {
                    new
                    {
                        trn = person.Trn,
                        dateOfBirth = person.DateOfBirth
                    }
                }
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var qtlsStatus = jsonResponse.RootElement.GetProperty("results").EnumerateArray().Single().GetProperty("qtlsStatus").GetString();
        Assert.Equal(QtlsStatus.Active.ToString(), qtlsStatus!);
    }

    [Fact]
    public async Task Get_WithoutQtlsDate_ReturnsNoneQtlsStatus()
    {
        // Arrange
        var lastName = "Smith";
        var dateOfBirth = new DateOnly(1990, 1, 1);

        var person = await TestData.CreatePersonAsync(p => p
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/v3/persons/find")
        {
            Content = JsonContent.Create(new
            {
                persons = new[]
                {
                    new
                    {
                        trn = person.Trn,
                        dateOfBirth = person.DateOfBirth
                    }
                }
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var qtlsStatus = jsonResponse.RootElement.GetProperty("results").EnumerateArray().Single().GetProperty("qtlsStatus").GetString();
        Assert.Equal(QtlsStatus.None.ToString(), qtlsStatus!);
    }

    [Fact]
    public async Task Get_WithExpiredQtlsDate_ReturnsExpiredQtlsStatus()
    {
        // Arrange
        var lastName = "Smith";
        var dateOfBirth = new DateOnly(1990, 1, 1);

        var person = await TestData.CreatePersonAsync(p => p
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithQtlsStatus(Core.Models.QtlsStatus.Expired));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/v3/persons/find")
        {
            Content = JsonContent.Create(new
            {
                persons = new[]
                {
                    new
                    {
                        trn = person.Trn,
                        dateOfBirth = person.DateOfBirth
                    }
                }
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var qtlsStatus = jsonResponse.RootElement.GetProperty("results").EnumerateArray().Single().GetProperty("qtlsStatus").GetString();
        Assert.Equal(QtlsStatus.Expired.ToString(), qtlsStatus!);
    }
}
