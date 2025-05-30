namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus;

public static class QuestionDriverHelper
{
    public static FieldRequirement FieldRequired(FieldRequirement routeFieldRequirement, FieldRequirement statusFieldRequirement)
    {
        if (routeFieldRequirement == FieldRequirement.NotApplicable || statusFieldRequirement == FieldRequirement.NotApplicable)
        {
            return FieldRequirement.NotApplicable;
        }
        else if (routeFieldRequirement == FieldRequirement.Mandatory || statusFieldRequirement == FieldRequirement.Mandatory)
        {
            return FieldRequirement.Mandatory;
        }
        else
        {
            return FieldRequirement.Optional;
        }
    }
}
