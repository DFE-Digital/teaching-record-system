using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.DqtOutbox.Messages;

namespace TeachingRecordSystem.Core.Services.DqtOutbox.Handlers;

public class TrnRequestMetadataMessageHandler(TrsDbContext dbContext) : IMessageHandler<TrnRequestMetadataMessage>
{
    public async Task HandleMessageAsync(TrnRequestMetadataMessage message)
    {
        if (!await dbContext.TrnRequestMetadata.AnyAsync(m => m.ApplicationUserId == message.ApplicationUserId && m.RequestId == message.RequestId))
        {
            dbContext.TrnRequestMetadata.Add(new DataStore.Postgres.Models.TrnRequestMetadata()
            {
                ApplicationUserId = message.ApplicationUserId,
                RequestId = message.RequestId,
                CreatedOn = message.CreatedOn,
                IdentityVerified = message.IdentityVerified,
                OneLoginUserSubject = message.OneLoginUserSubject,
                EmailAddress = message.EmailAddress,
                Name = message.Name,
                DateOfBirth = message.DateOfBirth,
                PotentialDuplicate = message.PotentialDuplicate,
                NationalInsuranceNumber = message.NationalInsuranceNumber,
                Gender = message.Gender,
                AddressLine1 = message.AddressLine1,
                AddressLine2 = message.AddressLine2,
                AddressLine3 = message.AddressLine3,
                City = message.City,
                Postcode = message.Postcode,
                Country = message.Country
            });
            await dbContext.SaveChangesAsync();
        }
    }
}
