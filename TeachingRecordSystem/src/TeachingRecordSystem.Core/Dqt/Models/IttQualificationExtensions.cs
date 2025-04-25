namespace TeachingRecordSystem.Core.Dqt.Models;

public static class IttQualificationExtensions
{
    public static async Task<dfeta_ittqualification> ConvertFromTrsDegreeTypeIdAsync(this Guid degreeTypeId, ReferenceDataCache referenceDataCache)
    {
        var result = await degreeTypeId.TryConvertFromTrsDegreeTypeIdAsync(referenceDataCache);
        if (result.IsSuccess)
        {
            return result.Result!;
        }

        throw new ArgumentException($"{degreeTypeId} cannot be converted to {nameof(dfeta_ittqualification)}.", nameof(dfeta_ittqualification));
    }

    public static async Task<(bool IsSuccess, dfeta_ittqualification? Result)> TryConvertFromTrsDegreeTypeIdAsync(this Guid degreeTypeId, ReferenceDataCache referenceDataCache)
    {
        if (TryConvertFromTrsDegreeTypeIdToIttQualificationValue(degreeTypeId, out var dqtIttQualificationTypeCode))
        {
            var converted = await referenceDataCache.GetIttQualificationByValueAsync(dqtIttQualificationTypeCode!);
            if (converted is not null)
            {
                return (true, converted);
            }
        }

        return (false, default);
    }

    public static string? ConvertFromTrsDegreeTypeIdToIttQualificationValue(this Guid degreeTypeId)
    {
        if (degreeTypeId.TryConvertFromTrsDegreeTypeIdToIttQualificationValue(out var result))
        {
            throw new FormatException($"Unknown {typeof(Guid).Name}: '{degreeTypeId}'.");
        }

        return result;
    }

    public static bool TryConvertFromTrsDegreeTypeIdToIttQualificationValue(this Guid degreeTypeId, out string? result)
    {
        var mapped = degreeTypeId switch
        {
            var guid when guid == Guid.Parse("969c89e7-35b8-43d8-be07-17ef76c3b4bf") => "007", // BA
            var guid when guid == Guid.Parse("dbb7c27b-8a27-4a94-908d-4b4404acebd5") => "008", // BA (Hons)
            var guid when guid == Guid.Parse("1fcd0543-14d1-4866-b961-2812239eec06") => "010", // BA (Hons) Combined Studies/Education of the Deaf
            var guid when guid == Guid.Parse("c06660d3-8964-40d0-985f-80b25eced995") => "014", // BA (Hons) with Intercalated PGCE
            var guid when guid == Guid.Parse("b96d4ad9-6da0-4dad-a9e4-e35b2a0838eb") => "009", // BA Combined Studies/Education of the Deaf
            var guid when guid == Guid.Parse("84e541d5-d55a-4d44-bc52-983322c1453f") => "011", // BA Education
            var guid when guid == Guid.Parse("eb04bde4-9a7b-4c68-b7e1-a9254e0e7467") => "018", // BA Education Certificate
            var guid when guid == Guid.Parse("826f6cc9-e5f8-4ce7-a5ee-6194d19f7e22") => "012", // BA with Intercalated PGCE
            var guid when guid == Guid.Parse("b7b0635a-22c3-41e3-a420-77b9b58c51cd") => "001", // BEd
            var guid when guid == Guid.Parse("9b35bdfa-cbd5-44fd-a45a-6167e7559de7") => "002", // BEd (Hons)
            var guid when guid == Guid.Parse("02e4f052-bd3b-490c-bea0-bd390bc5b227") => "016", // BEng (Hons)/Education
            var guid when guid == Guid.Parse("35d04fbb-c19b-4cd9-8fa6-39d90883a52a") => "003", // BSc
            var guid when guid == Guid.Parse("9959e914-f4f4-44cd-909f-e170a0f1ac42") => "004", // BSc (Hons)
            var guid when guid == Guid.Parse("9f4af7a8-14a5-4b34-af72-dc04c5245fc7") => "013", // BSc (Hons) with Intercalated PGCE
            var guid when guid == Guid.Parse("7330e2f5-dd02-4498-9b7c-5cf99d7d0cab") => "017", // BSc/Certificate in Education
            var guid when guid == Guid.Parse("72dbd225-6a7e-42af-b918-cf284bccaeef") => "015", // BSc/Education
            var guid when guid == Guid.Parse("984af9ff-bb42-48ac-a634-f2c4954c8158") => "006", // BTech (Hons)/Education
            var guid when guid == Guid.Parse("85ab05c8-be3a-4a72-9d04-9efc30d87289") => "005", // BTech/Education
            var guid when guid == Guid.Parse("b44e02b1-7257-4609-a9e5-46ed72c91b98") => "030", // Certificate in Education
            var guid when guid == Guid.Parse("4c0578b6-e9af-4c98-a3bc-038343b1436a") => "040", // Certificate in Education (FE)
            var guid when guid == Guid.Parse("fc85c7e2-7fd7-4585-8c37-c29852e6027f") => "400", // Degree
            var guid when guid == Guid.Parse("bc6c1f17-26a5-4987-9d50-2615e138e281") => "402", // Degree Equivalent (this will include foreign qualifications)
            var guid when guid == Guid.Parse("311ef3a9-6aba-4314-acf8-4bba46aebe9e") => "033", // Graduate Certificate in Education
            var guid when guid == Guid.Parse("d82637a0-33ed-4181-b00b-9d53e7853552") => "025", // Graduate Certificate in Mathematics and Education
            var guid when guid == Guid.Parse("b9ef569f-fb23-4f31-842e-a0d940d911be") => "024", // Graduate Certificate in Science and Education
            var guid when guid == Guid.Parse("6d07695e-5b5b-4dd4-997c-420e4424255c") => "997", // Graduate Diploma
            var guid when guid == Guid.Parse("54f72259-23b2-4d79-a6ca-c185084c903f") => "026", // PGCE (Articled Teachers Scheme)
            var guid when guid == Guid.Parse("2f7a914f-f95f-421a-a55e-60ed88074cf2") => "022", // Postgraduate Art Teachers Certificate
            var guid when guid == Guid.Parse("8d0440f2-f731-4ac2-b49c-927af903bf59") => "023", // Postgraduate Art Teachers Diploma
            var guid when guid == Guid.Parse("40a85dd0-8512-438e-8040-649d7d677d07") => "020", // Postgraduate Certificate in Education
            var guid when guid == Guid.Parse("c584eb2f-1419-4870-a230-5d81ae9b5f77") => "041", // Postgraduate Certificate in Education (Further Education)
            var guid when guid == Guid.Parse("63d80489-ee3d-43af-8c4a-1d6ae0d65f68") => "021", // Postgraduate Diploma in Education
            var guid when guid == Guid.Parse("d8e267d2-ed85-4eee-8119-45d0c6cc5f6b") => "031", // Professional Graduate Certificate in Education
            var guid when guid == Guid.Parse("7c703efb-a5d3-41d3-b243-ee8974695dd8") => "050", // Professional Graduate Diploma in Education
            var guid when guid == Guid.Parse("4ec0a016-07eb-47b4-8cdd-e276945d401e") => "049", // Qualification gained in Europe
            var guid when guid == Guid.Parse("b02914fe-3a30-4f7c-94ec-0cd87a75834d") => "043", // Teachers Certificate
            var guid when guid == Guid.Parse("7471551d-132e-4c5d-82cc-a41190f01245") => "042", // Teachers Certificate FE
            var guid when guid == Guid.Parse("dba69141-4101-4e05-80e0-524e3967d589") => "028", // Undergraduate Master of Teaching
            var guid when guid == Guid.Parse("9cf31754-5ac5-46a1-99e5-5c98cba1b881") => "999", // Unknown
            _ => (string?)null
        };

        if (mapped is not null)
        {
            result = mapped;
            return true;
        }
        else
        {
            result = default;
            return false;
        }
    }
}
