using System.Text.Json.Serialization;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Result;

public class EditMqResultState
{
    public bool Initialized { get; set; }

    public Guid? PersonId { get; set; }

    public string? PersonName { get; set; }

    public MandatoryQualificationStatus? CurrentStatus { get; set; }

    public MandatoryQualificationStatus? Status { get; set; }

    public DateOnly? CurrentEndDate { get; set; }

    public DateOnly? EndDate { get; set; }

    [JsonIgnore]
    public bool IsComplete => Status.HasValue &&
        (Status != MandatoryQualificationStatus.Passed || (Status == MandatoryQualificationStatus.Passed && EndDate.HasValue));

    public async Task EnsureInitialized(
        ICrmQueryDispatcher crmQueryDispatcher,
        dfeta_qualification qualification)
    {
        if (Initialized)
        {
            return;
        }

        var personDetail = await crmQueryDispatcher.ExecuteQuery(
            new GetContactDetailByIdQuery(
                qualification.dfeta_PersonId.Id,
                new ColumnSet(
                    Contact.Fields.Id,
                    Contact.Fields.FirstName,
                    Contact.Fields.MiddleName,
                    Contact.Fields.LastName,
                    Contact.Fields.dfeta_StatedFirstName,
                    Contact.Fields.dfeta_StatedLastName,
                    Contact.Fields.dfeta_StatedMiddleName,
                    Contact.Fields.dfeta_QTSDate)));

        EndDate = qualification.dfeta_MQ_Date.ToDateOnlyWithDqtBstFix(isLocalTime: true);
        CurrentEndDate = EndDate;
        Status = qualification.dfeta_MQ_Status?.ToMandatoryQualificationStatus() ?? (EndDate is not null ? MandatoryQualificationStatus.Passed : null);
        CurrentStatus = qualification.dfeta_MQ_Status?.ToMandatoryQualificationStatus();
        PersonId = personDetail!.Contact.Id;
        PersonName = personDetail!.Contact.ResolveFullName(includeMiddleName: false);
        Initialized = true;
    }
}
