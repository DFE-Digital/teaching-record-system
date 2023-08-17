using Microsoft.PowerPlatform.Dataverse.Client;

namespace TeachingRecordSystem.TestCommon;

public partial class CrmTestData
{
    private static readonly object _gate = new();
    private static readonly HashSet<string> _emails = new();

    private readonly Func<Task<string>> _generateTrn;

    public CrmTestData(
        IOrganizationServiceAsync organizationService,
        Func<Task<string>> generateTrn)
    {
        OrganizationService = organizationService;
        _generateTrn = generateTrn;
    }

    public IOrganizationServiceAsync OrganizationService;

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

    public virtual Task<string> GenerateTrn() => _generateTrn();
}
