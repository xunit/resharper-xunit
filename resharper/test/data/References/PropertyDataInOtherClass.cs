using System.Collections.Generic;
using Xunit;
using Xunit.Extensions;

namespace Foo
{
    public class MyTest
    {
        [Theory]
        [PropertyData("DataEnumerator", PropertyType = typeof(ProvidesPropertyData))]
        public void DataComesFromProperty(int value)
        {
            Assert.Equal(42, value);
        }
    }

    public class ProvidesPropertyData
    {
        public static IEnumerable<object[]> DataEnumerator
        {
            get { yield return new object[] { 42 }; }
        }
    }
}
