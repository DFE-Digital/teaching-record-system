using Microsoft.AspNetCore.WebUtilities;
using TeachingRecordSystem.SupportUi.Infrastructure.ModelBinding;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.IntegrationTransactions;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequestManualChecksNeeded;

namespace TeachingRecordSystem.SupportUi;

public partial class TrsLinkGenerator(LinkGenerator linkGenerator)
{
    protected const string DateOnlyFormat = DateOnlyModelBinder.Format;

    public string Index() => GetRequiredPathByPage("/Index");

    public string SignOut() => GetRequiredPathByPage("/SignOut");

    public string SignedOut() => GetRequiredPathByPage("/SignedOut");

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

    public string EditChangeRequest(string ticketNumber) => GetRequiredPathByPage("/ChangeRequests/EditChangeRequest/Index", routeValues: new { ticketNumber });

    public string ChangeRequestDocument(string ticketNumber, Guid documentId) => GetRequiredPathByPage("/ChangeRequests/EditChangeRequest/Index", "documents", routeValues: new { ticketNumber, id = documentId });

    public string AcceptChangeRequest(string ticketNumber) => GetRequiredPathByPage("/ChangeRequests/EditChangeRequest/Accept", routeValues: new { ticketNumber });

    public string RejectChangeRequest(string ticketNumber) => GetRequiredPathByPage("/ChangeRequests/EditChangeRequest/Reject", routeValues: new { ticketNumber });

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

    public string MqEditProviderReason(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/Provider/Reason", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditProviderConfirm(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/Provider/Confirm", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditProviderConfirmCancel(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/Provider/Confirm", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditSpecialism(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/Specialism/Index", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditSpecialismReason(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/Specialism/Reason", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditSpecialismCancel(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/Specialism/Index", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditSpecialismConfirm(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/Specialism/Confirm", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditSpecialismConfirmCancel(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/Specialism/Confirm", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditStartDate(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/StartDate/Index", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditStartDateReason(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/StartDate/Reason", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditStartDateCancel(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/StartDate/Index", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditStartDateConfirm(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/StartDate/Confirm", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditStartDateConfirmCancel(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/StartDate/Confirm", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditStatus(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/Status/Index", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditStatusReason(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/Status/Reason", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditStatusCancel(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/Status/Index", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditStatusConfirm(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/Status/Confirm", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqEditStatusConfirmCancel(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/EditMq/Status/Confirm", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqDelete(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/DeleteMq/Index", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqDeleteCancel(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/DeleteMq/Index", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqDeleteConfirm(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/DeleteMq/Confirm", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string MqDeleteConfirmCancel(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/DeleteMq/Confirm", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string LegacyUsers() => GetRequiredPathByPage("/LegacyUsers/Index");

    public string LegacyAddUser() => GetRequiredPathByPage("/LegacyUsers/AddUser/Index");

    public string LegacyAddUserConfirm(string userId) => GetRequiredPathByPage("/LegacyUsers/AddUser/Confirm", routeValues: new { userId });

    public string LegacyEditUser(Guid userId) => GetRequiredPathByPage("/LegacyUsers/EditUser", routeValues: new { userId });

    public string Users(string? keywords = null, string? status = null, string? role = null, int? pageNumber = null) =>
        GetRequiredPathByPage("/Users/Index", routeValues: new { keywords, status, role, pageNumber });

    public string AddUser() => GetRequiredPathByPage("/Users/AddUser/Index");

    public string AddUserConfirm(string userId) => GetRequiredPathByPage("/Users/AddUser/Confirm", routeValues: new { userId });

    public string EditUser(Guid userId) => GetRequiredPathByPage("/Users/EditUser/Index", routeValues: new { userId });

    public string EditUserDeactivate(Guid userId) => GetRequiredPathByPage("/Users/EditUser/Deactivate", routeValues: new { userId });

    public string EditUserDeactivateCancel(Guid userId) => GetRequiredPathByPage("/Users/EditUser/Deactivate", "cancel", routeValues: new { userId });

    public string ApplicationUsers() => GetRequiredPathByPage("/ApplicationUsers/Index");

    public string AddApplicationUser() => GetRequiredPathByPage("/ApplicationUsers/AddApplicationUser");

    public string EditApplicationUser(Guid userId) => GetRequiredPathByPage("/ApplicationUsers/EditApplicationUser", routeValues: new { userId });

    public string AddApiKey(Guid applicationUserId) => GetRequiredPathByPage("/ApiKeys/AddApiKey", routeValues: new { applicationUserId });

    public string EditApiKey(Guid apiKeyId) => GetRequiredPathByPage("/ApiKeys/EditApiKey", routeValues: new { apiKeyId });

    public string IntegrationTransactionDetail(long integrationTransactionId) => GetRequiredPathByPage("/SupportTasks/IntegrationTransactions/Detail", routeValues: new { integrationTransactionId });

    public string ExpireApiKey(Guid apiKeyId) => GetRequiredPathByPage("/ApiKeys/EditApiKey", handler: "Expire", routeValues: new { apiKeyId });

    public string SupportTasks(SupportTaskCategory[]? categories = null, Pages.SupportTasks.IndexModel.SortByOption? sortBy = null, string? reference = null, bool? filtersApplied = null) =>
        GetRequiredPathByPage("/SupportTasks/Index", routeValues: new { category = categories, sortBy, reference, _f = filtersApplied == true ? "1" : null });

    public string SupportTaskDetail(string supportTaskReference, SupportTaskType supportTaskType) =>
        supportTaskReference.StartsWith("CAS-") ? EditChangeRequest(supportTaskReference) :
        supportTaskType switch
        {
            SupportTaskType.ConnectOneLoginUser => ConnectOneLoginUserSupportTask(supportTaskReference),
            SupportTaskType.ApiTrnRequest => ApiTrnRequestMatches(supportTaskReference),
            SupportTaskType.TrnRequestManualChecksNeeded => ResolveTrnRequestManualChecksNeeded(supportTaskReference),
            SupportTaskType.NpqTrnRequest => NpqTrnRequestStartPage(supportTaskReference),
            _ => throw new ArgumentException($"Unknown {nameof(SupportTaskType)}: '{supportTaskType}'.", nameof(supportTaskType))
        };

    public string ApiTrnRequests(string? search = null, ApiTrnRequestsSortByOption? sortBy = null, SortDirection? sortDirection = null, int? pageNumber = null, string? waitForJobId = null) =>
        GetRequiredPathByPage("/SupportTasks/ApiTrnRequests/Index", routeValues: new { search, sortBy, sortDirection, pageNumber, waitForJobId });

    public string ApiTrnRequestMatches(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null) =>
        GetRequiredPathByPage("/SupportTasks/ApiTrnRequests/Resolve/Matches", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId);

    public string ApiTrnRequestMatchesCancel(string supportTaskReference, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/SupportTasks/ApiTrnRequests/Resolve/Matches", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId, handler: "Cancel");

    public string ApiTrnRequestMerge(string supportTaskReference, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/SupportTasks/ApiTrnRequests/Resolve/Merge", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId);

    public string NpqTrnRequestStartPage(string supportTaskReference) =>
        GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/Index", routeValues: new { supportTaskReference });

    public string NpqTrnRequestStartPageCancel(string supportTaskReference) =>
    GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/Index", routeValues: new { supportTaskReference }, handler: "Cancel");

    public string NpqTrnRequestMatches(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null) =>
        GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/Resolve/Matches", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId);

    public string NpqTrnRequestMatchesCancel(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null) =>
        GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/Resolve/Matches", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId, handler: "Cancel");

    public string NpqTrnRequestMerge(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null) =>
        GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/Resolve/Merge", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId);

    public string NpqTrnRequestMergeCancel(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null) =>
        GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/Resolve/Merge", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId, handler: "Cancel");

    public string NpqTrnRequestCheckAnswers(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null) =>
        GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/Resolve/CheckAnswers", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId);

    public string NpqTrnRequestCheckAnswersCancel(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null) =>
        GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/Resolve/CheckAnswers", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId, handler: "Cancel");

    public string NpqTrnRequestRejectionReason(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null) =>
        GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/Reject/RejectionReason", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId);

    public string NpqTrnRequestRejectionReasonCancel(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null) =>
        GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/Reject/RejectionReason", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId, handler: "Cancel");

    public string NpqTrnRequestNoMatchesCheckAnswers(string supportTaskReference) =>
        GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/NoMatches/CheckAnswers", routeValues: new { supportTaskReference });

    public string NpqTrnRequestNoMatchesCheckAnswersCancel(string supportTaskReference) =>
        GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/NoMatches/CheckAnswers", routeValues: new { supportTaskReference }, handler: "Cancel");

    public string IntegrationTransactions(IntegrationTransactionSortByOption? sortBy = null, SortDirection? sortDirection = null, int? pageNumber = null) =>
        GetRequiredPathByPage("/SupportTasks/IntegrationTransactions/Index", routeValues: new { sortBy, sortDirection, pageNumber });

    public string IntegrationTransactionDetail(IntegrationTransactionRecordSortByOption? sortBy = null, SortDirection? sortDirection = null, int? pageNumber = 1, long? IntegrationTransactionId = null) =>
        GetRequiredPathByPage($"/SupportTasks/IntegrationTransactions/detail", routeValues: new { sortBy, sortDirection, pageNumber, IntegrationTransactionId });

    public string ApiTrnRequestMergeCancel(string supportTaskReference, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/SupportTasks/ApiTrnRequests/Resolve/Merge", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId, handler: "Cancel");

    public string ApiTrnRequestCheckAnswers(string supportTaskReference, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/SupportTasks/ApiTrnRequests/Resolve/CheckAnswers", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId);

    public string ApiTrnRequestCheckAnswersCancel(string supportTaskReference, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/SupportTasks/ApiTrnRequests/Resolve/CheckAnswers", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId, handler: "Cancel");

    public string ConnectOneLoginUserSupportTask(string supportTaskReference) =>
        GetRequiredPathByPage("/SupportTasks/ConnectOneLoginUser/Index", routeValues: new { supportTaskReference });

    public string ConnectOneLoginUserSupportTaskConnect(string supportTaskReference, string trn) =>
        GetRequiredPathByPage("/SupportTasks/ConnectOneLoginUser/Connect", routeValues: new { supportTaskReference, trn });

    public string TrnRequestManualChecksNeeded(string? search = null, TrnRequestManualChecksNeededSortByOption? sortBy = null, SortDirection? sortDirection = null, int? pageNumber = null) =>
        GetRequiredPathByPage("/SupportTasks/TrnRequestManualChecksNeeded/Index", routeValues: new { search, sortBy, sortDirection, pageNumber });

    public string ResolveTrnRequestManualChecksNeeded(string supportTaskReference) =>
        GetRequiredPathByPage("/SupportTasks/TrnRequestManualChecksNeeded/Resolve/Index", routeValues: new { supportTaskReference });

    public string ResolveTrnRequestManualChecksNeededConfirm(string supportTaskReference) =>
        GetRequiredPathByPage("/SupportTasks/TrnRequestManualChecksNeeded/Resolve/Confirm", routeValues: new { supportTaskReference });

    private string GetRequiredPathByPage(string page, string? handler = null, object? routeValues = null, JourneyInstanceId? journeyInstanceId = null)
    {
        var url = linkGenerator.GetPathByPage(page, handler, values: routeValues) ?? throw new InvalidOperationException("Page was not found.");

        if (journeyInstanceId?.UniqueKey is string journeyInstanceUniqueKey)
        {
            url = QueryHelpers.AddQueryString(url, WebCommon.FormFlow.Constants.UniqueKeyQueryParameterName, journeyInstanceUniqueKey);
        }

        return url;
    }
}
