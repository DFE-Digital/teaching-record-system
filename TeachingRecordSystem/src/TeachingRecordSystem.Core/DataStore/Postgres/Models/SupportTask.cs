using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class SupportTask
{
    [Obsolete("Use ISupportTaskData.SerializerOptions instead.")]
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
    public Person? Person { get; }
    public Guid? TrnRequestApplicationUserId { get; init; }
    public string? TrnRequestId { get; init; }
    public TrnRequestMetadata? TrnRequestMetadata { get; }

    public required object Data
    {
#pragma warning disable CS0618 // Type or member is obsolete
        get => JsonSerializer.Deserialize(_data, SupportTaskType.GetDataType(), SerializerOptions)!;
#pragma warning restore CS0618 // Type or member is obsolete
        init => _data = JsonSerializer.SerializeToDocument(value, typeof(ISupportTaskData), ISupportTaskData.SerializerOptions);
    }

    public static SupportTask Create(
        SupportTaskType supportTaskType,
        object data,
        Guid? personId,
        string? oneLoginUserSubject,
        Guid? trnRequestApplicationUserId,
        string? trnRequestId,
        EventModels.RaisedByUserInfo createdBy,
        DateTime now,
        out SupportTaskCreatedEvent createdEvent)
    {
        var task = new SupportTask
        {
            SupportTaskReference = GenerateSupportTaskReference(),
            CreatedOn = now,
            UpdatedOn = now,
            SupportTaskType = supportTaskType,
            Status = SupportTaskStatus.Open,
            Data = data,
            PersonId = personId,
            OneLoginUserSubject = oneLoginUserSubject,
            TrnRequestApplicationUserId = trnRequestApplicationUserId,
            TrnRequestId = trnRequestId
        };

        createdEvent = new SupportTaskCreatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = createdBy,
            SupportTask = EventModels.SupportTask.FromModel(task)
        };

        return task;
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
        _data = JsonSerializer.SerializeToDocument(newValue, typeof(ISupportTaskData), ISupportTaskData.SerializerOptions);
        return newValue;
    }
}
