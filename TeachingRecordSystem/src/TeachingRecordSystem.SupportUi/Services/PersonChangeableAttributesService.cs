namespace TeachingRecordSystem.SupportUi.Services;

public class PersonChangeableAttributesService
{
    public IEnumerable<ResolvedAttribute> GetResolvableAttributes(IEnumerable<ResolvedAttribute> items)
    {
        var changeableAttributes = items.Where(x => x.Source is not null).ToList();
        return changeableAttributes;
    }
}

#pragma warning disable CA1711
public record ResolvedAttribute(PersonMatchedAttribute Attribute, PersonAttributeSource? Source);
#pragma warning restore CA1711


//public enum PersonAttributeSource
//{
//    ExistingRecord = 0,
//    TrnRequest = 1
//}


public enum PersonAttributeSource
{
    PrimaryPerson = 0,
    SecondaryPerson = 1
}
