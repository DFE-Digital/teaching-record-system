using TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.ConnectPerson;

namespace TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail;

public class OneLoginDetailLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(string oneLoginUserSubject) =>
        linkGenerator.GetRequiredPathByPage("/OneLogins/OneLoginDetail/Index", routeValues: new { oneLoginUserSubject });

    public ConnectPersonLinkGenerator ConnectPerson { get; } = new(linkGenerator);
}
