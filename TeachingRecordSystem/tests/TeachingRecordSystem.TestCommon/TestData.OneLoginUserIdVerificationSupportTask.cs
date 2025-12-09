using Optional;
using Optional.Unsafe;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public Task<SupportTask> CreateOneLoginUserIdVerificationSupportTaskAsync(
        string oneLoginUserSubject,
        Action<CreateOneLoginUserIdVerificationSupportTaskBuilder>? configure = null)
    {
        var builder = new CreateOneLoginUserIdVerificationSupportTaskBuilder(oneLoginUserSubject);
        configure?.Invoke(builder);
        return builder.ExecuteAsync(this);
    }

    public class CreateOneLoginUserIdVerificationSupportTaskBuilder(string oneLoginUserSubject)
    {
        private Option<string> _statedFirstName;
        private Option<string> _statedLastName;
        private Option<DateOnly> _statedDateOfBirth;
        private Option<string?> _statedNationalInsuranceNumber;
        private Option<string> _statedTrn;
        private Option<Guid> _evidenceFileId;
        private Option<string> _evidenceFileName;
        private Option<string?> _trnTokenTrn;
        private Option<Guid> _clientApplicationUserId;
        private Option<SupportTaskStatus> _status;
        private Option<DateTime> _createdOn;

        public CreateOneLoginUserIdVerificationSupportTaskBuilder WithStatedFirstName(string statedFirstName)
        {
            _statedFirstName = Option.Some(statedFirstName);
            return this;
        }

        public CreateOneLoginUserIdVerificationSupportTaskBuilder WithStatedLastName(string statedLastName)
        {
            _statedLastName = Option.Some(statedLastName);
            return this;
        }

        public CreateOneLoginUserIdVerificationSupportTaskBuilder WithStatedDateOfBirth(DateOnly statedDateOfBirth)
        {
            _statedDateOfBirth = Option.Some(statedDateOfBirth);
            return this;
        }

        public CreateOneLoginUserIdVerificationSupportTaskBuilder WithStatedNationalInsuranceNumber(string? statedNationalInsuranceNumber)
        {
            _statedNationalInsuranceNumber = Option.Some(statedNationalInsuranceNumber);
            return this;
        }

        public CreateOneLoginUserIdVerificationSupportTaskBuilder WithStatedTrn(string statedTrn)
        {
            _statedTrn = Option.Some(statedTrn);
            return this;
        }

        public CreateOneLoginUserIdVerificationSupportTaskBuilder WithEvidenceFileId(Guid evidenceFileId)
        {
            _evidenceFileId = Option.Some(evidenceFileId);
            return this;
        }

        public CreateOneLoginUserIdVerificationSupportTaskBuilder WithEvidenceFileName(string evidenceFileName)
        {
            _evidenceFileName = Option.Some(evidenceFileName);
            return this;
        }

        public CreateOneLoginUserIdVerificationSupportTaskBuilder WithTrnTokenTrn(string? trnTokenTrn)
        {
            _trnTokenTrn = Option.Some(trnTokenTrn);
            return this;
        }

        public CreateOneLoginUserIdVerificationSupportTaskBuilder WithClientApplicationUserId(Guid clientApplicationUserId)
        {
            _clientApplicationUserId = Option.Some(clientApplicationUserId);
            return this;
        }

        public CreateOneLoginUserIdVerificationSupportTaskBuilder WithStatus(SupportTaskStatus status)
        {
            _status = Option.Some(status);
            return this;
        }

        public CreateOneLoginUserIdVerificationSupportTaskBuilder WithCreatedOn(DateTime createdOn)
        {
            _createdOn = Option.Some(createdOn);
            return this;
        }

        public Task<SupportTask> ExecuteAsync(TestData testData) =>
            testData.WithDbContextAsync(async dbContext =>
            {
                var statedFirstName = _statedFirstName.ValueOr(testData.GenerateFirstName);
                var statedLastName = _statedLastName.ValueOr(testData.GenerateLastName);
                var statedDateOfBirth = _statedDateOfBirth.ValueOr(testData.GenerateDateOfBirth);
                var statedNationalInsuranceNumber = _statedNationalInsuranceNumber.ValueOr(testData.GenerateNationalInsuranceNumber);
                var statedTrn = _statedTrn.ValueOr(await testData.GenerateTrnAsync());
                var evidenceFileId = _evidenceFileId.ValueOr(Guid.NewGuid());
                var evidenceFileName = _evidenceFileName.ValueOr("evidence.pdf");
                var trnTokenTrn = _trnTokenTrn.ValueOrDefault();
                var clientApplicationUserId = _clientApplicationUserId.ValueOrDefault();
                var status = _status.ValueOr(SupportTaskStatus.Open);
                var createdOn = _createdOn.ValueOr(testData.Clock.UtcNow);

                var data = new OneLoginUserIdVerificationData
                {
                    OneLoginUserSubject = oneLoginUserSubject,
                    StatedFirstName = statedFirstName,
                    StatedLastName = statedLastName,
                    StatedDateOfBirth = statedDateOfBirth,
                    StatedNationalInsuranceNumber = statedNationalInsuranceNumber,
                    StatedTrn = statedTrn,
                    EvidenceFileId = evidenceFileId,
                    EvidenceFileName = evidenceFileName,
                    TrnTokenTrn = trnTokenTrn,
                    ClientApplicationUserId = clientApplicationUserId
                };

                var supportTask = SupportTask.Create(
                    supportTaskType: SupportTaskType.OneLoginUserIdVerification,
                    data: data,
                    personId: null,
                    oneLoginUserSubject: oneLoginUserSubject,
                    trnRequestApplicationUserId: null,
                    trnRequestId: null,
                    createdBy: SystemUser.SystemUserId,
                    now: createdOn,
                    out var createdEvent);
                supportTask.Status = status;

                dbContext.SupportTasks.Add(supportTask);
                dbContext.AddEventWithoutBroadcast(createdEvent);
                await dbContext.SaveChangesAsync();

                return supportTask;
            });
    }
}
