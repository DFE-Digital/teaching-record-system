namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification.Resolve;

public class SuggestedMatchViewModel
{
    public char Identifier { get; set; }
    public Guid? PersonId { get; set; }
    public string? Trn { get; set; }
    public string? EmailAddress { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? NationalInsuranceNumber { get; set; }
}
