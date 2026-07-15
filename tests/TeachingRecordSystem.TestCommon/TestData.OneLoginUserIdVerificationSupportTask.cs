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
        private Option<string> _emailAddress;
        private Option<string> _statedFirstName;
        private Option<string> _statedLastName;
        private Option<DateOnly> _statedDateOfBirth;
        private Option<string?> _statedNationalInsuranceNumber;
        private Option<string?> _statedTrn;
        private Option<Guid> _evidenceFileId;
        private Option<string> _evidenceFileName;
        private Option<string?> _trnTokenTrn;
        private Option<Guid> _clientApplicationUserId;
        private Option<SupportTaskStatus> _status;
        private Option<DateTime> _createdOn;
        private Option<string> _trnRequestId;
        private Option<string[]> _zendeskTickets;

        public CreateOneLoginUserIdVerificationSupportTaskBuilder WithEmailAddress(string emailAddress)
        {
            _emailAddress = Option.Some(emailAddress);
            return this;
        }

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

        public CreateOneLoginUserIdVerificationSupportTaskBuilder WithStatedTrn(string? statedTrn)
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
            _createdOn = Option.Some(DateTime.SpecifyKind(createdOn, DateTimeKind.Utc));
            return this;
        }

        public CreateOneLoginUserIdVerificationSupportTaskBuilder WithTrnRequestId(string trnRequestId)
        {
            _trnRequestId = Option.Some(trnRequestId);
            return this;
        }

        public CreateOneLoginUserIdVerificationSupportTaskBuilder WithZendeskTickets(
            params string[] zendeskTickets)
        {
            _zendeskTickets = Option.Some(zendeskTickets);
            return this;
        }

        public Task<SupportTask> ExecuteAsync(TestData testData) =>
            testData.WithDbContextAsync(async dbContext =>
            {
                var emailAddress = _emailAddress.ValueOr(testData.GenerateUniqueEmail());
                var statedFirstName = _statedFirstName.ValueOr(testData.GenerateFirstName);
                var statedLastName = _statedLastName.ValueOr(testData.GenerateLastName);
                var statedDateOfBirth = _statedDateOfBirth.ValueOr(testData.GenerateDateOfBirth);
                var statedNationalInsuranceNumber = _statedNationalInsuranceNumber.ValueOr(testData.GenerateNationalInsuranceNumber);
                var statedTrn = _statedTrn.HasValue ? _statedTrn.ValueOrDefault() : "0000000";
                var evidenceFileId = _evidenceFileId.ValueOr(Guid.NewGuid());
                var evidenceFileName = _evidenceFileName.ValueOr("evidence.pdf");
                var trnTokenTrn = _trnTokenTrn.ValueOrDefault();
                var status = _status.ValueOr(SupportTaskStatus.Open);
                var createdOn = _createdOn.ValueOr(testData.TimeProvider.UtcNow);
                var zendeskTickets = _zendeskTickets.ValueOr([]);

                Guid clientApplicationUserId;
                string? trnRequestId = _trnRequestId.ValueOrDefault();

                if (_clientApplicationUserId.HasValue)
                {
                    clientApplicationUserId = _clientApplicationUserId.ValueOrFailure();
                }
                else
                {
                    clientApplicationUserId = Guid.NewGuid();

                    var applicationUser = new ApplicationUser
                    {
                        UserId = clientApplicationUserId,
                        Name = testData.GenerateApplicationUserName(),
                        ApiRoles = [],
                        IsOidcClient = false,
                        RecordMatchingPolicy = RecordMatchingPolicy.Deferred
                    };

                    dbContext.ApplicationUsers.Add(applicationUser);
                }

                if (trnRequestId is not null)
                {
                    var trnRequestExists = await dbContext.TrnRequestMetadata.AnyAsync(m => m.RequestId == trnRequestId);

                    if (!trnRequestExists)
                    {
                        var firstName = statedFirstName;
                        var lastName = statedLastName;

                        var trnRequestMetadata = new TrnRequestMetadata
                        {
                            ApplicationUserId = clientApplicationUserId,
                            RequestId = trnRequestId,
                            CreatedOn = createdOn,
                            IdentityVerified = true,
                            EmailAddress = emailAddress,
                            OneLoginUserSubject = oneLoginUserSubject,
                            FirstName = firstName,
                            MiddleName = null,
                            LastName = lastName,
                            PreviousFirstName = null,
                            PreviousLastName = null,
                            Name = [firstName, lastName],
                            DateOfBirth = statedDateOfBirth,
                            PotentialDuplicate = false,
                            NationalInsuranceNumber = statedNationalInsuranceNumber,
                            Gender = null,
                            AddressLine1 = null,
                            AddressLine2 = null,
                            AddressLine3 = null,
                            City = null,
                            Postcode = null,
                            Country = null,
                            TrnToken = null,
                            Status = TrnRequestStatus.Dormant
                        };

                        dbContext.TrnRequestMetadata.Add(trnRequestMetadata);
                    }
                }

                var subject = SupportTask.Subject.FromOneLoginUser(statedFirstName, statedLastName);

                var supportTask = new SupportTask
                {
                    CreatedOn = createdOn,
                    UpdatedOn = createdOn,
                    SupportTaskType = SupportTaskType.OneLoginUserIdVerification,
                    Status = status,
                    Data = new OneLoginUserIdVerificationData
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
                    },
                    OneLoginUserSubject = oneLoginUserSubject,
                    SubjectName = subject.Name,
                    SubjectEmailAddress = subject.EmailAddress,
                    ZendeskTickets = zendeskTickets
                };

                dbContext.SupportTasks.Add(supportTask);
                await dbContext.SaveChangesAsync();

                return await dbContext.SupportTasks
                    .Include(t => t.OneLoginUser)
                    .Include(t => t.TrnRequestMetadata)
                    .ThenInclude(m => m!.ApplicationUser)
                    .SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            });
    }
}
