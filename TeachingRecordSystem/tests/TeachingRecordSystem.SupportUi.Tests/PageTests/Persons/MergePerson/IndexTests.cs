namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.MergePerson;

public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_RedirectsToEnterTrn()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/merge");

        // Act
        var response = await HttpClient.SendAsync(request);  // Initializes journey
        response = await response.FollowRedirectAsync(HttpClient);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/persons/{person.PersonId}/merge/enter-trn", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var nonExistentPersonId = Guid.NewGuid();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{nonExistentPersonId}/merge");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_PersonHasOpenAlert_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a.WithEndDate(null)));
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/merge");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_PersonHasMandatoryQualification_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithMandatoryQualification());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/merge");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(InductionStatus.InProgress, false)]
    [InlineData(InductionStatus.Passed, false)]
    [InlineData(InductionStatus.Failed, false)]
    [InlineData(InductionStatus.None, true)]
    [InlineData(InductionStatus.Exempt, false)]
    [InlineData(InductionStatus.FailedInWales, false)]
    [InlineData(InductionStatus.RequiredToComplete, false)]
    public async Task Get_PersonWithInductionStatus_ReturnsExpectedResult(InductionStatus status, bool expectIsValid)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithInductionStatus(i => i
                .WithStatus(status)
                .WithStartDate(new DateOnly(2024, 1, 1))
                .WithCompletedDate(new DateOnly(2024, 1, 1))));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/merge");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        if (expectIsValid)
        {
            response = await response.FollowRedirectAsync(HttpClient);
            Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
            Assert.StartsWith($"/persons/{person.PersonId}/merge/enter-trn", response.Headers.Location?.OriginalString);
        }
        else
        {
            Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        }
    }

    [Fact]
    public async Task Get_PersonHasQts_ReturnsBadRequest()
    {
        // Arrange
        var awardDate = Clock.Today;
        var person = await TestData.CreatePersonAsync(p => p
            .WithQts(awardDate));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/merge");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_PersonHasQtls_ReturnsBadRequest()
    {
        // Arrange
        var awardDate = Clock.Today;
        var person = await TestData.CreatePersonAsync(p => p
            .WithQtls(awardDate));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/merge");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_PersonHasEyts_ReturnsBadRequest()
    {
        // Arrange
        var awardDate = Clock.Today;
        var person = await TestData.CreatePersonAsync(p => p
            .WithHoldsRouteToProfessionalStatus(ProfessionalStatusType.EarlyYearsTeacherStatus, awardDate));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/merge");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_PersonHasEyps_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithHoldsRouteToProfessionalStatus(ProfessionalStatusType.EarlyYearsProfessionalStatus));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/merge");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_PersonHasPqts_ReturnsBadRequest()
    {
        // Arrange
        var awardDate = Clock.Today;
        var person = await TestData.CreatePersonAsync(p => p
            .WithHoldsRouteToProfessionalStatus(ProfessionalStatusType.PartialQualifiedTeacherStatus, awardDate));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/merge");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }
}
