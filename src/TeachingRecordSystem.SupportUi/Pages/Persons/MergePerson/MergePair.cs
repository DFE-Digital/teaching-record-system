using TeachingRecordSystem.Core.Services.Persons;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.MergePerson;

public record MergePair(MergeCandidate Primary, MergeCandidate Secondary)
{
    public PersonMergeBlockers MergeBlockers => PersonService.GetMergeBlockers(Primary.MergeDetails, Secondary.MergeDetails);
    public bool CanMerge => MergeBlockers is PersonMergeBlockers.None;
}
