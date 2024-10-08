using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class AlertCategoryMapping : IEntityTypeConfiguration<AlertCategory>
{
    public void Configure(EntityTypeBuilder<AlertCategory> builder)
    {
        builder.ToTable("alert_categories");
        builder.HasKey(x => x.AlertCategoryId);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(AlertCategory.NameMaxLength).UseCollation("case_insensitive");
        builder.HasIndex(x => x.DisplayOrder).HasDatabaseName(AlertCategory.DisplayOrderIndexName).IsUnique();
        builder.HasData(
            new AlertCategory { AlertCategoryId = Guid.Parse("ee78d44d-abf8-44a9-b22b-87a821f8d3c9"), Name = "EEA Decision", DisplayOrder = 1 },
            new AlertCategory { AlertCategoryId = Guid.Parse("0ae0707b-1503-477d-bc0f-1505ed95dbdf"), Name = "Failed induction", DisplayOrder = 2 },
            new AlertCategory { AlertCategoryId = Guid.Parse("768c9eb4-355b-4491-bb20-67eb59a97579"), Name = "Flag", DisplayOrder = 3 },
            new AlertCategory { AlertCategoryId = Guid.Parse("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), Name = "GTC Decision", DisplayOrder = 4 },
            new AlertCategory { AlertCategoryId = Guid.Parse("70b7d473-2ec8-4643-bfd4-d4ab9a9a0988"), Name = "GTC Prohibition from teaching", DisplayOrder = 5 },
            new AlertCategory { AlertCategoryId = Guid.Parse("790410c1-b884-4cdd-8db9-64a042ab54ae"), Name = "GTC Restriction", DisplayOrder = 6 },
            new AlertCategory { AlertCategoryId = Guid.Parse("b2b19019-b165-47a3-8745-3297ff152581"), Name = "Prohibition from teaching", DisplayOrder = 7 },
            new AlertCategory { AlertCategoryId = Guid.Parse("e8a9ee91-bf7f-4f70-bc66-a644d522384e"), Name = "Restricted/DBS", DisplayOrder = 8 },
            new AlertCategory { AlertCategoryId = Guid.Parse("cbf7633f-3904-407d-8371-42a473fa641f"), Name = "Restriction", DisplayOrder = 9 },
            new AlertCategory { AlertCategoryId = Guid.Parse("38df5a00-94ab-486f-8905-d5b2eac04000"), Name = "Section 128 (SoS)", DisplayOrder = 10 },
            new AlertCategory { AlertCategoryId = Guid.Parse("227b75e5-bb98-496c-8860-1baea37aa5c6"), Name = "TRA Decision (SoS)", DisplayOrder = 12 },
            new AlertCategory { AlertCategoryId = Guid.Parse("e4057fc2-a010-42a9-8cb2-7dcc5c9b5fa7"), Name = "SoS Restriction", DisplayOrder = 11 });
    }
}
