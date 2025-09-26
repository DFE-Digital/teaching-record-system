namespace TeachingRecordSystem.SupportUi.Pages.Persons.Merge;

#pragma warning disable CA1711
public record PersonAttribute<T>(T PrimaryPersonValue, T SecondaryPersonValue, bool Different);
#pragma warning restore CA1711
