using Xunit;
using Xunit.Extensions;

namespace tests.xunit.eyeball.coverage
{
    public class CodeCoverageTheoryTests
    {
        [Theory]
        [InlineData(2, 4, 6)]
        [InlineData(10, 15, 25)]
        [InlineData(64, 33, 97)]
        public void CoverTheory(int initialValue, int addition, int expectedValue)
        {
            var calculator = new Calculator(initialValue);
            var result = calculator.Add(addition);
            Assert.Equal(expectedValue, result);
        }
    }
}