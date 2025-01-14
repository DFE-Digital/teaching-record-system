namespace TeachingRecordSystem.Core.Jobs.EWCWalesImport;

public enum EwcWalesMatchStatus
{
    NoMatch,
    MultipleTrnMatched,
    TrnAndDateOfBirthMatchFailed,
    TeacherInactive,
    NoAssociatedQts,
    TeacherHasQts,
    OneMatch,
    MultipleMatchesFound,
}
