using System.Globalization;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.Core.Jobs;

public class DeleteOldEvidenceFilesJob
{
    public const string LastCutoffDateKey = "LastCutoffDate";

    private readonly TrsDbContext _dbContext;
    private readonly IFileService _fileService;
    private readonly ISafeFileService _safeFileService;
    private readonly IOptions<DeleteOldEvidenceFilesJobOptions> _options;
    private readonly TimeProvider _timeProvider;

    public DeleteOldEvidenceFilesJob(
        TrsDbContext dbContext,
        IFileService fileService,
        ISafeFileService safeFileService,
        IOptions<DeleteOldEvidenceFilesJobOptions> options,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _fileService = fileService;
        _safeFileService = safeFileService;
        _options = options;
        _timeProvider = timeProvider;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var currentCutoffDate = _timeProvider.GetUtcNow().AddDays(-_options.Value.RetentionPeriodDays);
        var (previousCutoffDate, jobMetadata) = await GetLastCutoffDateAsync(cancellationToken);

        var (regularFilesToDelete, safeFilesToDelete) = await GetExpiredIdentityEvidenceFileIdsAsync(previousCutoffDate, currentCutoffDate, cancellationToken);

        foreach (var fileId in regularFilesToDelete)
        {
            await _fileService.DeleteFileAsync(fileId);
        }

        foreach (var fileId in safeFilesToDelete)
        {
            await _safeFileService.DeleteFileAsync(fileId);
        }

        if (jobMetadata != null)
        {
            jobMetadata.Metadata = new Dictionary<string, string>
            {
                { LastCutoffDateKey, currentCutoffDate.ToString("s", CultureInfo.InvariantCulture) }
            };
        }
        else
        {
            var job = new JobMetadata
            {
                JobName = nameof(DeleteOldEvidenceFilesJob),
                Metadata = new Dictionary<string, string>
                {
                    { LastCutoffDateKey, currentCutoffDate.ToString("s", CultureInfo.InvariantCulture) }
                }
            };
            _dbContext.JobMetadata.Add(job);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<(DateTimeOffset?, JobMetadata?)> GetLastCutoffDateAsync(CancellationToken cancellationToken)
    {
        var item = await _dbContext.JobMetadata
            .FirstOrDefaultAsync(i => i.JobName == nameof(DeleteOldEvidenceFilesJob), cancellationToken);

        DateTimeOffset? lastCutoffDate = null;

        if (item?.Metadata != null && item.Metadata.TryGetValue(LastCutoffDateKey, out var dateStr) && DateTimeOffset.TryParse(dateStr, out var parsed))
        {
            lastCutoffDate = parsed.ToUniversalTime();
        }

        return (lastCutoffDate, item);
    }

    private async Task<(List<Guid> regularFiles, List<Guid> safeFiles)> GetExpiredIdentityEvidenceFileIdsAsync(DateTimeOffset? previousCutoffDate, DateTimeOffset currentCutoffDate, CancellationToken cancellationToken)
    {
        var regularFileIds = new HashSet<Guid>();
        var safeFileIds = new HashSet<Guid>();

        // Get file IDs from closed change name support tasks using UpdatedOn as the closed date
        var changeNameTasks = await _dbContext.SupportTasks
            .Where(st => st.SupportTaskType == SupportTaskType.ChangeNameRequest
                && st.Status == SupportTaskStatus.Closed
                && st.UpdatedOn < currentCutoffDate.UtcDateTime
                && (previousCutoffDate == null || st.UpdatedOn >= previousCutoffDate.Value.UtcDateTime))
            .ToListAsync(cancellationToken);

        foreach (var task in changeNameTasks)
        {
            if (task.Data is ChangeNameRequestData data)
            {
                regularFileIds.Add(data.EvidenceFileId);
            }
        }

        // Get file IDs from closed change DOB support tasks using UpdatedOn as the closed date
        var changeDobTasks = await _dbContext.SupportTasks
            .Where(st => st.SupportTaskType == SupportTaskType.ChangeDateOfBirthRequest
                && st.Status == SupportTaskStatus.Closed
                && st.UpdatedOn < currentCutoffDate.UtcDateTime
                && (previousCutoffDate == null || st.UpdatedOn >= previousCutoffDate.Value.UtcDateTime))
            .ToListAsync(cancellationToken);

        foreach (var task in changeDobTasks)
        {
            if (task.Data is ChangeDateOfBirthRequestData data)
            {
                regularFileIds.Add(data.EvidenceFileId);
            }
        }

        // Get file IDs from closed OneLogin identity verification tasks using UpdatedOn as the closed date
        // These files are stored in the safe storage account (with malware scanning)
        var idVerificationTasks = await _dbContext.SupportTasks
            .Where(st => st.SupportTaskType == SupportTaskType.OneLoginUserIdVerification
                && st.Status == SupportTaskStatus.Closed
                && st.UpdatedOn < currentCutoffDate.UtcDateTime
                && (previousCutoffDate == null || st.UpdatedOn >= previousCutoffDate.Value.UtcDateTime))
            .ToListAsync(cancellationToken);

        foreach (var task in idVerificationTasks)
        {
            if (task.Data is OneLoginUserIdVerificationData data)
            {
                safeFileIds.Add(data.EvidenceFileId);
            }
        }

        // Get file IDs from PersonDetailsUpdating processes (newer structure)
        // Use process UpdatedOn date as the completion timestamp
        var personDetailsUpdatingProcesses = await _dbContext.Processes
            .Where(p => p.ProcessType == ProcessType.PersonDetailsUpdating
                && p.UpdatedOn < currentCutoffDate.UtcDateTime
                && (previousCutoffDate == null || p.UpdatedOn >= previousCutoffDate.Value.UtcDateTime))
            .ToListAsync(cancellationToken);

        foreach (var process in personDetailsUpdatingProcesses)
        {
            if (process.ChangeReason is PersonDetailsChangeReasonInfo changeReason)
            {
                if (changeReason.NameChangeEvidenceFile is not null)
                {
                    regularFileIds.Add(changeReason.NameChangeEvidenceFile.FileId);
                }

                if (changeReason.EvidenceFile is not null)
                {
                    regularFileIds.Add(changeReason.EvidenceFile.FileId);
                }
            }
        }

        // Get file IDs from legacy PersonDetailsUpdatedEvent (manual changes before processes were introduced)
        // Use event Created date as the completion timestamp
        // TODO: This can be removed once we backfill old legacy events to the new process model
        var personDetailsEvents = await _dbContext.Events
            .Where(e => e.EventName == "PersonDetailsUpdatedEvent"
                && e.Created < currentCutoffDate.UtcDateTime
                && (previousCutoffDate == null || e.Created >= previousCutoffDate.Value.UtcDateTime))
            .ToListAsync(cancellationToken);

        foreach (var evt in personDetailsEvents)
        {
            var @event = (LegacyEvents.PersonDetailsUpdatedEvent)evt.ToEventBase();

            if (@event.NameChangeEvidenceFile is not null)
            {
                regularFileIds.Add(@event.NameChangeEvidenceFile.FileId);
            }

            if (@event.DetailsChangeEvidenceFile is not null)
            {
                regularFileIds.Add(@event.DetailsChangeEvidenceFile.FileId);
            }
        }

        return (regularFileIds.ToList(), safeFileIds.ToList());
    }
}
