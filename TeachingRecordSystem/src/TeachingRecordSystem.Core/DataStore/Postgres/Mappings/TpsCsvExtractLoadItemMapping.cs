using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class TpsCsvExtractLoadItemMapping : IEntityTypeConfiguration<TpsCsvExtractLoadItem>
{
    public void Configure(EntityTypeBuilder<TpsCsvExtractLoadItem> builder)
    {
        builder.ToTable("tps_csv_extract_load_items");
        builder.HasKey(x => x.TpsCsvExtractLoadItemId);
        builder.Property(x => x.TpsCsvExtractId).IsRequired();
        builder.Property(x => x.Trn).HasMaxLength(TpsCsvExtractLoadItem.FieldMaxLength);
        builder.Property(x => x.NationalInsuranceNumber).HasMaxLength(TpsCsvExtractLoadItem.FieldMaxLength);
        builder.Property(x => x.DateOfBirth).HasMaxLength(TpsCsvExtractLoadItem.FieldMaxLength);
        builder.Property(x => x.DateOfDeath).HasMaxLength(TpsCsvExtractLoadItem.FieldMaxLength);
        builder.Property(x => x.MemberPostcode).HasMaxLength(TpsCsvExtractLoadItem.FieldMaxLength);
        builder.Property(x => x.MemberEmailAddress).HasMaxLength(TpsCsvExtractLoadItem.FieldMaxLength);
        builder.Property(x => x.LocalAuthorityCode).HasMaxLength(TpsCsvExtractLoadItem.FieldMaxLength);
        builder.Property(x => x.EstablishmentNumber).HasMaxLength(TpsCsvExtractLoadItem.FieldMaxLength);
        builder.Property(x => x.EstablishmentPostcode).HasMaxLength(TpsCsvExtractLoadItem.FieldMaxLength);
        builder.Property(x => x.EstablishmentEmailAddress).HasMaxLength(TpsCsvExtractLoadItem.FieldMaxLength);
        builder.Property(x => x.MemberId).HasMaxLength(TpsCsvExtractLoadItem.FieldMaxLength);
        builder.Property(x => x.EmploymentStartDate).HasMaxLength(TpsCsvExtractLoadItem.FieldMaxLength);
        builder.Property(x => x.EmploymentEndDate).HasMaxLength(TpsCsvExtractLoadItem.FieldMaxLength);
        builder.Property(x => x.FullOrPartTimeIndicator).HasMaxLength(TpsCsvExtractLoadItem.FieldMaxLength);
        builder.Property(x => x.WithdrawlIndicator).HasMaxLength(TpsCsvExtractLoadItem.FieldMaxLength);
        builder.Property(x => x.ExtractDate).HasMaxLength(TpsCsvExtractLoadItem.FieldMaxLength);
        builder.Property(x => x.Gender).HasMaxLength(TpsCsvExtractLoadItem.FieldMaxLength);
        builder.Property(x => x.Created).IsRequired();
        builder.HasOne<TpsCsvExtract>().WithMany().HasForeignKey(x => x.TpsCsvExtractId).HasConstraintName(TpsCsvExtractLoadItem.TpsCsvExtractForeignKeyName);
    }
}
