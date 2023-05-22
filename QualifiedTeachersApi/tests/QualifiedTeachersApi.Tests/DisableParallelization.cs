using Xunit;

namespace QualifiedTeachersApi.Tests;

[CollectionDefinition(nameof(DisableParallelization), DisableParallelization = true)]
public class DisableParallelization : ICollectionFixture<ApiFixture> { }
