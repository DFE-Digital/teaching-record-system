namespace TeachingRecordSystem.SupportUi.Infrastructure.Filters;

public class FilterOrders
{
    public const int CheckUserExistsFilterOrder = int.MinValue;
    public const int RedirectWithPersonIdFilterOrder = -1000;
    public const int RequireFeatureEnabledFilterOrder = -300;
    public const int CheckPersonExistsFilterOrder = -200;
    public const int CheckMandatoryQualificationExistsFilterOrder = -200;
    public const int CheckSupportTaskExistsFilterOrder = -200;
    public const int CheckPersonCanBeMergedFilterOrder = -150;
    public const int ActivateInstanceFilterOrder = -100; // Form Flow
    public const int MissingInstanceFilterOrder = -100; // Form Flow
    public const int CheckJourneyStepsFilterOrder = -10; // Form Flow
    public const int CheckAlertExistsFilterOrder = 0;
    public const int CheckRouteToProfessionalStatusExistsFilterOrder = 0;
    public const int RequireOpenAlertFilterOrder = 1;
    public const int RequireClosedAlertFilterOrder = 1;

    public const int RequireActivePersonFilterOrder = 100;
}
