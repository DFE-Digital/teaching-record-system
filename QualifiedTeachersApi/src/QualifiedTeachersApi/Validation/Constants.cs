using System;

namespace QualifiedTeachersApi.Validation;

public static class Constants
{
    public static DateOnly MinCrmDate { get; } = new DateOnly(1753, 1, 1);

    public static DateTime MinCrmDateTime { get; } = new DateTime(1753, 1, 1);
}
