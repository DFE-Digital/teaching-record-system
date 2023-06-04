using Xunit;

namespace QualifiedTeachersApi.Tests.Jobs;

[CollectionDefinition("Job", DisableParallelization = true)]
public class JobCollection : ICollectionFixture<JobFixture>
{
}
