using Microsoft.EntityFrameworkCore.Diagnostics;
using OpenIddict.EntityFrameworkCore.Models;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Infrastructure.EntityFramework;

internal class PopulateOidcApplicationInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context!;
        var applicationUserEntries = context.ChangeTracker.Entries<ApplicationUser>();

        foreach (var entry in applicationUserEntries.ToArray())
        {
            var openIddictAppSet = context.Set<OpenIddictEntityFrameworkCoreApplication<Guid>>();

            var existingOpenIddictApp = context.ChangeTracker.Entries<OpenIddictEntityFrameworkCoreApplication<Guid>>()
                    .SingleOrDefault(e => e.Entity.Id == entry.Entity.UserId)?.Entity
                ?? await openIddictAppSet.SingleOrDefaultAsync(u => u.Id == entry.Entity.UserId);

            if (existingOpenIddictApp is not null)
            {
                entry.Entity.PopulateOpenIddictApplication(existingOpenIddictApp);
            }
            else
            {
                var newOpenIddictApp = new OpenIddictEntityFrameworkCoreApplication<Guid>();
                entry.Entity.PopulateOpenIddictApplication(newOpenIddictApp);
                openIddictAppSet.Add(newOpenIddictApp);
            }
        }

        return result;
    }
}
