using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.TrnRequests;

public record GetTrnRequestResult(TrnRequestMetadata Metadata, string? ResolvedPersonTrn);
