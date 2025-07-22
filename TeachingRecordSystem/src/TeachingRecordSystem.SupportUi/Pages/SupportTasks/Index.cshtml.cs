using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks;

public class IndexModel(
    TrsDbContext dbContext,
    ICrmQueryDispatcher crmQueryDispatcher,
    ReferenceDataCache referenceDataCache,
    ILogger<IndexModel> logger) : PageModel
{
    public (SupportTaskCategory SupportTaskCategory, string SupportTaskCategoryDescription, int Count)[]? SupportTaskCategories { get; set; }

    [FromQuery]
    public SortByOption? SortBy { get; set; }

    [FromQuery]
    public string? Reference { get; set; }

    [FromQuery(Name = "Category")]
    public SupportTaskCategory[]? Categories { get; set; }

    [FromQuery(Name = "_f")]
    public string? FiltersApplied { get; set; }

    public SupportTaskInfo[]? Results { get; set; }

    public async Task OnGetAsync()
    {
        SortBy ??= SortByOption.DateRequested;
        Categories ??= [];

        if (Categories.Length == 0 && FiltersApplied != "1")
        {
            Categories = SupportTaskCategoryRegistry.GetAll().Select(i => i.Value).ToArray();
        }

        var allSupportTasks = (await Task.WhenAll(GetCrmSupportTasksAsync(), GetTrsSupportTasksAsync()))
            .SelectMany(t => t)
            .ToArray();

        SupportTaskCategories = allSupportTasks
            .GroupBy(t => t.Type.GetCategory())
            .Select(g => (g.Key, g.Key.GetTitle(), g.Count()))
            .ToArray();

        var results = allSupportTasks.ToList();

        if (!string.IsNullOrEmpty(Reference))
        {
            results.RemoveAll(t => t.Reference != Reference);
        }

        results.RemoveAll(t => !Categories.Contains(t.Type.GetCategory()));

        Results = results
            .OrderBy(r => SortBy == SortByOption.Type ? (object)r.TypeTitle : r.RequestedOn)
            .ToArray();

        Task<SupportTaskInfo[]> GetTrsSupportTasksAsync() =>
            dbContext.SupportTasks
                .Where(t => t.Status == SupportTaskStatus.Open)
                .Select(t => new SupportTaskInfo(t.SupportTaskReference, t.SupportTaskType, t.SupportTaskType.GetTitle(), t.CreatedOn.ToGmt()))
                .ToArrayAsync();

        async Task<SupportTaskInfo[]> GetCrmSupportTasksAsync()
        {
            // We can't support paging here yet since we're blending data from both TRS and DQT.
            // In practice the list of pending incidents in production is fairly small so we'll return a single page of up to 50 results.
            // If we get more than that we'll log a warning.

            var pageSize = 50;
            var incidentsResult = await crmQueryDispatcher.ExecuteQueryAsync(new GetActiveIncidentsQuery(PageNumber: 1, pageSize));

            if (incidentsResult.TotalRecordCount > pageSize)
            {
                logger.LogWarning("Got more than {PageSize} active incidents from CRM.", pageSize);
            }

            var changeDateOfBirthRequestSubject = await referenceDataCache.GetSubjectByTitleAsync("Change of Date of Birth");
            var changeNameRequestSubject = await referenceDataCache.GetSubjectByTitleAsync("Change of Name");

            return incidentsResult.Incidents
                .Select(i =>
                {
                    var subject = i.Extract<Subject>("subject", Subject.PrimaryIdAttribute);
                    var supportTaskType = MapCrmIncidentSubjectToSupportTaskType(subject.SubjectId!.Value);
                    var supportTaskTypeTitle = supportTaskType.GetTitle();

                    return new SupportTaskInfo(
                        i.TicketNumber,
                        supportTaskType,
                        supportTaskTypeTitle,
                        i.CreatedOn!.Value.ToGmt());
                })
                .ToArray();

            SupportTaskType MapCrmIncidentSubjectToSupportTaskType(Guid subjectId)
            {
                if (subjectId == changeDateOfBirthRequestSubject.Id)
                {
                    return SupportTaskType.ChangeDateOfBirthRequest;
                }
                else if (subjectId == changeNameRequestSubject.Id)
                {
                    return SupportTaskType.ChangeNameRequest;
                }
                else
                {
                    throw new NotSupportedException($"Unexpected subject ID: '{subjectId}'.");
                }
            }
        }
    }

    public enum SortByOption { DateRequested, Type }

    public record SupportTaskInfo(string Reference, SupportTaskType Type, string TypeTitle, DateTime RequestedOn);
}
