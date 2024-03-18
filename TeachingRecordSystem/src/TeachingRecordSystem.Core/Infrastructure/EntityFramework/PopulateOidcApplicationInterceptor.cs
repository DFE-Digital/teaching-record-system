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
            if (entry.CurrentValues.GetValue<bool>(nameof(ApplicationUser.IsOidcClient)))
            {
                var openIddictAppSet = context.Set<OpenIddictEntityFrameworkCoreApplication<Guid>>();

                var existingOpenIddictApp = await openIddictAppSet.SingleOrDefaultAsync(u => u.Id == entry.Entity.UserId);

                if (existingOpenIddictApp is not null)
                {
                    openIddictAppSet.Remove(existingOpenIddictApp);
                }

                var newOpenIddictApp = entry.Entity.ToOpenIddictApplication()!;
                newOpenIddictApp.ConcurrencyToken = existingOpenIddictApp?.ConcurrencyToken;

                var openIddictAppEntry = openIddictAppSet.Attach(newOpenIddictApp);

                openIddictAppEntry.State = existingOpenIddictApp is null ? EntityState.Added : EntityState.Modified;
            }
        }

        return result;
    }
}
