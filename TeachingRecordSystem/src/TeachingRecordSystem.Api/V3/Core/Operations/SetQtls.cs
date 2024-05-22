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

public class SetQtlsHandler(ICrmQueryDispatcher _crmQueryDispatcher)
{
    public async Task<SetQtlsResult> Handle(SetQtlsCommand command)
    {
        var contact = (await _crmQueryDispatcher.ExecuteQuery(
            new GetActiveContactByTrnQuery(
                command.Trn,
                new ColumnSet(
                    Contact.Fields.dfeta_TRN,
                    Contact.Fields.dfeta_InductionStatus,
                    Contact.Fields.dfeta_qtlsdate,
                    Contact.Fields.dfeta_QTSDate
                    )
                )
            ))!;

        if (contact == null)
        {
            throw new ErrorException(ErrorRegistry.TeacherWithSpecifiedTrnNotFound());
        }

        var induction = await GetInductionWithAppropriateBody(contact.Id);
        if (!CanSetQtlsDate(hasQTS: contact.dfeta_QTSDate.HasValue, overallInductionStatus: contact.dfeta_InductionStatus, inductionStatus: induction?.Induction.dfeta_InductionStatus, hasInductionWithAB: induction?.HasAppropriateBody ?? false, existingQtlsdate: contact.dfeta_qtlsdate, incomingQtlsDate: command.QtsDate))
        {
            await _crmQueryDispatcher.ExecuteQuery(
                new CreateReviewTaskQuery()
                {
                    ContactId = contact.Id,
                    Category = "Unable to set QTLSDate",
                    Description = $"Unable to set QTLSDate {command.QtsDate:dd/MM/yyyy}",
                    Subject = "Notification for SET QTLS data collections team"
                }
            );

            return SetQtlsResult.Failed();
        }

        await _crmQueryDispatcher.ExecuteQuery(
             new SetQtlsDateQuery(contact.Id, command.QtsDate))!;


        return SetQtlsResult.Success(new QtlsInfo()
        {
            Trn = command.Trn,
            QtsDate = command.QtsDate
        });
    }

    private async Task<(dfeta_induction Induction, bool HasAppropriateBody)?> GetInductionWithAppropriateBody(Guid contactId)
    {
        var induction = await _crmQueryDispatcher.ExecuteQuery(new GetInductionByContactIdQuery(contactId));
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

    private bool CanSetQtlsDate(bool hasQTS, dfeta_InductionStatus? overallInductionStatus, dfeta_InductionStatus? inductionStatus, bool hasInductionWithAB, DateTime? existingQtlsdate, DateOnly? incomingQtlsDate) =>
        hasQTS switch
        {
            true when overallInductionStatus == dfeta_InductionStatus.InProgress && !existingQtlsdate.HasValue && incomingQtlsDate.HasValue => false,
            true when overallInductionStatus == dfeta_InductionStatus.InductionExtended && !hasInductionWithAB && !existingQtlsdate.HasValue && incomingQtlsDate.HasValue => true,
            true when overallInductionStatus == dfeta_InductionStatus.InductionExtended && hasInductionWithAB && !existingQtlsdate.HasValue && incomingQtlsDate.HasValue => false,
            true when overallInductionStatus == dfeta_InductionStatus.Fail => false,
            true when overallInductionStatus == dfeta_InductionStatus.Exempt && inductionStatus == dfeta_InductionStatus.FailedinWales && existingQtlsdate.HasValue && !incomingQtlsDate.HasValue => false,
            true when overallInductionStatus == dfeta_InductionStatus.FailedinWales && !existingQtlsdate.HasValue && incomingQtlsDate.HasValue => false,
            true when overallInductionStatus == dfeta_InductionStatus.FailedinWales && existingQtlsdate.HasValue && incomingQtlsDate.HasValue => false,
            true when overallInductionStatus == dfeta_InductionStatus.Exempt && existingQtlsdate.HasValue && !incomingQtlsDate.HasValue => true,
            false => true,
            _ when existingQtlsdate.HasValue && incomingQtlsDate.HasValue && incomingQtlsDate.Value != existingQtlsdate.ToDateOnlyWithDqtBstFix(isLocalTime: false) => false,
            _ => true
        };
}

