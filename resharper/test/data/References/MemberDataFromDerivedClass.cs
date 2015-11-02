using System.Collections.Generic;
using Xunit;

namespace Foo
{
    public class MyTest : MemberDataBase
    {
        [Theory]
        [MemberData("DataEnumerator")]
        public void DataComesFromProperty(int value)
        {
            Assert.Equal(42, value);
        }
    }

    public class MemberDataBase
    {
        public static IEnumerable<object[]> DataEnumerator
        {
            get { yield return new object[] { 42 }; }
        }
    }
}
