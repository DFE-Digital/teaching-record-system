#nullable disable
using System.ComponentModel;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Api.V2.ApiModels;

public enum ClassDivision
{
    [Description("Aegrotat (whether to honours or pass)")]
    Aegrotat = 389040007,

    [Description("Distinction")]
    Distinction = 389040011,

    [Description("First class honours")]
    FirstClassHonours = 389040000,

    [Description("Fourth class honours")]
    FourthClassHonours = 389040005,

    [Description("General Degree - degree awarded after following a non-honours course/degree that was not available to be classified")]
    GeneralDegree = 389040010,

    [Description("Higher Degree")]
    HigherDegree = 389040016,

    [Description("Lower second class honours")]
    LowerSecondClassHonours = 389040002,

    [Description("Merit")]
    Merit = 389040012,

    [Description("Not applicable")]
    Notapplicable = 389040014,

    [Description("Not Known")]
    NotKnown = 389040015,

    [Description("Ordinary (including divisions of ordinary, if any) degree awarded after following a non-honours course")]
    Ordinary = 389040009,

    [Description("Pass")]
    Pass = 389040013,

    [Description("Pass - degree awarded without honours following an honours course")]
    PassFollowingAnHonoursCourse = 389040008,

    [Description("Third class honours")]
    ThirdClassHonours = 389040004,

    [Description("Unclassified honours")]
    UnclassifiedHonours = 389040006,

    [Description("Undivided second class honours")]
    UndividedSecondClassHonours = 389040003,

    [Description("Upper second class honours")]
    UpperSecondClassHonours = 389040001,
}

public static class ClassDivisionExtensions
{
    public static dfeta_classdivision ConvertToClassDivision(this ClassDivision input) =>
        input.ConvertToEnum<ClassDivision, dfeta_classdivision>();

    public static bool TryConvertToClassDivision(this ClassDivision input, out dfeta_classdivision result) =>
        input.TryConvertToEnum(out result);
}
