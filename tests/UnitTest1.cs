using Rzeka;

namespace Rzeka.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            IRzeka rzeka = new SpringRiver("moew", RzekaRole.Root);
        }
    }
}
