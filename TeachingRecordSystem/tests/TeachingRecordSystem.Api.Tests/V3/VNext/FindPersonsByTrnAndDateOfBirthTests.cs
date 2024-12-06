namespace TeachingRecordSystem.Api.Tests.V3.VNext;

[Collection(nameof(DisableParallelization))]
public class FindPersonsByTrnAndDateOfBirthTests : TestBase
{
    public FindPersonsByTrnAndDateOfBirthTests(HostFixture hostFixture) : base(hostFixture)
    {
        XrmFakedContext.DeleteAllEntities<Contact>();
        SetCurrentApiClient([ApiRoles.GetPerson]);
    }

    [Fact]
    public async Task Get_PersonHasNullDqtInductionStatus_ReturnsNoneInductionStatus()
    {
        // Arrange
        var lastName = "Smith";
        var dateOfBirth = new DateOnly(1990, 1, 1);

        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
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
        var dqtInductionStatus = dfeta_InductionStatus.Pass;
        var inductionStatus = dqtInductionStatus.ToInductionStatus();
        var inductionStartDate = new DateOnly(1996, 2, 3);
        var inductionCompletedDate = new DateOnly(1996, 6, 7);

        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithDqtInduction(
                dqtInductionStatus,
                inductionExemptionReason: null,
                inductionStartDate,
                inductionCompletedDate));

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
}
