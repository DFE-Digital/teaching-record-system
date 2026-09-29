using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.OneLogin;

namespace TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserMatching;

public partial class OneLoginUserMatchingSupportTaskService
{
    public async Task<SupportTask> CreateRecordMatchingSupportTaskAsync(
        CreateOneLoginUserRecordMatchingSupportTaskOptions options,
        ProcessContext processContext,
        CancellationToken cancellationToken = default)
    {
        var trnRequest = options.TrnRequestId is not null
            ? (options.ClientApplicationUserId, options.TrnRequestId)
            : ((Guid ApplicationUserId, string RequestId)?)null;

        var supportTask = await supportTaskService.CreateSupportTaskAsync(
            new CreateSupportTaskOptions
            {
                SupportTaskType = SupportTaskType.OneLoginUserRecordMatching,
                Data = new OneLoginUserRecordMatchingData
                {
                    OneLoginUserSubject = options.OneLoginUserSubject,
                    OneLoginUserEmail = options.OneLoginUserEmailAddress,
                    VerifiedNames = options.VerifiedNames,
                    VerifiedDatesOfBirth = options.VerifiedDatesOfBirth,
                    StatedNationalInsuranceNumber = options.StatedNationalInsuranceNumber,
                    StatedTrn = options.StatedTrn,
                    ClientApplicationUserId = options.ClientApplicationUserId,
                    TrnTokenTrn = options.TrnTokenTrn,
                    YearQtsReceived = options.YearQtsReceived,
                    TrainingProviderId = options.TrainingProviderId,
                    TrainingProviderName = options.TrainingProviderName,
                    SubjectId = options.SubjectId,
                    SubjectName = options.SubjectName
                },
                PersonId = null,
                OneLoginUserSubject = options.OneLoginUserSubject,
                TrnRequest = trnRequest,
                Subject = SupportTask.Subject.FromOneLoginUser(options.VerifiedNames!),
                SourceApplicationUserId = options.ClientApplicationUserId
            },
            processContext,
            cancellationToken);

        return supportTask;
    }

    public async Task<ResolveRecordMatchingSupportTaskResult> ResolveRecordMatchingSupportTaskAsync(
        NotConnectingOutcomeOptions options,
        ProcessContext processContext,
        CancellationToken cancellationToken = default)
    {
        var supportTask = options.SupportTask;
        ThrowIfSupportTaskIsClosed(supportTask);

        var data = supportTask.GetData<OneLoginUserRecordMatchingData>();

        var applicationUser = await GetApplicationUserAsync(supportTask, cancellationToken);
        var appContent = applicationUser.AppContent;
        var recordMatchingPolicy = applicationUser.RecordMatchingPolicy;

        await supportTaskService.UpdateSupportTaskAsync(
            new UpdateSupportTaskOptions<OneLoginUserRecordMatchingData>
            {
                SupportTaskReference = supportTask.SupportTaskReference,
                UpdateData = data => data with
                {
                    Outcome = OneLoginUserRecordMatchingOutcome.NotConnecting,
                    NotConnectingReason = options.NotConnectingReason,
                    NotConnectingAdditionalDetails = options.NotConnectingAdditionalDetails
                },
                Status = SupportTaskStatus.Closed,
                Outcome = SupportTaskOutcome.OneLoginUserRecordMatching_NotConnecting
            },
            processContext,
            cancellationToken);

        if (supportTask.TrnRequestId is not null)
        {
            Debug.Assert(supportTask.TrnRequestMetadata is not null);

            if (supportTask.TrnRequestMetadata.Status is TrnRequestStatus.Pending)
            {
                await trnRequestService.TryResolveAsync(
                    supportTask.TrnRequestApplicationUserId!.Value,
                    supportTask.TrnRequestId,
                    processContext,
                    cancellationToken);
            }
        }

        if (recordMatchingPolicy == RecordMatchingPolicy.Deferred && appContent?.OneLoginNotConnectedEmailTemplateId is { } templateId)
        {
            var firstVerifiedOrStatedName = data.VerifiedOrStatedNames!.First();
            var name = $"{firstVerifiedOrStatedName.First()} {firstVerifiedOrStatedName.LastOrDefault()}";
            var reason = options.NotConnectingReason is OneLoginUserNotConnectingReason.AnotherReason
                ? options.NotConnectingAdditionalDetails!
                : options.NotConnectingReason.GetDisplayName()!;

            await oneLoginService.EnqueueNotConnectedEmailAsync(
                supportTask.OneLoginUser!.EmailAddress!,
                name,
                reason,
                templateId,
                processContext,
                cancellationToken: cancellationToken);

            return new() { EmailSent = true };
        }

        return new() { EmailSent = false };
    }

    public async Task<ResolveRecordMatchingSupportTaskResult> ResolveRecordMatchingSupportTaskAsync(NoMatchesOutcomeOptions options, ProcessContext processContext, CancellationToken cancellationToken = default)
    {
        var supportTask = options.SupportTask;
        ThrowIfSupportTaskIsClosed(supportTask);

        var data = supportTask.GetData<OneLoginUserRecordMatchingData>();

        var applicationUser = await dbContext.ApplicationUsers.SingleAsync(u => u.UserId == data.ClientApplicationUserId, cancellationToken);
        var appContent = applicationUser.AppContent;

        await supportTaskService.UpdateSupportTaskAsync(
            new UpdateSupportTaskOptions<OneLoginUserRecordMatchingData>
            {
                SupportTaskReference = supportTask.SupportTaskReference,
                UpdateData = data => data with
                {
                    Outcome = OneLoginUserRecordMatchingOutcome.NoMatches
                },
                Status = SupportTaskStatus.Closed,
                Outcome = SupportTaskOutcome.OneLoginUserRecordMatching_NoMatches
            },
            processContext,
            cancellationToken);

        if (supportTask.TrnRequestId is not null)
        {
            Debug.Assert(supportTask.TrnRequestMetadata is not null);

            if (supportTask.TrnRequestMetadata.Status is TrnRequestStatus.Pending)
            {
                await trnRequestService.TryResolveAsync(
                    supportTask.TrnRequestApplicationUserId!.Value,
                    supportTask.TrnRequestId,
                    processContext,
                    cancellationToken);
            }
        }

        var emailTemplateId = applicationUser.RecordMatchingPolicy is RecordMatchingPolicy.Deferred
            ? null
            : appContent?.OneLoginCannotFindRecordEmailTemplateId;

        if (emailTemplateId is not null)
        {
            var firstVerifiedOrStatedName = data.VerifiedOrStatedNames!.First();
            var name = $"{firstVerifiedOrStatedName.First()} {firstVerifiedOrStatedName.LastOrDefault()}";

            await oneLoginService.EnqueueRecordNotFoundEmailAsync(
                supportTask.OneLoginUser!.EmailAddress!,
                name,
                emailTemplateId,
                appContent?.SupportEmailAddressNotifyId,
                processContext,
                cancellationToken: cancellationToken);

            return new() { EmailSent = true };
        }

        return new() { EmailSent = false };
    }

    public async Task ResolveRecordMatchingSupportTaskAsync(ConnectedOutcomeOptions options, ProcessContext processContext, CancellationToken cancellationToken = default)
    {
        var supportTask = options.SupportTask;
        ThrowIfSupportTaskIsClosed(supportTask);

        var data = supportTask.GetData<OneLoginUserRecordMatchingData>();

        await oneLoginService.SetUserMatchedAsync(
            new SetUserMatchedOptions
            {
                OneLoginUserSubject = supportTask.OneLoginUserSubject!,
                MatchedPersonId = options.MatchedPersonId,
                MatchRoute = OneLoginUserMatchRoute.SupportUi,
                MatchedAttributes = options.MatchedAttributes
            },
            processContext,
            cancellationToken);

        await supportTaskService.UpdateSupportTaskAsync(
            new UpdateSupportTaskOptions<OneLoginUserRecordMatchingData>
            {
                SupportTaskReference = supportTask.SupportTaskReference,
                UpdateData = data => data with
                {
                    PersonId = options.MatchedPersonId,
                    Outcome = OneLoginUserRecordMatchingOutcome.Connected
                },
                Status = SupportTaskStatus.Closed,
                Outcome = SupportTaskOutcome.OneLoginUserRecordMatching_Connected
            },
            processContext,
            cancellationToken);

        if (supportTask.TrnRequestId is not null)
        {
            await trnRequestService.ResolveTrnRequestWithMatchedPersonAsync(
                supportTask.TrnRequestApplicationUserId!.Value,
                supportTask.TrnRequestId,
                options.MatchedPersonId,
                [],
                processContext,
                cancellationToken);
        }
        else
        {
            var appContent = await GetAppContentAsync(data.ClientApplicationUserId, cancellationToken);

            var firstVerifiedOrStatedName = data.VerifiedOrStatedNames!.First();
            var name = $"{firstVerifiedOrStatedName.First()} {firstVerifiedOrStatedName.LastOrDefault()}";

            await oneLoginService.EnqueueRecordMatchedEmailAsync(
                supportTask.OneLoginUser!.EmailAddress!,
                name,
                appContent?.OneLoginRecordMatchedEmailTemplateId,
                appContent?.SupportEmailAddressNotifyId, processContext, cancellationToken);
        }
    }
}
