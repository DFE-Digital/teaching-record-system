using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Provider;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Specialism;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.StartDate;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Status;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq;

public class EditMqLinkGenerator(LinkGenerator linkGenerator)
{
    public EditMqProviderLinkGenerator Provider => new(linkGenerator);
    public EditMqSpecialismLinkGenerator Specialism => new(linkGenerator);
    public EditMqStartDateLinkGenerator StartDate => new(linkGenerator);
    public EditMqStatusLinkGenerator Status => new(linkGenerator);
}
