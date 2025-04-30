using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTaskData;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public Task<SupportTask> CreateApiTrnRequestSupportTaskAsync(
        Guid applicationUserId,
        bool potentialDuplicate = false,
        Action<CreateApiTrnRequestSupportTaskBuilder>? configure = null)
    {
        var builder = new CreateApiTrnRequestSupportTaskBuilder(applicationUserId, potentialDuplicate);
        configure?.Invoke(builder);
        return builder.ExecuteAsync(this);
    }

    public class CreateApiTrnRequestSupportTaskBuilder(Guid applicationUserId, bool potentialDuplicate)
    {
        private Option<string> _requestId;
        private Option<string?> _emailAddress;
        private Option<string> _firstName;
        private Option<string> _middleName;
        private Option<string> _lastName;
        private Option<DateOnly> _dateOfBirth;
        private Option<string?> _nationalInsuranceNumber;

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

        public CreateApiTrnRequestSupportTaskBuilder WithEmailAddress(string? emailAddress)
        {
            _emailAddress = Option.Some(emailAddress);
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

            var metadata = new TrnRequestMetadata
            {
                ApplicationUserId = applicationUserId,
                RequestId = trnRequestId,
                CreatedOn = testData.Clock.UtcNow,
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
                ResolvedPersonId = null
            };

            var task = new SupportTask
            {
                SupportTaskReference = SupportTask.GenerateSupportTaskReference(),
                CreatedOn = testData.Clock.UtcNow,
                UpdatedOn = testData.Clock.UtcNow,
                SupportTaskType = SupportTaskType.ApiTrnRequest,
                Status = SupportTaskStatus.Open,
                Data = new ApiTrnRequestData(),
                TrnRequestApplicationUserId = applicationUserId,
                TrnRequestId = trnRequestId,
                TrnRequestMetadata = metadata
            };

            await testData.WithDbContextAsync(async dbContext =>
            {
                dbContext.TrnRequestMetadata.Add(metadata);
                dbContext.SupportTasks.Add(task);
                await dbContext.SaveChangesAsync();
            });

            return task;
        }
    }
}
