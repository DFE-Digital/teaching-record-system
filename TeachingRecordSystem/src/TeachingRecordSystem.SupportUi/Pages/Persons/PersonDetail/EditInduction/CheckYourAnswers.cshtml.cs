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

    public void OnPost()
    {
        // TODO - store the exemption reason

        // Final page - do all the special stuff
    }
}
