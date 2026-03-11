using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public async Task<TrnRequestMetadata> CreateDormantTrnRequestAsync(Guid applicationUserId)
    {
        var requestId = Guid.NewGuid().ToString();

        return await WithDbContextAsync(async dbContext =>
        {
            var emailAddress = GenerateUniqueEmail();
            var firstName = GenerateFirstName();
            var middleName = GenerateMiddleName();
            var lastName = GenerateLastName();
            var dateOfBirth = GenerateDateOfBirth();
            var nationalInsuranceNumber = GenerateNationalInsuranceNumber();

            var trnRequestMetadata = new TrnRequestMetadata
            {
                ApplicationUserId = applicationUserId,
                RequestId = requestId,
                CreatedOn = Clock.UtcNow,
                IdentityVerified = null,
                EmailAddress = emailAddress,
                OneLoginUserSubject = null,
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                PreviousFirstName = null,
                PreviousMiddleName = null,
                PreviousLastName = null,
                Name = [firstName, middleName, lastName],
                DateOfBirth = dateOfBirth,
                PotentialDuplicate = false,
                NationalInsuranceNumber = nationalInsuranceNumber,
                Gender = null,
                AddressLine1 = null,
                AddressLine2 = null,
                AddressLine3 = null,
                City = null,
                Postcode = null,
                Country = null,
                TrnToken = null,
                NpqWorkingInEducationalSetting = null,
                NpqApplicationId = null,
                NpqName = null,
                NpqTrainingProvider = null,
                NpqEvidenceFileId = null,
                NpqEvidenceFileName = null,
                WorkEmailAddress = null
            };
            trnRequestMetadata.SetDormant();

            dbContext.TrnRequestMetadata.Add(trnRequestMetadata);

            await dbContext.SaveChangesAsync();

            return trnRequestMetadata;
        });
    }
}
