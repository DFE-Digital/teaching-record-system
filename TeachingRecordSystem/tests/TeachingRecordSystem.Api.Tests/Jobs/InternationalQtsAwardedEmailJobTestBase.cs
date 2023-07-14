namespace TeachingRecordSystem.Api.Tests.Jobs;

[TestClass("InternationalQtsAwardedEmailJob")]
[ExecuteSqlSetup("delete from international_qts_awarded_emails_jobs")]
public abstract class InternationalQtsAwardedEmailJobTestBase
{
    public InternationalQtsAwardedEmailJobTestBase(DbFixture dbFixture)
    {
        DbFixture = dbFixture;
    }

    public DbFixture DbFixture { get; }
}
