using System.Reflection;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditDetails;

public class EditDetailsPostRequestBuilder
{
    private string? FirstName { get; set; }
    private string? MiddleName { get; set; }
    private string? LastName { get; set; }
    private DateOnly? DateOfBirth { get; set; }
    private string? EmailAddress { get; set; }
    private string? MobileNumber { get; set; }
    private string? NationalInsuranceNumber { get; set; }
    private EditDetailsChangeReasonOption? ChangeReason { get; set; }
    private string? ChangeReasonDetail { get; set; }
    private bool? UploadEvidence { get; set; }

    private string? _evidenceFileName;
    private HttpContent? _evidenceFileContent;

    public EditDetailsPostRequestBuilder WithFirstName(string? firstName)
    {
        FirstName = firstName;

        return this;
    }

    public EditDetailsPostRequestBuilder WithMiddleName(string? middleName)
    {
        MiddleName = middleName;

        return this;
    }

    public EditDetailsPostRequestBuilder WithLastName(string? lastName)
    {
        LastName = lastName;

        return this;
    }

    public EditDetailsPostRequestBuilder WithDateOfBirth(DateOnly? dateOfBirth)
    {
        DateOfBirth = dateOfBirth;

        return this;
    }

    public EditDetailsPostRequestBuilder WithEmailAddress(string? emailAddress)
    {
        EmailAddress = emailAddress;

        return this;
    }

    public EditDetailsPostRequestBuilder WithMobileNumber(string? mobileNumber)
    {
        MobileNumber = mobileNumber;

        return this;
    }

    public EditDetailsPostRequestBuilder WithNationalInsuranceNumber(string? nationalInsuranceNumber)
    {
        NationalInsuranceNumber = nationalInsuranceNumber;

        return this;
    }

    public EditDetailsPostRequestBuilder WithChangeReason(EditDetailsChangeReasonOption changeReason, string? detail = null)
    {
        ChangeReason = changeReason;
        ChangeReasonDetail = detail;
        return this;
    }

    public EditDetailsPostRequestBuilder WithNoFileUploadSelection()
    {
        UploadEvidence = false;
        return this;
    }

    public EditDetailsPostRequestBuilder WithFileUploadSelection(HttpContent binaryFileContent, string filename)
    {
        UploadEvidence = true;
        _evidenceFileName = filename;
        _evidenceFileContent = binaryFileContent;
        return this;
    }

    public IEnumerable<KeyValuePair<string, string?>> Build()
    {
        var properties = GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => f.GetValue(this) != null);

        foreach (var property in properties)
        {
            var value = property.GetValue(this);
            if (value is DateOnly date)
            {
                yield return new($"{property.Name}.Day", date.Day.ToString());
                yield return new($"{property.Name}.Month", date.Month.ToString());
                yield return new($"{property.Name}.Year", date.Year.ToString());
            }
            else if (value is Array array)
            {
                for (var i = 0; i < array.Length; i++)
                {
                    yield return new($"{property.Name}[{i}]", array.GetValue(i)?.ToString());
                }
            }
            else
            {
                yield return new(property.Name, value?.ToString());
            }
        }
    }
}
