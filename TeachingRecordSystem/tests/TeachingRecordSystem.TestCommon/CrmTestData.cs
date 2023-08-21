using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.TestCommon;

public partial class CrmTestData
{
    private static readonly object _gate = new();
    private static readonly HashSet<string> _emails = new();

    private readonly Func<Task<string>> _generateTrn;

    public CrmTestData(
        IOrganizationServiceAsync organizationService,
        ReferenceDataCache referenceDataCache,
        Func<Task<string>> generateTrn)
    {
        OrganizationService = organizationService;
        ReferenceDataCache = referenceDataCache;
        _generateTrn = generateTrn;
    }

    public IOrganizationServiceAsync OrganizationService;
    public ReferenceDataCache ReferenceDataCache { get; }  

    public DateOnly GenerateDateOfBirth() => DateOnly.FromDateTime(Faker.Identification.DateOfBirth());

    public DateOnly GenerateChangedDateOfBirth(DateOnly currentDateOfBirth)
    {
        DateOnly newDateOfBirth;

        do
        {
            newDateOfBirth = GenerateDateOfBirth();
        }
        while (newDateOfBirth == currentDateOfBirth);

        return newDateOfBirth;
    }

    public string GenerateFirstName() => Faker.Name.First();

    public string GenerateMiddleName() => Faker.Name.Middle();

    public string GenerateLastName() => Faker.Name.Last();

    public string GenerateChangedLastName(string currentLastName)
    {
        string newLastName;

        do
        {
            newLastName = GenerateFirstName();
        }
        while (newLastName == currentLastName);

        return newLastName;
    }

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

    public static async Task<string> GetBase64EncodedFileContent(Stream file)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var buffer = ms.ToArray();
        return Convert.ToBase64String(buffer);
    }
}
