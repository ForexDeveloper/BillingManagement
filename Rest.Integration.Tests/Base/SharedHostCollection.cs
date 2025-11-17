using Xunit;

namespace IntegrationTest.Server;

[CollectionDefinition(nameof(SharedHostCollection))]
public class SharedHostCollection : ICollectionFixture<SharedHostFixture>
{

}