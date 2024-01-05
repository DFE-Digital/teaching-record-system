using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

public class QualificationsModel(TrsDbContext dbContext) : PageModel
{
    [FromRoute]
    public Guid PersonId { get; set; }

    [FromQuery]
    public string? Search { get; set; }

    [FromQuery]
    public int? PageNumber { get; set; }

    [FromQuery]
    public ContactSearchSortByOption? SortBy { get; set; }

    public string? Name { get; set; }

    public MandatoryQualificationInfo[]? MandatoryQualifications { get; set; }

    public async Task OnGet()
    {
        MandatoryQualifications = await dbContext.MandatoryQualifications
            .Include(mq => mq.Provider)
            .Where(mq => mq.PersonId == PersonId)
            .Select(mq => new MandatoryQualificationInfo()
            {
                QualificationId = mq.QualificationId,
                ProviderName = mq.Provider != null ? mq.Provider.Name : null,
                Status = mq.Status,
                Specialism = mq.Specialism,
                StartDate = mq.StartDate,
                EndDate = mq.EndDate
            }).ToArrayAsync();
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        Name = personInfo.Name;
    }

    public record MandatoryQualificationInfo
    {
        public required Guid QualificationId { get; init; }
        public required string? ProviderName { get; init; }
        public required MandatoryQualificationStatus? Status { get; init; }
        public required MandatoryQualificationSpecialism? Specialism { get; init; }
        public required DateOnly? StartDate { get; init; }
        public required DateOnly? EndDate { get; init; }
    }
}
