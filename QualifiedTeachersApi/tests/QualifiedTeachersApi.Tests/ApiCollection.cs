using Xunit;

namespace QualifiedTeachersApi.Tests;

[CollectionDefinition("Api", DisableParallelization = true)]
public class ApiCollection : ICollectionFixture<ApiFixture>
{
}
