using CsvHelper;

namespace TeachingRecordSystem.Core.Jobs.EwcWalesImport;

[CsvRecord]
public partial class EwcWalesQtsFileImportData
{
    [CsvColumn(Index = 0, Optional = true)]
    public required string QtsRefNo { get; set; }

    [CsvColumn(Index = 1, Optional = true)]
    public required string Forename { get; set; }

    [CsvColumn(Index = 2, Optional = true)]
    public required string Surname { get; set; }

    [CsvColumn(Index = 3, Optional = true)]
    public required string DateOfBirth { get; set; }

    [CsvColumn(Index = 4, Optional = true)]
    public required string QtsStatus { get; set; }

    [CsvColumn(Index = 5, Optional = true)]
    public required string QtsDate { get; set; }

    [CsvColumn(Index = 6, Optional = true)]
    public required string IttStartMonth { get; set; }

    [CsvColumn(Index = 7, Optional = true)]
    public required string IttStartYear { get; set; }

    [CsvColumn(Index = 8, Optional = true)]
    public required string IttEndDate { get; set; }

    [CsvColumn(Index = 9, Optional = true)]
    public required string ITTCourseLength { get; set; }

    [CsvColumn(Index = 10, Optional = true)]
    public required string IttEstabLeaCode { get; set; }

    [CsvColumn(Index = 11, Optional = true)]
    public required string IttEstabCode { get; set; }

    [CsvColumn(Index = 12, Optional = true)]
    public required string IttQualCode { get; set; }

    [CsvColumn(Index = 13, Optional = true)]
    public required string IttClassCode { get; set; }

    [CsvColumn(Index = 14, Optional = true)]
    public required string IttSubjectCode1 { get; set; }

    [CsvColumn(Index = 15, Optional = true)]
    public required string IttSubjectCode2 { get; set; }

    [CsvColumn(Index = 16, Optional = true)]
    public required string IttMinAgeRange { get; set; }

    [CsvColumn(Index = 17, Optional = true)]
    public required string IttMaxAgeRange { get; set; }

    [CsvColumn(Index = 18, Optional = true)]
    public required string IttMinSpAgeRange { get; set; }

    [CsvColumn(Index = 19, Optional = true)]
    public required string IttMaxSpAgeRange { get; set; }

    [CsvColumn(Index = 20, Optional = true)]
    public required string PqCourseLength { get; set; }

    [CsvColumn(Index = 21, Optional = true)]
    public required string PqYearOfAward { get; set; }

    [CsvColumn(Index = 22, Optional = true)]
    public required string Country { get; set; }

    [CsvColumn(Index = 23, Optional = true)]
    public required string PqEstabCode { get; set; }

    [CsvColumn(Index = 24, Optional = true)]
    public required string PqQualCode { get; set; }

    [CsvColumn(Index = 25, Optional = true)]
    public required string Honours { get; set; }

    [CsvColumn(Index = 26, Optional = true)]
    public required string PqClassCode { get; set; }

    [CsvColumn(Index = 27, Optional = true)]
    public required string PqSubjectCode1 { get; set; }

    [CsvColumn(Index = 28, Optional = true)]
    public required string PqSubjectCode2 { get; set; }

    [CsvColumn(Index = 29, Optional = true)]
    public required string PqSubjectCode3 { get; set; }
}

