namespace TeachingRecordSystem.Core.Models;

public enum EmploymentType
{
    FullTime = 0,
    PartTimeRegular = 1,
    PartTimeIrregular = 2,
    PartTime = 3
}

public static class EmploymentTypeHelper
{
    public static EmploymentType FromFullOrPartTimeIndicator(string fullOrPartTimeIndicator)
    {
        return fullOrPartTimeIndicator switch
        {
            "FT" => EmploymentType.FullTime,
            "PTR" => EmploymentType.PartTimeRegular,
            "PTI" => EmploymentType.PartTimeIrregular,
            "PT" => EmploymentType.PartTime,
            _ => throw new ArgumentOutOfRangeException(nameof(fullOrPartTimeIndicator))
        };
    }
}
