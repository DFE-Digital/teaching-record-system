namespace TeachingRecordSystem.Core.Jobs.EWCWalesImport;
public enum EWCWalesMatchStatus
{
    NoMatch,
    MultipleTRNMatched,
    TRNandDOBMatchFailed,
    TeacherInactive,
    NoAssociatedQTS,
    TeacherHasQTS,

    OneMatch,
    MultipleMatchesFound,
}
