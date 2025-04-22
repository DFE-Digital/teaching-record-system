using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;
using TeachingRecordSystem.SupportUi.Pages.Common;
using TeachingRecordSystem.SupportUi.Pages.Shared;

namespace TeachingRecordSystem.SupportUi.Pages.Users;

[Authorize(Policy = AuthorizationPolicies.UserManagement)]
[RequireFeatureEnabledFilterFactory(FeatureNames.NewUserRoles)]
public class IndexModel : PageModel
{
    private const int UsersPerPage = 10;

    private readonly TrsDbContext _dbContext;
    private readonly TrsLinkGenerator _linkGenerator;

    public IndexModel(TrsDbContext dbContext, TrsLinkGenerator linkGenerator)
    {
        _dbContext = dbContext;
        _linkGenerator = linkGenerator;
    }

    public bool HasUsers { get; private set; } = false;
    public IEnumerable<UserViewModel> CurrentPageUsers { get; private set; } = [];
    public PaginationViewModel? Pagination { get; private set; }
    public FiltersViewModel? Filters { get; private set; }

    public async Task OnGetAsync()
    {
        var pagination = new Pagination("page", UsersPerPage, Request.Query);

        var filters = new FilterCollection<User>([
            new SingleValueFilter<User>("keywords", "Search", Request.Query,
                value => u => u.Name.ToLower().Contains(value.ToLower()) || u.Email!.ToLower().Contains(value.ToLower())),

            new MultiValueFilter<User>("role", "Role", Request.Query,
                u => u.Role,
                [.. UserRoles.All.Select(r => new MultiValueFilterValue(r, UserRoles.GetDisplayNameForRole(r)))]),

            new MultiValueFilter<User>("status", "Status", Request.Query,
                u => u.Active ? "active" : "deactivated",
                [
                    new MultiValueFilterValue("active", "Active"),
                    new MultiValueFilterValue("deactivated", "Deactivated")
                ])
        ]);

        var baseQuery = _dbContext.Users
            .Where(u => u.UserType == UserType.Person && !string.IsNullOrWhiteSpace(u.Email));

        var filteredQuery = filters.Apply(baseQuery);

        var totalUserCount = await filters.CountAsync(filteredQuery);

        var paginatedUsers = await pagination.PaginateAsync(
            filteredQuery.OrderBy(u => u.Name),
            totalUserCount);

        HasUsers = totalUserCount > 0;
        CurrentPageUsers = paginatedUsers.Select(CreateViewModel);
        Filters = FiltersViewModel.Create(filters, "Find user");
        Pagination = PaginationViewModel.Create(pagination, Request.Query);
    }

    private UserViewModel CreateViewModel(User user)
    {
        return new()
        {
            Id = user.UserId.ToString(),
            Name = user.Name,
            EditUrl = _linkGenerator.LegacyEditUser(user.UserId),
            EmailAddress = user.Email ?? "No email address",
            Role = user.Role == null ? "No role assigned" : UserRoles.GetDisplayNameForRole(user.Role),
            Status = user.Active ? "Active" : "Inactive"
        };
    }
}
