using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class OneLoginUserMapping : IEntityTypeConfiguration<OneLoginUser>
{
    public void Configure(EntityTypeBuilder<OneLoginUser> builder)
    {
        builder.HasKey(o => o.Subject);
        builder.Property(o => o.Subject).HasMaxLength(255);
        builder.Property(o => o.EmailAddress).HasMaxLength(200);
        builder.HasOne(o => o.Person).WithMany(p => p.OneLoginUsers).HasForeignKey(o => o.PersonId);
        builder.Property(o => o.VerifiedNames).HasColumnType("jsonb").HasConversion(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<string[][]>(v, (JsonSerializerOptions?)null),
            new ValueComparer<string[][]>(
                (a, b) => (a == null && b == null) || (a != null && b != null && a.SequenceEqual(b, new StringArrayEqualityComparer())),
                v => HashCode.Combine(v.Select(names => string.Join(" ", names)))));
        builder.Property(o => o.VerifiedDatesOfBirth).HasColumnType("jsonb").HasConversion(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<DateOnly[]>(v, (JsonSerializerOptions?)null),
            new ValueComparer<DateOnly[]>(
                (a, b) => (a == null && b == null) || (a != null && b != null && a.SequenceEqual(b)),
                v => HashCode.Combine(v)));
        builder.Property(o => o.LastCoreIdentityVc).HasColumnType("jsonb");
        builder.Property(o => o.MatchedAttributes).HasColumnType("jsonb").HasConversion(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<KeyValuePair<PersonMatchedAttribute, string>[]>(v, (JsonSerializerOptions?)null),
            new ValueComparer<KeyValuePair<PersonMatchedAttribute, string>[]>(
                (a, b) => a == b,  // Reference equality is fine here; we'll always replace the entire collection
                v => v.GetHashCode()));
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(o => o.VerifiedByApplicationUserId);
    }
}

file class StringArrayEqualityComparer : IEqualityComparer<string[]>
{
    public bool Equals(string[]? x, string[]? y) =>
        (x is null && y is null) ||
        (x is not null && y is not null && x.SequenceEqual(y));

    public int GetHashCode([DisallowNull] string[] obj) => HashCode.Combine(obj);
}
