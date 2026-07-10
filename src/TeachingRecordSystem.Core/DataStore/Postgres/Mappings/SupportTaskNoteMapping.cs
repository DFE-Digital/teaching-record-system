using Dfe.Analytics.EFCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class SupportTaskNoteMapping : IEntityTypeConfiguration<SupportTaskNote>
{
    public void Configure(EntityTypeBuilder<SupportTaskNote> builder)
    {
        builder.IncludeInAnalyticsSync(hidden: false);
        builder.ToTable("support_task_notes");
        builder.HasKey(x => x.SupportTaskNoteId);
        builder.Property(x => x.SupportTaskNoteId);
        builder.Property(x => x.SupportTaskReference).HasMaxLength(16).IsRequired();
        builder.Property(x => x.Content).HasMaxLength(4000).IsRequired().ConfigureAnalyticsSync(hidden: true);
        builder.Property(x => x.CreatedOn).IsRequired();
        builder.HasOne(x => x.CreatedBy).WithMany().HasForeignKey(x => x.CreatedByUserId);
        builder.HasOne<SupportTask>().WithMany().HasForeignKey(x => x.SupportTaskReference);
        builder.HasIndex(x => x.SupportTaskReference).IsCreatedConcurrently();
    }
}
