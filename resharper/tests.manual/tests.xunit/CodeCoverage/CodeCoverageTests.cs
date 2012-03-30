using Xunit;

namespace tests.xunit.eyeball.coverage
{
    // Testing integration with dotCover
    public class CodeCoverageTests
    {
        [Fact]
        public void CoverConstructor()
        {
            var calculator = new Calculator(23);
            Assert.NotNull(calculator);
        }

        [Fact]
        public void CoverAddition()
        {
            var calculator = new Calculator(23);
            var result = calculator.Add(2);
            Assert.Equal(25, result);
        }

        [Fact]
        public void CoverSubtraction()
        {
            var calculator = new Calculator(33);
            var result = calculator.Subtract(5);
            Assert.Equal(28, result);
        }

        [Fact]
        public void CoverMultiplication()
        {
            var calculator = new Calculator(53);
            var result = calculator.Multiply(2);
            Assert.Equal(106, result);
        }

        [Fact(Skip = "Ignoring so that Calculator.Divide is not covered")]
        public void CoverDivision()
        {
            var calculator = new Calculator(88);
            var result = calculator.Divide(2);
            Assert.Equal(44, result);
        }
    }
}