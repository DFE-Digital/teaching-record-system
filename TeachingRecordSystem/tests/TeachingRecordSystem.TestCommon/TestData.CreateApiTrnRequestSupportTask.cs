using Optional;
using Optional.Unsafe;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTaskData;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public Task<SupportTask> CreateApiTrnRequestSupportTaskAsync(
        Guid applicationUserId,
        Action<CreateApiTrnRequestSupportTaskBuilder>? configure = null)
    {
        var builder = new CreateApiTrnRequestSupportTaskBuilder(applicationUserId);
        configure?.Invoke(builder);
        return builder.ExecuteAsync(this);
    }

    public Task<SupportTask> CreateApiTrnRequestSupportTaskAsync(
        Guid applicationUserId,
        Person matchedPerson,
        Action<CreateApiTrnRequestSupportTaskBuilder>? configure = null)
    {
        return CreateApiTrnRequestSupportTaskAsync(
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
                    .WithResolvedPersonId(matchedPerson.PersonId);

                configure?.Invoke(t);
            });
    }

    public class CreateApiTrnRequestSupportTaskBuilder(Guid applicationUserId)
    {
        private Option<string> _requestId;
        private Option<string?> _emailAddress;
        private Option<string> _firstName;
        private Option<string> _middleName;
        private Option<string> _lastName;
        private Option<DateOnly> _dateOfBirth;
        private Option<string?> _nationalInsuranceNumber;
        private Option<TrnRequestMatchedRecord[]> _matchedRecords;
        private Option<SupportTaskStatus> _status;
        private Option<DateTime> _createdOn;
        private Option<TrnRequestStatus> _trnRequestStatus;
        private Option<Guid?> _resolvedPersonId;

        public CreateApiTrnRequestSupportTaskBuilder WithFirstName(string firstName)
        {
            _firstName = Option.Some(firstName);
            return this;
        }

        public CreateApiTrnRequestSupportTaskBuilder WithMiddleName(string? middleName)
        {
            _middleName = Option.Some(middleName ?? string.Empty);
            return this;
        }

        public CreateApiTrnRequestSupportTaskBuilder WithLastName(string lastName)
        {
            _lastName = Option.Some(lastName);
            return this;
        }

        public CreateApiTrnRequestSupportTaskBuilder WithDateOfBirth(DateOnly dateOfBirth)
        {
            _dateOfBirth = Option.Some(dateOfBirth);
            return this;
        }

        public CreateApiTrnRequestSupportTaskBuilder WithEmailAddress(string? emailAddress)
        {
            _emailAddress = Option.Some(emailAddress);
            return this;
        }

        public CreateApiTrnRequestSupportTaskBuilder WithNationalInsuranceNumber(string? nationalInsuranceNumber)
        {
            _nationalInsuranceNumber = Option.Some(nationalInsuranceNumber);
            return this;
        }

        public CreateApiTrnRequestSupportTaskBuilder WithMatchedRecords(params Guid[] personIds)
        {
            _matchedRecords = Option.Some(personIds.Select(id => new TrnRequestMatchedRecord() { PersonId = id }).ToArray());
            return this;
        }

        public CreateApiTrnRequestSupportTaskBuilder WithStatus(SupportTaskStatus status)
        {
            _status = Option.Some(status);
            return this;
        }

        public CreateApiTrnRequestSupportTaskBuilder WithCreatedOn(DateTime created)
        {
            _createdOn = Option.Some(DateTime.SpecifyKind(created, DateTimeKind.Utc));
            return this;
        }

        public CreateApiTrnRequestSupportTaskBuilder WithResolvedPersonId(Guid? personId)
        {
            _resolvedPersonId = Option.Some(personId);
            return this;
        }

        public CreateApiTrnRequestSupportTaskBuilder WithTrnRequestStatus(TrnRequestStatus status)
        {
            _trnRequestStatus = Option.Some(status);
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

            var matchedRecords = _matchedRecords.ValueOrDefault();

            if (matchedRecords is null)
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

                        return new TrnRequestMatchedRecord() { PersonId = person.PersonId };
                    })
                    .ToArrayAsync();
            }

            var matches = new TrnRequestMatches() { MatchedRecords = matchedRecords };

            var potentialDuplicate = matchedRecords.Length > 0;

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
                Matches = matches
            };

            if (_resolvedPersonId.ValueOrDefault() is Guid personId)
            {
                metadata.SetResolvedPerson(personId, _trnRequestStatus.ValueOr(TrnRequestStatus.Completed));
            }

            var status = _status.ValueOr(() => SupportTaskStatus.Open);

            var task = SupportTask.Create(
                SupportTaskType.ApiTrnRequest,
                new ApiTrnRequestData(),
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
                    .ThenInclude(m => m!.ApplicationUser!)
                    .SingleAsync(t => t.SupportTaskReference == task.SupportTaskReference);
            });
        }
    }
}
