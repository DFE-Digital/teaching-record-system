namespace TeachingRecordSystem.Api.Tests.V3.VNext;

public class SetWelshInductionStatusTests : TestBase
{
    public SetWelshInductionStatusTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentApiClient([ApiRoles.SetWelshInduction]);
    }

    [Fact]
    public async Task Put_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentApiClient(roles: []);

        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQts());

        var startDate = person.QtsDate!.Value.AddDays(6);
        var completedDate = startDate.AddMonths(12);

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v3/persons/{person.Trn}/welsh-induction")
        {
            Content = CreateJsonContent(new
            {
                passed = true,
                startDate = startDate,
                completedDate = completedDate
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Act
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Put_PersonDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var trn = await TestData.GenerateTrnAsync();

        var startDate = new DateOnly(2022, 1, 1);
        var completedDate = startDate.AddMonths(12);

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v3/persons/{trn}/welsh-induction")
        {
            Content = CreateJsonContent(new
            {
                passed = true,
                startDate = startDate,
                completedDate = completedDate
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Act
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.PersonNotFound, StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Put_PersonDoesNotHaveQts_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn());

        var startDate = new DateOnly(2022, 1, 1);
        var completedDate = startDate.AddMonths(12);

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v3/persons/{person.Trn}/welsh-induction")
        {
            Content = CreateJsonContent(new
            {
                passed = true,
                startDate = startDate,
                completedDate = completedDate
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Act
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.PersonDoesNotHaveQts, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Put_ValidRequestWithPassedForPersonWithRequiredToCompleteStatus_UpdatesDbAndReturnsNoContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQts()
            .WithInductionStatus(InductionStatus.RequiredToComplete));

        var startDate = person.QtsDate!.Value.AddDays(6);
        var completedDate = startDate.AddMonths(12);

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v3/persons/{person.Trn}/welsh-induction")
        {
            Content = CreateJsonContent(new
            {
                passed = true,
                startDate = startDate,
                completedDate = completedDate
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Act
        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);

        await WithDbContextAsync(async dbContext =>
        {
            var dbPerson = await dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId);
            Assert.Equal(InductionStatus.Exempt, dbPerson.InductionStatus);
        });
    }

    [Fact]
    public async Task Put_ValidRequestWithFailedForPersonWithRequiredToCompleteStatus_UpdatesDbAndReturnsNoContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQts()
            .WithInductionStatus(InductionStatus.RequiredToComplete));

        var startDate = person.QtsDate!.Value.AddDays(6);
        var completedDate = startDate.AddMonths(12);

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v3/persons/{person.Trn}/welsh-induction")
        {
            Content = CreateJsonContent(new
            {
                passed = false,
                startDate = startDate,
                completedDate = completedDate
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Act
        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);

        await WithDbContextAsync(async dbContext =>
        {
            var dbPerson = await dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId);
            Assert.Equal(InductionStatus.FailedInWales, dbPerson.InductionStatus);
        });
    }

    [Fact]
    public async Task Put_ValidRequestWithPassedForPersonWithHighPriorityStatus_DoesNotUpdateStatusAndReturnsNoContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQts()
            .WithInductionStatus(InductionStatus.Passed));

        var startDate = person.QtsDate!.Value.AddDays(6);
        var completedDate = startDate.AddMonths(12);

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v3/persons/{person.Trn}/welsh-induction")
        {
            Content = CreateJsonContent(new
            {
                passed = true,
                startDate = startDate,
                completedDate = completedDate
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Act
        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);

        await WithDbContextAsync(async dbContext =>
        {
            var dbPerson = await dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId);
            Assert.NotEqual(InductionStatus.Exempt, dbPerson.InductionStatus);
        });
    }

    [Fact]
    public async Task Put_ValidRequestWithFailedForPersonWithHighPriorityStatus_DoesNotUpdateStatusAndReturnsNoContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQts()
            .WithInductionStatus(InductionStatus.Passed));

        var startDate = person.QtsDate!.Value.AddDays(6);
        var completedDate = startDate.AddMonths(12);

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v3/persons/{person.Trn}/welsh-induction")
        {
            Content = CreateJsonContent(new
            {
                passed = false,
                startDate = startDate,
                completedDate = completedDate
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Act
        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);

        await WithDbContextAsync(async dbContext =>
        {
            var dbPerson = await dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId);
            Assert.NotEqual(InductionStatus.Exempt, dbPerson.InductionStatus);
        });
    }
}
