using Xunit;

namespace Rest.Integration.Tests.Base;

[CollectionDefinition(nameof(SharedHostCollection))]
public class SharedHostCollection : ICollectionFixture<SharedHostFixture>;