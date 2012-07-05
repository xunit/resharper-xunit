using NUnit.Framework;

namespace tests.reference.nunit.CodeCoverage
{
    [TestFixture]
    public class CodeCoverageRowTests
    {
        [Test]
        [TestCase(2, 4, 6)]
        [TestCase(10, 15, 25)]
        [TestCase(64, 33, 97)]
        public void CoverTheory(int initialValue, int addition, int expectedValue)
        {
            var calculator = new Calculator(initialValue);
            var result = calculator.Add(addition);
            Assert.AreEqual(expectedValue, result);
        }
    }
}