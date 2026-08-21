using System.Linq.Expressions;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public static class SupportTasksSortExtensions
{
    // Shared by the active tasks list and the assign page so that the tasks a user picked are
    // confirmed in the order they saw them in. Selections can span several pages, so the order they
    // arrive in isn't the order they were listed in.
    public static IOrderedQueryable<SupportTask> OrderBySupportTasksSortOption(
        this IQueryable<SupportTask> tasks,
        SupportTasksSortByOption sortBy,
        SortDirection sortDirection) =>
        sortBy switch
        {
            SupportTasksSortByOption.Subject => tasks
                .OrderBy(t => t.SubjectName ?? t.SubjectEmailAddress, sortDirection),
            SupportTasksSortByOption.TaskType => tasks
                .OrderBy(GetOrderByTypeExpression(), sortDirection),
            SupportTasksSortByOption.Status => tasks
                .OrderBy(t => t.Status, sortDirection),
            SupportTasksSortByOption.AssignedTo => tasks
                .OrderBy(t => t.AssignedTo!.Name, sortDirection),
            SupportTasksSortByOption.RequestedOn => tasks
                .OrderBy(t => t.CreatedOn, sortDirection),
            SupportTasksSortByOption.Source => tasks
                .OrderBy(t => t.SourceApplicationUser!.ShortName ?? t.SourceApplicationUser!.Name, sortDirection),
            _ => tasks
                .OrderBy(t => t.SupportTaskReference, sortDirection)
        };

    public static Expression<Func<SupportTask, int>> GetOrderByTypeExpression()
    {
        var typesOrderedByTitle = SupportTaskTypeRegistry.GetAll()
            .OrderBy(t => t.Title)
            .Select(t => (int)t.SupportTaskType)
            .ToArray();

        var parameter = Expression.Parameter(typeof(SupportTask), "t");
        var typeAsInt = Expression.Convert(
            Expression.Property(parameter, nameof(SupportTask.SupportTaskType)),
            typeof(int));

        // Build a CASE expression that maps each task type to its position in typesOrderedByTitle.
        Expression body = Expression.Constant(typesOrderedByTitle.Length);
        for (var i = typesOrderedByTitle.Length - 1; i >= 0; i--)
        {
            body = Expression.Condition(
                Expression.Equal(typeAsInt, Expression.Constant(typesOrderedByTitle[i])),
                Expression.Constant(i),
                body);
        }

        return Expression.Lambda<Func<SupportTask, int>>(body, parameter);
    }
}
