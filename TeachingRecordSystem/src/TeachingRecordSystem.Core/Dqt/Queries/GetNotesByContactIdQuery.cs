namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetNotesByContactIdQuery(Guid ContactId) : ICrmQuery<TeacherNotesResult>;

public record TeacherNotesResult(Annotation[] Annotations, (IncidentResolution, Incident)[]? IncidentResolutions, CrmTask[] Tasks);
