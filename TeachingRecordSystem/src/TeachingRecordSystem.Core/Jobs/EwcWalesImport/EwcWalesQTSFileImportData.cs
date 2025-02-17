using CsvHelper.Configuration.Attributes;
using IndexAttribute = CsvHelper.Configuration.Attributes.IndexAttribute;

namespace TeachingRecordSystem.Core.Jobs.EwcWalesImport;

public class EwcWalesQtsFileImportData
{
    [Name("QTS_REF_NO")]
    [Index(0)]
    public required string QtsRefNo { get; set; }

    [Name("FORENAME")]
    [Index(1)]
    public required string Forename { get; set; }

    [Name("SURNAME")]
    [Index(2)]
    public required string Surname { get; set; }

    [Name("DATE_OF_BIRTH")]
    [Index(3)]
    public required string DateOfBirth { get; set; }

    [Name("QTS_STATUS")]
    [Index(4)]
    public required string QtsStatus { get; set; }

    [Name("QTS_DATE")]
    [Index(5)]
    public required string QtsDate { get; set; }

    [Name("ITT StartMONTH")]
    [Index(6)]
    public required string? IttStartMonth { get; set; }

    [Name("ITT START YY")]
    [Index(7)]
    public required string? IttStartYear { get; set; }

    [Name("ITT End Date")]
    [Index(8)]
    public required string? IttEndDate { get; set; }

    [Name("ITT Course Length")]
    [Index(9)]
    public required string? ITTCourseLength { get; set; }

    [Name("ITT Estab LEA code")]
    [Index(10)]
    public required string? IttEstabLeaCode { get; set; }

    [Name("ITT Estab Code")]
    [Index(11)]
    public required string? IttEstabCode { get; set; }

    [Name("ITT Qual Code")]
    [Index(12)]
    public required string? IttQualCode { get; set; }

    [Name("ITT Class Code")]
    [Index(13)]
    public required string? IttClassCode { get; set; }

    [Name("ITT Subject Code 1")]
    [Index(14)]
    public required string? IttSubjectCode1 { get; set; }

    [Name("ITT Subject Code 2")]
    [Index(15)]
    public required string? IttSubjectCode2 { get; set; }

    [Name("ITT Min Age Range")]
    [Index(16)]
    public required string? IttMinAgeRange { get; set; }

    [Name("ITT Max Age Range")]
    [Index(17)]
    public required string? IttMaxAgeRange { get; set; }

    [Name("ITT Min Sp Age Range")]
    [Index(18)]
    public required string? IttMinSpAgeRange { get; set; }

    [Name("ITT Max Sp Age Range")]
    [Index(19)]
    public required string? IttMaxSpAgeRange { get; set; }

    [Name("ITT Course Length")]
    [Index(20)]
    public required string? PqCourseLength { get; set; }

    [Name("PQ Year of Award")]
    [Index(21)]
    public required string? PqYearOfAward { get; set; }

    [Name("COUNTRY")]
    [Index(22)]
    public required string? Country { get; set; }

    [Name("PQ Estab Code")]
    [Index(23)]
    public required string? PqEstabCode { get; set; }

    [Name("PQ Qual Code")]
    [Index(24)]
    public required string? PqQualCode { get; set; }

    [Name("HONOURS")]
    [Index(25)]
    public required string? Honours { get; set; }

    [Name("PQ Class Code")]
    [Index(26)]
    public required string? PqClassCode { get; set; }

    [Name("PQ Subject Code 1")]
    [Index(27)]
    public required string? PqSubjectCode1 { get; set; }

    [Name("PQ Subject Code 2")]
    [Index(28)]
    public required string? PqSubjectCode2 { get; set; }

    [Name("PQ Subject Code 3")]
    [Index(29)]
    public required string? PqSubjectCode3 { get; set; }
}


