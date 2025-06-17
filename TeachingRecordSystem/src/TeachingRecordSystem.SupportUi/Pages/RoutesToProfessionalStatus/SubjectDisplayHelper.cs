namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus;

public static class SubjectDisplayHelper
{
    public static async Task<string[]?> GetFormattedSubjectNamesAsync(
        IEnumerable<Guid>? subjectIds,
        ReferenceDataCache referenceDataCache)
    {
        if (subjectIds == null)
        {
            return null;
        }

        var allSubjects = await referenceDataCache.GetTrainingSubjectsAsync();

        return subjectIds
            .Join(
                allSubjects,
                id => id,
                subject => subject.TrainingSubjectId,
                (id, subject) => subject
            )
            .OrderBy(subject => subject.Name)
            .Select(subject => $"{subject.Reference} - {subject.Name}")
            .ToArray();
    }
}
