using System.Collections.Generic;
using Xunit;
using Xunit.Extensions;

namespace Foo
{
    public class MyTest : PropertyDataBase
    {
        [Theory]
        [PropertyData("DataEnumerator")]
        public void DataComesFromProperty(int value)
        {
            Assert.Equal(42, value);
        }
    }

    public class PropertyDataBase
    {
        public static IEnumerable<object[]> DataEnumerator
        {
            get { yield return new object[] { 42 }; }
        }
    }
}
