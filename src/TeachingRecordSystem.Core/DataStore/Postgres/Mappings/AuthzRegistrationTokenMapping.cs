using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class AuthzRegistrationTokenMapping : IEntityTypeConfiguration<AuthzRegistrationToken>
{
    public void Configure(EntityTypeBuilder<AuthzRegistrationToken> builder)
    {
        builder.ToTable("authz_registration_tokens");

        builder.HasKey(t => t.Token)
            .HasName("pk_authz_registration_tokens");

        builder.Property(t => t.Token)
            .HasColumnName("token")
            .HasMaxLength(128);

        builder.Property(t => t.Trn)
            .HasColumnName("trn")
            .HasColumnType("character(7)")
            .IsRequired();

        builder.Property(t => t.EmailAddress)
            .HasColumnName("emailaddress")
            .HasMaxLength(200)
            .UseCollation("case_insensitive")
            .IsRequired();

        builder.Property(t => t.CreatedUtc)
            .HasColumnName("created_utc")
            .IsRequired();

        builder.Property(t => t.ExpiresUtc)
            .HasColumnName("expires_utc")
            .IsRequired();

        builder.Property(t => t.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasIndex(t => t.EmailAddress)
            .HasDatabaseName("ix_authz_registration_tokens_emailaddress");

        builder.HasIndex(t => t.Trn)
            .HasDatabaseName("ix_authz_registration_tokens_trn");

        builder.HasIndex(t => t.IsActive)
            .HasDatabaseName("ix_authz_registration_tokens_is_active");
    }
}
