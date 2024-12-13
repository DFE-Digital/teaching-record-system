using TeachingRecordSystem.SupportUi.Services.InductionWizardPageLogic;

namespace TeachingRecordSystem.SupportUi.Services.InductionRouting;

public static class InductionWizardPageLogicService
{
    public static InductionJourneyPage NextPageFromStatusPage(InductionStatus status)
    {
        if (status == InductionStatus.Exempt)
        {
            return InductionJourneyPage.ExemptionReason;
        }
        if (status == InductionStatus.RequiredToComplete)
        {
            return InductionJourneyPage.ChangeReason;
        }
        else
        {
            return InductionJourneyPage.StartDate;
        }
    }
}
