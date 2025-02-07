using FluentAssertions;

namespace WebApp.Tests.Unit
{
    public class Dummy
    {
        [Fact]
        public void Test1()
        {
            true.Should().BeTrue();
        }
    }
}
