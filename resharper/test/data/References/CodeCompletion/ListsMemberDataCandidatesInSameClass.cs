using System.Collections.Generic;
using Xunit;

namespace Foo
{
    public class MyTest
    {
        [Theory]
        [MemberData("{caret}")]
        public void Test1(int value)
        {
            Assert.Equal(42, value);
        }

        public static IEnumerable<object[]> DataEnumerator
        {
            get { yield return new object[] { 42 }; }
        }

        public static IEnumerable<object[]> AnotherDataEnumerator
        {
            get { yield return new object[] { 42 }; }
        }

        public static IEnumerable<object[]> MoreDataEnumerator
        {
            get { yield return new object[] { 42 }; }
        }

        public static IEnumerable<object[]> GetDataMethod()
        {
            yield return new object[] { 42 };
        }

        public static IEnumerable<object[]> GetDataField = new List<object[]> {
            new object[] { 42 }
        };
    }
}
