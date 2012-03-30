using NUnit.Framework;

namespace tests.reference.nunit.CodeCoverage
{
    // Testing integration with dotCover
    [TestFixture]
    public class CodeCoverageTests
    {
        [Test]
        public void CoverConstructor()
        {
            var calculator = new Calculator(23);
            Assert.IsNotNull(calculator);
        }

        [Test]
        public void CoverAddition()
        {
            var calculator = new Calculator(23);
            var result = calculator.Add(2);
            Assert.AreEqual(25, result);
        }

        [Test]
        public void CoverSubtraction()
        {
            var calculator = new Calculator(33);
            var result = calculator.Subtract(5);
            Assert.AreEqual(28, result);
        }

        [Test]
        public void CoverMultiplication()
        {
            var calculator = new Calculator(53);
            var result = calculator.Multiply(2);
            Assert.AreEqual(106, result);
        }

        [Test, Ignore("Ignoring so that Calculator.Divide is not covered")]
        public void CoverDivision()
        {
            var calculator = new Calculator(88);
            var result = calculator.Divide(2);
            Assert.AreEqual(44, result);
        }
    }
}