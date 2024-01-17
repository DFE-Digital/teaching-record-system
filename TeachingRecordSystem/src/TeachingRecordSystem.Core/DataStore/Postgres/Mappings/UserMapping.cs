using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class UserBaseMapping : IEntityTypeConfiguration<UserBase>
{
    public void Configure(EntityTypeBuilder<UserBase> builder)
    {
        builder.ToTable("users");
        builder.HasKey(e => e.UserId);
        builder.HasDiscriminator(e => e.UserType)
            .HasValue<User>(UserType.Person)
            .HasValue<ApplicationUser>(UserType.Application)
            .HasValue<Models.SystemUser>(UserType.System);
        builder.Property(e => e.UserType).IsRequired();
        builder.Property(e => e.Name).IsRequired().HasMaxLength(User.NameMaxLength);
    }
}

public class UserMapping : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(e => e.Email).HasMaxLength(200).UseCollation("case_insensitive");
        builder.Property(e => e.AzureAdUserId).HasMaxLength(100);
        builder.Property(e => e.Roles).HasColumnType("varchar[]");
        builder.HasIndex(e => e.AzureAdUserId).IsUnique();
    }
}

public class ApplicationUserMapping : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(e => e.ApiRoles).HasColumnType("varchar[]");
    }
}

public class SystemUserMapping : IEntityTypeConfiguration<Models.SystemUser>
{
    public void Configure(EntityTypeBuilder<Models.SystemUser> builder)
    {
        builder.HasData(GetSystemUser());
    }

    private static Models.SystemUser GetSystemUser() => new()
    {
        UserId = Models.SystemUser.SystemUserId,
        Name = "System",
        Active = true
    };
}
