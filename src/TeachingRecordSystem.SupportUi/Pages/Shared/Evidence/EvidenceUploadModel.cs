using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

public class EvidenceUploadModel
{
    [BindProperty]
    public bool? UploadEvidence { get; set; }

    [JsonIgnore]
    [BindProperty]
    public IFormFile? EvidenceFile { get; set; }

    [BindProperty]
    public UploadedEvidenceFile? UploadedEvidenceFile { get; set; }

    public bool IsComplete => UploadEvidence is bool uploadEvidence &&
        (!uploadEvidence || UploadedEvidenceFile is not null);

    public void Clear()
    {
        UploadEvidence = null;
        EvidenceFile = null;
        UploadedEvidenceFile = null;
    }
}

public class EvidenceUploadModelValidator : AbstractValidator<EvidenceUploadModel>
{
    private static readonly string[] _evidenceFileExtensions =
        [".bmp", ".csv", ".doc", ".docx", ".eml", ".jpeg", ".jpg", ".mbox", ".msg", ".ods", ".odt", ".pdf", ".png", ".tif", ".txt", ".xls", ".xlsx"];

    public EvidenceUploadModelValidator()
    {
        RuleFor(m => m.UploadEvidence)
            .NotNull()
                .WithMessage("Select yes if you want to upload evidence");

        RuleFor(m => m.EvidenceFile)
            .Cascade(CascadeMode.Stop)
            .NotNull()
                .WithMessage("Select a file")
                .When(m => m.UploadEvidence is true && m.UploadedEvidenceFile is null, ApplyConditionTo.CurrentValidator)
            .Must(f => _evidenceFileExtensions.Any(e => f!.FileName.EndsWith(e, StringComparison.OrdinalIgnoreCase)))
                .WithMessage("The selected file must be a BMP, CSV, DOC, DOCX, EML, JPEG, JPG, MBOX, MSG, ODS, ODT, PDF, PNG, TIF, TXT, XLS or XLSX")
                .When(m => m.EvidenceFile is not null, ApplyConditionTo.CurrentValidator)
            .Must(f => f!.Length <= UiDefaults.MaxFileUploadSizeMb * 1024 * 1024)
                .WithMessage("The selected file must be smaller than 50MB")
                .When(m => m.EvidenceFile is not null, ApplyConditionTo.CurrentValidator);
    }
}

public static class EvidenceUploadModelValidationExtensions
{
    public static IRuleBuilderOptions<T, EvidenceUploadModel> Evidence<T>(this IRuleBuilder<T, EvidenceUploadModel> ruleBuilder)
    {
        return ruleBuilder.SetValidator(new EvidenceUploadModelValidator());
    }
}
