using Xunit;

namespace JWTTesting.UnitTests.Fixtures
{
    [CollectionDefinition("Static State Collection")]
    public class StaticStateCollection : ICollectionFixture<StaticStateFixture>
    {
    }
}
