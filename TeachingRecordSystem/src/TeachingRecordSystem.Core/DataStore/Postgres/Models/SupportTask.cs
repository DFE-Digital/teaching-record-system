using System.Security.Cryptography;
using System.Text;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class SupportTask
{
    private static readonly char[] _validReferenceChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

    public required string SupportTaskReference { get; set; }
    public required DateTime CreatedOn { get; init; }
    public required DateTime UpdatedOn { get; set; }
    public DateTime? DeletedOn { get; set; }
    public required SupportTaskType SupportTaskType { get; init; }
    public required SupportTaskStatus Status { get; set; }
    public string? OneLoginUserSubject { get; init; }
    public Guid? PersonId { get; init; }
    public Person? Person { get; }
    public Guid? TrnRequestApplicationUserId { get; init; }
    public string? TrnRequestId { get; init; }
    public TrnRequestMetadata? TrnRequestMetadata { get; }
    public required ISupportTaskData Data { get; set; }

    public static SupportTask Create(
        SupportTaskType supportTaskType,
        ISupportTaskData data,
        Guid? personId,
        string? oneLoginUserSubject,
        Guid? trnRequestApplicationUserId,
        string? trnRequestId,
        EventModels.RaisedByUserInfo createdBy,
        DateTime now,
        out LegacyEvents.SupportTaskCreatedEvent createdEvent)
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

        createdEvent = new LegacyEvents.SupportTaskCreatedEvent
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

    public T GetData<T>() where T : ISupportTaskData => (T)Data;

    public T UpdateData<T>(Func<T, T> update) where T : ISupportTaskData
    {
        var currentValue = GetData<T>();
        Data = update(currentValue);
        return (T)Data;
    }
}
