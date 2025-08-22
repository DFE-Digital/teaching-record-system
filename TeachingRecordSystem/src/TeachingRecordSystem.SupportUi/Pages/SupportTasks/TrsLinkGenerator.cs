using TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.IntegrationTransactions;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequestManualChecksNeeded;

namespace TeachingRecordSystem.SupportUi;

public partial class TrsLinkGenerator
{
    public string IntegrationTransactionDetail(long integrationTransactionId) => GetRequiredPathByPage("/SupportTasks/IntegrationTransactions/Detail", routeValues: new { integrationTransactionId });

    public string SupportTasks(SupportTaskCategory[]? categories = null, Pages.SupportTasks.IndexModel.SortByOption? sortBy = null, string? reference = null, bool? filtersApplied = null) =>
        GetRequiredPathByPage("/SupportTasks/Index", routeValues: new { category = categories, sortBy, reference, _f = filtersApplied == true ? "1" : null });

    public string SupportTaskDetail(string supportTaskReference, SupportTaskType supportTaskType) =>
        supportTaskType switch
        {
            SupportTaskType.ConnectOneLoginUser => ConnectOneLoginUserSupportTask(supportTaskReference),
            SupportTaskType.ApiTrnRequest => ApiTrnRequestMatches(supportTaskReference),
            SupportTaskType.TrnRequestManualChecksNeeded => ResolveTrnRequestManualChecksNeeded(supportTaskReference),
            SupportTaskType.NpqTrnRequest => NpqTrnRequestDetailsPage(supportTaskReference),
            SupportTaskType.ChangeDateOfBirthRequest => EditChangeRequest(supportTaskReference),
            SupportTaskType.ChangeNameRequest => EditChangeRequest(supportTaskReference),
            _ => throw new ArgumentException($"Unknown {nameof(SupportTaskType)}: '{supportTaskType}'.", nameof(supportTaskType))
        };

    public string ApiTrnRequests(string? search = null, ApiTrnRequestsSortByOption? sortBy = null, SortDirection? sortDirection = null, int? pageNumber = null) =>
        GetRequiredPathByPage("/SupportTasks/ApiTrnRequests/Index", routeValues: new { search, sortBy, sortDirection, pageNumber });

    public string ApiTrnRequestMatches(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/SupportTasks/ApiTrnRequests/Resolve/Matches", routeValues: new { supportTaskReference, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string ApiTrnRequestMatchesCancel(string supportTaskReference, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/SupportTasks/ApiTrnRequests/Resolve/Matches", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId, handler: "Cancel");

    public string ApiTrnRequestMerge(string supportTaskReference, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/SupportTasks/ApiTrnRequests/Resolve/Merge", routeValues: new { supportTaskReference, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string NpqTrnRequests(string? search = null, Pages.SupportTasks.NpqTrnRequests.SortByOption? sortBy = null, SortDirection? sortDirection = null, int? pageNumber = null) =>
        GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/Index", routeValues: new { search, sortBy, sortDirection, pageNumber });

    public string NpqTrnRequestDetailsPage(string supportTaskReference) =>
        GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/Details", routeValues: new { supportTaskReference });

    public string NpqTrnRequestDetailsPageCancel(string supportTaskReference) =>
        GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/Details", routeValues: new { supportTaskReference }, handler: "Cancel");

    public string NpqTrnRequestMatches(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/Resolve/Matches", routeValues: new { supportTaskReference, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string NpqTrnRequestMatchesCancel(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null) =>
        GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/Resolve/Matches", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId, handler: "Cancel");

    public string NpqTrnRequestMerge(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/Resolve/Merge", routeValues: new { supportTaskReference, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string NpqTrnRequestMergeCancel(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null) =>
        GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/Resolve/Merge", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId, handler: "Cancel");

    public string NpqTrnRequestMergeCheckAnswers(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null) =>
        GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/Resolve/CheckAnswers", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId);

    public string NpqTrnRequestMergeCheckAnswersCancel(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null) =>
        GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/Resolve/CheckAnswers", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId, handler: "Cancel");

    public string NpqTrnRequestRejectionReason(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/Reject/RejectionReason", routeValues: new { supportTaskReference, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string NpqTrnRequestRejectionReasonCancel(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null) =>
        GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/Reject/RejectionReason", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId, handler: "Cancel");

    public string NpqTrnRequestRejectionCheckAnswers(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null) =>
        GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/Reject/CheckAnswers", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId);

    public string NpqTrnRequestRejectionCheckAnswersCancel(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null) =>
        GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/Reject/CheckAnswers", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId, handler: "Cancel");

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
}
