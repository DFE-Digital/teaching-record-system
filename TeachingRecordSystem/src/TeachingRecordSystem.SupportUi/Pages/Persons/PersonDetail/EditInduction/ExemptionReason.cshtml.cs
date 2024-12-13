namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

public class ExemptionReasonModel : CommonJourneyPage
{
    public ExemptionReasonModel(TrsLinkGenerator linkGenerator) : base(linkGenerator)
    {
    }

    public void OnGet()
    {
    }

    public void OnPost()
    {
        // TODO - store the exemption reason

        Redirect(NextPage()(PersonId, JourneyInstance!.InstanceId));
    }

    private Func<Guid, JourneyInstanceId, string> NextPage()
    {
        return (Id, journeyInstanceId) => _linkGenerator.InductionChangeReason(Id, journeyInstanceId);
    }

}
