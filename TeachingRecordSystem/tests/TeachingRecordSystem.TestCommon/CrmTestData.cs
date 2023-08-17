using Microsoft.PowerPlatform.Dataverse.Client;

namespace TeachingRecordSystem.TestCommon;

public partial class CrmTestData
{
    private static readonly object _gate = new();
    private static readonly HashSet<string> _emails = new();

    private readonly IOrganizationServiceAsync _organizationService;

    public CrmTestData(IOrganizationServiceAsync organizationService)
    {
        _organizationService = organizationService;
    }

    public DateOnly GenerateDateOfBirth() => DateOnly.FromDateTime(Faker.Identification.DateOfBirth());

    public string GenerateFirstName() => Faker.Name.First();

    public string GenerateMiddleName() => Faker.Name.Middle();

    public string GenerateLastName() => Faker.Name.Last();

    public string GenerateName() => Faker.Name.FullName();

    public string GenerateChangedName(string currentName)
    {
        string newName;

        do
        {
            newName = GenerateName();
        }
        while (newName == currentName);

        return newName;
    }

    public string GenerateUniqueEmail()
    {
        string email;

        lock (_gate)
        {
            do
            {
                email = Faker.Internet.Email();
            }
            while (!_emails.Add(email));
        }

        return email;
    }
}
