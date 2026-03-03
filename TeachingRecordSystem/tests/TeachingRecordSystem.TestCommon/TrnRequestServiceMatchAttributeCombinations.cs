namespace TeachingRecordSystem.TestCommon;

/// <summary>
/// Test helper that defines combinations of attributes that lead to matches / non-matches from TrnRequestService person matching.
/// Needs to be kept up to date with the matching logic in TrnRequestService.
/// </summary>
public static class TrnRequestServiceMatchAttributeCombinations
{
    public static IReadOnlyCollection<PersonMatchedAttribute[]> GetMatchAttributeCombinations() => new List<PersonMatchedAttribute[]>
    {
        new [] {PersonMatchedAttribute.FirstName, PersonMatchedAttribute.MiddleName, PersonMatchedAttribute.LastName},
        new [] {PersonMatchedAttribute.FirstName, PersonMatchedAttribute.LastName, PersonMatchedAttribute.DateOfBirth},
        new [] {PersonMatchedAttribute.MiddleName, PersonMatchedAttribute.LastName, PersonMatchedAttribute.DateOfBirth},
        new [] {PersonMatchedAttribute.FirstName, PersonMatchedAttribute.MiddleName, PersonMatchedAttribute.DateOfBirth},
        new [] {PersonMatchedAttribute.EmailAddress},
        new [] {PersonMatchedAttribute.NationalInsuranceNumber},
    };

    public static IReadOnlyCollection<PersonMatchedAttribute[]> GetNonMatchAttributeCombinationExamples() => new List<PersonMatchedAttribute[]>
    {
        new [] {PersonMatchedAttribute.FirstName, PersonMatchedAttribute.MiddleName},
        new [] {PersonMatchedAttribute.FirstName, PersonMatchedAttribute.LastName},
        new [] {PersonMatchedAttribute.MiddleName, PersonMatchedAttribute.DateOfBirth},
        new [] {PersonMatchedAttribute.MiddleName, PersonMatchedAttribute.LastName}
    };
}
