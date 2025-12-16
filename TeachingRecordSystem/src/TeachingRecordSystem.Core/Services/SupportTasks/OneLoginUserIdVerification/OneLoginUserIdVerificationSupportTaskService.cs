using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.OneLogin;

namespace TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserIdVerification;

public class OneLoginUserIdVerificationSupportTaskService(
    SupportTaskService supportTaskService,
    OneLoginService oneLoginService)
{
    public async Task ResolveSupportTaskAsync(NotVerifiedOutcomeOptions options, ProcessContext processContext)
    {
        var supportTask = options.SupportTask;
        ThrowIfSupportTaskIsNotOpen(supportTask);

        var updateTaskResult = await supportTaskService.UpdateSupportTaskAsync(
            new UpdateSupportTaskOptions<OneLoginUserIdVerificationData>
            {
                SupportTask = supportTask,
                UpdateData = data => data with
                {
                    Verified = false,
                    Outcome = OneLoginUserIdVerificationOutcome.NotVerified,
                    RejectReason = options.RejectReason,
                    RejectionAdditionalDetails = options.RejectionAdditionalDetails
                },
                Status = SupportTaskStatus.Closed
            },
            processContext);
        Debug.Assert(updateTaskResult is UpdateSupportTaskResult.Ok);
    }

    public async Task ResolveSupportTaskAsync(VerifiedOnlyWithMatchesOutcomeOptions options, ProcessContext processContext)
    {
        var supportTask = options.SupportTask;
        ThrowIfSupportTaskIsNotOpen(supportTask);

        var data = supportTask.GetData<OneLoginUserIdVerificationData>();

        await oneLoginService.SetUserVerifiedAsync(
            new SetUserVerifiedOptions
            {
                OneLoginUserSubject = supportTask.OneLoginUserSubject!,
                VerificationRoute = OneLoginUserVerificationRoute.Support,
                VerifiedDatesOfBirth = [data.StatedDateOfBirth],
                VerifiedNames = [[data.StatedFirstName, data.StatedLastName]]
            },
            processContext);

        var updateTaskResult = await supportTaskService.UpdateSupportTaskAsync(
            new UpdateSupportTaskOptions<OneLoginUserIdVerificationData>
            {
                SupportTask = supportTask.SupportTaskReference,
                UpdateData = data => data with
                {
                    Verified = true,
                    Outcome = OneLoginUserIdVerificationOutcome.VerifiedOnlyWithMatches,
                    NotConnectingReason = options.NotConnectingReason,
                    NotConnectingAdditionalDetails = options.NotConnectingAdditionalDetails
                },
                Status = SupportTaskStatus.Closed
            },
            processContext);
        Debug.Assert(updateTaskResult is UpdateSupportTaskResult.Ok);

        var name = $"{data.StatedFirstName} {data.StatedLastName}";
        await oneLoginService.EnqueueRecordNotFoundEmailAsync(supportTask.OneLoginUser!.EmailAddress!, name, processContext);
    }

    public async Task ResolveSupportTaskAsync(VerifiedOnlyWithoutMatchesOutcomeOptions options, ProcessContext processContext)
    {
        var supportTask = options.SupportTask;
        ThrowIfSupportTaskIsNotOpen(supportTask);

        var data = supportTask.GetData<OneLoginUserIdVerificationData>();

        await oneLoginService.SetUserVerifiedAsync(
            new SetUserVerifiedOptions
            {
                OneLoginUserSubject = supportTask.OneLoginUserSubject!,
                VerificationRoute = OneLoginUserVerificationRoute.Support,
                VerifiedDatesOfBirth = [data.StatedDateOfBirth],
                VerifiedNames = [[data.StatedFirstName, data.StatedLastName]]
            },
            processContext);

        var updateTaskResult = await supportTaskService.UpdateSupportTaskAsync(
            new UpdateSupportTaskOptions<OneLoginUserIdVerificationData>
            {
                SupportTask = supportTask.SupportTaskReference,
                UpdateData = data => data with
                {
                    Verified = true,
                    Outcome = OneLoginUserIdVerificationOutcome.VerifiedOnlyWithoutMatches
                },
                Status = SupportTaskStatus.Closed
            },
            processContext);
        Debug.Assert(updateTaskResult is UpdateSupportTaskResult.Ok);

        var name = $"{data.StatedFirstName} {data.StatedLastName}";
        await oneLoginService.EnqueueRecordNotFoundEmailAsync(supportTask.OneLoginUser!.EmailAddress!, name, processContext);
    }

    public async Task ResolveSupportTaskAsync(VerifiedAndConnectedOutcomeOptions options, ProcessContext processContext)
    {
        var supportTask = options.SupportTask;
        ThrowIfSupportTaskIsNotOpen(supportTask);

        var data = supportTask.GetData<OneLoginUserIdVerificationData>();

        await oneLoginService.SetUserVerifiedAsync(
            new SetUserVerifiedOptions
            {
                OneLoginUserSubject = supportTask.OneLoginUserSubject!,
                VerificationRoute = OneLoginUserVerificationRoute.Support,
                VerifiedDatesOfBirth = [data.StatedDateOfBirth],
                VerifiedNames = [[data.StatedFirstName, data.StatedLastName]]
            },
            processContext);

        await oneLoginService.SetUserMatchedAsync(
            new SetUserMatchedOptions
            {
                OneLoginUserSubject = supportTask.OneLoginUserSubject!,
                MatchedPersonId = options.MatchedPersonId,
                MatchRoute = OneLoginUserMatchRoute.Support,
                MatchedAttributes = GetMatchedAttributes(supportTask, data, options.MatchedAttributeTypes)
            },
            processContext);

        var name = $"{data.StatedFirstName} {data.StatedLastName}";
        await oneLoginService.EnqueueRecordMatchedEmailAsync(supportTask.OneLoginUser!.EmailAddress!, name, processContext);

        var updateTaskResult = await supportTaskService.UpdateSupportTaskAsync(
            new UpdateSupportTaskOptions<OneLoginUserIdVerificationData>
            {
                SupportTask = supportTask.SupportTaskReference,
                UpdateData = data => data with
                {
                    Verified = true,
                    PersonId = options.MatchedPersonId,
                    Outcome = OneLoginUserIdVerificationOutcome.VerifiedAndConnected
                },
                Status = SupportTaskStatus.Closed
            },
            processContext);
        Debug.Assert(updateTaskResult is UpdateSupportTaskResult.Ok);
    }

    private static IEnumerable<KeyValuePair<PersonMatchedAttribute, string>> GetMatchedAttributes(
        SupportTask supportTask,
        OneLoginUserIdVerificationData data,
        IEnumerable<PersonMatchedAttribute> matchedAttributes)
    {
        foreach (var attribute in matchedAttributes)
        {
            var value = attribute switch
            {
                PersonMatchedAttribute.FirstName => data.StatedFirstName,
                PersonMatchedAttribute.LastName => data.StatedLastName,
                PersonMatchedAttribute.FullName => $"{data.StatedFirstName} {data.StatedLastName}",
                PersonMatchedAttribute.DateOfBirth => data.StatedDateOfBirth.ToString("yyyy-MM-dd"),
                PersonMatchedAttribute.NationalInsuranceNumber => data.StatedNationalInsuranceNumber,
                PersonMatchedAttribute.Trn => data.StatedTrn,
                PersonMatchedAttribute.EmailAddress => supportTask.OneLoginUserSubject!,
                _ => throw new NotSupportedException($"Unknown {nameof(PersonMatchedAttribute)}: '{attribute}'.")
            };

            yield return KeyValuePair.Create(attribute, value!);
        }
    }

    private void ThrowIfSupportTaskIsNotOpen(SupportTask supportTask)
    {
        if (supportTask.Status is not SupportTaskStatus.Open)
        {
            throw new InvalidOperationException("Support task is not open.");
        }
    }
}
