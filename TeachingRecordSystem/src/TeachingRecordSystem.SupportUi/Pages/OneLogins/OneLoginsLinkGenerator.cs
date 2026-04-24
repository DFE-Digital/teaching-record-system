using TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail;

namespace TeachingRecordSystem.SupportUi.Pages.OneLogins;

public class OneLoginsLinkGenerator(LinkGenerator linkGenerator)
{
    public OneLoginDetailLinkGenerator OneLoginDetail { get; } = new(linkGenerator);
}
