using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Finds the <see cref="Email"/> a back-filled <see cref="EmailSentEvent"/> should point at, so the
/// event references a real row rather than a made-up id.
/// </summary>
public class SentEmailMatcher
{
    // The email is sent by a background job shortly after the process runs, so an email sent within
    // this window of the process is taken to be that process's email.
    private static readonly TimeSpan _sentBeforeTolerance = TimeSpan.FromHours(1);
    private static readonly TimeSpan _sentAfterTolerance = TimeSpan.FromDays(1);

    private readonly Dictionary<(string TemplateId, string EmailAddress), Email[]> _candidates;
    private readonly HashSet<Guid> _claimedEmailIds;

    private SentEmailMatcher(
        Dictionary<(string TemplateId, string EmailAddress), Email[]> candidates,
        HashSet<Guid> claimedEmailIds)
    {
        _candidates = candidates;
        _claimedEmailIds = claimedEmailIds;
    }

    public static async Task<SentEmailMatcher> CreateAsync(
        TrsDbContext dbContext,
        string[] templateIds,
        ProcessType[] processTypes,
        CancellationToken cancellationToken)
    {
        var candidates = (await dbContext.Emails
                .Where(e => templateIds.Contains(e.TemplateId) && e.SentOn != null)
                .ToListAsync(cancellationToken))
            .GroupBy(e => (e.TemplateId, e.EmailAddress))
            .ToDictionary(g => g.Key, g => g.OrderBy(e => e.SentOn).ToArray());

        // Emails that a process already points at are not up for grabs.
        var claimedEmailIds = (await dbContext.ProcessEvents
                .Where(pe => pe.EventName == nameof(EmailSentEvent))
                .Where(pe => dbContext.Processes.Any(p => p.ProcessId == pe.ProcessId && processTypes.Contains(p.ProcessType)))
                .Select(pe => pe.Payload)
                .ToListAsync(cancellationToken))
            .Select(payload => ((EmailSentEvent)payload).Email.EmailId)
            .ToHashSet();

        return new SentEmailMatcher(candidates, claimedEmailIds);
    }

    /// <summary>
    /// Returns the closest unclaimed email sent to <paramref name="emailAddress"/> from
    /// <paramref name="templateId"/> around <paramref name="sentAround"/>, claiming it so a later
    /// call can't return it again.
    /// </summary>
    public Email? Match(string templateId, string emailAddress, DateTime sentAround)
    {
        if (!_candidates.TryGetValue((templateId, emailAddress), out var emails))
        {
            return null;
        }

        var match = emails
            .Where(e => !_claimedEmailIds.Contains(e.EmailId))
            .Where(e =>
                e.SentOn >= sentAround - _sentBeforeTolerance &&
                e.SentOn <= sentAround + _sentAfterTolerance)
            .OrderBy(e => (e.SentOn!.Value - sentAround).Duration())
            .FirstOrDefault();

        if (match is not null)
        {
            Claim(match.EmailId);
        }

        return match;
    }

    public void Claim(Guid emailId) => _claimedEmailIds.Add(emailId);
}
