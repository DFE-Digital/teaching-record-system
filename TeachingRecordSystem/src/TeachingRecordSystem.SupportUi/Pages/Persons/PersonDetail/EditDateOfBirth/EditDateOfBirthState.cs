using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDateOfBirth;

public class EditDateOfBirthState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.EditDateOfBirth,
        typeof(EditDateOfBirthState),
        requestDataKeys: ["personId"],
        appendUniqueKey: true);

    public bool Initialized { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(DateOfBirth))]
    public bool IsComplete => DateOfBirth.HasValue;

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
                    Contact.Fields.BirthDate)));
        DateOfBirth = person!.Contact.BirthDate.ToDateOnlyWithDqtBstFix(isLocalTime: false);
        Initialized = true;
    }
}
