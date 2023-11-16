namespace TeachingRecordSystem.Core.Dqt.Models;

public record IncidentDetail(Incident Incident, Contact Contact, Subject Subject, IncidentDocument[] IncidentDocuments);

public record IncidentDocument(dfeta_document Document, Annotation Annotation);
