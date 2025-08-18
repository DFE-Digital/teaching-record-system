namespace TeachingRecordSystem.SupportUi.Pages.Persons.Merge;

public record PersonAttribute<T>(T PrimaryPersonValue, T SecondaryPersonValue, bool Different);
