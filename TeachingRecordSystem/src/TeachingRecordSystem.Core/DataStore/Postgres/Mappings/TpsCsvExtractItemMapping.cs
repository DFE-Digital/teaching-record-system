using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class TpsCsvExtractItemMapping : IEntityTypeConfiguration<TpsCsvExtractItem>
{
    public void Configure(EntityTypeBuilder<TpsCsvExtractItem> builder)
    {
        builder.ToTable("tps_csv_extract_items");
        builder.HasKey(x => x.TpsCsvExtractItemId);
        builder.Property(x => x.Trn).HasMaxLength(7).IsFixedLength().IsRequired();
        builder.Property(x => x.DateOfBirth).IsRequired();
        builder.Property(x => x.NationalInsuranceNumber).HasMaxLength(9).IsFixedLength().IsRequired();
        builder.Property(x => x.MemberPostcode).HasMaxLength(10);
        builder.Property(x => x.MemberEmailAddress).HasMaxLength(200);
        builder.Property(x => x.LocalAuthorityCode).HasMaxLength(3).IsFixedLength().IsRequired();
        builder.Property(x => x.EstablishmentNumber).HasMaxLength(4).IsFixedLength();
        builder.Property(x => x.EstablishmentPostcode).HasMaxLength(10);
        builder.Property(x => x.EstablishmentEmailAddress).HasMaxLength(200);
        builder.Property(x => x.EmploymentStartDate).IsRequired();
        builder.Property(x => x.EmploymentType).IsRequired();
        builder.Property(x => x.WithdrawlIndicator).HasMaxLength(1).IsFixedLength();
        builder.Property(x => x.Gender).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Created).IsRequired();
        builder.Property(x => x.Key).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.Key).HasDatabaseName(TpsCsvExtractItem.KeyIndexName);
        builder.HasIndex(x => x.Trn).HasDatabaseName(TpsCsvExtractItem.TrnIndexName);
        builder.HasIndex(x => new { x.LocalAuthorityCode, x.EstablishmentNumber }).HasDatabaseName(TpsCsvExtractItem.LaCodeEstablishmentNumberIndexName);
        builder.HasIndex(x => x.TpsCsvExtractId).HasDatabaseName(TpsCsvExtractItem.TpsCsvExtractIdIndexName);
        builder.HasIndex(x => x.TpsCsvExtractLoadItemId).HasDatabaseName(TpsCsvExtractItem.TpsCsvExtractLoadItemIdIndexName);
        builder.HasOne<TpsCsvExtract>().WithMany().HasForeignKey(x => x.TpsCsvExtractId).HasConstraintName(TpsCsvExtractItem.TpsCsvExtractForeignKeyName);
        builder.HasOne<TpsCsvExtractLoadItem>().WithMany().HasForeignKey(x => x.TpsCsvExtractLoadItemId).HasConstraintName(TpsCsvExtractItem.TpsCsvExtractLoadItemIdForeignKeyName);
    }
}
