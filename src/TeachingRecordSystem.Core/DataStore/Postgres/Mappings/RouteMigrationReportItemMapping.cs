using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class RouteMigrationReportItemMapping : IEntityTypeConfiguration<RouteMigrationReportItem>
{
    public void Configure(EntityTypeBuilder<RouteMigrationReportItem> builder)
    {
        builder.ToTable("route_migration_report_items");
        builder.HasKey(r => r.RouteMigrationReportItemId);
        builder.HasIndex(r => r.PersonId).HasDatabaseName(RouteMigrationReportItem.PersonIdIndexName);
        builder.Property(r => r.NotMigratedReason).HasMaxLength(100);
        builder.Property(r => r.DqtIttSlugId).HasMaxLength(150);
        builder.Property(r => r.DqtIttProgrammeType).HasMaxLength(100);
        builder.Property(r => r.DqtIttResult).HasMaxLength(50);
        builder.Property(r => r.DqtIttQualificationName).HasMaxLength(250);
        builder.Property(r => r.DqtIttQualificationValue).HasMaxLength(20);
        builder.Property(r => r.DqtIttProviderName).HasMaxLength(200);
        builder.Property(r => r.DqtIttProviderUkprn).HasMaxLength(20);
        builder.Property(r => r.DqtIttCountryName).HasMaxLength(250);
        builder.Property(r => r.DqtIttCountryValue).HasMaxLength(20);
        builder.Property(r => r.DqtIttSubject1Name).HasColumnName("dqt_itt_subject1_name").HasMaxLength(250);
        builder.Property(r => r.DqtIttSubject1Value).HasColumnName("dqt_itt_subject1_value").HasMaxLength(20);
        builder.Property(r => r.DqtIttSubject2Name).HasColumnName("dqt_itt_subject2_name").HasMaxLength(250);
        builder.Property(r => r.DqtIttSubject2Value).HasColumnName("dqt_itt_subject2_value").HasMaxLength(20);
        builder.Property(r => r.DqtIttSubject3Name).HasColumnName("dqt_itt_subject3_name").HasMaxLength(250);
        builder.Property(r => r.DqtIttSubject3Value).HasColumnName("dqt_itt_subject3_value").HasMaxLength(20);
        builder.Property(r => r.DqtAgeRangeFrom).HasMaxLength(20);
        builder.Property(r => r.DqtAgeRangeTo).HasMaxLength(20);
        builder.Property(r => r.DqtTeacherStatusName).HasMaxLength(450);
        builder.Property(r => r.DqtTeacherStatusValue).HasMaxLength(20);
        builder.Property(r => r.DqtEarlyYearsStatusName).HasMaxLength(450);
        builder.Property(r => r.DqtEarlyYearsStatusValue).HasMaxLength(20);
        builder.Property(r => r.StatusDerivedRouteToProfessionalStatusTypeName).HasMaxLength(100);
        builder.Property(r => r.ProgrammeTypeDerivedRouteToProfessionalStatusTypeName).HasMaxLength(100);
        builder.Property(r => r.IttQualificationDerivedRouteToProfessionalStatusTypeName).HasMaxLength(100);
        builder.Property(r => r.RouteToProfessionalStatusTypeName).HasMaxLength(100);
        builder.Property(r => r.SourceApplicationReference).HasMaxLength(200);
        builder.Property(r => r.SourceApplicationUserShortName).HasMaxLength(25);
        builder.Property(r => r.Status).HasMaxLength(25);
        builder.Property(r => r.TrainingSubject1Name).HasColumnName("training_subject1_name").HasMaxLength(200);
        builder.Property(r => r.TrainingSubject1Reference).HasColumnName("training_subject1_reference").HasMaxLength(10);
        builder.Property(r => r.TrainingSubject2Name).HasColumnName("training_subject2_name").HasMaxLength(200);
        builder.Property(r => r.TrainingSubject2Reference).HasColumnName("training_subject2_reference").HasMaxLength(10);
        builder.Property(r => r.TrainingSubject3Name).HasColumnName("training_subject3_name").HasMaxLength(200);
        builder.Property(r => r.TrainingSubject3Reference).HasColumnName("training_subject3_reference").HasMaxLength(10);
        builder.Property(r => r.TrainingAgeSpecialismType).HasMaxLength(20);
        builder.Property(r => r.TrainingCountryName).HasMaxLength(200);
        builder.Property(r => r.TrainingCountryId).HasMaxLength(20);
        builder.Property(r => r.TrainingProviderName).HasMaxLength(200);
        builder.Property(r => r.TrainingProviderUkprn).HasMaxLength(8).IsFixedLength();
        builder.Property(r => r.DegreeTypeName).HasMaxLength(200);
    }
}
