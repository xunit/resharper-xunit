using Xunit;

namespace Foo
{
    // No discovery
    internal class InternalClass
    {
        [Fact]
        public void PublicMethod()
        {
        }

        [Fact]
        internal void InternalMethod()
        {
        }

        [Fact]
        protected void ProtectedMethod()
        {
        }

        [Fact]
        private void PrivateMethod()
        {
        }

        [Fact]
        protected internal void ProtectedInternalMethod()
        {
        }

        [Fact]
        public static void PublicStaticMethod()
        {
        }

        [Fact]
        internal static void InternalStaticMethod()
        {
        }

        [Fact]
        protected static void ProtectedStaticMethod()
        {
        }

        [Fact]
        private static void PrivateStaticMethod()
        {
        }

        [Fact]
        protected internal static void ProtectedInternalStaticMethod()
        {
        }
    }

    // Mixed discovery
    public class PublicClass
    {
        [Fact]
        public void PublicMethod()
        {
        }

        [Fact]
        internal void InternalMethod()
        {
        }

        [Fact]
        protected void ProtectedMethod()
        {
        }

        [Fact]
        private void PrivateMethod()
        {
        }

        [Fact]
        protected internal void ProtectedInternalMethod()
        {
        }

        [Fact]
        public static void PublicStaticMethod()
        {
        }

        [Fact]
        internal static void InternalStaticMethod()
        {
        }

        [Fact]
        protected static void ProtectedStaticMethod()
        {
        }

        [Fact]
        private static void PrivateStaticMethod()
        {
        }

        [Fact]
        protected internal static void ProtectedInternalStaticMethod()
        {
        }
    }
}
