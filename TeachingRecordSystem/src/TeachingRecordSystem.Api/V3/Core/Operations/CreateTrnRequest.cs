using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Core.SharedModels;
using TeachingRecordSystem.Api.Validation;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Services.NameSynonyms;
using TeachingRecordSystem.Core.Services.TrnGenerationApi;

namespace TeachingRecordSystem.Api.V3.Core.Operations;

public record CreateTrnRequestCommand
{
    public required string RequestId { get; init; }
    public required string FirstName { get; init; }
    public required string? MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required IReadOnlyCollection<string> EmailAddresses { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
}

public class CreateTrnRequestHandler(
    ICrmQueryDispatcher crmQueryDispatcher,
    TrsDbContext trsDbContext,
    ICurrentClientProvider currentClientProvider,
    ITrnGenerationApiClient trnGenerationApiClient,
    INameSynonymProvider nameSynonymProvider)
{
    public async Task<TrnRequestInfo> Handle(CreateTrnRequestCommand command)
    {
        var currentClientId = currentClientProvider.GetCurrentClientId();

        var trnRequest = await trsDbContext.TrnRequests
            .SingleOrDefaultAsync(r => r.ClientId == currentClientId && r.RequestId == command.RequestId);

        if (trnRequest != null)
        {
            throw new ErrorException(ErrorRegistry.CannotResubmitRequest());
        }

        string? trn = null;

        var firstAndMiddleNames = $"{command.FirstName} {command.MiddleName}".Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var firstName = firstAndMiddleNames.First();
        var middleName = string.Join(' ', firstAndMiddleNames.Skip(1));

        var firstNameSynonyms = (await nameSynonymProvider.GetAllNameSynonyms()).GetValueOrDefault(firstName, []);

        var potentialDuplicates = await crmQueryDispatcher.ExecuteQuery(
            new FindPotentialDuplicateContactsQuery()
            {
                FirstNames = firstNameSynonyms.Append(firstName),
                MiddleName = middleName,
                LastName = command.LastName,
                DateOfBirth = command.DateOfBirth,
                EmailAddresses = command.EmailAddresses
            });

        if (potentialDuplicates.Length == 0)
        {
            trn = await trnGenerationApiClient.GenerateTrn();
        }

        var emailAddress = command.EmailAddresses?.FirstOrDefault();

        var contactId = await crmQueryDispatcher.ExecuteQuery(new CreateContactQuery()
        {
            FirstName = firstName,
            MiddleName = middleName,
            LastName = command.LastName,
            StatedFirstName = command.FirstName,
            StatedMiddleName = command.MiddleName ?? "",
            StatedLastName = command.LastName,
            DateOfBirth = command.DateOfBirth,
            EmailAddress = emailAddress,
            NationalInsuranceNumber = NationalInsuranceNumberHelper.NormalizeNationalInsuranceNumber(command.NationalInsuranceNumber),
            PotentialDuplicates = potentialDuplicates,
            Trn = trn
        });

        trsDbContext.TrnRequests.Add(new TrnRequest()
        {
            ClientId = currentClientId,
            RequestId = command.RequestId,
            TeacherId = contactId,
            LinkedToIdentity = false
        });

        await trsDbContext.SaveChangesAsync();

        var status = trn is not null ? TrnRequestStatus.Completed : TrnRequestStatus.Pending;

        return new TrnRequestInfo()
        {
            RequestId = command.RequestId,
            Person = new TrnRequestInfoPerson()
            {
                FirstName = command.FirstName,
                MiddleName = command.MiddleName,
                LastName = command.LastName,
                EmailAddress = emailAddress,
                DateOfBirth = command.DateOfBirth,
                NationalInsuranceNumber = command.NationalInsuranceNumber
            },
            Trn = trn,
            Status = status
        };
    }
}
