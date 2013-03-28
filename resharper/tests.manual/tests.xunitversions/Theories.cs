using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;
using Xunit.Extensions;

namespace tests.xunitversions
{
    public class Theories
    {
        [Theory]
        [InlineData("hello")]
        [InlineData("world")]
        public void TheoryTest(string value)
        {
            Assert.Equal(5, value.Length);
        }

        [Theory]
        [PropertyData("Data")]
        public void Sausages(int i)
        {
            Thread.Sleep(2000);
        }

        public static IEnumerable<object[]> Data
        {
            get { return Enumerable.Range(1, 100).Select(x => new object[] { x }); }
        }
    }
}