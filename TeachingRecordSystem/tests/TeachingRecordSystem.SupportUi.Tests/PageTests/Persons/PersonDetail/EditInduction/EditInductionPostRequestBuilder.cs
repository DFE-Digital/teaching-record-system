using System.Reflection;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditInduction;

public class EditInductionPostRequestBuilder
{
    private DateOnly? startDate;
    private DateOnly? completedDate;
    private InductionStatus? inductionStatus;
    private InductionChangeReasonOption? changeReason;
    private bool? hasAdditionalReasonDetail;
    private string? changeReasonDetail;
    private bool? uploadEvidence;

    public EditInductionPostRequestBuilder WithStartDate(DateOnly date)
    {
        startDate = date;
        return this;
    }

    public EditInductionPostRequestBuilder WithCompletedDate(DateOnly date)
    {
        completedDate = date;
        return this;
    }

    public EditInductionPostRequestBuilder WithInductionStatus(InductionStatus status)
    {
        inductionStatus = status;
        return this;
    }

    public EditInductionPostRequestBuilder WithChangeReason(InductionChangeReasonOption changeReason)
    {
        this.changeReason = changeReason;
        return this;
    }

    public EditInductionPostRequestBuilder WithChangeReasonSelections(InductionChangeReasonOption? changeReason, bool? hasAdditionalDetail, string? detail, bool uploadEvidence = false)
    {
        this.changeReason = changeReason;
        this.hasAdditionalReasonDetail = hasAdditionalDetail;
        this.changeReasonDetail = detail;
        this.uploadEvidence = uploadEvidence;
        return this;
    }

    public Dictionary<string, string> Build()
    {
        var dictionary = new Dictionary<string, string>();

        var properties = GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => f.GetValue(this) != null);

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
