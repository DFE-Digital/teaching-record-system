using CsvHelper.Configuration.Attributes;

namespace TeachingRecordSystem.Core.Jobs.EWCWalesImport;

public class EWCWalesQTSFileImportData
{
    [Name("QTS_REF_NO")]
    public required string QtsRefNo { get; init; }

    [Name("FORENAME")]
    public required string Forename { get; init; }

    [Name("SURNAME")]
    public required string Surname { get; init; }

    [Name("DATE_OF_BIRTH")]
    public required string DateOfBirth { get; init; }

    [Name("QTS_STATUS")]
    public required string QtsStatus { get; init; }

    [Name("QTS_DATE")]
    public required string QtsDate { get; set; }

    [Name("ITT StartMONTH")]
    public required string IttStartMonth { get; init; }

    [Name("ITT START YY")]
    public required string IttStartYear { get; init; }

    [Name("ITT End Date")]
    public string IttEndDate { get; init; }

    [Name("ITT Course Length")]
    public required string ITTCourseLength { get; init; }

    [Name("ITT Estab LEA code")]
    public required string IttEstabLeaCode { get; init; }

    [Name("ITT Estab Code")]
    public required string IttEstabCode { get; init; }

    [Name("ITT Qual Code")]
    public required string IttQualCode { get; init; }

    [Name("ITT Class Code")]
    public required string IttClassCode { get; init; }

    [Name("ITT Subject Code 1")]
    public string IttSubjectCode1 { get; init; }

    [Name("ITT Subject Code 2")]
    public required string IttSubjectCode2 { get; init; }

    [Name("ITT Min Age Range")]
    public required string IttMinAgeRange { get; init; }

    [Name("ITT Max Age Range")]
    public required string IttMaxAgeRange { get; init; }

    [Name("ITT Min Sp Age Range")]
    public string IttMinSpAgeRange { get; init; }

    [Name("ITT Max Sp Age Range")]
    public string IttMaxSpAgeRange { get; init; }

    [Name("ITT StartMONTH")]
    public required string IttCourseLength { get; init; }

    [Name("PQ Year of Award")]
    public required string PqYearOfAward { get; init; }

    [Name("COUNTRY")]
    public required string Country { get; init; }

    [Name("PQ Estab Code")]
    public string PqEstabCode { get; init; }

    [Name("PQ Qual Code")]
    public required string PqQualCode { get; init; }

    [Name("HONOURS")]
    public required string Honours { get; init; }

    [Name("PQ Class Code")]
    public required string PqClassCode { get; init; }

    [Name("PQ Subject Code 1")]
    public required string PqSubjectCode1 { get; init; }

    [Name("PQ Subject Code 2")]
    public required string PqSubjectCode2 { get; init; }

    [Name("PQ Subject Code 3")]
    public required string PqSubjectCode3 { get; init; }
}
