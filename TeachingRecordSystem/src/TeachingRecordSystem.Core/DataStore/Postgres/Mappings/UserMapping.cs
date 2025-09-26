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
            .HasValue<SystemUser>(UserType.System);
        builder.Property(e => e.UserType).IsRequired();
        builder.Property(e => e.Name).IsRequired().HasMaxLength(UserBase.NameMaxLength);
    }
}

public class UserMapping : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(e => e.Email).HasMaxLength(User.EmailMaxLength);
        builder.Property(e => e.AzureAdUserId).HasMaxLength(User.AzureAdUserIdMaxLength);
        builder.Property(e => e.Role).HasMaxLength(User.RoleMaxLength);
        builder.HasIndex(e => e.AzureAdUserId).IsUnique();
    }
}

public class ApplicationUserMapping : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(e => e.ApiRoles).HasColumnType("varchar[]");
        builder.Property(e => e.ClientId).HasMaxLength(ApplicationUser.ClientIdMaxLength);
        builder.Property(e => e.ClientSecret).HasMaxLength(ApplicationUser.ClientSecretMaxLength);
        builder.Property(e => e.RedirectUris).HasColumnType("varchar[]");
        builder.Property(e => e.PostLogoutRedirectUris).HasColumnType("varchar[]");
        builder.Property(e => e.OneLoginClientId).HasMaxLength(ApplicationUser.OneLoginClientIdMaxLength);
        builder.Property(e => e.OneLoginPrivateKeyPem).HasMaxLength(2000);
        builder.Property(e => e.OneLoginAuthenticationSchemeName).HasMaxLength(ApplicationUser.AuthenticationSchemeNameMaxLength);
        builder.Property(e => e.OneLoginRedirectUriPath).HasMaxLength(ApplicationUser.RedirectUriPathMaxLength);
        builder.Property(e => e.OneLoginPostLogoutRedirectUriPath).HasMaxLength(ApplicationUser.RedirectUriPathMaxLength);
        builder.HasIndex(e => e.OneLoginAuthenticationSchemeName).IsUnique().HasDatabaseName(ApplicationUser.OneLoginAuthenticationSchemeNameUniqueIndexName)
            .HasFilter("one_login_authentication_scheme_name is not null");
        builder.HasIndex(e => e.ClientId).IsUnique().HasDatabaseName(ApplicationUser.ClientIdUniqueIndexName).HasFilter("client_id is not null");
        builder.Property(e => e.ShortName).HasMaxLength(ApplicationUser.ShortNameMaxLength);

        builder.HasData(new ApplicationUser { UserId = ApplicationUser.NpqApplicationUserGuid, Name = "NPQ", Active = true });
    }
}

public class SystemUserMapping : IEntityTypeConfiguration<SystemUser>
{
    public void Configure(EntityTypeBuilder<SystemUser> builder)
    {
        builder.HasData(SystemUser.Instance);
    }
}
