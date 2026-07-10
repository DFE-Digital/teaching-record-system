using Optional;
using Optional.Unsafe;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public async Task<CreateApiTrnRequestSupportTaskResult> CreateTrnRequestSupportTaskAsync(
        Guid? applicationUserId = null,
        Action<CreateTrnRequestSupportTaskBuilder>? configure = null)
    {
        if (applicationUserId is null)
        {
            var applicationUser = await CreateApplicationUserAsync();
            applicationUserId = applicationUser.UserId;
        }

        var builder = new CreateTrnRequestSupportTaskBuilder(applicationUserId!.Value);
        configure?.Invoke(builder);
        var task = await builder.ExecuteAsync(this);

        return task;
    }

    public Task<CreateApiTrnRequestSupportTaskResult> CreateTrnRequestSupportTaskAsync(
        Guid applicationUserId,
        Person matchedPerson,
        Action<CreateTrnRequestSupportTaskBuilder>? configure = null)
    {
        return CreateTrnRequestSupportTaskAsync(
            applicationUserId,
            t =>
            {
                // NINO is not set here to avoid a definite match

                t
                    .WithFirstName(matchedPerson.FirstName)
                    .WithMiddleName(matchedPerson.MiddleName)
                    .WithLastName(matchedPerson.LastName)
                    .WithDateOfBirth(matchedPerson.DateOfBirth!.Value)
                    .WithEmailAddress(matchedPerson.EmailAddress)
                    .WithGender(matchedPerson.Gender)
                    .WithMatchedPersons(matchedPerson.PersonId);

                configure?.Invoke(t);
            });
    }

    public Task<CreateApiTrnRequestSupportTaskResult> CreateResolvedApiTrnRequestSupportTaskAsync(
        Guid applicationUserId,
        Person matchedPerson,
        Action<CreateTrnRequestSupportTaskBuilder>? configure = null)
    {
        return CreateTrnRequestSupportTaskAsync(
            applicationUserId,
            t =>
            {
                t
                    .WithFirstName(matchedPerson.FirstName)
                    .WithMiddleName(matchedPerson.MiddleName)
                    .WithLastName(matchedPerson.LastName)
                    .WithDateOfBirth(matchedPerson.DateOfBirth!.Value)
                    .WithNationalInsuranceNumber(matchedPerson.NationalInsuranceNumber)
                    .WithEmailAddress(matchedPerson.EmailAddress)
                    .WithGender(matchedPerson.Gender)
                    .WithResolvedPersonId(matchedPerson.PersonId);

                configure?.Invoke(t);
            });
    }

    public class CreateTrnRequestSupportTaskBuilder(Guid applicationUserId)
    {
        private Option<string> _requestId;
        private Option<string?> _emailAddress;
        private Option<string> _firstName;
        private Option<string?> _middleName;
        private Option<string> _lastName;
        private Option<DateOnly> _dateOfBirth;
        private Option<string?> _nationalInsuranceNumber;
        private Option<Gender?> _gender;
        private Option<Guid[]> _matchedPersonIds;
        private Option<SupportTaskStatus> _status;
        private Option<DateTime> _createdOn;
        private Option<TrnRequestStatus> _trnRequestStatus;
        private Option<Guid?> _resolvedPersonId;
        private Option<bool> _identityVerified;
        private Option<string> _oneLoginUserSubject;

        public CreateTrnRequestSupportTaskBuilder WithFirstName(string firstName)
        {
            _firstName = Option.Some(firstName);
            return this;
        }

        public CreateTrnRequestSupportTaskBuilder WithMiddleName(string? middleName)
        {
            _middleName = Option.Some(middleName);
            return this;
        }

        public CreateTrnRequestSupportTaskBuilder WithLastName(string lastName)
        {
            _lastName = Option.Some(lastName);
            return this;
        }

        public CreateTrnRequestSupportTaskBuilder WithDateOfBirth(DateOnly dateOfBirth)
        {
            _dateOfBirth = Option.Some(dateOfBirth);
            return this;
        }

        public CreateTrnRequestSupportTaskBuilder WithEmailAddress(string? emailAddress)
        {
            _emailAddress = Option.Some(emailAddress);
            return this;
        }

        public CreateTrnRequestSupportTaskBuilder WithNationalInsuranceNumber(string? nationalInsuranceNumber)
        {
            _nationalInsuranceNumber = Option.Some(nationalInsuranceNumber);
            return this;
        }

        public CreateTrnRequestSupportTaskBuilder WithGender(Gender? gender)
        {
            _gender = Option.Some(gender);
            return this;
        }

        public CreateTrnRequestSupportTaskBuilder WithMatchedPersons(params Guid[] personIds)
        {
            _matchedPersonIds = Option.Some(personIds);
            return this;
        }

        public CreateTrnRequestSupportTaskBuilder WithStatus(SupportTaskStatus status)
        {
            _status = Option.Some(status);
            return this;
        }

        public CreateTrnRequestSupportTaskBuilder WithCreatedOn(DateTime created)
        {
            _createdOn = Option.Some(DateTime.SpecifyKind(created, DateTimeKind.Utc));
            return this;
        }

        public CreateTrnRequestSupportTaskBuilder WithResolvedPersonId(Guid? personId)
        {
            _resolvedPersonId = Option.Some(personId);
            return this;
        }

        public CreateTrnRequestSupportTaskBuilder WithRequestId(string requestId)
        {
            _requestId = Option.Some(requestId);
            return this;
        }

        public CreateTrnRequestSupportTaskBuilder WithTrnRequestStatus(TrnRequestStatus status)
        {
            _trnRequestStatus = Option.Some(status);
            return this;
        }

        public CreateTrnRequestSupportTaskBuilder WithIdentityVerified(bool identityVerified)
        {
            _identityVerified = Option.Some(identityVerified);
            return this;
        }

        public CreateTrnRequestSupportTaskBuilder WithOneLoginUserSubject(string oneLoginUserSubject)
        {
            _oneLoginUserSubject = Option.Some(oneLoginUserSubject);
            return this;
        }

        public async Task<CreateApiTrnRequestSupportTaskResult> ExecuteAsync(TestData testData)
        {
            var trnRequestId = _requestId.ValueOr(Guid.NewGuid().ToString);
            var emailAddress = _emailAddress.ValueOr(testData.GenerateUniqueEmail);
            var firstName = _firstName.ValueOr(testData.GenerateFirstName);
            var middleName = _middleName.ValueOrDefault();
            var lastName = _lastName.ValueOr(testData.GenerateLastName);
            var dateOfBirth = _dateOfBirth.ValueOr(testData.GenerateDateOfBirth);
            var nationalInsuranceNumber = _nationalInsuranceNumber.ValueOr(testData.GenerateNationalInsuranceNumber);
            var gender = _gender.ValueOr(testData.GenerateGender());
            var createdOn = _createdOn.ValueOr(testData.TimeProvider.UtcNow);

            var matchedPersons = _matchedPersonIds.ValueOrDefault();

            if (matchedPersons is null)
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
                                .WithMiddleName(middleName ?? string.Empty)
                                .WithLastName(lastName)
                                .WithPreviousNames((firstName, middleName ?? testData.GenerateMiddleName(), testData.GenerateChangedLastName(lastName), new DateTime(2020, 8, 1).ToUniversalTime()))
                                .WithDateOfBirth(dateOfBirth)
                                .WithEmailAddress(emailAddress);

                            if (gender is not null)
                            {
                                p.WithGender(gender.Value);
                            }

                            if (nationalInsuranceNumber is not null)
                            {
                                p.WithNationalInsuranceNumber(nationalInsuranceNumber);
                            }
                        });

                        return person.PersonId;
                    })
                    .ToArrayAsync();
            }

            var potentialDuplicate = matchedPersons.Length > 0;

            var metadata = new TrnRequestMetadata
            {
                ApplicationUserId = applicationUserId,
                RequestId = trnRequestId,
                CreatedOn = createdOn,
                IdentityVerified = _identityVerified.ValueOr(false),
                EmailAddress = emailAddress,
                OneLoginUserSubject = _oneLoginUserSubject.ValueOrDefault(),
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
                TrnToken = null
            };

            if (_resolvedPersonId.ValueOrDefault() is Guid personId)
            {
                metadata.ResolvedPersonId = personId;
                metadata.Status = _trnRequestStatus.ValueOr(TrnRequestStatus.Completed);
            }

            var status = _status.ValueOr(() => SupportTaskStatus.Open);

            var task = new SupportTask
            {
                CreatedOn = createdOn,
                UpdatedOn = createdOn,
                SupportTaskType = SupportTaskType.TrnRequest,
                Status = status,
                Data = new TrnRequestData(),
                TrnRequestApplicationUserId = applicationUserId,
                TrnRequestId = trnRequestId
            };

            return await testData.WithDbContextAsync(async dbContext =>
            {
                dbContext.TrnRequestMetadata.Add(metadata);
                dbContext.SupportTasks.Add(task);
                await dbContext.SaveChangesAsync();

                // Re-query what we've just added so we return a SupportTask with TrnRequestMetadata populated
                var dbTask = await dbContext.SupportTasks
                    .Include(t => t.TrnRequestMetadata)
                    .ThenInclude(m => m!.ApplicationUser!)
                    .SingleAsync(t => t.SupportTaskReference == task.SupportTaskReference);

                return new CreateApiTrnRequestSupportTaskResult(dbTask, dbTask.TrnRequestMetadata!, matchedPersons);
            });
        }
    }

    public record CreateApiTrnRequestSupportTaskResult(SupportTask SupportTask, TrnRequestMetadata TrnRequest, Guid[] MatchedPersonIds)
        : ISupportTaskCreateResult;

    public interface ISupportTaskCreateResult
    {
        SupportTask SupportTask { get; }
    }
}
