namespace TeachingRecordSystem.SupportUi.Pages.Persons.MergePerson;

#pragma warning disable CA1711
public record PersonAttribute<T>(T PrimaryPersonValue, T SecondaryPersonValue, bool Different);
#pragma warning restore CA1711
