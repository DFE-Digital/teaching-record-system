using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class RouteMapping : IEntityTypeConfiguration<Route>
{
    public void Configure(EntityTypeBuilder<Route> builder)
    {
        builder.HasKey(r => r.RouteId);
        builder.HasOne<Person>().WithMany(p => p.Routes).HasForeignKey(r => r.PersonId);
        builder.HasOne<Qualification>(r => r.Qualification).WithOne(q => q.Route).HasForeignKey<Route>(r => r.QualificationId);
        builder.HasOne<Country>().WithMany().HasForeignKey(r => r.CountryId);
        builder.HasIndex(r => r.PersonId);
    }
}
