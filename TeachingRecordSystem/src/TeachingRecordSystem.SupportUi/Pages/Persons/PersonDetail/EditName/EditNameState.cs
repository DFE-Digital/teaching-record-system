using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditName;

public class EditNameState
{
    public bool Initialized { get; set; }

    public string? FirstName { get; set; }

    public string? MiddleName { get; set; }

    public string? LastName { get; set; }

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(FirstName), nameof(LastName))]
    public bool IsComplete => !string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName);

    public async Task EnsureInitialized(ICrmQueryDispatcher crmQueryDispatcher, Guid personId)
    {
        if (Initialized)
        {
            return;
        }

        var person = await crmQueryDispatcher.ExecuteQuery(
            new GetActiveContactDetailByIdQuery(
                personId,
                new ColumnSet(
                    Contact.PrimaryIdAttribute,
                    Contact.Fields.FirstName,
                    Contact.Fields.MiddleName,
                    Contact.Fields.LastName)));
        FirstName = person!.Contact.FirstName;
        MiddleName = person.Contact.MiddleName;
        LastName = person!.Contact.LastName;
        Initialized = true;
    }
}
