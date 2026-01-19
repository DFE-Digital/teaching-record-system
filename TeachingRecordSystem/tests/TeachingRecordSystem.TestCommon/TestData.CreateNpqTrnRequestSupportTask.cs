using System.Diagnostics;
using Optional;
using Optional.Unsafe;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public async Task<CreateNpqTrnRequestSupportTaskResult> CreateNpqTrnRequestSupportTaskAsync(
        Guid? applicationUserId = null,
        Action<CreateNpqTrnRequestSupportTaskBuilder>? configure = null)
    {
        if (applicationUserId is null)
        {
            var applicationUser = await CreateApplicationUserAsync("NPQ");
            applicationUserId = applicationUser.UserId;
        }

        var builder = new CreateNpqTrnRequestSupportTaskBuilder(applicationUserId!.Value);
        configure?.Invoke(builder);
        var task = await builder.ExecuteAsync(this);

        return task;
    }

    public Task<CreateNpqTrnRequestSupportTaskResult> CreateNpqTrnRequestSupportTaskAsync(
        Guid applicationUserId,
        Person matchedPerson,
        Action<CreateNpqTrnRequestSupportTaskBuilder>? configure = null)
    {
        Debug.Assert(matchedPerson.EmailAddress is not null);

        return CreateNpqTrnRequestSupportTaskAsync(
            applicationUserId,
            t =>
            {
                // NINO is not set here to avoid a definite match

                t
                    .WithFirstName(matchedPerson.FirstName)
                    .WithMiddleName(matchedPerson.MiddleName)
                    .WithLastName(matchedPerson.LastName)
                    .WithDateOfBirth(matchedPerson.DateOfBirth!.Value)
                    .WithEmailAddress(matchedPerson.EmailAddress!)
                    .WithGender(matchedPerson.Gender)
                    .WithMatchedPersons(matchedPerson.PersonId);

                configure?.Invoke(t);
            });
    }

    public class CreateNpqTrnRequestSupportTaskBuilder(Guid applicationUserId)
    {
        private Option<string> _requestId;
        private Option<string> _emailAddress;
        private Option<string> _firstName;
        private Option<string?> _middleName;
        private Option<string> _lastName;
        private Option<DateOnly> _dateOfBirth;
        private Option<string?> _nationalInsuranceNumber;
        private Option<Gender?> _gender;
        private Option<Guid[]> _matchedPersonIds;
        private Option<string> _npqApplicationId;
        private Option<bool> _npqIsInEducationalSetting;
        private Option<string> _npqName;
        private Option<string> _npqTrainingProvider;
        private Option<Guid> _npqEvidenceFileId;
        private Option<string> _npqEvidenceFileName;
        private Option<SupportTaskStatus> _status;
        private bool _withMatches = true;
        private Option<DateTime> _createdOn;

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

        public CreateNpqTrnRequestSupportTaskBuilder WithFirstName(string firstName)
        {
            _firstName = Option.Some(firstName);
            return this;
        }

        public CreateNpqTrnRequestSupportTaskBuilder WithMiddleName(string? middleName)
        {
            _middleName = Option.Some(middleName);
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

        public CreateNpqTrnRequestSupportTaskBuilder WithEmailAddress(string emailAddress)
        {
            _emailAddress = Option.Some(emailAddress);
            return this;
        }

        public CreateNpqTrnRequestSupportTaskBuilder WithNationalInsuranceNumber(string? nationalInsuranceNumber)
        {
            _nationalInsuranceNumber = Option.Some(nationalInsuranceNumber);
            return this;
        }

        public CreateNpqTrnRequestSupportTaskBuilder WithGender(Gender? gender)
        {
            _gender = Option.Some(gender);
            return this;
        }

        public CreateNpqTrnRequestSupportTaskBuilder WithMatchedPersons(params Guid[] personIds)
        {
            if (_withMatches is false)
            {
                throw new InvalidOperationException("WithMatchedPersons cannot be called when WithMatches is false.");
            }
            _matchedPersonIds = Option.Some(personIds);

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

        public async Task<CreateNpqTrnRequestSupportTaskResult> ExecuteAsync(TestData testData)
        {
            var trnRequestId = _requestId.ValueOr(Guid.NewGuid().ToString);
            var emailAddress = _emailAddress.ValueOr(testData.GenerateUniqueEmail);
            var firstName = _firstName.ValueOr(testData.GenerateFirstName);
            var middleName = _middleName.ValueOrDefault();
            var lastName = _lastName.ValueOr(testData.GenerateLastName);
            var dateOfBirth = _dateOfBirth.ValueOr(testData.GenerateDateOfBirth);
            var nationalInsuranceNumber = _nationalInsuranceNumber.ValueOrDefault();
            var gender = _gender.ValueOrDefault();
            var createdOn = _createdOn.ValueOr(testData.Clock.UtcNow);
            var npqApplicationId = _npqApplicationId.ValueOr(testData.GenerateNpqApplicationId());
            var npqIsInEducationalSetting = _npqIsInEducationalSetting.ValueOr(Faker.Boolean.Random);
            var npqName = _npqName.ValueOr(Faker.Name.Last);
            var npqTrainingProvider = _npqTrainingProvider.ValueOr(Faker.Company.Name);
            var npqEvidenceFileId = _npqEvidenceFileId.ValueOr(Guid.NewGuid);
            var npqEvidenceFileName = _npqEvidenceFileName.ValueOr("Filename1.txt");

            var matchedPersons = _matchedPersonIds.ValueOrDefault();

            if (_withMatches && matchedPersons is null)
            {
                // Matches wasn't explicitly specified; create two person records that match details in this request

                matchedPersons = await Enumerable.Range(1, 2)
                    .ToAsyncEnumerable()
                    .Select(async (int _, CancellationToken _) =>
                    {
                        var person = await testData.CreatePersonAsync(p =>
                        {
                            p
                                .WithFirstName(firstName)
                                .WithMiddleName(middleName)
                                .WithLastName(lastName)
                                .WithDateOfBirth(dateOfBirth)
                                .WithEmailAddress(emailAddress);

                            if (nationalInsuranceNumber is not null)
                            {
                                p.WithNationalInsuranceNumber(nationalInsuranceNumber);
                            }

                            if (gender is not null)
                            {
                                p.WithGender(gender.Value);
                            }
                        });

                        return person.PersonId;
                    })
                    .ToArrayAsync();
            }

            var potentialDuplicate = matchedPersons?.Length > 0;

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
                Name = new[] { firstName, middleName!, lastName }.Where(n => n is not null).ToArray(),
                DateOfBirth = dateOfBirth,
                PotentialDuplicate = potentialDuplicate,
                NationalInsuranceNumber = nationalInsuranceNumber,
                Gender = gender,
                AddressLine1 = null,
                AddressLine2 = null,
                AddressLine3 = null,
                City = null,
                Postcode = null,
                Country = null,
                TrnToken = null,
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
                var dbTask = await dbContext.SupportTasks
                    .Include(t => t.TrnRequestMetadata)
                    .ThenInclude(m => m!.ApplicationUser)
                    .SingleAsync(t => t.SupportTaskReference == task.SupportTaskReference);

                return new CreateNpqTrnRequestSupportTaskResult(dbTask, dbTask.TrnRequestMetadata!, matchedPersons ?? []);
            });
        }
    }

    public record CreateNpqTrnRequestSupportTaskResult(SupportTask SupportTask, TrnRequestMetadata TrnRequest, Guid[] MatchedPersonIds)
        : ISupportTaskCreateResult;
}
