using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;
using TeachingRecordSystem.SupportUi.Pages.Common;
using TeachingRecordSystem.SupportUi.Pages.Shared;

namespace TeachingRecordSystem.SupportUi.Pages.Users;

[Authorize(Policy = AuthorizationPolicies.UserManagement)]
public class IndexModel(TrsDbContext dbContext, SupportUiLinkGenerator linkGenerator) : PageModel
{
    private const int UsersPerPage = 10;

    [BindProperty(SupportsGet = true)]
    public int? PageNumber { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Keywords { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Role { get; set; }

    public bool HasUsers { get; private set; }

    public IEnumerable<UserViewModel> CurrentPageUsers { get; private set; } = [];

    public PaginationViewModel? Pagination { get; private set; }

    public FiltersViewModel? Filters { get; private set; }

    public async Task OnGetAsync()
    {
        var showAdminRole = User.IsInRole(UserRoles.Administrator);
        var userRoles = UserRoles.All.Where(r => showAdminRole || r != UserRoles.Administrator);

        var filters = new FilterCollection<User>([
            new SingleValueFilter<User>(nameof(Keywords), "Search", Request.Query,
#pragma warning disable CA1862
#pragma warning disable CA1862
#pragma warning disable CA1304
#pragma warning disable CA1311
                value => u => u.Name.ToLower().Contains(value.ToLower()) || u.Email!.ToLower().Contains(value.ToLower())),
#pragma warning restore CA1311
#pragma warning restore CA1304
#pragma warning restore CA1304
#pragma warning restore CA1862

            new MultiValueFilter<User>(nameof(Role), "Role", Request.Query,
                u => u.Role,
                [.. userRoles.Select(r => new MultiValueFilterValue(r, UserRoles.GetDisplayNameForRole(r)))]),

            new MultiValueFilter<User>(nameof(Status), "Status", Request.Query,
                u => u.Active ? "active" : "deactivated",
                [
                    new MultiValueFilterValue("active", "Active"),
                    new MultiValueFilterValue("deactivated", "Deactivated")
                ])
        ]);

        var baseQuery = dbContext.Users
            .Where(u => u.UserType == UserType.Person && !string.IsNullOrWhiteSpace(u.Email))
            .Where(u => showAdminRole || u.Role != UserRoles.Administrator);

        var filteredQuery = filters.Apply(baseQuery);

        var totalUserCount = await filters.CalculateFilterCountsAsync(filteredQuery);

        var paginatedUsers = await filteredQuery
            .OrderBy(u => u.Name)
            .GetPageAsync(PageNumber, UsersPerPage, totalUserCount);

        HasUsers = totalUserCount > 0;
        CurrentPageUsers = paginatedUsers.Select(CreateViewModel);
        Filters = FiltersViewModel.Create(filters, "Find user");

        Pagination = PaginationViewModel.Create(
            paginatedUsers,
            pageNumber => linkGenerator.Users.Index(Keywords, Status, Role, pageNumber));
    }

    private UserViewModel CreateViewModel(User user)
    {
        return new()
        {
            Id = user.UserId.ToString(),
            Name = user.Name,
            EditUrl = linkGenerator.Users.EditUser.Index(user.UserId),
            EmailAddress = user.Email ?? "No email address",
            Role = user.Role == null ? "No role assigned" : UserRoles.GetDisplayNameForRole(user.Role),
            Status = user.Active ? "Active" : "Inactive"
        };
    }
}
