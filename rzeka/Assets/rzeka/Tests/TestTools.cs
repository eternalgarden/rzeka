using NUnit.Framework;


namespace Rzeka.Tests
{
    public static class TestTools
    {
        public static void AssertEqual<T>(T expected, T actual)
        {
            Assert.AreEqual(expected, actual, $"Expected: {expected} while actual: {actual}.");
        }
    }
}

