namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetNotesByContactIdQuery(Guid ContactId) : ICrmQuery<TeacherNotesResult>;

public record TeacherNotesResult(Annotation[] Annotations, (IncidentResolution Resolution, Incident Incident)[] IncidentResolutions, CrmTask[] Tasks);
