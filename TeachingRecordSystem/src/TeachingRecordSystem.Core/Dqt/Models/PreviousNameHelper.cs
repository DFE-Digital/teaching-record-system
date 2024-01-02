namespace TeachingRecordSystem.Core.Dqt.Models;
using FullName = (string FirstName, string MiddleName, string LastName);

public static class PreviousNameHelper
{
    public static FullName[] GetFullPreviousNames(
        IEnumerable<dfeta_previousname> previousNames,
        Contact contact,
        TimeSpan concurrentNameChangeWindow)
    {
        var result = new List<FullName>();

        var currentFirstName = contact.FirstName!;
        var currentMiddleName = contact.MiddleName ?? "";
        var currentLastName = contact.LastName!;
        DateTime? createdOnBaseline = null;

        foreach (var previousName in previousNames.OrderByDescending(p => p.CreatedOn))
        {
            if (createdOnBaseline is null)
            {
                createdOnBaseline = previousName.CreatedOn;
            }
            else if (createdOnBaseline - previousName.CreatedOn > concurrentNameChangeWindow)
            {
                result.Add(new FullName(currentFirstName, currentMiddleName, currentLastName));
                createdOnBaseline = previousName.CreatedOn;
            }

            switch (previousName.dfeta_Type)
            {
                case dfeta_NameType.FirstName:
                    currentFirstName = previousName.dfeta_name!;
                    break;
                case dfeta_NameType.MiddleName:
                    currentMiddleName = previousName.dfeta_name ?? "";
                    break;
                case dfeta_NameType.LastName:
                    currentLastName = previousName.dfeta_name!;
                    break;
                default:
                    break;
            }
        }

        if (createdOnBaseline is not null)
        {
            result.Add(new FullName(currentFirstName, currentMiddleName, currentLastName));
        }

        return result.ToArray();
    }
}
