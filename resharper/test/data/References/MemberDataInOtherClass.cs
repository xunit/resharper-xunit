using System.Collections.Generic;
using Xunit;

namespace Foo
{
    public class MyTest
    {
        [Theory]
        [MemberData("DataEnumerator", MemberType = typeof(ProvidesMemberData))]
        public void DataComesFromProperty(int value)
        {
            Assert.Equal(42, value);
        }
    }

    public class ProvidesMemberData
    {
        public static IEnumerable<object[]> DataEnumerator
        {
            get { yield return new object[] { 42 }; }
        }
    }
}
