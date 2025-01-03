using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), RequireJourneyInstance]
public class CheckYourAnswersModel : CommonJourneyPage
{
    public CheckYourAnswersModel(TrsLinkGenerator linkGenerator) : base(linkGenerator)
    {
    }

    public string BackLink => PageLink(InductionJourneyPage.ChangeReasons);

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        // TODO - end of journey logic

        return Redirect(NextPage()(PersonId, JourneyInstance!.InstanceId));
    }

    public Func<Guid, JourneyInstanceId, string> NextPage()
    {
        return (Id, journeyInstanceId) => LinkGenerator.PersonInduction(Id);
    }
}
