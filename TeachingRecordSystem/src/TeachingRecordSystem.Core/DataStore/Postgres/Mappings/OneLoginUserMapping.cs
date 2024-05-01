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
        builder.Property(o => o.Email).HasMaxLength(200);
        builder.HasOne<Person>(o => o.Person).WithOne().HasForeignKey<OneLoginUser>(o => o.PersonId);
        builder.Property(o => o.VerifiedNames).HasColumnType("jsonb").HasConversion<string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<string[][]>(v, (JsonSerializerOptions?)null),
            new ValueComparer<string[][]>(
                (a, b) => (a == null && b == null) || (a != null && b != null && a.SequenceEqual(b, new StringArrayEqualityComparer())),
                v => HashCode.Combine(v.Select(names => string.Join(" ", names)))));
        builder.Property(o => o.VerifiedDatesOfBirth).HasColumnType("jsonb").HasConversion<string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<DateOnly[]>(v, (JsonSerializerOptions?)null),
            new ValueComparer<DateOnly[]>(
                (a, b) => (a == null && b == null) || (a != null && b != null && a.SequenceEqual(b)),
                v => HashCode.Combine(v)));
        builder.Property(o => o.LastCoreIdentityVc).HasColumnType("jsonb");
        builder.Property(o => o.MatchedAttributes).HasColumnType("jsonb").HasConversion<string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<KeyValuePair<OneLoginUserMatchedAttribute, string>[]>(v, (JsonSerializerOptions?)null),
            new ValueComparer<KeyValuePair<OneLoginUserMatchedAttribute, string>[]>(
                (a, b) => a == b,  // Reference equality is fine here; we'll always replace the entire collection
                v => v.GetHashCode()));
    }
}

file class StringArrayEqualityComparer : IEqualityComparer<string[]>
{
    public bool Equals(string[]? x, string[]? y) =>
        (x is null && y is null) ||
        (x is not null && y is not null && x.SequenceEqual(y));

    public int GetHashCode([DisallowNull] string[] obj) => HashCode.Combine(obj);
}
