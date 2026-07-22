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
        var oldLegacyNameChangeFileId = Guid.NewGuid();
        var oldLegacyDetailsChangeFileId = Guid.NewGuid();

        var recentChangeNameFileId = Guid.NewGuid();
        var recentChangeDobFileId = Guid.NewGuid();
        var recentOneLoginFileId = Guid.NewGuid();
        var recentProcessNameChangeFileId = Guid.NewGuid();
        var recentProcessDetailsChangeFileId = Guid.NewGuid();
        var recentLegacyNameChangeFileId = Guid.NewGuid();
        var recentLegacyDetailsChangeFileId = Guid.NewGuid();

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

        // Create old legacy PersonDetailsUpdatedEvent with name change evidence only
        var person5 = await TestData.CreatePersonAsync();
        var oldLegacyNameChangeEvent = new LegacyEvents.PersonDetailsUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = cutoffDate.AddDays(-7),
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person5.PersonId,
            PersonAttributes = new EventModels.PersonDetails
            {
                FirstName = person5.FirstName,
                MiddleName = person5.MiddleName,
                LastName = "NewLastName",
                DateOfBirth = person5.DateOfBirth,
                EmailAddress = person5.EmailAddress,
                NationalInsuranceNumber = person5.NationalInsuranceNumber,
                Gender = person5.Gender
            },
            OldPersonAttributes = new EventModels.PersonDetails
            {
                FirstName = person5.FirstName,
                MiddleName = person5.MiddleName,
                LastName = person5.LastName,
                DateOfBirth = person5.DateOfBirth,
                EmailAddress = person5.EmailAddress,
                NationalInsuranceNumber = person5.NationalInsuranceNumber,
                Gender = person5.Gender
            },
            NameChangeReason = "Marriage",
            NameChangeEvidenceFile = new EventModels.File { FileId = oldLegacyNameChangeFileId, Name = "old-name.pdf" },
            DetailsChangeReason = null,
            DetailsChangeReasonDetail = null,
            DetailsChangeEvidenceFile = null,
            Changes = LegacyEvents.PersonDetailsUpdatedEventChanges.LastName
        };

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(oldLegacyNameChangeEvent);
            await dbContext.SaveChangesAsync();
        });

        // Create old legacy PersonDetailsUpdatedEvent with details change evidence only
        var person6 = await TestData.CreatePersonAsync();
        var oldLegacyDetailsChangeEvent = new LegacyEvents.PersonDetailsUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = cutoffDate.AddDays(-5),
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person6.PersonId,
            PersonAttributes = new EventModels.PersonDetails
            {
                FirstName = person6.FirstName,
                MiddleName = person6.MiddleName,
                LastName = person6.LastName,
                DateOfBirth = person6.DateOfBirth.AddYears(-1),
                EmailAddress = person6.EmailAddress,
                NationalInsuranceNumber = person6.NationalInsuranceNumber,
                Gender = person6.Gender
            },
            OldPersonAttributes = new EventModels.PersonDetails
            {
                FirstName = person6.FirstName,
                MiddleName = person6.MiddleName,
                LastName = person6.LastName,
                DateOfBirth = person6.DateOfBirth,
                EmailAddress = person6.EmailAddress,
                NationalInsuranceNumber = person6.NationalInsuranceNumber,
                Gender = person6.Gender
            },
            NameChangeReason = null,
            NameChangeEvidenceFile = null,
            DetailsChangeReason = "DOB correction",
            DetailsChangeReasonDetail = "Birth certificate",
            DetailsChangeEvidenceFile = new EventModels.File { FileId = oldLegacyDetailsChangeFileId, Name = "old-dob.pdf" },
            Changes = LegacyEvents.PersonDetailsUpdatedEventChanges.DateOfBirth
        };

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(oldLegacyDetailsChangeEvent);
            await dbContext.SaveChangesAsync();
        });

        // Create recent legacy PersonDetailsUpdatedEvent with name change evidence
        var person7 = await TestData.CreatePersonAsync();
        var recentLegacyNameChangeEvent = new LegacyEvents.PersonDetailsUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = cutoffDate.AddDays(8),
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person7.PersonId,
            PersonAttributes = new EventModels.PersonDetails
            {
                FirstName = person7.FirstName,
                MiddleName = person7.MiddleName,
                LastName = "NewName",
                DateOfBirth = person7.DateOfBirth,
                EmailAddress = person7.EmailAddress,
                NationalInsuranceNumber = person7.NationalInsuranceNumber,
                Gender = person7.Gender
            },
            OldPersonAttributes = new EventModels.PersonDetails
            {
                FirstName = person7.FirstName,
                MiddleName = person7.MiddleName,
                LastName = person7.LastName,
                DateOfBirth = person7.DateOfBirth,
                EmailAddress = person7.EmailAddress,
                NationalInsuranceNumber = person7.NationalInsuranceNumber,
                Gender = person7.Gender
            },
            NameChangeReason = "Deed poll",
            NameChangeEvidenceFile = new EventModels.File { FileId = recentLegacyNameChangeFileId, Name = "recent-name.pdf" },
            DetailsChangeReason = null,
            DetailsChangeReasonDetail = null,
            DetailsChangeEvidenceFile = null,
            Changes = Core.Events.Legacy.PersonDetailsUpdatedEventChanges.LastName
        };

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(recentLegacyNameChangeEvent);
            await dbContext.SaveChangesAsync();
        });

        // Create recent legacy PersonDetailsUpdatedEvent with details change evidence
        var person8 = await TestData.CreatePersonAsync();
        var recentLegacyDetailsChangeEvent = new LegacyEvents.PersonDetailsUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = cutoffDate.AddDays(10),
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person8.PersonId,
            PersonAttributes = new EventModels.PersonDetails
            {
                FirstName = person8.FirstName,
                MiddleName = person8.MiddleName,
                LastName = person8.LastName,
                DateOfBirth = person8.DateOfBirth.AddYears(-1),
                EmailAddress = person8.EmailAddress,
                NationalInsuranceNumber = person8.NationalInsuranceNumber,
                Gender = person8.Gender
            },
            OldPersonAttributes = new EventModels.PersonDetails
            {
                FirstName = person8.FirstName,
                MiddleName = person8.MiddleName,
                LastName = person8.LastName,
                DateOfBirth = person8.DateOfBirth,
                EmailAddress = person8.EmailAddress,
                NationalInsuranceNumber = person8.NationalInsuranceNumber,
                Gender = person8.Gender
            },
            NameChangeReason = null,
            NameChangeEvidenceFile = null,
            DetailsChangeReason = "Other",
            DetailsChangeReasonDetail = "Details",
            DetailsChangeEvidenceFile = new EventModels.File { FileId = recentLegacyDetailsChangeFileId, Name = "recent-details.pdf" },
            Changes = LegacyEvents.PersonDetailsUpdatedEventChanges.DateOfBirth
        };

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(recentLegacyDetailsChangeEvent);
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

        // Should delete 6 old files from regular storage (2 from support tasks, 2 from process, 2 from legacy event)
        Assert.Equal(6, fakeFileService.DeletedFileIds.Count);
        Assert.Contains(oldChangeNameFileId, fakeFileService.DeletedFileIds);
        Assert.Contains(oldChangeDobFileId, fakeFileService.DeletedFileIds);
        Assert.Contains(oldProcessNameChangeFileId, fakeFileService.DeletedFileIds);
        Assert.Contains(oldProcessDetailsChangeFileId, fakeFileService.DeletedFileIds);
        Assert.Contains(oldLegacyNameChangeFileId, fakeFileService.DeletedFileIds);
        Assert.Contains(oldLegacyDetailsChangeFileId, fakeFileService.DeletedFileIds);

        // Should delete 1 old file from safe storage (OneLogin verification)
        Assert.Single(fakeSafeFileService.DeletedFileIds);
        Assert.Contains(oldOneLoginFileId, fakeSafeFileService.DeletedFileIds);

        // Should not delete recent files from regular storage
        Assert.DoesNotContain(recentChangeNameFileId, fakeFileService.DeletedFileIds);
        Assert.DoesNotContain(recentChangeDobFileId, fakeFileService.DeletedFileIds);
        Assert.DoesNotContain(recentProcessNameChangeFileId, fakeFileService.DeletedFileIds);
        Assert.DoesNotContain(recentProcessDetailsChangeFileId, fakeFileService.DeletedFileIds);
        Assert.DoesNotContain(recentLegacyNameChangeFileId, fakeFileService.DeletedFileIds);
        Assert.DoesNotContain(recentLegacyDetailsChangeFileId, fakeFileService.DeletedFileIds);

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

        public Task<bool> DeleteFileAsync(Guid fileId)
        {
            DeletedFileIds.Add(fileId);
            return Task.FromResult(true);
        }

        public Task<string> GetFileUrlAsync(Guid fileId, TimeSpan expiresAfter)
        {
            return Task.FromResult($"https://fake-storage.example.com/{fileId}");
        }

        public Task<string?> TryGetFileUrlAsync(Guid fileId, TimeSpan expiresAfter)
        {
            return Task.FromResult<string?>($"https://fake-storage.example.com/{fileId}");
        }

        public Task<Stream> OpenReadStreamAsync(Guid fileId)
        {
            return Task.FromResult<Stream>(new MemoryStream());
        }

        public Task<Guid> UploadFileAsync(Stream stream, string? contentType, Guid? fileIdOverride = null)
        {
            return Task.FromResult(fileIdOverride ?? Guid.NewGuid());
        }

        public Task<bool> UploadFileAsync(string fileName, Stream stream, string? contentType)
        {
            return Task.FromResult(true);
        }
    }

    private class FakeSafeFileService : ISafeFileService
    {
        public HashSet<Guid> DeletedFileIds { get; } = [];

        public Task<bool> DeleteFileAsync(Guid fileId)
        {
            DeletedFileIds.Add(fileId);
            return Task.FromResult(true);
        }

        public Task<string> GetFileUrlAsync(Guid fileId, TimeSpan expiresAfter)
        {
            return Task.FromResult($"https://fake-safe-storage.example.com/{fileId}");
        }

        public Task<Stream> OpenReadStreamAsync(Guid fileId)
        {
            return Task.FromResult<Stream>(new MemoryStream());
        }

        public Task<bool> TrySafeUploadAsync(Stream stream, string? contentType, out Guid fileId, Guid? fileIdOverride = null)
        {
            fileId = fileIdOverride ?? Guid.NewGuid();
            return Task.FromResult(true);
        }
    }
}
