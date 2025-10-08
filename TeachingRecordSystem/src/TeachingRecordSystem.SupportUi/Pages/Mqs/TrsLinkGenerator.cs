namespace TeachingRecordSystem.SupportUi;

public partial class TrsLinkGenerator
{
    public string MqAdd(Guid personId) =>
        GetRequiredPathByPage("/Mqs/AddMq/Index", routeValues: new { personId });

    public string MqAddProvider(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Mqs/AddMq/Provider", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string MqAddProviderCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/AddMq/Provider", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string MqAddSpecialism(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Mqs/AddMq/Specialism", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string MqAddSpecialismCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/AddMq/Specialism", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string MqAddStartDate(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Mqs/AddMq/StartDate", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string MqAddStartDateCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/AddMq/StartDate", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string MqAddStatus(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Mqs/AddMq/Status", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string MqAddStatusCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/AddMq/Status", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string MqAddCheckAnswers(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Mqs/AddMq/CheckAnswers", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string MqAddCheckAnswersCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/AddMq/CheckAnswers", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string MqEditProvider(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/Provider/Index", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditProviderCancel(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/Provider/Index", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditProviderChangeReason(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/Provider/Reason", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditProviderChangeReasonCancel(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/Provider/Reason", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditProviderCheckAnswers(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/Provider/CheckAnswers", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditProviderCheckAnswersCancel(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/Provider/CheckAnswers", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditSpecialism(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/Specialism/Index", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditSpecialismCancel(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/Specialism/Index", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditSpecialismChangeReason(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/Specialism/Reason", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditSpecialismChangeReasonCancel(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/Specialism/Reason", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditSpecialismCheckAnswers(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/Specialism/CheckAnswers", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditSpecialismCheckAnswersCancel(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/Specialism/CheckAnswers", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditStartDate(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/StartDate/Index", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditStartDateCancel(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/StartDate/Index", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditStartDateChangeReason(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/StartDate/Reason", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditStartDateChangeReasonCancel(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/StartDate/Reason", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditStartDateCheckAnswers(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/StartDate/CheckAnswers", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditStartDateCheckAnswersCancel(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/StartDate/CheckAnswers", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditStatus(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/Status/Index", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditStatusCancel(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/Status/Index", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditStatusChangeReason(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/Status/Reason", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditStatusChangeReasonCancel(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/Status/Reason", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditStatusCheckAnswersConfirm(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/Status/CheckAnswers", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditStatusCheckAnswersCancel(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/Status/CheckAnswers", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqDelete(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/DeleteMq/Index", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqDeleteCancel(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/DeleteMq/Index", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqDeleteCheckAnswers(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/DeleteMq/CheckAnswers", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqDeleteCheckAnswersCancel(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/DeleteMq/CheckAnswers", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
}
