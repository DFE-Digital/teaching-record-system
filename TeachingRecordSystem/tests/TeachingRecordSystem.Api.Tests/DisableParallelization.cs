using Xunit;

namespace TeachingRecordSystem.Api.Tests;

[CollectionDefinition(nameof(DisableParallelization), DisableParallelization = true)]
public class DisableParallelization : ICollectionFixture<ApiFixture> { }
