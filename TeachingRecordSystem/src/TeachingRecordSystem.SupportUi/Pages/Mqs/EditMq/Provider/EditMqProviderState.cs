using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Provider;

public class EditMqProviderState
{
    public bool Initialized { get; set; }

    public Guid? PersonId { get; set; }

    public string? PersonName { get; set; }

    public string? CurrentMqEstablishmentName { get; set; }

    public string? MqEstablishmentValue { get; set; }

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(MqEstablishmentValue))]
    public bool IsComplete => !string.IsNullOrWhiteSpace(MqEstablishmentValue);

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

        var mqEstablishment = qualification.dfeta_MQ_MQEstablishmentId is not null ? await referenceDataCache.GetMqEstablishmentById(qualification.dfeta_MQ_MQEstablishmentId.Id) : null;
        MqEstablishmentValue = mqEstablishment is not null ? mqEstablishment.dfeta_Value : null;
        CurrentMqEstablishmentName = mqEstablishment is not null ? mqEstablishment.dfeta_name : null;
        PersonId = personDetail!.Contact.Id;
        PersonName = personDetail!.Contact.ResolveFullName(includeMiddleName: false);
        Initialized = true;
    }
}
