namespace TeachingRecordSystem.SupportUi.Pages.Persons.MergePerson;

public record MergePair(MergeCandidate Primary, MergeCandidate Secondary)
{
    public bool CanMerge => Primary.CanMerge && Secondary.CanMerge;
}
