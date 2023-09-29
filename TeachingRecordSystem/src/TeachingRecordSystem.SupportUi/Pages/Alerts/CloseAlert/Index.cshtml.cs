using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.CloseAlert;

public class IndexModel : PageModel
{
    [FromRoute]
    public Guid AlertId { get; set; }

    [BindProperty(SupportsGet = true)]
    [Required(ErrorMessage = "Add an end date")]
    [Display(Name = "End date")]
    public DateOnly? EndDate { get; set; }

    public void OnGet()
    {
    }
}
