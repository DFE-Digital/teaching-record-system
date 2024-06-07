using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Api.V3.Core.SharedModels;
using TeachingRecordSystem.Api.Validation;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.V3.Core.Operations;

public record SetQtlsCommand(string Trn, DateOnly? QtsDate);

public sealed class SetQtlsResult
{
    private SetQtlsResult() { }

    public bool Succeeded { get; private set; }

    public QtlsInfo? QtlsInfo { get; private set; }

    public static SetQtlsResult Success(QtlsInfo qtlsInfo) => new()
    {
        Succeeded = true,
        QtlsInfo = qtlsInfo
    };

    public static SetQtlsResult Failed() => new()
    {
        Succeeded = false
    };
}

public class SetQtlsHandler(ICrmQueryDispatcher crmQueryDispatcher, IClock clock)
{
    public async Task<SetQtlsResult> Handle(SetQtlsCommand command)
    {
        var contact = (await crmQueryDispatcher.ExecuteQuery(
            new GetActiveContactByTrnQuery(
                command.Trn,
                new ColumnSet(
                    Contact.Fields.dfeta_TRN,
                    Contact.Fields.dfeta_InductionStatus,
                    Contact.Fields.dfeta_qtlsdate,
                    Contact.Fields.dfeta_QTSDate,
                    Contact.Fields.dfeta_ActiveSanctions))
            ))!;

        if (contact == null)
        {
            throw new ErrorException(ErrorRegistry.TeacherWithSpecifiedTrnNotFound());
        }

        var induction = await GetInductionWithAppropriateBody(contact.Id);
        var (canSetQtlsDate, reviewTaskDescription) = CanSetQtlsDate(hasQTS: contact.dfeta_QTSDate.HasValue, overallInductionStatus: contact.dfeta_InductionStatus, inductionStatus: induction?.Induction.dfeta_InductionStatus, hasInductionWithAB: induction?.HasAppropriateBody ?? false, existingQtlsdate: contact.dfeta_qtlsdate, incomingQtlsDate: command.QtsDate);
        if (!canSetQtlsDate)
        {
            await crmQueryDispatcher.ExecuteQuery(
                new CreateTaskQuery()
                {
                    ContactId = contact.Id,
                    Category = "Unable to set QTLSDate",
                    Description = reviewTaskDescription!,
                    Subject = "Notification for SET QTLS data collections team",
                    ScheduledEnd = clock.UtcNow
                }
            );

            return SetQtlsResult.Failed();
        }
        await crmQueryDispatcher.ExecuteQuery(
             new SetQtlsDateQuery(contact.Id, command.QtsDate, contact.dfeta_ActiveSanctions == true, clock.UtcNow))!;

        return SetQtlsResult.Success(new QtlsInfo()
        {
            Trn = command.Trn,
            QtsDate = command.QtsDate
        });
    }

    private async Task<(dfeta_induction Induction, bool HasAppropriateBody)?> GetInductionWithAppropriateBody(Guid contactId)
    {
        var induction = await crmQueryDispatcher.ExecuteQuery(new GetActiveInductionByContactIdQuery(contactId));
        if (induction.Induction == null)
        {
            return null;
        }
        else
        {
            var inductionPeriod = induction.InductionPeriods.Where(x => x.dfeta_EndDate == null).OrderByDescending(x => x.dfeta_StartDate).FirstOrDefault();
            return (induction.Induction, inductionPeriod != null);
        }
    }

    private (bool CanSetQtlsDate, string? TaskMessage) CanSetQtlsDate(bool hasQTS, dfeta_InductionStatus? overallInductionStatus, dfeta_InductionStatus? inductionStatus, bool hasInductionWithAB, DateTime? existingQtlsdate, DateOnly? incomingQtlsDate) =>
        hasQTS switch
        {
            true when overallInductionStatus == dfeta_InductionStatus.InProgress && !existingQtlsdate.HasValue && incomingQtlsDate.HasValue => (false, $"Unable to set QTLSDate {incomingQtlsDate}, teacher induction currently set to 'In Progress'"),
            true when overallInductionStatus == dfeta_InductionStatus.InductionExtended && !hasInductionWithAB && !existingQtlsdate.HasValue && incomingQtlsDate.HasValue => (true, null),
            true when overallInductionStatus == dfeta_InductionStatus.InductionExtended && hasInductionWithAB && !existingQtlsdate.HasValue && incomingQtlsDate.HasValue => (false, $"Unable to set QTLSDate {incomingQtlsDate}, teacher induction currently set to 'Induction Extended' claimed with an AB"),
            true when overallInductionStatus == dfeta_InductionStatus.Fail => (false, $"Unable to set QTLSDate {incomingQtlsDate}, teacher induction currently set to 'Fail'"),
            true when overallInductionStatus == dfeta_InductionStatus.Exempt && inductionStatus == dfeta_InductionStatus.FailedinWales && existingQtlsdate.HasValue && !incomingQtlsDate.HasValue => (false, $"Unable to remove QTLSDate, teacher induction currently set to 'Failed in Wales'"),
            true when overallInductionStatus == dfeta_InductionStatus.FailedinWales && !existingQtlsdate.HasValue && incomingQtlsDate.HasValue => (false, $"Unable to set QTLSDate {incomingQtlsDate}, teacher induction currently set to 'Failed in Wales'"),
            true when overallInductionStatus == dfeta_InductionStatus.FailedinWales && existingQtlsdate.HasValue && incomingQtlsDate.HasValue => (false, $"Unable to set QTLSDate {incomingQtlsDate}, teacher induction currently set to 'Failed in Wales'"),
            true when overallInductionStatus == dfeta_InductionStatus.Exempt && existingQtlsdate.HasValue && !incomingQtlsDate.HasValue => (true, null),
            false => (true, null),
            _ when existingQtlsdate.HasValue && incomingQtlsDate.HasValue && incomingQtlsDate.Value == existingQtlsdate.ToDateOnlyWithDqtBstFix(isLocalTime: false) => (false, $"Unable to set QTLSDate {incomingQtlsDate}, this matches existing QTLS date on teacher record"),
            _ => (true, null)
        };
}

