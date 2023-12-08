using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.StartDate;

public class EditMqStartDateState
{
    public bool Initialized { get; set; }

    public Guid? PersonId { get; set; }

    public string? PersonName { get; set; }

    public DateOnly? CurrentStartDate { get; set; }

    public DateOnly? StartDate { get; set; }

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(StartDate))]
    public bool IsComplete => StartDate is not null;

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

        StartDate = qualification.dfeta_MQStartDate.ToDateOnlyWithDqtBstFix(isLocalTime: true);
        CurrentStartDate = StartDate;
        PersonId = personDetail!.Contact.Id;
        PersonName = personDetail!.Contact.ResolveFullName(includeMiddleName: false);
        Initialized = true;
    }
}
