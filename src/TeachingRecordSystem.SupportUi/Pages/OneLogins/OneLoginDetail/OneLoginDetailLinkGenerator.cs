using TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.ConnectPerson;
using TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.DisconnectPerson;

namespace TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail;

public class OneLoginDetailLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(string oneLoginUserSubject) =>
        linkGenerator.GetRequiredPathByPage("/OneLogins/OneLoginDetail/Index", routeValues: new { oneLoginUserSubject });

    public string ChangeHistory(string oneLoginUserSubject, int? pageNumber = null) =>
        linkGenerator.GetRequiredPathByPage("/OneLogins/OneLoginDetail/ChangeHistory", routeValues: new { oneLoginUserSubject, pageNumber });

    public ConnectPersonLinkGenerator ConnectPerson { get; } = new(linkGenerator);

    public DisconnectPersonLinkGenerator DisconnectPerson { get; } = new(linkGenerator);
}
