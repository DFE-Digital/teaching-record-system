using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class DeleteOldEvidenceFilesJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Fact]
    public async Task ExecuteAsync_DeletesOnlyIdentityEvidenceFilesOlderThanConfiguredRetentionPeriod()
    {
        var retentionPeriodDays = 180;
        var cutoffDate = TimeProvider.UtcNow.AddDays(-retentionPeriodDays);

        var oldChangeNameFileId = Guid.NewGuid();
        var oldChangeDobFileId = Guid.NewGuid();
        var oldOneLoginFileId = Guid.NewGuid();
        var oldProcessNameChangeFileId = Guid.NewGuid();
        var oldProcessDetailsChangeFileId = Guid.NewGuid();

        var recentChangeNameFileId = Guid.NewGuid();
        var recentChangeDobFileId = Guid.NewGuid();
        var recentOneLoginFileId = Guid.NewGuid();
        var recentProcessNameChangeFileId = Guid.NewGuid();
        var recentProcessDetailsChangeFileId = Guid.NewGuid();

        var openTaskFileId = Guid.NewGuid();

        // Create old closed support tasks (using UpdatedOn as the closed date)
        var oldChangeNameTask = await TestData.CreateChangeNameRequestSupportTaskAsync(b => b
            .WithEvidenceFileId(oldChangeNameFileId)
            .WithCreatedOn(cutoffDate.AddDays(-30))
            .WithStatus(SupportTaskStatus.Closed));

        await UpdateSupportTaskUpdatedOnAsync(oldChangeNameTask.SupportTaskReference, cutoffDate.AddDays(-10));

        var oldChangeDobTask = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(b => b
            .WithEvidenceFileId(oldChangeDobFileId)
            .WithCreatedOn(cutoffDate.AddDays(-30))
            .WithStatus(SupportTaskStatus.Closed));

        await UpdateSupportTaskUpdatedOnAsync(oldChangeDobTask.SupportTaskReference, cutoffDate.AddDays(-8));

        var oldOneLoginUser = await TestData.CreateOneLoginUserAsync();
        var oldOneLoginTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oldOneLoginUser.Subject,
            b => b.WithEvidenceFileId(oldOneLoginFileId)
                  .WithCreatedOn(cutoffDate.AddDays(-30))
                  .WithStatus(SupportTaskStatus.Closed));

        await UpdateSupportTaskUpdatedOnAsync(oldOneLoginTask.SupportTaskReference, cutoffDate.AddDays(-5));

        // Create recent closed support tasks (closed after the cutoff)
        var recentChangeNameTask = await TestData.CreateChangeNameRequestSupportTaskAsync(b => b
            .WithEvidenceFileId(recentChangeNameFileId)
            .WithCreatedOn(cutoffDate.AddDays(-5))
            .WithStatus(SupportTaskStatus.Closed));

        await UpdateSupportTaskUpdatedOnAsync(recentChangeNameTask.SupportTaskReference, cutoffDate.AddDays(10));

        var recentChangeDobTask = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(b => b
            .WithEvidenceFileId(recentChangeDobFileId)
            .WithCreatedOn(cutoffDate.AddDays(-5))
            .WithStatus(SupportTaskStatus.Closed));

        await UpdateSupportTaskUpdatedOnAsync(recentChangeDobTask.SupportTaskReference, cutoffDate.AddDays(12));

        var recentOneLoginUser = await TestData.CreateOneLoginUserAsync();
        var recentOneLoginTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            recentOneLoginUser.Subject,
            b => b.WithEvidenceFileId(recentOneLoginFileId)
                  .WithCreatedOn(cutoffDate.AddDays(-5))
                  .WithStatus(SupportTaskStatus.Closed));

        await UpdateSupportTaskUpdatedOnAsync(recentOneLoginTask.SupportTaskReference, cutoffDate.AddDays(15));

        // Create an old open task (should NOT be deleted even though it's old)
        var openTask = await TestData.CreateChangeNameRequestSupportTaskAsync(b => b
            .WithEvidenceFileId(openTaskFileId)
            .WithCreatedOn(cutoffDate.AddDays(-30))
            .WithStatus(SupportTaskStatus.Open));

        // Create old PersonDetailsUpdating process with name change evidence only
        var person1 = await TestData.CreatePersonAsync();
        var oldProcessNameChange = new Process
        {
            ProcessId = Guid.NewGuid(),
            ProcessType = ProcessType.PersonDetailsUpdating,
            CreatedOn = cutoffDate.AddDays(-20),
            UpdatedOn = cutoffDate.AddDays(-6),
            UserId = null,
            DqtUserId = null,
            DqtUserName = null,
            PersonIds = [person1.PersonId],
            OneLoginUserSubjects = [],
            SupportTaskReferences = [],
            ChangeReason = new Events.ChangeReasons.PersonDetailsChangeReasonInfo
            {
                NameChangeReason = "Marriage",
                NameChangeEvidenceFile = new EventModels.File
                {
                    FileId = oldProcessNameChangeFileId,
                    Name = "old-process-name.pdf"
                },
                Reason = "Name change",
                Details = "Name changed after marriage",
                EvidenceFile = null,
                AdditionalInformation = null
            }
        };

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Processes.Add(oldProcessNameChange);
            await dbContext.SaveChangesAsync();
        });

        // Create old PersonDetailsUpdating process with other details evidence only  
        var person2 = await TestData.CreatePersonAsync();
        var oldProcessDetailsChange = new Process
        {
            ProcessId = Guid.NewGuid(),
            ProcessType = ProcessType.PersonDetailsUpdating,
            CreatedOn = cutoffDate.AddDays(-18),
            UpdatedOn = cutoffDate.AddDays(-4),
            UserId = null,
            DqtUserId = null,
            DqtUserName = null,
            PersonIds = [person2.PersonId],
            OneLoginUserSubjects = [],
            SupportTaskReferences = [],
            ChangeReason = new Events.ChangeReasons.PersonDetailsChangeReasonInfo
            {
                NameChangeReason = null,
                NameChangeEvidenceFile = null,
                Reason = "DOB correction",
                Details = "Birth certificate provided",
                EvidenceFile = new EventModels.File
                {
                    FileId = oldProcessDetailsChangeFileId,
                    Name = "old-process-details.pdf"
                },
                AdditionalInformation = null
            }
        };

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Processes.Add(oldProcessDetailsChange);
            await dbContext.SaveChangesAsync();
        });

        // Create recent PersonDetailsUpdating process with name change evidence
        var person3 = await TestData.CreatePersonAsync();
        var recentProcessNameChange = new Process
        {
            ProcessId = Guid.NewGuid(),
            ProcessType = ProcessType.PersonDetailsUpdating,
            CreatedOn = cutoffDate.AddDays(-10),
            UpdatedOn = cutoffDate.AddDays(9),
            UserId = null,
            DqtUserId = null,
            DqtUserName = null,
            PersonIds = [person3.PersonId],
            OneLoginUserSubjects = [],
            SupportTaskReferences = [],
            ChangeReason = new Events.ChangeReasons.PersonDetailsChangeReasonInfo
            {
                NameChangeReason = "Deed poll",
                NameChangeEvidenceFile = new EventModels.File
                {
                    FileId = recentProcessNameChangeFileId,
                    Name = "recent-process-name.pdf"
                },
                Reason = "Name change",
                Details = "Recent name change",
                EvidenceFile = null,
                AdditionalInformation = null
            }
        };

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Processes.Add(recentProcessNameChange);
            await dbContext.SaveChangesAsync();
        });

        // Create recent PersonDetailsUpdating process with other details evidence
        var person4 = await TestData.CreatePersonAsync();
        var recentProcessDetailsChange = new Process
        {
            ProcessId = Guid.NewGuid(),
            ProcessType = ProcessType.PersonDetailsUpdating,
            CreatedOn = cutoffDate.AddDays(-8),
            UpdatedOn = cutoffDate.AddDays(11),
            UserId = null,
            DqtUserId = null,
            DqtUserName = null,
            PersonIds = [person4.PersonId],
            OneLoginUserSubjects = [],
            SupportTaskReferences = [],
            ChangeReason = new Events.ChangeReasons.PersonDetailsChangeReasonInfo
            {
                NameChangeReason = null,
                NameChangeEvidenceFile = null,
                Reason = "Other correction",
                Details = "Recent correction",
                EvidenceFile = new EventModels.File
                {
                    FileId = recentProcessDetailsChangeFileId,
                    Name = "recent-process-details.pdf"
                },
                AdditionalInformation = null
            }
        };

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Processes.Add(recentProcessDetailsChange);
            await dbContext.SaveChangesAsync();
        });

        var fakeFileService = new FakeFileService();
        var fakeSafeFileService = new FakeSafeFileService();
        var options = Options.Create(new DeleteOldEvidenceFilesJobOptions
        {
            JobSchedule = "0 0 * * *",
            RetentionPeriodDays = retentionPeriodDays
        });

        await WithServiceAsync<DeleteOldEvidenceFilesJob>(
            async job => await job.ExecuteAsync(CancellationToken.None),
            fakeFileService,
            fakeSafeFileService,
            options);

        // Should delete 4 old files from regular storage (2 from support tasks, 2 from process)
        Assert.Equal(4, fakeFileService.DeletedFileIds.Count);
        Assert.Contains(oldChangeNameFileId, fakeFileService.DeletedFileIds);
        Assert.Contains(oldChangeDobFileId, fakeFileService.DeletedFileIds);
        Assert.Contains(oldProcessNameChangeFileId, fakeFileService.DeletedFileIds);
        Assert.Contains(oldProcessDetailsChangeFileId, fakeFileService.DeletedFileIds);

        // Should delete 1 old file from safe storage (OneLogin verification)
        Assert.Single(fakeSafeFileService.DeletedFileIds);
        Assert.Contains(oldOneLoginFileId, fakeSafeFileService.DeletedFileIds);

        // Should not delete recent files from regular storage
        Assert.DoesNotContain(recentChangeNameFileId, fakeFileService.DeletedFileIds);
        Assert.DoesNotContain(recentChangeDobFileId, fakeFileService.DeletedFileIds);
        Assert.DoesNotContain(recentProcessNameChangeFileId, fakeFileService.DeletedFileIds);
        Assert.DoesNotContain(recentProcessDetailsChangeFileId, fakeFileService.DeletedFileIds);

        // Should not delete recent files from safe storage
        Assert.DoesNotContain(recentOneLoginFileId, fakeSafeFileService.DeletedFileIds);

        // Should not delete files from open/non-closed tasks
        Assert.DoesNotContain(openTaskFileId, fakeFileService.DeletedFileIds);

        await WithDbContextAsync(async dbContext =>
        {
            var jobMetadata = await dbContext.JobMetadata.FirstOrDefaultAsync(j => j.JobName == nameof(DeleteOldEvidenceFilesJob));
            Assert.NotNull(jobMetadata);
            Assert.True(jobMetadata.Metadata.ContainsKey(DeleteOldEvidenceFilesJob.LastCutoffDateKey));
        });
    }

    private async Task UpdateSupportTaskUpdatedOnAsync(string supportTaskReference, DateTimeOffset updatedOn)
    {
        await WithDbContextAsync(async dbContext =>
        {
            var task = await dbContext.SupportTasks.FirstAsync(st => st.SupportTaskReference == supportTaskReference);
            task.UpdatedOn = updatedOn.UtcDateTime;
            await dbContext.SaveChangesAsync();
        });
    }

    [Fact]
    public async Task ExecuteAsync_OnSecondRun_OnlyDeletesFilesExpiredSinceLastRun()
    {
        var retentionPeriodDays = 180;
        var firstCutoffDate = TimeProvider.UtcNow.AddDays(-retentionPeriodDays);

        var firstRunFileId = Guid.NewGuid();
        var firstRunTask = await TestData.CreateChangeNameRequestSupportTaskAsync(b => b
            .WithEvidenceFileId(firstRunFileId)
            .WithCreatedOn(firstCutoffDate.AddDays(-30))
            .WithStatus(SupportTaskStatus.Closed));

        await UpdateSupportTaskUpdatedOnAsync(firstRunTask.SupportTaskReference, firstCutoffDate.AddDays(-10));

        var fakeFileService = new FakeFileService();
        var fakeSafeFileService = new FakeSafeFileService();
        var options = Options.Create(new DeleteOldEvidenceFilesJobOptions
        {
            JobSchedule = "0 0 * * *",
            RetentionPeriodDays = retentionPeriodDays
        });

        await WithServiceAsync<DeleteOldEvidenceFilesJob>(
            async job => await job.ExecuteAsync(CancellationToken.None),
            fakeFileService,
            fakeSafeFileService,
            options);

        Assert.Single(fakeFileService.DeletedFileIds);
        Assert.Contains(firstRunFileId, fakeFileService.DeletedFileIds);

        fakeFileService.DeletedFileIds.Clear();
        fakeSafeFileService.DeletedFileIds.Clear();

        var advancedTime = TimeProvider.UtcNow.AddDays(10).ToUniversalTime();
        TimeProvider.SetUtcNow(advancedTime);
        var secondCutoffDate = advancedTime.AddDays(-retentionPeriodDays);

        // Create a task that was closed between firstCutoffDate and secondCutoffDate (should be deleted)
        var secondRunFileId = Guid.NewGuid();
        var secondRunTask = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(b => b
            .WithEvidenceFileId(secondRunFileId)
            .WithCreatedOn(secondCutoffDate.AddDays(-30))
            .WithStatus(SupportTaskStatus.Closed));

        await UpdateSupportTaskUpdatedOnAsync(secondRunTask.SupportTaskReference, secondCutoffDate.AddDays(-5));

        // Create a task closed after the new cutoff (should NOT be deleted)
        var thirdFileId = Guid.NewGuid();
        var thirdTask = await TestData.CreateChangeNameRequestSupportTaskAsync(b => b
            .WithEvidenceFileId(thirdFileId)
            .WithCreatedOn(secondCutoffDate.AddDays(-10))
            .WithStatus(SupportTaskStatus.Closed));

        await UpdateSupportTaskUpdatedOnAsync(thirdTask.SupportTaskReference, secondCutoffDate.AddDays(5));

        await WithServiceAsync<DeleteOldEvidenceFilesJob>(
            async job => await job.ExecuteAsync(CancellationToken.None),
            fakeFileService,
            fakeSafeFileService,
            options);

        // Should only delete the file that expired between first and second run
        Assert.Single(fakeFileService.DeletedFileIds);
        Assert.Contains(secondRunFileId, fakeFileService.DeletedFileIds);
        Assert.DoesNotContain(firstRunFileId, fakeFileService.DeletedFileIds);
        Assert.DoesNotContain(thirdFileId, fakeFileService.DeletedFileIds);
    }

    private class FakeFileService : IFileService
    {
        public HashSet<Guid> DeletedFileIds { get; } = [];

        public Task<bool> DeleteFileAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            DeletedFileIds.Add(fileId);
            return Task.FromResult(true);
        }

        public Task<string> GetFileUrlAsync(Guid fileId, TimeSpan expiresAfter, CancellationToken cancellationToken = default)
        {
            return Task.FromResult($"https://fake-storage.example.com/{fileId}");
        }

        public Task<string?> TryGetFileUrlAsync(Guid fileId, TimeSpan expiresAfter, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<string?>($"https://fake-storage.example.com/{fileId}");
        }

        public Task<Stream> OpenReadStreamAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Stream>(new MemoryStream());
        }

        public Task<Guid> UploadFileAsync(Stream stream, string? contentType, Guid? fileIdOverride = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(fileIdOverride ?? Guid.NewGuid());
        }

        public Task<bool> UploadFileAsync(string fileName, Stream stream, string? contentType, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }
    }

    private class FakeSafeFileService : ISafeFileService
    {
        public HashSet<Guid> DeletedFileIds { get; } = [];

        public Task<bool> DeleteFileAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            DeletedFileIds.Add(fileId);
            return Task.FromResult(true);
        }

        public Task<string> GetFileUrlAsync(Guid fileId, TimeSpan expiresAfter, CancellationToken cancellationToken = default)
        {
            return Task.FromResult($"https://fake-safe-storage.example.com/{fileId}");
        }

        public Task<Stream> OpenReadStreamAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Stream>(new MemoryStream());
        }

        public Task<bool> TrySafeUploadAsync(Stream stream, string? contentType, out Guid fileId, Guid? fileIdOverride = null, CancellationToken cancellationToken = default)
        {
            fileId = fileIdOverride ?? Guid.NewGuid();
            return Task.FromResult(true);
        }
    }
}
