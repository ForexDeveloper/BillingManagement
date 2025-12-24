using Xunit;

namespace Rest.Integration.Tests.Base;

[CollectionDefinition(nameof(SharedHostCollection), DisableParallelization = true)]
public class SharedHostCollection : ICollectionFixture<SharedHostFixture>;