using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

[Journey(JourneyNames.AddMq), RequireJourneyInstance]
public class SpecialismModel(TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<AddMqState>? JourneyInstance { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select a specialism")]
    [ValidSpecialism(ErrorMessage = "Select a valid specialism")]
    public MandatoryQualificationSpecialism? Specialism { get; set; }

    public MandatoryQualificationSpecialismInfo[]? Specialisms { get; set; }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state => state.Specialism = Specialism);

        return Redirect(FromCheckAnswers ?
            linkGenerator.MqAddCheckAnswers(PersonId, JourneyInstance.InstanceId) :
            linkGenerator.MqAddStartDate(PersonId, JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancel()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        Specialisms = MandatoryQualificationSpecialismRegistry.GetAll(forNewRecord: true)
            .OrderBy(t => t.Title)
            .ToArray();

        PersonName = personInfo.Name;
        Specialism ??= JourneyInstance!.State.Specialism;
    }

    private class ValidSpecialismAttribute : AllowedValuesAttribute
    {
        public ValidSpecialismAttribute()
            : base(GetAllowedValues())
        {
        }

        private static object[] GetAllowedValues() =>
            MandatoryQualificationSpecialismRegistry.GetAll(forNewRecord: true).Select(v => (object)v.Value).ToArray();
    }
}
