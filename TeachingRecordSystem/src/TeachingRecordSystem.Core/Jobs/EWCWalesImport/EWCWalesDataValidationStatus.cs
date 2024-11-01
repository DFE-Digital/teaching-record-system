namespace TeachingRecordSystem.Core.Jobs.EWCWalesImport;

public enum EWCWalesDataValidationStatus
{
    InvalidEstablishmentID,
    InvalidTRN,
    InvalidLastName,
    InvalidFirstName,
    InvalidDateOfBirth,
    InvalidInductionOutcome,
    InvalidInductionStartDate,
    InvalidInductionEndDate,
    InvalidInductionNumberOfTerms,
    InvalidInductionExtendedNumberOfTerms,

    DifferentEstablishmentId,
    DifferentTRN,
    DifferentLastName,
    DifferentFirstName,
    DifferentDateOfBirth,
    DifferentInductionOutcome,
    DifferentInductionStartDate,
    DifferentInductionEndDate,
    DifferentInductionNumberOfTerms,
    DifferentInductionExtendedNumberOfTerms,

    PotentialDuplicate,

    ValidationError,

}
