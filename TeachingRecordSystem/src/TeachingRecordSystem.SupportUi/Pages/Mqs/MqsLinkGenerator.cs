using TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;
using TeachingRecordSystem.SupportUi.Pages.Mqs.DeleteMq;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs;

public class MqsLinkGenerator(LinkGenerator linkGenerator)
{
    public AddMqLinkGenerator AddMq => new(linkGenerator);
    public EditMqLinkGenerator EditMq => new(linkGenerator);
    public DeleteMqLinkGenerator DeleteMq => new(linkGenerator);
}
