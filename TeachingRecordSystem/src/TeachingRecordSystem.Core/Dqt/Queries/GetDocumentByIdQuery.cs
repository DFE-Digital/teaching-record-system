namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetDocumentByIdQuery(Guid DocumentId) : ICrmQuery<dfeta_document?>;
