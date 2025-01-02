namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditInduction;

public class EditInductionPostRequestBuilder
{
    private DateOnly? _startDate;
    private InductionStatus? _inductionStatus;

    public EditInductionPostRequestBuilder WithStartDate(DateOnly startDate)
    {
        _startDate = startDate;
        return this;
    }

    public EditInductionPostRequestBuilder WithInductionStatus(InductionStatus status)
    {
        _inductionStatus = status;
        return this;
    }

    public Dictionary<string, string> Build()
    {
        var dictionary = new Dictionary<string, string>();

        if (_startDate.HasValue)
        {
            dictionary["StartDate.Day"] = _startDate.Value.Day.ToString();
            dictionary["StartDate.Month"] = _startDate.Value.Month.ToString();
            dictionary["StartDate.Year"] = _startDate.Value.Year.ToString();
        }
        if (_inductionStatus.HasValue)
        {
            dictionary["InductionStatus"] = _inductionStatus.Value.ToString();
        }

        return dictionary;
    }
}
