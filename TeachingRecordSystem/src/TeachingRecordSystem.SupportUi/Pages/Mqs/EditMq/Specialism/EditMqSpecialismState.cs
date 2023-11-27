using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Specialism;

public class EditMqSpecialismState
{
    public bool Initialized { get; set; }

    public Guid? PersonId { get; set; }

    public string? PersonName { get; set; }

    public string? CurrentSpecialismName { get; set; }

    public string? SpecialismValue { get; set; }

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(SpecialismValue))]
    public bool IsComplete => !string.IsNullOrWhiteSpace(SpecialismValue);

    public async Task EnsureInitialized(
        ICrmQueryDispatcher crmQueryDispatcher,
        ReferenceDataCache referenceDataCache,
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

        var mqSpecialism = qualification.dfeta_MQ_SpecialismId is not null ? await referenceDataCache.GetMqSpecialismById(qualification.dfeta_MQ_SpecialismId.Id) : null;
        SpecialismValue = mqSpecialism is not null ? mqSpecialism.dfeta_Value : null;
        CurrentSpecialismName = mqSpecialism is not null ? mqSpecialism.dfeta_name : null;
        PersonId = personDetail!.Contact.Id;
        PersonName = personDetail!.Contact.ResolveFullName(includeMiddleName: false);
        Initialized = true;
    }
}
