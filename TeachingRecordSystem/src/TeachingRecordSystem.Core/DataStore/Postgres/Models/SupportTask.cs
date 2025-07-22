using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TeachingRecordSystem.Core.Models.SupportTaskData;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class SupportTask
{
    internal static readonly JsonSerializerOptions SerializerOptions = new();
    private static readonly char[] _validReferenceChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

    private JsonDocument _data = null!;

    public required string SupportTaskReference { get; set; }
    public required DateTime CreatedOn { get; init; }
    public required DateTime UpdatedOn { get; set; }
    public required SupportTaskType SupportTaskType { get; init; }
    public required SupportTaskStatus Status { get; set; }
    public string? OneLoginUserSubject { get; init; }
    public Guid? PersonId { get; init; }
    public Guid? TrnRequestApplicationUserId { get; init; }
    public string? TrnRequestId { get; init; }
    public TrnRequestMetadata? TrnRequestMetadata { get; set; }

    public required object Data
    {
        get => JsonSerializer.Deserialize(_data, GetDataType(), SerializerOptions)!;
        init => _data = JsonSerializer.SerializeToDocument(value, GetDataType(), SerializerOptions);
    }

    public static string GenerateSupportTaskReference()
    {
        var random = GetEncodedRandomBytes();
        var checkDigit = GetCheckDigit(random);

        return $"TRS-{random}{checkDigit}";

        static string GetEncodedRandomBytes()
        {
            var randomData = RandomNumberGenerator.GetBytes(7);

            var result = new StringBuilder(randomData.Length);
            var counter = 0;

            foreach (var value in randomData)
            {
                counter = (counter + value) % (_validReferenceChars.Length - 1);
                result.Append(_validReferenceChars[counter]);
            }

            return result.ToString();
        }

        static char GetCheckDigit(string input)
        {
            // Luhn_mod_N_algorithm

            int factor = 2;
            int sum = 0;
            int n = _validReferenceChars.Length;

            for (int i = input.Length - 1; i >= 0; i--)
            {
                int codePoint = Array.IndexOf(_validReferenceChars, input[i]);
                int addend = factor * codePoint;

                factor = (factor == 2) ? 1 : 2;

                addend = (addend / n) + (addend % n);
                sum += addend;
            }

            int remainder = sum % n;
            int checkCodePoint = (n - remainder) % n;

            return _validReferenceChars[checkCodePoint];
        }
    }

    public T GetData<T>() => (T)Data;

    public T UpdateData<T>(Func<T, T> update)
    {
        var currentValue = GetData<T>();
        var newValue = update(currentValue);
        _data = JsonSerializer.SerializeToDocument(newValue, GetDataType(), SerializerOptions);
        return newValue;
    }

    internal static Type GetDataType(SupportTaskType supportTaskType) => supportTaskType switch
    {
        SupportTaskType.ConnectOneLoginUser => typeof(ConnectOneLoginUserData),
        SupportTaskType.ApiTrnRequest => typeof(ApiTrnRequestData),
        SupportTaskType.TrnRequestManualChecksNeeded => typeof(TrnRequestManualChecksNeededData),
        _ => throw new ArgumentException($"Unknown {nameof(SupportTaskType)}: {supportTaskType}'.")
    };

    private Type GetDataType() => GetDataType(SupportTaskType);
}
