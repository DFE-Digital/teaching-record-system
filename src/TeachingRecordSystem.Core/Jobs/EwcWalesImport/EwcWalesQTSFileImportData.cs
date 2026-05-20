using CsvHelper.Configuration.Attributes;

namespace TeachingRecordSystem.Core.Jobs.EwcWalesImport;

public class EwcWalesQtsFileImportData
{
    [Name("QTS_REF_NO")]
    public required string QtsRefNo { get; set; }

    [Name("FORENAME")]
    public required string Forename { get; set; }

    [Name("SURNAME")]
    public required string Surname { get; set; }

    [Name("DATE_OF_BIRTH")]
    public required string DateOfBirth { get; set; }

    [Name("QTS_STATUS")]
    public required string QtsStatus { get; set; }

    [Name("QTS_DATE")]
    public required string QtsDate { get; set; }

    [Name("ITT StartMONTH")]
    public required string IttStartMonth { get; set; }

    [Name("ITT START YY")]
    public required string IttStartYear { get; set; }

    [Name("ITT End Date")]
    public required string IttEndDate { get; set; }

    [Name("ITT Course Length")]
    public required string ITTCourseLength { get; set; }

    [Name("ITT Estab LEA code")]
    public required string IttEstabLeaCode { get; set; }

    [Name("ITT Estab Code")]
    public required string IttEstabCode { get; set; }

    [Name("ITT Qual Code")]
    public required string IttQualCode { get; set; }

    [Name("ITT Class Code")]
    public required string IttClassCode { get; set; }

    [Name("ITT Subject Code 1")]
    public required string IttSubjectCode1 { get; set; }

    [Name("ITT Subject Code 2")]
    public required string IttSubjectCode2 { get; set; }

    [Name("ITT Min Age Range")]
    public required string IttMinAgeRange { get; set; }

    [Name("ITT Max Age Range")]
    public required string IttMaxAgeRange { get; set; }

    [Name("ITT Min Sp Age Range")]
    public required string IttMinSpAgeRange { get; set; }

    [Name("ITT Max Sp Age Range")]
    public required string IttMaxSpAgeRange { get; set; }

    [Name("ITT Course Length")]
    public required string PqCourseLength { get; set; }

    [Name("PQ Year of Award")]
    public required string PqYearOfAward { get; set; }

    [Name("COUNTRY")]
    public required string Country { get; set; }

    [Name("PQ Estab Code")]
    public required string PqEstabCode { get; set; }

    [Name("PQ Qual Code")]
    public required string PqQualCode { get; set; }

    [Name("HONOURS")]
    public required string Honours { get; set; }

    [Name("PQ Class Code")]
    public required string PqClassCode { get; set; }

    [Name("PQ Subject Code 1")]
    public required string PqSubjectCode1 { get; set; }

    [Name("PQ Subject Code 2")]
    public required string PqSubjectCode2 { get; set; }

    [Name("PQ Subject Code 3")]
    public required string PqSubjectCode3 { get; set; }
}

