namespace TeachingRecordSystem.Core.Models;

public enum EmploymentType
{
    FullTime = 0,
    PartTimeRegular = 1,
    PartTimeIrregular = 2
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
            _ => throw new ArgumentOutOfRangeException(nameof(fullOrPartTimeIndicator))
        };
    }
}
