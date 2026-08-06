using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditInduction;

public class PermissionsTests(HostFixture hostFixture) : EditInductionTestBase(hostFixture)
{
    [Theory]
    [MemberData(nameof(GetPagesForUserWithoutInductionWriteRoleForAllHttpMethodsData))]
    public async Task UserDoesNotHavePermission_ReturnsForbidden(string page, string? role, InductionStatus inductionStatus, HttpMethod httpMethod)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));

        var person = await TestData.CreatePersonAsync(
            p => p
                .WithQts()
                .WithInductionStatus(i => i
                    .WithStatus(inductionStatus)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus,
                StartDate = TimeProvider.Today.AddYears(-2)
            });

        var request = new HttpRequestMessage(httpMethod,
            $"/persons/{person.PersonId}/{page}?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(GetPagesForAllHttpMethodsData))]
    public async Task PersonIsDeactivated_ReturnsBadRequest(string page, InductionStatus inductionStatus, HttpMethod httpMethod)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person);
            person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus,
                StartDate = TimeProvider.Today.AddYears(-2)
            });

        var request = new HttpRequestMessage(httpMethod,
            $"/persons/{person.PersonId}/{page}?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    public static (string Page, InductionStatus InductionStatus, HttpMethod HttpMethod)[] GetPagesForAllHttpMethodsData()
    {
        var data = new List<(string, InductionStatus, HttpMethod)>();

        foreach (var (page, status) in _pagesAndValidStatuses)
        {
            data.Add((page, status, HttpMethod.Get));
            data.Add((page, status, HttpMethod.Post));
        }

        return data.ToArray();
    }

    public static (string Page, string? Role, InductionStatus InductionStatus, HttpMethod HttpMethod)[] GetPagesForUserWithoutInductionWriteRoleForAllHttpMethodsData()
    {
        var data = new List<(string, string?, InductionStatus, HttpMethod)>();

        foreach (var (page, status) in _pagesAndValidStatuses)
        {
            foreach (var role in _rolesWithoutWritePermission)
            {
                data.Add((page, role, status, HttpMethod.Get));
                data.Add((page, role, status, HttpMethod.Post));
            }
        }

        return data.ToArray();
    }

    private static readonly string?[] _rolesWithoutWritePermission = UserRoles.All
        .Except([UserRoles.RecordManager, UserRoles.AccessManager, UserRoles.Administrator])
        .Append(null)
        .ToArray();

    private static readonly (string, InductionStatus)[] _pagesAndValidStatuses =
    [
        ("edit-induction/status", InductionStatus.Exempt),
        ("edit-induction/status", InductionStatus.InProgress),
        ("edit-induction/status", InductionStatus.Failed),
        ("edit-induction/status", InductionStatus.FailedInWales),
        ("edit-induction/status", InductionStatus.Passed),
        ("edit-induction/status", InductionStatus.RequiredToComplete),
        ("edit-induction/exemption-reasons", InductionStatus.Exempt),
        ("edit-induction/start-date", InductionStatus.InProgress),
        ("edit-induction/start-date", InductionStatus.Failed),
        ("edit-induction/start-date", InductionStatus.FailedInWales),
        ("edit-induction/start-date", InductionStatus.Passed),
        ("edit-induction/date-completed", InductionStatus.Failed),
        ("edit-induction/date-completed", InductionStatus.FailedInWales),
        ("edit-induction/date-completed", InductionStatus.Passed),
        ("edit-induction/reason", InductionStatus.Exempt),
        ("edit-induction/reason", InductionStatus.InProgress),
        ("edit-induction/reason", InductionStatus.Failed),
        ("edit-induction/reason", InductionStatus.FailedInWales),
        ("edit-induction/reason", InductionStatus.Passed),
        ("edit-induction/reason", InductionStatus.RequiredToComplete)
    ];
}
