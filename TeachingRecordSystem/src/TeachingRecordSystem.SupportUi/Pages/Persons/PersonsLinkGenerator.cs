using TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;
using TeachingRecordSystem.SupportUi.Pages.Persons.MergePerson;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

namespace TeachingRecordSystem.SupportUi.Pages.Persons;

public class PersonsLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(string? search = null, PersonStatus[]? statuses = null, PersonSearchSortByOption? sortBy = null, int? pageNumber = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/Index", routeValues: new { search, statuses, sortBy, pageNumber });

    public AddPersonLinkGenerator AddPerson { get; } = new(linkGenerator);
    public MergePersonLinkGenerator MergePerson { get; } = new(linkGenerator);
    public PersonDetailLinkGenerator PersonDetail { get; } = new(linkGenerator);
}
