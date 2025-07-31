namespace TeachingRecordSystem.SupportUi.Pages.Persons.ManualMerge;

public record PersonAttribute<T>(T PrimaryPersonValue, T SecondaryPersonValue, bool Different);
