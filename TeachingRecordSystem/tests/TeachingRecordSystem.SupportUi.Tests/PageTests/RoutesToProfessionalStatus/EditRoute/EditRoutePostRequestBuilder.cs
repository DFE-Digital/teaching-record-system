using System.Reflection;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.EditRoute;

public class EditRoutePostRequestBuilder
{
    private DateOnly? TrainingStartDate { get; set; }
    private DateOnly? TrainingEndDate { get; set; }
    private RouteToProfessionalStatusStatus? RouteStatus { get; set; }
    private Guid[]? ExemptionReasonIds { get; set; }
    private ChangeReasonOption? ChangeReason { get; set; }
    private bool? HasAdditionalReasonDetail { get; set; }
    private string? ChangeReasonDetail { get; set; }
    private bool? UploadEvidence { get; set; }

    private string? _evidenceFileName;
    private HttpContent? _evidenceFileContent;

    public EditRoutePostRequestBuilder WithStartDate(DateOnly date)
    {
        TrainingStartDate = date;
        return this;
    }

    public EditRoutePostRequestBuilder WithCompletedDate(DateOnly date)
    {
        TrainingEndDate = date;
        return this;
    }

    public EditRoutePostRequestBuilder WithRouteStatus(RouteToProfessionalStatusStatus status)
    {
        RouteStatus = status;
        return this;
    }

    public EditRoutePostRequestBuilder WithExemptionReasonIds(Guid[] exemptionReasons)
    {
        ExemptionReasonIds = exemptionReasons;
        return this;
    }

    public EditRoutePostRequestBuilder WithChangeReason(ChangeReasonOption changeReason)
    {
        this.ChangeReason = changeReason;
        return this;
    }

    public EditRoutePostRequestBuilder WithChangeReasonDetailSelections(bool? hasAdditionalDetail, string? detail = null)
    {
        HasAdditionalReasonDetail = hasAdditionalDetail;
        ChangeReasonDetail = detail;
        return this;
    }

    public EditRoutePostRequestBuilder WithNoFileUploadSelection()
    {
        UploadEvidence = false;
        return this;
    }

    public EditRoutePostRequestBuilder WithFileUploadSelection(HttpContent binaryFileContent, string filename)
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
