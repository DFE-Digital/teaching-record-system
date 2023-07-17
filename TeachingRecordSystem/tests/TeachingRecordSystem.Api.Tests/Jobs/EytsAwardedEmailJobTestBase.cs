namespace TeachingRecordSystem.Api.Tests.Jobs;

[TestClass("EytsAwardedEmailJob")]
[ExecuteSqlSetup("delete from eyts_awarded_emails_jobs")]
public abstract class EytsAwardedEmailJobTestBase
{
    public EytsAwardedEmailJobTestBase(DbFixture dbFixture)
    {
        DbFixture = dbFixture;
    }

    public DbFixture DbFixture { get; }
}
