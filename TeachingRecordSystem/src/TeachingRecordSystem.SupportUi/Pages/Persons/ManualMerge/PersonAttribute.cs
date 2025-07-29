namespace TeachingRecordSystem.SupportUi.Pages.Persons.ManualMerge;

public record PersonAttribute<T>(T PrimaryRecordValue, T SecondaryRecordValue, bool Different);
