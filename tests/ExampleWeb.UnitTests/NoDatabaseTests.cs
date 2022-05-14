using Xunit;

namespace ExampleWeb.UnitTests;

public class NoDatabaseTests : UnitTestBase
{
    public NoDatabaseTests() : base(null) { }

    [Fact]
    public void Test()
    {
        Assert.True(true);
    }
}
