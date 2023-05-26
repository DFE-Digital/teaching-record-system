using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace QualifiedTeachersApi.Infrastructure.EntityFramework;

public class DictionaryValueComparer : ValueComparer<Dictionary<string, string>>
{
    public DictionaryValueComparer()
        : base(
            (c1, c2) => (c1 == null && c2 == null) || c1!.Equals(c2),
            c => c.GetHashCode(),
            c => c.ToDictionary(entry => entry.Key, entry => entry.Value))
    {
    }
}
