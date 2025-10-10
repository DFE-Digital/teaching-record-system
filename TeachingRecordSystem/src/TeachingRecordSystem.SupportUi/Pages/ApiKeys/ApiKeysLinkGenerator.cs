using TeachingRecordSystem.SupportUi.Pages.ApiKeys.AddApiKey;
using TeachingRecordSystem.SupportUi.Pages.ApiKeys.EditApiKey;

namespace TeachingRecordSystem.SupportUi.Pages.ApiKeys;

public class ApiKeysLinkGenerator(LinkGenerator linkGenerator)
{
    public AddApiKeyLinkGenerator AddApiKey => new(linkGenerator);
    public EditApiKeyLinkGenerator EditApiKey => new(linkGenerator);
}
