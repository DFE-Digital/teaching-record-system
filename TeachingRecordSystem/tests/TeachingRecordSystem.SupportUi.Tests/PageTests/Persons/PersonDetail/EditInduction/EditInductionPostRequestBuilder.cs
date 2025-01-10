using System.Reflection;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditInduction;

public class EditInductionPostRequestBuilder
{
    private DateOnly? StartDate { get; set; }
    private DateOnly? CompletedDate { get; set; }
    private InductionStatus? InductionStatus { get; set; }
    private InductionChangeReasonOption? ChangeReason { get; set; }
    private bool? HasAdditionalReasonDetail { get; set; }
    private string? ChangeReasonDetail { get; set; }
    private bool? UploadEvidence { get; set; }

    private string? _evidenceFileName;
    private HttpContent? _evidenceFileContent;

    public EditInductionPostRequestBuilder WithStartDate(DateOnly date)
    {
        StartDate = date;
        return this;
    }

    public EditInductionPostRequestBuilder WithCompletedDate(DateOnly date)
    {
        CompletedDate = date;
        return this;
    }

    public EditInductionPostRequestBuilder WithInductionStatus(InductionStatus status)
    {
        InductionStatus = status;
        return this;
    }

    public EditInductionPostRequestBuilder WithChangeReason(InductionChangeReasonOption changeReason)
    {
        this.ChangeReason = changeReason;
        return this;
    }

    public EditInductionPostRequestBuilder WithChangeReasonDetailSelections(bool? hasAdditionalDetail, string? detail)
    {
        HasAdditionalReasonDetail = hasAdditionalDetail;
        ChangeReasonDetail = detail;
        return this;
    }

    public EditInductionPostRequestBuilder WithNoFileUploadSelection()
    {
        UploadEvidence = false;
        return this;
    }

    public EditInductionPostRequestBuilder WithFileUploadSelection(HttpContent binaryFileContent, string filename)
    {
        UploadEvidence = true;
        _evidenceFileName = filename;
        _evidenceFileContent = binaryFileContent;
        return this;
    }

    public Dictionary<string, string> Build()
    {
        var properties = GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => f.GetValue(this) != null);

        if (UploadEvidence == true)
        {
            foreach (var property in properties)
            {

            }
        }

        var dictionary = new Dictionary<string, string>();

        foreach (var property in properties)
        {
            var value = property.GetValue(this);
            if (value is DateOnly date)
            {
                dictionary[$"{property.Name}.Day"] = date.Day.ToString();
                dictionary[$"{property.Name}.Month"] = date.Month.ToString();
                dictionary[$"{property.Name}.Year"] = date.Year.ToString();
            }
            else
            {
                dictionary[property.Name] = value!.ToString()!; // CML TODO !
            }
        }

        return dictionary;
    }
}
