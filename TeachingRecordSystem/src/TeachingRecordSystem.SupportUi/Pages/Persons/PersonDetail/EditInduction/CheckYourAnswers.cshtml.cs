using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), RequireJourneyInstance]
public class CheckYourAnswersModel : CommonJourneyPage
{
    public CheckYourAnswersModel(TrsLinkGenerator linkGenerator) : base(linkGenerator)
    {
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost()
    {
        // Final page - complete the journey

        return Redirect(NextPage()(PersonId, JourneyInstance!.InstanceId));
    }

    private Func<Guid, JourneyInstanceId, string> NextPage()
    {
        return (Id, journeyInstanceId) => _linkGenerator.PersonInduction(Id);
    }
}
