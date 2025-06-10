namespace TeachingRecordSystem.SupportUi.Pages.Shared;

public class PersonProfessionalStatusDetailViewModel
{
    public required InductionStatusInfo InductionStatusInfo { get; set; }
    public required DateOnly? QtsDate { get; set; }
    public required DateOnly? EytsDate { get; set; }
    public required bool HasEyps { get; set; }
    public required DateOnly? PqtsDate { get; set; }
}
