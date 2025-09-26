using System.Net;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.DeleteRoute;
using TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.AddRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.EditRoute;

[Collection(nameof(DisableParallelization))]
public class PermissionsTests(HostFixture hostFixture) : TestBase(hostFixture), IAsyncLifetime
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
        (JourneyNames.AddRouteToProfessionalStatus, "/route/add/age-range?{1}&personId={2}"),
        (JourneyNames.AddRouteToProfessionalStatus, "/route/add/change-reason?{1}&personId={2}"),
        (JourneyNames.AddRouteToProfessionalStatus, "/route/add/check-answers?{1}&personId={2}"),
        (JourneyNames.AddRouteToProfessionalStatus, "/route/add/country?{1}&personId={2}"),
        (JourneyNames.AddRouteToProfessionalStatus, "/route/add/degree-type?{1}&personId={2}"),
        (JourneyNames.AddRouteToProfessionalStatus, "/route/add/holds-from?{1}&personId={2}"),
        (JourneyNames.AddRouteToProfessionalStatus, "/route/add/induction-exemption?{1}&personId={2}"),
        (JourneyNames.AddRouteToProfessionalStatus, "/route/add/route?{1}&personId={2}"),
        (JourneyNames.AddRouteToProfessionalStatus, "/route/add/start-and-end-date?{1}&personId={2}"),
        (JourneyNames.AddRouteToProfessionalStatus, "/route/add/status?{1}&personId={2}"),
        (JourneyNames.AddRouteToProfessionalStatus, "/route/add/subjects?{1}&personId={2}"),
        (JourneyNames.AddRouteToProfessionalStatus, "/route/add/training-provider?{1}&personId={2}"),
        (JourneyNames.EditRouteToProfessionalStatus, "/route/{0}/edit/change-reason?{1}"),
        (JourneyNames.EditRouteToProfessionalStatus, "/route/{0}/edit/check-answers?{1}"),
        (JourneyNames.EditRouteToProfessionalStatus, "/route/{0}/edit/country?{1}"),
        (JourneyNames.EditRouteToProfessionalStatus, "/route/{0}/edit/degree-type?{1}"),
        (JourneyNames.EditRouteToProfessionalStatus, "/route/{0}/edit/detail?{1}"),
        (JourneyNames.EditRouteToProfessionalStatus, "/route/{0}/edit/holds-from?{1}"),
        (JourneyNames.EditRouteToProfessionalStatus, "/route/{0}/edit/induction-exemption?{1}"),
        (JourneyNames.EditRouteToProfessionalStatus, "/route/{0}/edit/start-and-end-date?{1}"),
        (JourneyNames.EditRouteToProfessionalStatus, "/route/{0}/edit/status?{1}"),
        (JourneyNames.EditRouteToProfessionalStatus, "/route/{0}/edit/subjects?{1}"),
        (JourneyNames.EditRouteToProfessionalStatus, "/route/{0}/edit/training-provider?{1}"),
        (JourneyNames.DeleteRouteToProfessionalStatus, "/route/{0}/delete/change-reason?{1}"),
        (JourneyNames.DeleteRouteToProfessionalStatus, "/route/{0}/delete/check-answers?{1}")
    ];

    Guid _personId;
    Guid _qualificationId;
    RouteToProfessionalStatusType? _route;
    RouteToProfessionalStatusStatus _status;

    public async Task InitializeAsync()
    {
        _route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional && r.InductionExemptionRequired != FieldRequirement.NotApplicable)
            .First();

        _status = ProfessionalStatusStatusRegistry.All
.First(s => s.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional && s.HoldsFromRequired == FieldRequirement.NotApplicable).Value;

        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(_route.RouteToProfessionalStatusTypeId)
                .WithStatus(_status)));

        _personId = person.PersonId;
        _qualificationId = person.ProfessionalStatuses.First().QualificationId;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Theory]
    [MemberData(nameof(GetData))]
    public async Task Get_RoutesPage_UserRoles_CanViewPageAsExpected(string journeyName, string pageFormat, string? userRole, bool canViewPage)
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(userRole));

        JourneyInstance journey = journeyName switch
        {
            JourneyNames.EditRouteToProfessionalStatus => await CreateJourneyInstance(
                JourneyNames.EditRouteToProfessionalStatus,
                new EditRouteStateBuilder()
                    .WithRouteToProfessionalStatusId(_route!.RouteToProfessionalStatusTypeId)
                    .WithStatus(_status)
                    .Build(),
                new KeyValuePair<string, object>("qualificationId", _qualificationId)),

            JourneyNames.AddRouteToProfessionalStatus => await CreateJourneyInstance(
                JourneyNames.AddRouteToProfessionalStatus,
                new AddRouteStateBuilder()
                    .WithRouteToProfessionalStatusId(_route!.RouteToProfessionalStatusTypeId)
                    .WithStatus(_status)
                    .Build(),
                new KeyValuePair<string, object>("personId", _personId)),

            _ => await CreateJourneyInstance(
                JourneyNames.DeleteRouteToProfessionalStatus,
                new DeleteRouteState
                {
                    ChangeReason = ChangeReasonOption.RemovedQtlsStatus,
                    ChangeReasonDetail = new ChangeReasonStateBuilder()
                        .WithValidChangeReasonDetail()
                        .Build()
                },
                new KeyValuePair<string, object>("qualificationId", _qualificationId))
        };

        var page = string.Format(pageFormat, _qualificationId, journey.GetUniqueIdQueryParameter(), _personId);
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

    public static TheoryData<string, string, string?, bool> GetData()
    {
        var data = new TheoryData<string, string, string?, bool>();

        foreach (var (journeyName, pageFormat) in _pageFormats)
        {
            foreach (var (role, canEdit) in _roleAccess)
            {
                data.Add(journeyName, pageFormat, role, canEdit);
            }
        }

        return data;
    }
}
