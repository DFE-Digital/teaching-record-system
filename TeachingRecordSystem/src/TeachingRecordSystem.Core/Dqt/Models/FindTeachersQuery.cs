#nullable disable
namespace TeachingRecordSystem.Core.Dqt.Models;

public class FindTeachersQuery
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PreviousFirstName { get; set; }
    public string PreviousLastName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string NationalInsuranceNumber { get; set; }
    public IEnumerable<Guid> IttProviderOrganizationIds { get; set; }
    public string EmailAddress { get; set; }
    public string Trn { get; set; }
}
