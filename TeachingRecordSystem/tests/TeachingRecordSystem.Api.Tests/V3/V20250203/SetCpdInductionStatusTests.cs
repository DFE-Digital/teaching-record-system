using SystemUser = TeachingRecordSystem.Core.DataStore.Postgres.Models.SystemUser;

namespace TeachingRecordSystem.Api.Tests.V3.V20250203;

public class SetCpdInductionStatusTests : TestBase
{
    public SetCpdInductionStatusTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentApiClient([ApiRoles.SetCpdInduction]);
    }

    public static TheoryData<InductionStatus> AllStatusesExceptNoneData => new()
    {
        InductionStatus.RequiredToComplete,
        InductionStatus.InProgress,
        InductionStatus.Passed,
        InductionStatus.Failed,
        InductionStatus.Exempt,
        InductionStatus.FailedInWales
    };

    [Fact]
    public async Task Put_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentApiClient(roles: []);

        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQts());

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v3/persons/{person.Trn}/cpd-induction")
        {
            Content = CreateJsonContent(new
            {
                status = InductionStatus.RequiredToComplete,
                modifiedOn = Clock.UtcNow
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

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v3/persons/{trn}/cpd-induction")
        {
            Content = CreateJsonContent(new
            {
                status = InductionStatus.RequiredToComplete,
                modifiedOn = Clock.UtcNow
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

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v3/persons/{person.Trn}/cpd-induction")
        {
            Content = CreateJsonContent(new
            {
                status = InductionStatus.RequiredToComplete,
                modifiedOn = Clock.UtcNow
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Act
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.PersonDoesNotHaveQts, StatusCodes.Status400BadRequest);
    }

    [Theory]
    [InlineData(InductionStatus.None)]
    [InlineData(InductionStatus.Exempt)]
    [InlineData(InductionStatus.FailedInWales)]
    public async Task Put_StatusIsInvalid_ReturnsError(InductionStatus status)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQts());

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v3/persons/{person.Trn}/cpd-induction")
        {
            Content = CreateJsonContent(new
            {
                status = status,
                modifiedOn = Clock.UtcNow
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Act
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.InvalidInductionStatus, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Put_TimestampIsBeforePreviousUpdate_ReturnsConflict()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQts());

        var startDate = person.QtsDate!.Value.AddDays(6);

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);

            person.Person.SetCpdInductionStatus(
                InductionStatus.InProgress,
                startDate,
                completedDate: null,
                cpdModifiedOn: Clock.UtcNow,
                updatedBy: SystemUser.SystemUserId,
                now: Clock.UtcNow,
                out _);

            await dbContext.SaveChangesAsync();
        });

        var completedDate = startDate.AddMonths(12);

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v3/persons/{person.Trn}/cpd-induction")
        {
            Content = CreateJsonContent(new
            {
                status = InductionStatus.Passed,
                startDate = startDate,
                completedDate = completedDate,
                modifiedOn = Clock.UtcNow.AddDays(-1)
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Act
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.StaleRequest, StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task Put_RequiredToCompleteWithStartDate_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQts());

        var startDate = person.QtsDate!.Value.AddDays(6);

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v3/persons/{person.Trn}/cpd-induction")
        {
            Content = CreateJsonContent(new
            {
                status = InductionStatus.RequiredToComplete,
                startDate = startDate,
                modifiedOn = Clock.UtcNow
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Act
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.InductionStartDateIsNotPermitted, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Put_RequiredToCompleteWithCompletedDate_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQts());

        var startDate = person.QtsDate!.Value.AddDays(6);
        var completedDate = startDate.AddMonths(12);

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v3/persons/{person.Trn}/cpd-induction")
        {
            Content = CreateJsonContent(new
            {
                status = InductionStatus.RequiredToComplete,
                completedDate = completedDate,
                modifiedOn = Clock.UtcNow
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Act
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.InductionCompletedDateIsNotPermitted, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Put_InProgressWithoutStartDate_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQts());

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v3/persons/{person.Trn}/cpd-induction")
        {
            Content = CreateJsonContent(new
            {
                status = InductionStatus.InProgress,
                modifiedOn = Clock.UtcNow
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Act
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.InductionStartDateIsRequired, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Put_InProgressWithCompletedDate_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQts());

        var startDate = person.QtsDate!.Value.AddDays(6);
        var completedDate = startDate.AddMonths(12);

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v3/persons/{person.Trn}/cpd-induction")
        {
            Content = CreateJsonContent(new
            {
                status = InductionStatus.InProgress,
                startDate = startDate,
                completedDate = completedDate,
                modifiedOn = Clock.UtcNow
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Act
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.InductionCompletedDateIsNotPermitted, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Put_FailedWithoutStartDate_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQts());

        var startDate = person.QtsDate!.Value.AddDays(6);
        var completedDate = startDate.AddMonths(12);

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v3/persons/{person.Trn}/cpd-induction")
        {
            Content = CreateJsonContent(new
            {
                status = InductionStatus.Failed,
                completedDate = completedDate,
                modifiedOn = Clock.UtcNow
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Act
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.InductionStartDateIsRequired, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Put_FailedWithoutCompletedDate_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQts());

        var startDate = person.QtsDate!.Value.AddDays(6);
        var completedDate = startDate.AddMonths(12);

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v3/persons/{person.Trn}/cpd-induction")
        {
            Content = CreateJsonContent(new
            {
                status = InductionStatus.Failed,
                startDate = startDate,
                modifiedOn = Clock.UtcNow
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Act
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.InductionCompletedDateIsRequired, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Put_PassedWithoutStartDate_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQts());

        var startDate = person.QtsDate!.Value.AddDays(6);
        var completedDate = startDate.AddMonths(12);

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v3/persons/{person.Trn}/cpd-induction")
        {
            Content = CreateJsonContent(new
            {
                status = InductionStatus.Passed,
                completedDate = completedDate,
                modifiedOn = Clock.UtcNow
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Act
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.InductionStartDateIsRequired, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Put_PassedWithoutCompletedDate_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQts());

        var startDate = person.QtsDate!.Value.AddDays(6);

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v3/persons/{person.Trn}/cpd-induction")
        {
            Content = CreateJsonContent(new
            {
                status = InductionStatus.Passed,
                startDate = startDate,
                modifiedOn = Clock.UtcNow
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Act
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.InductionCompletedDateIsRequired, StatusCodes.Status400BadRequest);
    }

    [Theory]
    [MemberData(nameof(AllStatusesExceptNoneData))]
    public async Task Put_ValidRequestWithRequiredToComplete_UpdatesDbAndReturnsNoContent(InductionStatus currentStatus)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQts()
            .WithInductionStatus(currentStatus));

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v3/persons/{person.Trn}/cpd-induction")
        {
            Content = CreateJsonContent(new
            {
                status = InductionStatus.RequiredToComplete,
                modifiedOn = Clock.UtcNow
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Act
        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AllStatusesExceptNoneData))]
    public async Task Put_ValidRequestWithInProgress_UpdatesDbAndReturnsNoContent(InductionStatus currentStatus)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQts()
            .WithInductionStatus(currentStatus));

        var startDate = person.QtsDate!.Value.AddDays(6);

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v3/persons/{person.Trn}/cpd-induction")
        {
            Content = CreateJsonContent(new
            {
                status = InductionStatus.InProgress,
                startDate = startDate,
                modifiedOn = Clock.UtcNow
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Act
        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AllStatusesExceptNoneData))]
    public async Task Put_ValidRequestWithFailed_UpdatesDbAndReturnsNoContent(InductionStatus currentStatus)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQts()
            .WithInductionStatus(currentStatus));

        var startDate = person.QtsDate!.Value.AddDays(6);
        var completedDate = startDate.AddMonths(12);

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v3/persons/{person.Trn}/cpd-induction")
        {
            Content = CreateJsonContent(new
            {
                status = InductionStatus.Failed,
                startDate = startDate,
                completedDate = completedDate,
                modifiedOn = Clock.UtcNow
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Act
        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AllStatusesExceptNoneData))]
    public async Task Put_ValidRequestWithPassed_UpdatesDbAndReturnsNoContent(InductionStatus currentStatus)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQts()
            .WithInductionStatus(currentStatus));

        var startDate = person.QtsDate!.Value.AddDays(6);
        var completedDate = startDate.AddMonths(12);

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v3/persons/{person.Trn}/cpd-induction")
        {
            Content = CreateJsonContent(new
            {
                status = InductionStatus.Passed,
                startDate = startDate,
                completedDate = completedDate,
                modifiedOn = Clock.UtcNow
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Act
        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);
    }
}
