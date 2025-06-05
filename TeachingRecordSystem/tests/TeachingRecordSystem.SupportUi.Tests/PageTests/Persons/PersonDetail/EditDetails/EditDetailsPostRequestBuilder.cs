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
    private EditDetailsOtherDetailsChangeReasonOption? OtherDetailsChangeReason { get; set; }
    private string? OtherDetailsChangeReasonDetail { get; set; }
    private bool? OtherDetailsChangeUploadEvidence { get; set; }
    private EditDetailsNameChangeReasonOption? NameChangeReason { get; set; }
    private bool? NameChangeUploadEvidence { get; set; }

    private string? _otherDetailsChangeEvidenceFileName;
    private HttpContent? _otherDetailsChangeEvidenceFileContent;
    private string? _nameChangeEvidenceFileName;
    private HttpContent? _nameChangeEvidenceFileContent;

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

    public EditDetailsPostRequestBuilder WithNameChangeReason(EditDetailsNameChangeReasonOption nameChangeReason)
    {
        NameChangeReason = nameChangeReason;
        return this;
    }

    public EditDetailsPostRequestBuilder WithNoNameChangeFileUploadSelection()
    {
        NameChangeUploadEvidence = false;
        return this;
    }

    public EditDetailsPostRequestBuilder WithNameChangeFileUploadSelection(HttpContent binaryFileContent, string filename)
    {
        NameChangeUploadEvidence = true;
        _nameChangeEvidenceFileName = filename;
        _nameChangeEvidenceFileContent = binaryFileContent;
        return this;
    }

    public EditDetailsPostRequestBuilder WithOtherDetailsChangeReason(EditDetailsOtherDetailsChangeReasonOption otherDetailsChangeReason, string? detail = null)
    {
        OtherDetailsChangeReason = otherDetailsChangeReason;
        OtherDetailsChangeReasonDetail = detail;
        return this;
    }

    public EditDetailsPostRequestBuilder WithNoOtherDetailsChangeFileUploadSelection()
    {
        OtherDetailsChangeUploadEvidence = false;
        return this;
    }

    public EditDetailsPostRequestBuilder WithOtherDetailsChangeFileUploadSelection(HttpContent binaryFileContent, string filename)
    {
        OtherDetailsChangeUploadEvidence = true;
        _otherDetailsChangeEvidenceFileName = filename;
        _otherDetailsChangeEvidenceFileContent = binaryFileContent;
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
