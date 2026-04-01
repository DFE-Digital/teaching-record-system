using Optional;
using Optional.Unsafe;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public Task<SupportTask> CreateOneLoginUserRecordMatchingSupportTaskAsync(
        string oneLoginUserSubject,
        Action<CreateOneLoginUserRecordMatchingSupportTaskBuilder>? configure = null)
    {
        var builder = new CreateOneLoginUserRecordMatchingSupportTaskBuilder(oneLoginUserSubject);
        configure?.Invoke(builder);
        return builder.ExecuteAsync(this);
    }

    public class CreateOneLoginUserRecordMatchingSupportTaskBuilder(string oneLoginUserSubject)
    {
        private Option<string> _emailAddress;
        private Option<string[][]> _verifiedNames;
        private Option<DateOnly> _verifiedDateOfBirth;
        private Option<string?> _statedNationalInsuranceNumber;
        private Option<string> _statedTrn;
        private Option<string?> _trnTokenTrn;
        private Option<Guid> _clientApplicationUserId;
        private Option<SupportTaskStatus> _status;
        private Option<DateTime> _createdOn;
        private Option<string> _trnRequestId;

        public CreateOneLoginUserRecordMatchingSupportTaskBuilder WithEmailAddress(string emailAddress)
        {
            _emailAddress = Option.Some(emailAddress);
            return this;
        }

        public CreateOneLoginUserRecordMatchingSupportTaskBuilder WithVerifiedNames(params string[][] verifiedNames)
        {
            _verifiedNames = Option.Some(verifiedNames.ToArray());
            return this;
        }

        public CreateOneLoginUserRecordMatchingSupportTaskBuilder WithVerifiedDateOfBirth(DateOnly verifiedDateOfBirth)
        {
            _verifiedDateOfBirth = Option.Some(verifiedDateOfBirth);
            return this;
        }

        public CreateOneLoginUserRecordMatchingSupportTaskBuilder WithStatedNationalInsuranceNumber(string? statedNationalInsuranceNumber)
        {
            _statedNationalInsuranceNumber = Option.Some(statedNationalInsuranceNumber);
            return this;
        }

        public CreateOneLoginUserRecordMatchingSupportTaskBuilder WithStatedTrn(string statedTrn)
        {
            _statedTrn = Option.Some(statedTrn);
            return this;
        }

        public CreateOneLoginUserRecordMatchingSupportTaskBuilder WithTrnTokenTrn(string? trnTokenTrn)
        {
            _trnTokenTrn = Option.Some(trnTokenTrn);
            return this;
        }

        public CreateOneLoginUserRecordMatchingSupportTaskBuilder WithClientApplicationUserId(Guid clientApplicationUserId)
        {
            _clientApplicationUserId = Option.Some(clientApplicationUserId);
            return this;
        }

        public CreateOneLoginUserRecordMatchingSupportTaskBuilder WithStatus(SupportTaskStatus status)
        {
            _status = Option.Some(status);
            return this;
        }

        public CreateOneLoginUserRecordMatchingSupportTaskBuilder WithCreatedOn(DateTime createdOn)
        {
            _createdOn = Option.Some(DateTime.SpecifyKind(createdOn, DateTimeKind.Utc));
            return this;
        }

        public CreateOneLoginUserRecordMatchingSupportTaskBuilder WithTrnRequest(string trnRequestId)
        {
            _trnRequestId = Option.Some(trnRequestId);
            return this;
        }

        public async Task<SupportTask> ExecuteAsync(TestData testData)
        {
            var emailAddress = _emailAddress.ValueOr(testData.GenerateUniqueEmail());
            var verifiedNames = _verifiedNames.ValueOr([[testData.GenerateFirstName(), testData.GenerateLastName()]]);
            var verifiedDateOfBirth = _verifiedDateOfBirth.ValueOr(testData.GenerateDateOfBirth);
            var statedNationalInsuranceNumber = _statedNationalInsuranceNumber.ValueOr(testData.GenerateNationalInsuranceNumber);
            var statedTrn = _statedTrn.ValueOr(await testData.GenerateTrnAsync());
            var trnTokenTrn = _trnTokenTrn.ValueOrDefault();
            var clientApplicationUserId = _clientApplicationUserId.ValueOr(Guid.NewGuid());
            var status = _status.ValueOr(SupportTaskStatus.Open);
            var createdOn = _createdOn.ValueOr(testData.Clock.UtcNow);
            var trnRequestId = _trnRequestId.ValueOrDefault();

            return await testData.WithDbContextAsync(async dbContext =>
            {
                var data = new OneLoginUserRecordMatchingData()
                {
                    OneLoginUserSubject = oneLoginUserSubject,
                    OneLoginUserEmail = emailAddress,
                    VerifiedNames = verifiedNames,
                    VerifiedDatesOfBirth = [verifiedDateOfBirth],
                    StatedNationalInsuranceNumber = statedNationalInsuranceNumber,
                    StatedTrn = statedTrn,
                    ClientApplicationUserId = clientApplicationUserId,
                    TrnTokenTrn = trnTokenTrn
                };

                if (trnRequestId is not null)
                {
                    var applicationUser = await dbContext.ApplicationUsers.SingleOrDefaultAsync(u => u.UserId == clientApplicationUserId);

                    if (applicationUser is null)
                    {
                        applicationUser = new ApplicationUser()
                        {
                            Name = testData.GenerateApplicationUserName(),
                            UserId = clientApplicationUserId,
                            ApiRoles = [],
                            IsOidcClient = false,
                            RecordMatchingPolicy = RecordMatchingPolicy.Deferred
                        };

                        dbContext.ApplicationUsers.Add(applicationUser);
                    }

                    var firstName = verifiedNames.First().First();
                    var lastName = verifiedNames.First().LastOrDefault() ?? testData.GenerateLastName();

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
                        DateOfBirth = verifiedDateOfBirth,
                        PotentialDuplicate = false,
                        NationalInsuranceNumber = statedNationalInsuranceNumber,
                        Gender = null,
                        AddressLine1 = null,
                        AddressLine2 = null,
                        AddressLine3 = null,
                        City = null,
                        Postcode = null,
                        Country = null,
                        TrnToken = null
                    };

                    dbContext.TrnRequestMetadata.Add(trnRequestMetadata);
                }

                var supportTask = SupportTask.Create(
                    supportTaskType: SupportTaskType.OneLoginUserRecordMatching,
                    data: data,
                    personId: null,
                    oneLoginUserSubject: oneLoginUserSubject,
                    trnRequestApplicationUserId: trnRequestId is not null ? clientApplicationUserId : null,
                    trnRequestId: trnRequestId,
                    createdBy: SystemUser.SystemUserId,
                    now: createdOn,
                    out var createdEvent);
                supportTask.Status = status;

                dbContext.SupportTasks.Add(supportTask);
                dbContext.AddEventWithoutBroadcast(createdEvent);
                await dbContext.SaveChangesAsync();

                return await dbContext.SupportTasks
                    .Include(t => t.OneLoginUser)
                    .Include(t => t.TrnRequestMetadata)
                    .ThenInclude(m => m!.ApplicationUser)
                    .SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            });
        }
    }
}
