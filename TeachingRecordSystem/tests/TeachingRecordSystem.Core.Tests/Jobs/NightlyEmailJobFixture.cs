using Microsoft.Extensions.Logging;

namespace TeachingRecordSystem.Core.Tests.Jobs;

[CollectionDefinition(nameof(NightEmailJobCollection), DisableParallelization = true)]
public class NightEmailJobCollection;

public class NightlyEmailJobFixture
{
    public NightlyEmailJobFixture(
        DbFixture dbFixture,
        ReferenceDataCache referenceDataCache,
        FakeTrnGenerator trnGenerator,
        ILoggerFactory loggerFactory)
    {
        DbFixture = dbFixture;
        LoggerFactory = loggerFactory;
        Clock = new();
        LoggerFactory = loggerFactory;

        TestData = new TestData(
            dbFixture.GetDbContextFactory(),
            referenceDataCache,
            Clock,
            trnGenerator);
    }

    public TestableClock Clock { get; }

    public DbFixture DbFixture { get; }

    public ILoggerFactory LoggerFactory { get; }

    public TestData TestData { get; }
}
