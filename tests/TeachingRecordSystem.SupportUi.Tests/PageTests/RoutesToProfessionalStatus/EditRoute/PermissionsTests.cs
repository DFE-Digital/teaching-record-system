using System.Net;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.DeleteRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.EditRoute;

public class PermissionsTests(HostFixture hostFixture) : EditRouteTestBase(hostFixture), IAsyncLifetime
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

    private static readonly IReadOnlyCollection<(string, string)> _pageFormats = [
        // {0}: qualification ID
        // {1}: journey ID
        // {2}: person ID
        (JourneyNames.EditRouteToProfessionalStatus, "/routes/{0}/edit/reason?{1}"),
        (JourneyNames.EditRouteToProfessionalStatus, "/routes/{0}/edit/check-answers?{1}"),
        (JourneyNames.EditRouteToProfessionalStatus, "/routes/{0}/edit/country?{1}"),
        (JourneyNames.EditRouteToProfessionalStatus, "/routes/{0}/edit/degree-type?{1}"),
        (JourneyNames.EditRouteToProfessionalStatus, "/routes/{0}/edit/detail?{1}"),
        (JourneyNames.EditRouteToProfessionalStatus, "/routes/{0}/edit/holds-from?{1}"),
        (JourneyNames.EditRouteToProfessionalStatus, "/routes/{0}/edit/induction-exemption?{1}"),
        (JourneyNames.EditRouteToProfessionalStatus, "/routes/{0}/edit/start-and-end-date?{1}"),
        (JourneyNames.EditRouteToProfessionalStatus, "/routes/{0}/edit/status?{1}"),
        (JourneyNames.EditRouteToProfessionalStatus, "/routes/{0}/edit/subjects?{1}"),
        (JourneyNames.EditRouteToProfessionalStatus, "/routes/{0}/edit/training-provider?{1}"),
        (JourneyNames.DeleteRouteToProfessionalStatus, "/routes/{0}/delete/reason?{1}"),
        (JourneyNames.DeleteRouteToProfessionalStatus, "/routes/{0}/delete/check-answers?{1}")
    ];

    private Guid _personId;
    private Guid _qualificationId;
    private RouteToProfessionalStatusType? _route;
    private RouteToProfessionalStatusStatus _status;

    async ValueTask IAsyncLifetime.InitializeAsync()
    {
        _route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .First(r => r.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional && r.InductionExemptionRequired != FieldRequirement.NotApplicable);

        _status = ProfessionalStatusStatusRegistry.All
            .First(s => s.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional && s.HoldsFromRequired == FieldRequirement.NotApplicable).Value;

        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(_route.RouteToProfessionalStatusTypeId)
                .WithStatus(_status)));

        _personId = person.PersonId;
        _qualificationId = person.Qualifications!.OfType<RouteToProfessionalStatus>().First().QualificationId;
    }

    ValueTask IAsyncDisposable.DisposeAsync() => ValueTask.CompletedTask;

    [Theory]
    [MemberData(nameof(GetData))]
    public async Task Get_RoutesPage_UserRoles_CanViewPageAsExpected(string journeyName, string pageFormat, string? userRole, bool canViewPage)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: userRole));

        // This class covers both journeys, so it takes Edit Route's seeding from its base class and
        // seeds Delete Route itself.
        var journeyQueryParameter = journeyName switch
        {
            JourneyNames.EditRouteToProfessionalStatus =>
                (await CreateJourneyInstanceAsync(
                    _qualificationId,
                    new Pages.RoutesToProfessionalStatus.EditRoute.EditRouteState
                    {
                        RouteToProfessionalStatusId = _route!.RouteToProfessionalStatusTypeId,
                        Status = _status,
                        CurrentStatus = _status
                    })).GetUniqueIdQueryParameter(),

            _ => (await CreateDeleteRouteJourneyInstanceAsync()).GetUniqueIdQueryParameter()
        };

        var page = string.Format(pageFormat, _qualificationId, journeyQueryParameter, _personId);
        var request = new HttpRequestMessage(HttpMethod.Get, page);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        if (canViewPage)
        {
            Assert.Contains(response.StatusCode, new HttpStatusCode[] { HttpStatusCode.OK, HttpStatusCode.Found });
        }
        else
        {
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    // Seeds the whole path so that every Delete Route page is reachable.
    private Task<DeleteRouteJourneyCoordinator> CreateDeleteRouteJourneyInstanceAsync() =>
        JourneyHelper.CreateInstanceAsync<DeleteRouteJourneyCoordinator>(
            JourneyNames.DeleteRouteToProfessionalStatus,
            new RouteValueDictionary { ["qualificationId"] = _qualificationId },
            _ => Task.FromResult<object>(new DeleteRouteState
            {
                ChangeReason = ChangeReasonOption.RemovedQtlsStatus,
                ChangeReasonDetail = new ChangeReasonStateBuilder()
                    .WithValidChangeReasonDetail()
                    .Build()
            }),
            pathUrls:
            [
                $"/routes/{_qualificationId}/delete/reason",
                $"/routes/{_qualificationId}/delete/check-answers"
            ],
            coordinatorFactory: CreateJourneyCoordinator<DeleteRouteJourneyCoordinator>);

    public static (string JourneyName, string PageFormat, string? UserRole, bool CanViewPage)[] GetData()
    {
        var data = new List<(string, string, string?, bool)>();

        foreach (var (journeyName, pageFormat) in _pageFormats)
        {
            foreach (var (role, canEdit) in _roleAccess)
            {
                data.Add((journeyName, pageFormat, role, canEdit));
            }
        }

        return data.ToArray();
    }
}
