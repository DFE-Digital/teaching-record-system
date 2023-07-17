namespace TeachingRecordSystem.Api.Tests.Jobs;

[TestClass("InductionCompletedEmailJob")]
[ExecuteSqlSetup("delete from induction_completed_emails_jobs")]
public abstract class InductionCompletedEmailJobTestBase
{
    public InductionCompletedEmailJobTestBase(DbFixture dbFixture)
    {
        DbFixture = dbFixture;
    }

    public DbFixture DbFixture { get; }
}
