using System.Net;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.AddRoute;

public class PermissionsTests(HostFixture hostFixture) : AddRouteTestBase(hostFixture), IAsyncLifetime
{
    private static readonly IReadOnlyCollection<(string? UserRole, bool CanEdit)> _roleAccess = [
        (null, false),
        (UserRoles.Viewer, false),
        (UserRoles.AlertsManagerTra, false),
        (UserRoles.AlertsManagerTraDbs, false),
        (UserRoles.RecordManager, true),
        (UserRoles.AccessManager, true),
        (UserRoles.Administrator, true)
    ];

    private Guid _personId;
    private RouteToProfessionalStatusType? _route;
    private RouteToProfessionalStatusStatus _status;

    async ValueTask IAsyncLifetime.InitializeAsync()
    {
        _route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .First(r => r.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional && r.InductionExemptionRequired != FieldRequirement.NotApplicable);

        _status = ProfessionalStatusStatusRegistry.All
            .First(s => s.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional && s.HoldsFromRequired == FieldRequirement.NotApplicable).Value;

        var person = await TestData.CreatePersonAsync();
        _personId = person.PersonId;
    }

    ValueTask IAsyncDisposable.DisposeAsync() => ValueTask.CompletedTask;

    [Theory]
    [MemberData(nameof(GetData))]
    public async Task Get_UserRoles_CanViewPageAsExpected(AddRoutePage page, string? userRole, bool canViewPage)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: userRole));

        var journeyInstance = await CreateJourneyInstanceAsync(
            _personId,
            new AddRouteState
            {
                RouteToProfessionalStatusId = _route!.RouteToProfessionalStatusTypeId,
                Status = _status
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"{GetPageUrl(page, _personId)}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        if (canViewPage)
        {
            Assert.Contains(response.StatusCode, new[] { HttpStatusCode.OK, HttpStatusCode.Found });
        }
        else
        {
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    public static (AddRoutePage Page, string? UserRole, bool CanViewPage)[] GetData() =>
        Enum.GetValues<AddRoutePage>()
            .SelectMany(page => _roleAccess.Select(r => (page, r.UserRole, r.CanEdit)))
            .ToArray();
}
