using System.Collections.Generic;
using Xunit;

namespace Foo
{
    public class MyTest
    {
        [Theory]
        [MemberData("NonPublicDataEnumerator")]
        public void Test1(int value)
        {
            Assert.Equal(42, value);
        }

        [Theory]
        [MemberData("NonStaticDataEnumerator")]
        public void Test2(int value)
        {
            Assert.Equal(42, value);
        }

        [Theory]
        [MemberData("InvalidReturnTypeDataEnumerator")]
        public void Test3(int value)
        {
            Assert.Equal(42, value);
        }

        [Theory]
        [MemberData("NonPublicMethod")]
        public void Test4(int value)
        {
            Assert.Equal(42, value);
        }

        [Theory]
        [MemberData("NonStaticMethod")]
        public void Test5(int value)
        {
            Assert.Equal(42, value);
        }

        [Theory]
        [MemberData("InvalidReturnTypeMethod")]
        public void Test6(int value)
        {
            Assert.Equal(42, value);
        }

        [Theory]
        [MemberData("NonPublicField")]
        public void Test7(int value)
        {
            Assert.Equal(42, value);
        }

        [Theory]
        [MemberData("NonStaticField")]
        public void Test8(int value)
        {
            Assert.Equal(42, value);
        }

        [Theory]
        [MemberData("InvalidReturnTypeField")]
        public void Test9(int value)
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
 
        // Needs to be public
        private static IEnumerable<object[]> NonPublicMethod()
        {
            yield return new object[] { 42 };
        }

        // Needs to be static
        public IEnumerable<object[]> NonStaticMethod()
        {
            yield return new object[] { 42 };
        }

        // Invalid return type signature
        public static IEnumerable<int[]> InvalidReturnTypeMethod()
        {
            return new List<int[]>(new int[] { 42 });
        }
 
        // Needs to be public
        private static IEnumerable<object[]> NonPublicField = new List<object[]> {
            new object[] { 42 }
        };

        // Needs to be static
        public IEnumerable<object[]> NonStaticField = new List<object[]> {
            new object[] { 42 }
        };

        // Invalid return type signature
        public static IEnumerable<int[]> InvalidReturnTypeField = new List<int[]> {
            new int[] { 42 }
        };
    }
}
