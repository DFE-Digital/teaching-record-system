using Optional;
using Optional.Unsafe;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTaskData;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public Task<SupportTask> CreateNpqTrnRequestSupportTaskAsync(
        Guid applicationUserId,
        Action<CreateNpqTrnRequestSupportTaskBuilder>? configure = null)
    {
        var builder = new CreateNpqTrnRequestSupportTaskBuilder(applicationUserId);
        configure?.Invoke(builder);
        return builder.ExecuteAsync(this);
    }

    public class CreateNpqTrnRequestSupportTaskBuilder(Guid applicationUserId)
    {
        private Option<string> _requestId;
        private Option<string?> _emailAddress;
        private Option<string> _firstName;
        private Option<string> _middleName;
        private Option<string> _lastName;
        private Option<DateOnly> _dateOfBirth;
        private Option<string?> _nationalInsuranceNumber;
        private Option<TrnRequestMatchedPerson[]> _matchedRecords;
        private Option<string> _npqApplicationId;
        private Option<bool> _npqIsInEducationalSetting;
        private Option<string> _npqName;
        private Option<string> _npqTrainingProvider;
        private Option<Guid> _npqEvidenceFileId;
        private Option<string> _npqEvidenceFileName;
        private Option<SupportTaskStatus> _status;
        private bool _withMatches = true;

        public CreateNpqTrnRequestSupportTaskBuilder WithMatches(bool withMatches = true)
        {
            _withMatches = withMatches;
            return this;
        }

        public CreateNpqTrnRequestSupportTaskBuilder WithNpqApplicationId(string npqApplicationId)
        {
            _npqApplicationId = Option.Some(npqApplicationId);
            return this;
        }

        public CreateNpqTrnRequestSupportTaskBuilder WithNpqIsInEducationalSetting(bool isInEducationalSetting)
        {
            _npqIsInEducationalSetting = Option.Some(isInEducationalSetting);
            return this;
        }

        public CreateNpqTrnRequestSupportTaskBuilder WithNpqName(string npqName)
        {
            _npqName = Option.Some(npqName);
            return this;
        }

        public CreateNpqTrnRequestSupportTaskBuilder WithNpqTrainingProvider(string npqTrainingProvider)
        {
            _npqTrainingProvider = Option.Some(npqTrainingProvider);
            return this;
        }

        public CreateNpqTrnRequestSupportTaskBuilder WithNpqEvidenceFileId(Guid npqEvidenceFileId)
        {
            _npqEvidenceFileId = Option.Some(npqEvidenceFileId);
            return this;
        }

        public CreateNpqTrnRequestSupportTaskBuilder WithNpqEvidenceFileName(string npqEvidenceFileName)
        {
            _npqEvidenceFileName = Option.Some(npqEvidenceFileName);
            return this;
        }
        public Option<DateTime> _createdOn;

        public CreateNpqTrnRequestSupportTaskBuilder WithFirstName(string firstName)
        {
            _firstName = Option.Some(firstName);
            return this;
        }

        public CreateNpqTrnRequestSupportTaskBuilder WithMiddleName(string? middleName)
        {
            _middleName = Option.Some(middleName ?? string.Empty);
            return this;
        }

        public CreateNpqTrnRequestSupportTaskBuilder WithLastName(string lastName)
        {
            _lastName = Option.Some(lastName);
            return this;
        }

        public CreateNpqTrnRequestSupportTaskBuilder WithDateOfBirth(DateOnly dateOfBirth)
        {
            _dateOfBirth = Option.Some(dateOfBirth);
            return this;
        }

        public CreateNpqTrnRequestSupportTaskBuilder WithEmailAddress(string? emailAddress)
        {
            _emailAddress = Option.Some(emailAddress);
            return this;
        }

        public CreateNpqTrnRequestSupportTaskBuilder WithNationalInsuranceNumber(string? nationalInsuranceNumber)
        {
            _nationalInsuranceNumber = Option.Some(nationalInsuranceNumber);
            return this;
        }

        public CreateNpqTrnRequestSupportTaskBuilder WithMatchedRecords(params Guid[] personIds)
        {
            if (_withMatches is false)
            {
                throw new InvalidOperationException("WithMatchedRecords cannot be called when WithMatches is false.");
            }
            _matchedRecords = Option.Some(personIds.Select(id => new TrnRequestMatchedPerson() { PersonId = id }).ToArray());

            return this;
        }

        public CreateNpqTrnRequestSupportTaskBuilder WithStatus(SupportTaskStatus status)
        {
            _status = Option.Some(status);
            return this;
        }

        public CreateNpqTrnRequestSupportTaskBuilder WithCreatedOn(DateTime created)
        {
            _createdOn = Option.Some(DateTime.SpecifyKind(created, DateTimeKind.Utc));
            return this;
        }

        public async Task<SupportTask> ExecuteAsync(TestData testData)
        {
            var trnRequestId = _requestId.ValueOr(Guid.NewGuid().ToString);
            var emailAddress = _emailAddress.ValueOr(testData.GenerateUniqueEmail);
            var firstName = _firstName.ValueOr(testData.GenerateFirstName);
            var middleName = _middleName.ValueOr(testData.GenerateMiddleName);
            var lastName = _lastName.ValueOr(testData.GenerateLastName);
            var dateOfBirth = _dateOfBirth.ValueOr(testData.GenerateDateOfBirth);
            var nationalInsuranceNumber = _nationalInsuranceNumber.ValueOr(testData.GenerateNationalInsuranceNumber);
            var createdOn = _createdOn.ValueOr(testData.Clock.UtcNow);
            var npqApplicationId = _npqApplicationId.ValueOr(testData.GenerateEstablishmentUrn().ToString()); // CML TODO - a realistic NPQ application ID should be used here
            var npqIsInEducationalSetting = _npqIsInEducationalSetting.ValueOr(Faker.Boolean.Random);
            var npqName = _npqName.ValueOr(Faker.Name.Last);
            var npqTrainingProvider = _npqTrainingProvider.ValueOr(Faker.Company.Name);
            var npqEvidenceFileId = _npqEvidenceFileId.ValueOr(Guid.NewGuid);
            var npqEvidenceFileName = _npqEvidenceFileName.ValueOr("Filename1.txt");

            var matchedRecords = _matchedRecords.ValueOrDefault();

            if (_withMatches && matchedRecords is null)
            {
                // Matches wasn't explicitly specified; create two person records that match details in this request

                matchedRecords = await Enumerable.Range(1, 2)
                    .ToAsyncEnumerable()
                    .SelectAwait(async _ =>
                    {
                        var person = await testData.CreatePersonAsync(p =>
                        {
                            p
                                .WithTrn()
                                .WithFirstName(firstName)
                                .WithMiddleName(middleName)
                                .WithLastName(lastName)
                                .WithDateOfBirth(dateOfBirth)
                                .WithEmail(emailAddress);

                            if (nationalInsuranceNumber is not null)
                            {
                                p.WithNationalInsuranceNumber(nationalInsuranceNumber);
                            }
                        });

                        return new TrnRequestMatchedPerson() { PersonId = person.PersonId };
                    })
                    .ToArrayAsync();
            }

            var matches = new TrnRequestMatches() { MatchedPersons = matchedRecords };

            var potentialDuplicate = matchedRecords?.Length > 0;

            var metadata = new TrnRequestMetadata
            {
                ApplicationUserId = applicationUserId,
                RequestId = trnRequestId,
                CreatedOn = createdOn,
                IdentityVerified = null,
                EmailAddress = emailAddress,
                OneLoginUserSubject = null,
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                PreviousFirstName = null,
                PreviousLastName = null,
                Name = [firstName, middleName, lastName],
                DateOfBirth = dateOfBirth,
                PotentialDuplicate = potentialDuplicate,
                NationalInsuranceNumber = nationalInsuranceNumber,
                Gender = null,
                AddressLine1 = null,
                AddressLine2 = null,
                AddressLine3 = null,
                City = null,
                Postcode = null,
                Country = null,
                TrnToken = null,
                Matches = matches,
                NpqApplicationId = npqApplicationId,
                NpqEvidenceFileId = npqEvidenceFileId,
                NpqEvidenceFileName = npqEvidenceFileName,
                NpqName = npqName,
                NpqTrainingProvider = npqTrainingProvider,
                NpqWorkingInEducationalSetting = npqIsInEducationalSetting
            };

            var status = _status.ValueOr(() => SupportTaskStatus.Open);

            var task = SupportTask.Create(
                SupportTaskType.NpqTrnRequest,
                new NpqTrnRequestData(),
                personId: null,
                oneLoginUserSubject: null,
                applicationUserId,
                trnRequestId,
                SystemUser.SystemUserId,
                createdOn,
                out var createdEvent);
            task.Status = status;

            return await testData.WithDbContextAsync(async dbContext =>
            {
                dbContext.TrnRequestMetadata.Add(metadata);
                dbContext.SupportTasks.Add(task);
                dbContext.AddEventWithoutBroadcast(createdEvent);
                await dbContext.SaveChangesAsync();

                // Re-query what we've just added so we return a SupportTask with TrnRequestMetadata populated
                return await dbContext.SupportTasks
                    .Include(t => t.TrnRequestMetadata)
                    .ThenInclude(m => m!.ApplicationUser)
                    .SingleAsync(t => t.SupportTaskReference == task.SupportTaskReference);
            });
        }
    }
}
