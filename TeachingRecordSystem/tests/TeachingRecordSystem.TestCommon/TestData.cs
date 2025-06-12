using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    private static readonly object _gate = new();
    private static readonly HashSet<string> _emails = [];
    private static readonly HashSet<string> _mobileNumbers = [];
    private static int _applicationUserNumber = 1;

    private readonly Func<Task<string>> _generateTrn;

    public TestData(
        IDbContextFactory<TrsDbContext> dbContextFactory,
        IOrganizationServiceAsync organizationService,
        ReferenceDataCache referenceDataCache,
        IClock clock,
        FakeTrnGenerator trnGenerator,
        TestDataSyncConfiguration syncConfiguration)
        : this(
              dbContextFactory,
              organizationService,
              referenceDataCache,
              clock,
              generateTrn: () => Task.FromResult(trnGenerator.GenerateTrn()),
              syncConfiguration)
    {
    }

    private TestData(
        IDbContextFactory<TrsDbContext> dbContextFactory,
        IOrganizationServiceAsync organizationService,
        ReferenceDataCache referenceDataCache,
        IClock clock,
        Func<Task<string>> generateTrn,
        TestDataSyncConfiguration syncConfiguration)
    {
        DbContextFactory = dbContextFactory;
        OrganizationService = organizationService;
        ReferenceDataCache = referenceDataCache;
        Clock = clock;
        _generateTrn = generateTrn;
        SyncConfiguration = syncConfiguration;
    }

    // https://stackoverflow.com/a/30290754
    public static byte[] JpegImage { get; } =
    {
        0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01, 0x01, 0x01, 0x00, 0x48, 0x00, 0x48, 0x00, 0x00,
        0xFF, 0xDB, 0x00, 0x43, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xC2, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01,
        0x11, 0x00, 0xFF, 0xC4, 0x00, 0x14, 0x10, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0xFF, 0xDA, 0x00, 0x08, 0x01, 0x01, 0x00, 0x01, 0x3F, 0x10
    };

    public IClock Clock { get; }

    public IDbContextFactory<TrsDbContext> DbContextFactory { get; }

    public IOrganizationServiceAsync OrganizationService { get; }

    public ReferenceDataCache ReferenceDataCache { get; }

    private TestDataSyncConfiguration SyncConfiguration { get; }

    public static TestData CreateWithCustomTrnGeneration(
        IDbContextFactory<TrsDbContext> dbContextFactory,
        IOrganizationServiceAsync organizationService,
        ReferenceDataCache referenceDataCache,
        IClock clock,
        Func<Task<string>> generateTrn,
        TestDataSyncConfiguration syncConfiguration)
    {
        return new TestData(dbContextFactory, organizationService, referenceDataCache, clock, generateTrn, syncConfiguration);
    }

    public static async Task<string> GetBase64EncodedFileContentAsync(Stream file)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var buffer = ms.ToArray();
        return Convert.ToBase64String(buffer);
    }

    public string GenerateApplicationUserName() => Faker.Company.Name();

    public string GenerateApplicationUserShortName() => $"app-{Interlocked.Increment(ref _applicationUserNumber)}";

    public string GenerateChangedApplicationUserName(string currentName)
    {
        string newName;

        do
        {
            newName = GenerateApplicationUserName();
        }
        while (newName == currentName);

        return newName;
    }

    public DateOnly GenerateDateOfBirth()
    {
        DateOnly dateOfBirth;

        do
        {
            dateOfBirth = DateOnly.FromDateTime(Faker.Identification.DateOfBirth());
        }
        while (dateOfBirth >= DateOnly.FromDateTime(Clock.UtcNow) || dateOfBirth < new DateOnly(1940, 1, 1));

        return dateOfBirth;
    }

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

    public string GenerateLastName()
    {
        string lastName;

        do
        {
            lastName = Faker.Name.Last();
        }
        while (lastName.Contains('\''));

        return lastName;
    }

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

    public string GenerateName()
    {
        string fullName;

        do
        {
            fullName = Faker.Name.FullName();
        }
        while (fullName.Contains('\''));

        return fullName;
    }

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

    public string GenerateChangedNationalInsuranceNumber(string currentNationalInsuranceNumber)
    {
        string newNationalInsuranceNumber;

        do
        {
            newNationalInsuranceNumber = GenerateNationalInsuranceNumber();
        }
        while (newNationalInsuranceNumber == currentNationalInsuranceNumber);

        return newNationalInsuranceNumber;
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
                mobileNumber = $"0{Faker.RandomNumber.Next(7000000000, 7999999999)}";
            }
            while (!_mobileNumbers.Add(mobileNumber));
        }

        return mobileNumber;
    }

    public Task<string> GenerateTrnAsync() => _generateTrn();

    public Contact_GenderCode GenerateGender() => Faker.Enum.Random<Contact_GenderCode>();

    public DateOnly GenerateDate() => GenerateDate(min: new DateOnly(1990, 1, 1), max: new DateOnly(2030, 1, 1));

    public DateOnly GenerateDate(DateOnly min, DateOnly? max = null)
    {
        if (max is not null && max <= min)
        {
            throw new ArgumentOutOfRangeException(nameof(max), "max must be after min.");
        }

        max ??= min.AddYears(1);

        var daysDiff = (int)(max.Value.ToDateTime(TimeOnly.MinValue) - min.ToDateTime(TimeOnly.MinValue)).TotalDays;
        Debug.Assert(daysDiff > 0);
        return min.AddDays(Random.Shared.Next(minValue: 1, maxValue: daysDiff + 1));
    }

    public DateOnly GenerateChangedDate(DateOnly currentDate, DateOnly min, DateOnly? max = null)
    {
        DateOnly newDate;

        do
        {
            newDate = GenerateDate(min, max);
        }
        while (newDate == currentDate);

        return newDate;
    }

    public string GenerateNationalInsuranceNumber() => Faker.Identification.UkNationalInsuranceNumber();

    public string GenerateLoremIpsum() => Faker.Lorem.Paragraph();

    public string GenerateUrl() => Faker.Internet.Url();

    public T GenerateEnumValue<T>() where T : Enum => Faker.Enum.Random<T>();

    public T GenerateChangedEnumValue<T>(T? currentValue, T[]? excluding = null) where T : struct, Enum
    {
        T newValue;

        do
        {
            newValue = GenerateEnumValue<T>();
        }
        while (newValue.Equals(currentValue) || (excluding is not null && excluding.Contains(newValue)));

        return newValue;
    }

    protected async Task<T> WithDbContextAsync<T>(Func<TrsDbContext, Task<T>> action)
    {
        using var dbContext = await DbContextFactory.CreateDbContextAsync();
        return await action(dbContext);
    }

    protected async Task WithDbContextAsync(Func<TrsDbContext, Task> action)
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

    public async Task<bool> SyncIfEnabledAsync(Func<TrsDataSyncHelper, Task> action, bool? overrideSync = null)
    {
        if (overrideSync == true && !SyncEnabled)
        {
            throw new InvalidOperationException("TestData instance has not been configured to support syncing.");
        }

        if (SyncEnabled && overrideSync != false)
        {
            await action(TrsDataSyncHelper);
            return true;
        }

        return false;
    }
}
