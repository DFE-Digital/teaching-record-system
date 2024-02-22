namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

[Flags]
public enum TpsCsvExtractItemLoadErrors
{
    None = 0,

    TrnIncorrectFormat = 1 << 0,
    NationalInsuranceNumberIncorrectFormat = 1 << 1,
    DateOfBirthIncorrectFormat = 1 << 2,
    DateOfDeathIncorrectFormat = 1 << 3,
    MemberPostcodeIncorrectFormat = 1 << 4,
    MemberEmailAddressIncorrectFormat = 1 << 5,
    LocalAuthorityCodeIncorrectFormat = 1 << 6,
    EstablishmentNumberIncorrectFormat = 1 << 7,
    EstablishmentPostcodeIncorrectFormat = 1 << 8,
    MemberIdIncorrectFormat = 1 << 9,
    EmploymentStartDateIncorrectFormat = 1 << 10,
    EmploymentEndDateIncorrectFormat = 1 << 11,
    FullOrPartTimeIndicatorIncorrectFormat = 1 << 12,
    WithdrawlIndicatorIncorrectFormat = 1 << 13,
    ExtractDateIncorrectFormat = 1 << 14,
    GenderIncorrectFormat = 1 << 15
}
