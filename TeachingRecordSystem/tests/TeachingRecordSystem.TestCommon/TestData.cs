using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    private static readonly object _gate = new();
    private static readonly HashSet<string> _emails = [];
    private static readonly HashSet<string> _mobileNumbers = [];

    private readonly Func<Task<string>> _generateTrn;

    public TestData(
        IDbContextFactory<TrsDbContext> dbContextFactory,
        IOrganizationServiceAsync organizationService,
        ReferenceDataCache referenceDataCache,
        FakeTrnGenerator trnGenerator,
        TestDataSyncConfiguration syncConfiguration)
        : this(
              dbContextFactory,
              organizationService,
              referenceDataCache,
              generateTrn: () => Task.FromResult(trnGenerator.GenerateTrn()),
              syncConfiguration)
    {
    }

    private TestData(
        IDbContextFactory<TrsDbContext> dbContextFactory,
        IOrganizationServiceAsync organizationService,
        ReferenceDataCache referenceDataCache,
        Func<Task<string>> generateTrn,
        TestDataSyncConfiguration syncConfiguration)
    {
        DbContextFactory = dbContextFactory;
        OrganizationService = organizationService;
        ReferenceDataCache = referenceDataCache;
        _generateTrn = generateTrn;
        SyncConfiguration = syncConfiguration;
    }

    public IDbContextFactory<TrsDbContext> DbContextFactory { get; }

    public IOrganizationServiceAsync OrganizationService { get; }

    public ReferenceDataCache ReferenceDataCache { get; }

    private TestDataSyncConfiguration SyncConfiguration { get; }

    public static TestData CreateWithCustomTrnGeneration(
        IDbContextFactory<TrsDbContext> dbContextFactory,
        IOrganizationServiceAsync organizationService,
        ReferenceDataCache referenceDataCache,
        Func<Task<string>> generateTrn,
        TestDataSyncConfiguration syncConfiguration)
    {
        return new TestData(dbContextFactory, organizationService, referenceDataCache, generateTrn, syncConfiguration);
    }

    public static async Task<string> GetBase64EncodedFileContent(Stream file)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var buffer = ms.ToArray();
        return Convert.ToBase64String(buffer);
    }

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

    public string GenerateChangedFirstName(string currentFirstName)
    {
        string newFirstName;

        do
        {
            newFirstName = GenerateLastName();
        }
        while (newFirstName == currentFirstName);

        return newFirstName;
    }

    public string GenerateMiddleName() => Faker.Name.Middle();

    public string GenerateChangedMiddleName(string currentMiddleName)
    {
        string newMiddleName;

        do
        {
            newMiddleName = GenerateMiddleName();
        }
        while (newMiddleName == currentMiddleName);

        return newMiddleName;
    }

    public string GenerateLastName() => Faker.Name.Last();

    public string GenerateChangedLastName(string currentLastName)
    {
        string newLastName;

        do
        {
            newLastName = GenerateLastName();
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

    public string GenerateUniqueMobileNumber()
    {
        string mobileNumber;

        lock (_gate)
        {
            do
            {
                mobileNumber = Faker.Phone.Number();
            }
            while (!_mobileNumbers.Add(mobileNumber));
        }

        return mobileNumber;
    }

    public Task<string> GenerateTrn() => _generateTrn();

    public Contact_GenderCode GenerateGender() => Faker.Enum.Random<Contact_GenderCode>();

    public string GenerateNationalInsuranceNumber() => Faker.Identification.UkNationalInsuranceNumber();

    protected async Task<T> WithDbContext<T>(Func<TrsDbContext, Task<T>> action)
    {
        using var dbContext = await DbContextFactory.CreateDbContextAsync();
        return await action(dbContext);
    }

    protected async Task WithDbContext(Func<TrsDbContext, Task> action)
    {
        using var dbContext = await DbContextFactory.CreateDbContextAsync();
        await action(dbContext);
    }
}

public sealed class TestDataSyncConfiguration
{
    private TestDataSyncConfiguration(bool syncEnabled, TrsDataSyncHelper? helper)
    {
        SyncEnabled = syncEnabled;
        TrsDataSyncHelper = helper;
    }

    [MemberNotNullWhen(true, nameof(TrsDataSyncHelper))]
    public bool SyncEnabled { get; }

    public TrsDataSyncHelper? TrsDataSyncHelper { get; }

    public static TestDataSyncConfiguration NoSync() => new(false, null);

    public static TestDataSyncConfiguration Sync(TrsDataSyncHelper helper) => new(true, helper);

    public async Task SyncIfEnabled(Func<TrsDataSyncHelper, Task> action, bool? overrideSync = null)
    {
        if (overrideSync == true && !SyncEnabled)
        {
            throw new InvalidOperationException("TestData instance has not been configured to support syncing.");
        }

        if (SyncEnabled && overrideSync != false)
        {
            await action(TrsDataSyncHelper);
        }
    }
}
