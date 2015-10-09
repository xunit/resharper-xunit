using Xunit;

namespace Foo
{
    // No discovery
    internal static class InternalClass
    {
        [Fact]
        public static void PublicStaticMethod()
        {
        }

        [Fact]
        internal static void InternalStaticMethod()
        {
        }

        [Fact]
        private static void PrivateStaticMethod()
        {
        }
    }

    // Mixed discovery
    public static class PublicClass
    {
        [Fact]
        public static void PublicStaticMethod()
        {
        }

        [Fact]
        internal static void InternalStaticMethod()
        {
        }

        [Fact]
        private static void PrivateStaticMethod()
        {
        }
    }
}
