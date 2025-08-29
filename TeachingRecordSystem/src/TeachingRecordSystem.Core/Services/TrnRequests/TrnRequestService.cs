using System.Diagnostics;
using Microsoft.Extensions.Options;
using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;

namespace TeachingRecordSystem.Core.Services.TrnRequests;

public class TrnRequestService(
    TrsDbContext dbContext,
    IGetAnIdentityApiClient idApiClient,
    IOptions<AccessYourTeachingQualificationsOptions> aytqOptionsAccessor,
    IOptions<TrnRequestOptions> trnRequestOptionsAccessor)
{
    public async Task<string> CreateTrnTokenAsync(string trn, string emailAddress)
    {
        var response = await idApiClient.CreateTrnTokenAsync(new CreateTrnTokenRequest() { Email = emailAddress, Trn = trn });
        return response.TrnToken;
    }

    public string GetAccessYourTeachingQualificationsLink(string trnToken) =>
        $"{aytqOptionsAccessor.Value.BaseAddress}{aytqOptionsAccessor.Value.StartUrlPath}?trn_token={Uri.EscapeDataString(trnToken)}";

    public async Task<bool> TryEnsureTrnTokenAsync(TrnRequestMetadata requestData, string? resolvedPersonTrn)
    {
        if (requestData.TrnToken is not null || requestData.EmailAddress is null || resolvedPersonTrn is null)
        {
            return false;
        }

        requestData.TrnToken = await CreateTrnTokenAsync(resolvedPersonTrn, requestData.EmailAddress);
        return true;
    }

    public async Task<bool> RequiresFurtherChecksNeededSupportTaskAsync(Guid personId, Guid trnRequestApplicationUserId)
    {
        if (!trnRequestOptionsAccessor.Value.FlagFurtherChecksRequiredFromUserIds.Contains(trnRequestApplicationUserId))
        {
            return false;
        }

        var personFlags = await dbContext.Persons
            .Where(p => p.PersonId == personId)
            .Select(p => new { HasQts = p.QtsDate != null, HasEyts = p.EytsDate != null, HasOpenAlert = p.Alerts!.Any(a => a.IsOpen) })
            .SingleAsync();

        if (personFlags is { HasQts: false, HasEyts: false, HasOpenAlert: false })
        {
            return false;
        }

        return true;
    }

    public CreatePersonResult CreatePersonFromTrnRequest(TrnRequestMetadata requestData, DateTime now) =>
        Person.Create(
            requestData.FirstName!,
            requestData.MiddleName ?? string.Empty,
            requestData.LastName!,
            requestData.DateOfBirth,
            requestData.EmailAddress is string emailAddress ? EmailAddress.Parse(emailAddress) : null,
            requestData.NationalInsuranceNumber is string nationalInsuranceNumber ? NationalInsuranceNumber.Parse(nationalInsuranceNumber) : null,
            requestData.Gender,
            now,
            (requestData.ApplicationUserId, requestData.RequestId));

    public UpdatePersonDetailsResult UpdatePersonFromTrnRequest(
        Person person,
        TrnRequestMetadata requestData,
        IReadOnlyCollection<PersonMatchedAttribute> attributesToUpdate,
        DateTime now)
    {
        Debug.Assert(person.PersonId == requestData.ResolvedPersonId);

        return person.UpdateDetails(
            firstName: attributesToUpdate.Contains(PersonMatchedAttribute.FirstName)
                ? Option.Some(requestData.FirstName!)
                : Option.None<string>(),
            middleName: attributesToUpdate.Contains(PersonMatchedAttribute.MiddleName)
                ? Option.Some(requestData.MiddleName ?? string.Empty)
                : Option.None<string>(),
            lastName: attributesToUpdate.Contains(PersonMatchedAttribute.LastName)
                ? Option.Some(requestData.LastName!)
                : Option.None<string>(),
            dateOfBirth: attributesToUpdate.Contains(PersonMatchedAttribute.DateOfBirth)
                ? Option.Some<DateOnly?>(requestData.DateOfBirth)
                : Option.None<DateOnly?>(),
            emailAddress: attributesToUpdate.Contains(PersonMatchedAttribute.EmailAddress)
                ? Option.Some(requestData.EmailAddress is string emailAddress ? EmailAddress.Parse(emailAddress) : null)
                : Option.None<EmailAddress?>(),
            nationalInsuranceNumber: attributesToUpdate.Contains(PersonMatchedAttribute.NationalInsuranceNumber)
                ? Option.Some(requestData.NationalInsuranceNumber is string nationalInsuranceNumber
                    ? NationalInsuranceNumber.Parse(nationalInsuranceNumber)
                    : null)
                : Option.None<NationalInsuranceNumber?>(),
            gender: attributesToUpdate.Contains(PersonMatchedAttribute.Gender)
                ? Option.Some(requestData.Gender)
                : Option.None<Gender?>(),
            now);
    }

    public static string GetCrmTrnRequestId(Guid currentApplicationUserId, string requestId) =>
        $"{currentApplicationUserId}::{requestId}";
}
