namespace TeachingRecordSystem.SupportUi;

public partial class TrsLinkGenerator
{
    public string AlertDetail(Guid alertId) => GetRequiredPathByPage("/Alerts/AlertDetail", routeValues: new { alertId });

    public string AlertAdd(Guid personId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/AddAlert/Index", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string AlertAddType(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Alerts/AddAlert/Type", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string AlertAddTypeCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/AddAlert/Type", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string AlertAddDetails(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Alerts/AddAlert/Details", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string AlertAddDetailsCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/AddAlert/Details", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string AlertAddLink(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Alerts/AddAlert/Link", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string AlertAddLinkCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/AddAlert/Link", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string AlertAddStartDate(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Alerts/AddAlert/StartDate", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string AlertAddStartDateCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/AddAlert/StartDate", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string AlertAddReason(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Alerts/AddAlert/Reason", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string AlertAddReasonCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/AddAlert/Reason", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string AlertAddCheckAnswers(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/AddAlert/CheckAnswers", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string AlertAddConfirmCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/AddAlert/CheckAnswers", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string AlertEditDetails(Guid alertId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Alerts/EditAlert/Details/Index", routeValues: new { alertId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string AlertEditDetailsCancel(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/EditAlert/Details/Index", "cancel", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string AlertEditDetailsReason(Guid alertId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Alerts/EditAlert/Details/Reason", routeValues: new { alertId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string AlertEditDetailsReasonCancel(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/EditAlert/Details/Reason", "cancel", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string AlertEditDetailsCheckAnswers(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/EditAlert/Details/CheckAnswers", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string AlertEditDetailsCheckAnswersCancel(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/EditAlert/Details/CheckAnswers", "cancel", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string AlertEditStartDate(Guid alertId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Alerts/EditAlert/StartDate/Index", routeValues: new { alertId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string AlertEditStartDateCancel(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/EditAlert/StartDate/Index", "cancel", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string AlertEditStartDateReason(Guid alertId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Alerts/EditAlert/StartDate/Reason", routeValues: new { alertId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string AlertEditStartDateReasonCancel(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/EditAlert/StartDate/Reason", "cancel", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string AlertEditStartDateCheckAnswers(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/EditAlert/StartDate/CheckAnswers", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string AlertEditStartDateCheckAnswersCancel(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/EditAlert/StartDate/CheckAnswers", "cancel", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string AlertEditEndDate(Guid alertId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Alerts/EditAlert/EndDate/Index", routeValues: new { alertId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string AlertEditEndDateCancel(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/EditAlert/EndDate/Index", "cancel", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string AlertEditEndDateReason(Guid alertId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Alerts/EditAlert/EndDate/Reason", routeValues: new { alertId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string AlertEditEndDateReasonCancel(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/EditAlert/EndDate/Reason", "cancel", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string AlertEditEndDateCheckAnswers(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/EditAlert/EndDate/CheckAnswers", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string AlertEditEndDateCheckAnswersCancel(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/EditAlert/EndDate/CheckAnswers", "cancel", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string AlertEditLink(Guid alertId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Alerts/EditAlert/Link/Index", routeValues: new { alertId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string AlertEditLinkCancel(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/EditAlert/Link/Index", "cancel", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string AlertEditLinkReason(Guid alertId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Alerts/EditAlert/Link/Reason", routeValues: new { alertId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string AlertEditLinkReasonCancel(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/EditAlert/Link/Reason", "cancel", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string AlertEditLinkCheckAnswers(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/EditAlert/Link/CheckAnswers", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string AlertEditLinkCheckAnswersCancel(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/EditAlert/Link/CheckAnswers", "cancel", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string AlertClose(Guid alertId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Alerts/CloseAlert/Index", routeValues: new { alertId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string AlertCloseCancel(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/CloseAlert/Index", "cancel", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string AlertCloseReason(Guid alertId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Alerts/CloseAlert/Reason", routeValues: new { alertId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string AlertCloseReasonCancel(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/CloseAlert/Reason", "cancel", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string AlertCloseCheckAnswers(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/CloseAlert/CheckAnswers", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string AlertCloseCheckAnswersCancel(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/CloseAlert/CheckAnswers", "cancel", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string AlertReopen(Guid alertId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/ReopenAlert/Index", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string AlertReopenCancel(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/ReopenAlert/Index", "cancel", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string AlertReopenCheckAnswers(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/ReopenAlert/CheckAnswers", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string AlertReopenCheckAnswersCancel(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/ReopenAlert/CheckAnswers", "cancel", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string AlertDelete(Guid alertId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Alerts/DeleteAlert/Index", routeValues: new { alertId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string AlertDeleteCancel(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/DeleteAlert/Index", "cancel", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string AlertDeleteCheckAnswers(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/DeleteAlert/CheckAnswers", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string AlertDeleteCheckAnswersCancel(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/DeleteAlert/CheckAnswers", "cancel", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);
}
