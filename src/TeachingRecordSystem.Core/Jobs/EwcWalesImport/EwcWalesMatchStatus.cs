namespace TeachingRecordSystem.Core.Jobs.EwcWalesImport;

public enum EwcWalesMatchStatus
{
    NoMatch,
    MultipleTrnMatched,
    TrnAndDateOfBirthMatchFailed,
    TeacherInactive,
    NoAssociatedQts,
    TeacherHasQts,
    OneMatch,
    MultipleMatchesFound
}
