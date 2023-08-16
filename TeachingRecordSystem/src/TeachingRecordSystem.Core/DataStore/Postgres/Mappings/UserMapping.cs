using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class UserMapping : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(e => e.UserId);
        builder.Property(e => e.UserType).IsRequired();
        builder.Property(e => e.Name).IsRequired().HasMaxLength(User.NameMaxLength);
        builder.Property(e => e.Email).HasMaxLength(200).UseCollation("case_insensitive");
        builder.Property(e => e.AzureAdUserId).HasMaxLength(100);
        builder.Property(e => e.Roles).HasColumnType("varchar[]");
        builder.HasIndex(e => e.AzureAdUserId).IsUnique();

        builder.HasData(GetDefaultUsers());
    }

    private static IEnumerable<User> GetDefaultUsers()
    {
        yield return new()
        {
            UserId = Guid.Parse("a81394d1-a498-46d8-af3e-e077596ab303"),
            UserType = UserType.Application,
            Name = "System",
            Active = true,
            Roles = new[] { UserRoles.Administrator }
        };
    }
}
