using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class DegreeTypeMapping : IEntityTypeConfiguration<DegreeType>
{
    public void Configure(EntityTypeBuilder<DegreeType> builder)
    {
        builder.ToTable("degree_types");
        builder.HasKey(x => x.DegreeTypeId);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(DegreeType.NameMaxLength);
        builder.Property(x => x.IsActive).IsRequired();

        builder.HasData(
            new DegreeType { DegreeTypeId = new("969c89e7-35b8-43d8-be07-17ef76c3b4bf"), Name = "BA", IsActive = true },
            new DegreeType { DegreeTypeId = new("dbb7c27b-8a27-4a94-908d-4b4404acebd5"), Name = "BA (Hons)", IsActive = true },
            new DegreeType { DegreeTypeId = new("1fcd0543-14d1-4866-b961-2812239eec06"), Name = "BA (Hons) Combined Studies/Education of the Deaf", IsActive = true },
            new DegreeType { DegreeTypeId = new("c06660d3-8964-40d0-985f-80b25eced995"), Name = "BA (Hons) with Intercalated PGCE", IsActive = true },
            new DegreeType { DegreeTypeId = new("b96d4ad9-6da0-4dad-a9e4-e35b2a0838eb"), Name = "BA Combined Studies/Education of the Deaf", IsActive = true },
            new DegreeType { DegreeTypeId = new("eb04bde4-9a7b-4c68-b7e1-a9254e0e7467"), Name = "BA Education Certificate", IsActive = true },
            new DegreeType { DegreeTypeId = new("84e541d5-d55a-4d44-bc52-983322c1453f"), Name = "BA Education", IsActive = true },
            new DegreeType { DegreeTypeId = new("b7b0635a-22c3-41e3-a420-77b9b58c51cd"), Name = "BEd", IsActive = true },
            new DegreeType { DegreeTypeId = new("9b35bdfa-cbd5-44fd-a45a-6167e7559de7"), Name = "BEd (Hons)", IsActive = true },
            new DegreeType { DegreeTypeId = new("02e4f052-bd3b-490c-bea0-bd390bc5b227"), Name = "BEng (Hons)/Education", IsActive = true },
            new DegreeType { DegreeTypeId = new("35d04fbb-c19b-4cd9-8fa6-39d90883a52a"), Name = "BSc", IsActive = true },
            new DegreeType { DegreeTypeId = new("9959e914-f4f4-44cd-909f-e170a0f1ac42"), Name = "BSc (Hons)", IsActive = true },
            new DegreeType { DegreeTypeId = new("9f4af7a8-14a5-4b34-af72-dc04c5245fc7"), Name = "BSc (Hons) with Intercalated PGCE", IsActive = true },
            new DegreeType { DegreeTypeId = new("7330e2f5-dd02-4498-9b7c-5cf99d7d0cab"), Name = "BSc/Certificate in Education", IsActive = true },
            new DegreeType { DegreeTypeId = new("72dbd225-6a7e-42af-b918-cf284bccaeef"), Name = "BSc/Education", IsActive = true },
            new DegreeType { DegreeTypeId = new("984af9ff-bb42-48ac-a634-f2c4954c8158"), Name = "BTech (Hons)/Education", IsActive = true },
            new DegreeType { DegreeTypeId = new("85ab05c8-be3a-4a72-9d04-9efc30d87289"), Name = "BTech/Education", IsActive = true },
            new DegreeType { DegreeTypeId = new("b44e02b1-7257-4609-a9e5-46ed72c91b98"), Name = "Certificate in Education", IsActive = true },
            new DegreeType { DegreeTypeId = new("4c0578b6-e9af-4c98-a3bc-038343b1436a"), Name = "Certificate in Education (FE)", IsActive = true },
            new DegreeType { DegreeTypeId = new("fc85c7e2-7fd7-4585-8c37-c29852e6027f"), Name = "Degree", IsActive = true },
            new DegreeType { DegreeTypeId = new("bc6c1f17-26a5-4987-9d50-2615e138e281"), Name = "Degree Equivalent (this will include foreign qualifications)", IsActive = true },
            new DegreeType { DegreeTypeId = new("e0b22ab0-fa25-4c31-aa61-cab56a4e6a2b"), Name = "PGCE", IsActive = true },
            new DegreeType { DegreeTypeId = new("ae28704f-cfa3-4c6e-a47d-c4a048383018"), Name = "Professional PGCE", IsActive = true },
            new DegreeType { DegreeTypeId = new("311ef3a9-6aba-4314-acf8-4bba46aebe9e"), Name = "Graduate Certificate in Education", IsActive = true },
            new DegreeType { DegreeTypeId = new("d82637a0-33ed-4181-b00b-9d53e7853552"), Name = "Graduate Certificate in Mathematics and Education", IsActive = true },
            new DegreeType { DegreeTypeId = new("b9ef569f-fb23-4f31-842e-a0d940d911be"), Name = "Graduate Certificate in Science and Education", IsActive = true },
            new DegreeType { DegreeTypeId = new("6d07695e-5b5b-4dd4-997c-420e4424255c"), Name = "Graduate Diploma", IsActive = true },
            new DegreeType { DegreeTypeId = new("54f72259-23b2-4d79-a6ca-c185084c903f"), Name = "PGCE (Articled Teachers Scheme)", IsActive = true },
            new DegreeType { DegreeTypeId = new("2f7a914f-f95f-421a-a55e-60ed88074cf2"), Name = "Postgraduate Art Teachers Certificate", IsActive = true },
            new DegreeType { DegreeTypeId = new("8d0440f2-f731-4ac2-b49c-927af903bf59"), Name = "Postgraduate Art Teachers Diploma", IsActive = true },
            new DegreeType { DegreeTypeId = new("40a85dd0-8512-438e-8040-649d7d677d07"), Name = "Postgraduate Certificate in Education", IsActive = true },
            new DegreeType { DegreeTypeId = new("78a8d033-06c8-4beb-b415-5907f5f39207"), Name = "Postgraduate Certificate in Education", IsActive = true },
            new DegreeType { DegreeTypeId = new("c584eb2f-1419-4870-a230-5d81ae9b5f77"), Name = "Postgraduate Certificate in Education (Further Education)", IsActive = true },
            new DegreeType { DegreeTypeId = new("63d80489-ee3d-43af-8c4a-1d6ae0d65f68"), Name = "Postgraduate Diploma in Education", IsActive = true },
            new DegreeType { DegreeTypeId = new("d8e267d2-ed85-4eee-8119-45d0c6cc5f6b"), Name = "Professional Graduate Certificate in Education", IsActive = true },
            new DegreeType { DegreeTypeId = new("7c703efb-a5d3-41d3-b243-ee8974695dd8"), Name = "Professional Graduate Diploma in Education", IsActive = true },
            new DegreeType { DegreeTypeId = new("4ec0a016-07eb-47b4-8cdd-e276945d401e"), Name = "Qualification gained in Europe", IsActive = true },
            new DegreeType { DegreeTypeId = new("b02914fe-3a30-4f7c-94ec-0cd87a75834d"), Name = "Teachers Certificate", IsActive = true },
            new DegreeType { DegreeTypeId = new("7471551d-132e-4c5d-82cc-a41190f01245"), Name = "Teachers Certificate FE", IsActive = true },
            new DegreeType { DegreeTypeId = new("dba69141-4101-4e05-80e0-524e3967d589"), Name = "Undergraduate Master of Teaching", IsActive = true },
            new DegreeType { DegreeTypeId = new("9cf31754-5ac5-46a1-99e5-5c98cba1b881"), Name = "Unknown", IsActive = true },
            new DegreeType { DegreeTypeId = new("826f6cc9-e5f8-4ce7-a5ee-6194d19f7e22"), Name = "BA with Intercalated PGCE", IsActive = true }
        );
    }
}
