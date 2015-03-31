using System.Collections.Generic;
using Xunit;
using Xunit.Extensions;

namespace Foo
{
    public class MyTest
    {
        [Theory]
        [PropertyData("DataEnumerator")]
        public void Test1(int value)
        {
            Assert.Equal(42, value);
        }

        [Theory]
        [PropertyData("ListReturnTypeDataEnumerator")]
        public void Test2(int value)
        {
            Assert.Equal(42, value);
        }

        public static IEnumerable<object[]> DataEnumerator
        {
            get { yield return new object[] { 42 }; }
        }

        // Note return type
        public static IList<object[]> ListReturnTypeDataEnumerator
        {
            get { return new List<object[]>(new object[] { 42 }); }
        }
    }
}
