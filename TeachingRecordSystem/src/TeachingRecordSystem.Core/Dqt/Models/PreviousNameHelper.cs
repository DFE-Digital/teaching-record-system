using Microsoft.Extensions.Configuration;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using FullName = (string FirstName, string MiddleName, string LastName);

namespace TeachingRecordSystem.Core.Dqt.Models;

public class PreviousNameHelper(IConfiguration configuration)
{
    public FullName[] GetFullPreviousNames(IEnumerable<dfeta_previousname> previousNames, Person person) =>
        GetFullPreviousNames(previousNames, (person.FirstName, person.MiddleName, person.LastName));

    public FullName[] GetFullPreviousNames(IEnumerable<dfeta_previousname> previousNames, Contact contact) =>
        GetFullPreviousNames(previousNames, (contact.FirstName, contact.MiddleName, contact.LastName));

    public FullName[] GetFullPreviousNames(IEnumerable<dfeta_previousname> previousNames, (string First, string? Middle, string Last) currentName)
    {
        var concurrentNameChangeWindow = TimeSpan.FromSeconds(configuration.GetValue("ConcurrentNameChangeWindowSeconds", 5));

        var result = new List<FullName>();

        var currentFirstName = currentName.First;
        var currentMiddleName = currentName.Middle ?? "";
        var currentLastName = currentName.Last;
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
