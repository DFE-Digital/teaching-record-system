namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Email templates we no longer send, but which historical <see cref="DataStore.Postgres.Models.Email"/> rows
/// still reference. They live here rather than in <see cref="EmailTemplateIds"/> so nothing can pick one up to
/// send with; only back-fills that read old data have any business knowing about them.
/// </summary>
internal static class RetiredEmailTemplateIds
{
    // #2661 (2025-10-22) replaced the change request rejection templates with ones carrying a rejection reason.
    public const string ChangeOfNameRequestRejectedEmailConfirmation = "bc790721-11c7-42e0-8f88-41ea96296602";
    public const string ChangeOfDateOfBirthRequestRejectedEmailConfirmation = "abe622ac-79b0-40b3-a29d-94f359a0f03e";

    // In prod the last email on a retired template went out at 07:36 that day and the first on a new one at
    // 18:19, so any point between the two puts every rejection on the right side of the swap.
    public static readonly DateTime RejectionTemplatesChangedOn = new(2025, 10, 22, 12, 0, 0, DateTimeKind.Utc);
}
