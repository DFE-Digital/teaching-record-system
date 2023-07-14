namespace TeachingRecordSystem.Api.Tests.Jobs;

[TestClass("QtsAwardedEmailJob")]
[ExecuteSqlSetup("delete from qts_awarded_emails_jobs")]
public abstract class QtsAwardedEmailJobTestBase
{
    public QtsAwardedEmailJobTestBase(DbFixture dbFixture)
    {
        DbFixture = dbFixture;
    }

    public DbFixture DbFixture { get; }
}
