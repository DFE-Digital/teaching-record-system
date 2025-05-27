using System.Diagnostics.CodeAnalysis;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.TrnRequests;

public record GetTrnRequestResult(Contact Contact, TrnRequestMetadata Metadata)
{
    public string? Trn => Contact.dfeta_TRN;
    public bool PotentialDuplicate => Metadata.PotentialDuplicate == true;
    public string? TrnToken => Metadata.TrnToken;
    [MemberNotNullWhen(true, nameof(Trn))]
    public bool IsCompleted => Trn is not null;
}
