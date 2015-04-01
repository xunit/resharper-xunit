using System.Collections.Generic;
using Xunit;
using Xunit.Extensions;

namespace Foo
{
    public class MyTest
    {
        [Theory]
        [PropertyData("NonPublicDataEnumerator")]
        public void Test1(int value)
        {
            Assert.Equal(42, value);
        }

        [Theory]
        [PropertyData("NonStaticDataEnumerator")]
        public void Test2(int value)
        {
            Assert.Equal(42, value);
        }

        [Theory]
        [PropertyData("InvalidReturnTypeDataEnumerator")]
        public void Test3(int value)
        {
            Assert.Equal(42, value);
        }

        // Needs to be public
        private static IEnumerable<object[]> NonPublicDataEnumerator
        {
            get { yield return new object[] { 42 }; }
        }

        // Needs to be static
        public IEnumerable<object[]> NonStaticDataEnumerator
        {
            get { yield return new object[] { 42 }; }
        }

        // Invalid return type signature
        public static IEnumerable<int[]> InvalidReturnTypeDataEnumerator
        {
            get { return new List<int[]>(new int[] { 42 }); }
        }
    }
}
