using Dfe.Analytics.EFCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class TeamMapping : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.IncludeInAnalyticsSync(hidden: false);
        builder.ToTable("teams");
        builder.HasKey(x => x.TeamId);
        builder.HasQueryFilter(QueryFilterNames.Deleted, x => EF.Property<DateTime?>(x, nameof(Team.DeletedOn)) == null);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(Team.NameMaxLength);
        builder.HasIndex(x => x.Name).IsUnique().HasFilter("deleted_on is null");
        builder.Property(x => x.CreatedOn).IsRequired();
        builder.Property(x => x.DeletedOn);
    }
}
