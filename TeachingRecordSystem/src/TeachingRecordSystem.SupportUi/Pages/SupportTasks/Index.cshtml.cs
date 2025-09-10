using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks;

public class IndexModel(TrsDbContext dbContext) : PageModel
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

        var allSupportTasks = await dbContext.SupportTasks
            .Where(t => t.Status == SupportTaskStatus.Open)
            .Select(t => new SupportTaskInfo(t.SupportTaskReference, t.SupportTaskType, t.SupportTaskType.Title, t.CreatedOn.ToGmt()))
            .ToArrayAsync();

        SupportTaskCategories = allSupportTasks
            .GroupBy(t => t.Type.Category)
            .Select(g => (g.Key, g.Key.GetTitle(), g.Count()))
            .ToArray();

        var results = allSupportTasks.ToList();

        if (!string.IsNullOrEmpty(Reference))
        {
            results.RemoveAll(t => t.Reference != Reference);
        }

        results.RemoveAll(t => !Categories.Contains(t.Type.Category));

        Results = results
            .OrderBy(r => SortBy == SortByOption.Type ? (object)r.TypeTitle : r.RequestedOn)
            .ToArray();
    }

    public enum SortByOption { DateRequested, Type }

    public record SupportTaskInfo(string Reference, SupportTaskType Type, string TypeTitle, DateTime RequestedOn);
}
