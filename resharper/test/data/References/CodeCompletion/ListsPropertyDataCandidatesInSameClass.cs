using System.Collections.Generic;
using Xunit;
using Xunit.Extensions;

namespace Foo
{
    public class MyTest
    {
        [Theory]
        [PropertyData("{caret}")]
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
    }
}
